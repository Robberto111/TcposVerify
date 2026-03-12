
using Microsoft.Data.SqlClient;
using System.Data;

namespace TcposVerify.Data;

public class VerifyRepository : IVerifyRepository
{
	private readonly string _connectionString;

	public VerifyRepository(string connectionString)
	{
		_connectionString = connectionString;
	}

	//////////////////////////////////////////////////////////////
	public async Task<bool> ExistsAsync(string uniqueIdentifier)
	{
		const string sql = """
			SELECT COUNT(1)
			FROM   TCPOS_VERIFY
			WHERE  VER_IDENTIFIER = @VER_IDENTIFIER
			""";

		await using var conn = new SqlConnection(_connectionString);
		await using var cmd  = new SqlCommand(sql, conn);
		{
			cmd.Parameters.AddWithValue("@VER_IDENTIFIER", uniqueIdentifier);
		}
		await conn.OpenAsync();
		return (((int?)await cmd.ExecuteScalarAsync()) ?? 0) > 0;
	}


	//////////////////////////////////////////////////////////////
	public async Task<bool/*Creato*/> EnsureRecordAsync(
		DateTime emissioneScontrino,
		DateTime scansionatoIl,
		string negozio,
		string cassa,
		string nTransazione,
		string totaleEuro,
		string uniqueIdentifier
		)
	{
		const string sql = """
			INSERT INTO [dbo].[TCPOS_VERIFY]
					  (VER_STATO_LETTO,  VER_EMESSO,  VER_NEGOZIO,  VER_CASSA,  VER_TRANSAZIONE,  VER_TOTALE,  VER_DATA_LETTO,  VER_DATA_ULTMO_LETTO,  VER_IDENTIFIER)
			   SELECT @VER_STATO_LETTO, @VER_EMESSO, @VER_NEGOZIO, @VER_CASSA, @VER_TRANSAZIONE, @VER_TOTALE, @VER_DATA_LETTO, @VER_DATA_ULTMO_LETTO, @VER_IDENTIFIER
			WHERE NOT EXISTS (SELECT 1 FROM [dbo].[TCPOS_VERIFY] WHERE VER_IDENTIFIER = @VER_IDENTIFIER);

			SELECT @@ROWCOUNT;
			""";

		await using var connction = new SqlConnection(_connectionString);
		await using var command = new SqlCommand(sql, connction);
		
		// IN
		command.Parameters.AddWithValue("@VER_STATO_LETTO", (int)LETTO_STATO.VALIDO);
		command.Parameters.AddWithValue("@VER_EMESSO", emissioneScontrino);
		command.Parameters.AddWithValue("@VER_NEGOZIO", negozio);
		command.Parameters.AddWithValue("@VER_CASSA", cassa);
		command.Parameters.AddWithValue("@VER_TRANSAZIONE", nTransazione);
		command.Parameters.AddWithValue("@VER_TOTALE", int.TryParse(totaleEuro, out int totaleInt) ? totaleInt : -1);
		command.Parameters.AddWithValue("@VER_DATA_LETTO", scansionatoIl);
		command.Parameters.AddWithValue("@VER_DATA_ULTMO_LETTO", scansionatoIl);
		command.Parameters.AddWithValue("@VER_IDENTIFIER", uniqueIdentifier);

		
		await connction.OpenAsync();
		return (int)(await command.ExecuteScalarAsync() ?? -1) == 1;
	}


	//////////////////////////////////////////////////////////////
	public async Task<int/*count*/> UpdateRecordAsync(DateTime ultimoTentativoDiLettura, string uniqueIdentifier, LETTO_STATO? stato = null)
	{
		int statoLetto = (int)LETTO_STATO.VALIDO;

		switch(stato)
		{
			case LETTO_STATO.INVALIDO:
			case LETTO_STATO.SCADUTO:
				statoLetto = (int)stato;
				break;
		}

		const string sql = """
			UPDATE TCPOS_VERIFY
			SET    VER_STATO_LETTO = @VER_STATO_LETTO,
					VER_DATA_ULTMO_LETTO = @VER_DATA_ULTMO_LETTO
			WHERE  VER_IDENTIFIER = @VER_IDENTIFIER;

			SELECT
				@OUTPUT_COUNT = ISNULL(TRIGGER_COUNT, 0)
			FROM TCPOS_VERIFY
			WHERE VER_IDENTIFIER = @VER_IDENTIFIER
			""";

		await using var connction = new SqlConnection(_connectionString);
		await using var command  = new SqlCommand(sql, connction);
		
		// IN
		command.Parameters.AddWithValue("@VER_STATO_LETTO", statoLetto);
		command.Parameters.AddWithValue("@VER_DATA_ULTMO_LETTO", ultimoTentativoDiLettura);
		command.Parameters.AddWithValue("@VER_IDENTIFIER", uniqueIdentifier);

		// OUT
		SqlParameter OUTPUT_COUNT;
		command.Parameters.Add(OUTPUT_COUNT = new SqlParameter("@OUTPUT_COUNT", SqlDbType.Int) { Direction = ParameterDirection.Output });
		
		await connction.OpenAsync();
		await command.ExecuteNonQueryAsync();

		return Convert.ToInt32(OUTPUT_COUNT.Value);
	}
	
}
