
using Microsoft.Data.SqlClient;

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

        await using var conn = new SqlConnection(_connectionString);
        await using var insertCmd = new SqlCommand(sql, conn);
        {
            insertCmd.Parameters.AddWithValue("@VER_STATO_LETTO", (int)LETTO_STATO.VALIDO);
            insertCmd.Parameters.AddWithValue("@VER_EMESSO", emissioneScontrino);
            insertCmd.Parameters.AddWithValue("@VER_NEGOZIO", negozio);
            insertCmd.Parameters.AddWithValue("@VER_CASSA", cassa);
            insertCmd.Parameters.AddWithValue("@VER_TRANSAZIONE", nTransazione);
            insertCmd.Parameters.AddWithValue("@VER_TOTALE", int.TryParse(totaleEuro, out int totaleInt) ? totaleInt : -1);
            insertCmd.Parameters.AddWithValue("@VER_DATA_LETTO", scansionatoIl);
            insertCmd.Parameters.AddWithValue("@VER_DATA_ULTMO_LETTO", scansionatoIl);
            insertCmd.Parameters.AddWithValue("@VER_IDENTIFIER", uniqueIdentifier);
        }
        await conn.OpenAsync();
        return (int)(await insertCmd.ExecuteScalarAsync() ?? -1) == 1;
    }


    //////////////////////////////////////////////////////////////
    public async Task UpdateRecordAsync(DateTime ultimoTentativoDiLettura, string uniqueIdentifier, LETTO_STATO? stato = null)
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
			WHERE  VER_IDENTIFIER = @VER_IDENTIFIER
			""";

        await using var conn = new SqlConnection(_connectionString);
		await using var cmd  = new SqlCommand(sql, conn);
		{
			cmd.Parameters.AddWithValue("@VER_STATO_LETTO", statoLetto);
			cmd.Parameters.AddWithValue("@VER_DATA_ULTMO_LETTO", ultimoTentativoDiLettura);
			cmd.Parameters.AddWithValue("@VER_IDENTIFIER", uniqueIdentifier);
		}
		await conn.OpenAsync();
		await cmd.ExecuteNonQueryAsync();
	}
    
}
