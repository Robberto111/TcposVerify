
using TcposVerify.Data;
using TcposVerify.Models;

namespace TcposVerify.Services;

public class VerifyService : IVerifyService
{
	private const int ScadenzaSecondi = 4 * 3600;

	private readonly IVerifyRepository m_Repository;
	private readonly ChecksumService m_Checksum;
	private readonly MondrianService m_Mondrian;
	private readonly ILogger<VerifyService> _logger;


	//////////////////////////////////////////////////////////////
	public VerifyService(
		IVerifyRepository repository,
		ChecksumService checksum,
		MondrianService mondrian,
		ILogger<VerifyService> logger)
	{
		m_Repository = repository;
		m_Checksum   = checksum;
		m_Mondrian   = mondrian;
		_logger      = logger;
	}


	//////////////////////////////////////////////////////////////
	public async Task<VerifyViewModel> VerifyAsync(VerifyRequest req, string uniqueIdentifier, string queryString)
	{
		var emissioneScontrino = new DateTime(
			year: int.Parse(req.D.Substring(4, 4)),
			month: int.Parse(req.D.Substring(2, 2)),
			day: int.Parse(req.D.Substring(0, 2)),
			hour: int.Parse(req.T.Substring(0, 2)),
			minute: int.Parse(req.T.Substring(2, 2)),
			second: int.Parse(req.T.Substring(4, 2)),
			DateTimeKind.Local)
		;

		bool esistevaGia = !(await m_Repository.EnsureRecordAsync(emissioneScontrino, DateTime.Now, req.Neg, req.Ti, req.Tr, req.Tot, uniqueIdentifier));

		// 3. Verifica checksum
		string ckCalcolato = m_Checksum.CalcolaChecksum(req.Neg, req.Ti, req.Tr, req.D, req.T, req.Tot);

		if (req.Ck != ckCalcolato)
		{
			_logger.LogWarning("CK_ERRATO p={Path}", queryString);

			await m_Repository.UpdateRecordAsync(DateTime.Now, uniqueIdentifier, LETTO_STATO.INVALIDO);

			return VerifyViewModel.Falso(MondrianAvvelenato(req));
		}

		// 4. Honey-trap h
		string hCalcolato = m_Checksum.CalcolaH(req.Neg, req.Ti, req.Tr, req.D);
		bool   hValida    = req.H == hCalcolato;

		if (!hValida)
			_logger.LogWarning("HONEY_TRAP p={Path}", queryString);

		string[] mondrian = hValida
			? m_Mondrian.Genera(m_Checksum.GetKSeed(req.D))
			: MondrianAvvelenato(req);

		// 5. Scadenza 4 ore
		bool scaduto = Utilities.GenericChekers.IsScaduto(emissioneScontrino, ScadenzaSecondi);

		// Dati display
		string totEuro   = Utilities.GenericChekers.FormatTotale(req.Tot);
		string dataOra   = Utilities.GenericChekers.FormatDataOra(req.D, req.T);

		// 6. Controllo unicità
		if (esistevaGia)
		{
			_logger.LogInformation("GIA_USATO p={Path}", queryString);

			await m_Repository.UpdateRecordAsync(DateTime.Now, uniqueIdentifier);

			return VerifyViewModel.GiaUsato(req.Neg, req.Ti, req.Tr, totEuro, dataOra, mondrian);
		}

		if (scaduto)
		{
			_logger.LogInformation("SCADUTO p={Path}", queryString);

			await m_Repository.UpdateRecordAsync(DateTime.Now, uniqueIdentifier, LETTO_STATO.SCADUTO);

			return VerifyViewModel.Scaduto(req.Neg, req.Ti, req.Tr, totEuro, dataOra, mondrian);
		}

		return VerifyViewModel.Valido(req.Neg, req.Ti, req.Tr, totEuro, dataOra, mondrian);
	}


	// ── Helper ───────────────────────────────────────────────────────────────

	//////////////////////////////////////////////////////////////
	private string[] MondrianAvvelenato(VerifyRequest req)
	{
		long seed = Math.Abs((long)(req.H + req.Tr + req.D).GetHashCode() ^ Random.Shared.Next());
		return m_Mondrian.Genera(seed);
	}
}