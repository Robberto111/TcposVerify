namespace TcposVerify.Data;

/// <summary>
/// Contratto comune tra VerifyRepository (SQL Server) e InMemoryVerifyRepository (Development).
/// Il controller dipende solo da questa interfaccia.
/// </summary>
public interface IVerifyRepository
{
    //////////////////////////////////////////////////////////////
    /// <summary>
    /// Ritorna false se il record è stato creato (non esisteva già), true se esisteva già.
    /// </summary>
    Task<bool> EnsureRecordAsync(
		DateTime emissioneScontrino,
		DateTime scansionatoIl,
		string negozio,
		string cassa,
		string nTransazione,
		string totaleEuro,
		string queryString
	);

	//////////////////////////////////////////////////////////////
	Task<bool> ExistsAsync(string queryString);

	//////////////////////////////////////////////////////////////
	Task<int/*count*/> UpdateRecordAsync(DateTime ultimoTentativoDiLettura, string queryString, LETTO_STATO? stato = null);
}
