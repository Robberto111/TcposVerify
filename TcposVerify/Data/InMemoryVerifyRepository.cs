
namespace TcposVerify.Data;

/// <summary>
/// Implementazione in-memory del repository, usata in Development.
/// Nessuna connessione a SQL Server richiesta.
/// I dati si azzerano al riavvio del processo.
/// </summary>
public class InMemoryVerifyRepository : IVerifyRepository
{
	// HashSet thread-safe tramite lock — chiave: "neg|ti|tr|d"
	private readonly HashSet<string> _records = [];
	private readonly object            _lock    = new();

	//////////////////////////////////////////////////////////////
	public Task<bool/*Creato*/> EnsureRecordAsync(
		DateTime emissioneScontrino,
		DateTime scansionatoIl,
		string negozio,
		string cassa,
		string nTransazione,
		string TotaleEuro,
		string uniqueIdentifier
		)
	{
		lock (_lock)
		{
			return Task.FromResult(_records.Add(uniqueIdentifier));
		}
	}

	//////////////////////////////////////////////////////////////
	public Task<bool> ExistsAsync(string uniqueIdentifier)
	{
		lock (_lock)
		{
			bool exixsts = _records.Any(i => i.StartsWith(uniqueIdentifier));

			return Task.FromResult(exixsts);
		}
	}

    //////////////////////////////////////////////////////////////
    public async Task UpdateRecordAsync(DateTime ultimoTentativoDiLettura, string uniqueIdentifier, LETTO_STATO? stato = null)
    {
		string key = uniqueIdentifier + ultimoTentativoDiLettura.ToString("o"); // "o" = formato ISO 8601, es: "2024-06-30T12:34:56.789Z"
		lock (_lock)
		{
			// Rimuove eventuali record precedenti con la stessa query (ignora timestamp precedente)
			_records.RemoveWhere(i => i.StartsWith(uniqueIdentifier));
			// Aggiunge nuovo record con timestamp aggiornato
			_records.Add(key);
		}
	}
}
