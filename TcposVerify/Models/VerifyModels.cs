using System.Data;

namespace TcposVerify.Models;

/// <summary>Parametri GET del QR code.</summary>
public class VerifyRequest
{
	public string Neg { get; set; } = string.Empty;
	public string Ti  { get; set; } = string.Empty;
	public string Tr  { get; set; } = string.Empty;
	public string D   { get; set; } = string.Empty;
	public string T   { get; set; } = string.Empty;
	public string Tot { get; set; } = string.Empty;
	public string Ck  { get; set; } = string.Empty;
	public string H   { get; set; } = string.Empty;
}

/// <summary>Risposta JSON del controller.</summary>
public class VerifyResponse
{
	/// <summary>VALIDO | FALSO | SCADUTO | GIA_USATO | ERRORE</summary>
	public string   Stato        { get; set; } = string.Empty;
	public string   Messaggio    { get; set; } = string.Empty;
	public bool     Valido       { get; set; }
	public string?  Negozio      { get; set; }
	public string?  Cassa        { get; set; }
	public string?  NTransazione { get; set; }
	public string?  TotaleEuro   { get; set; }
	public string?  DataOra      { get; set; }
	public string[] Mondrian     { get; set; } = [];
}


/// <summary>ViewModel passato alla View Razor.</summary>
public class VerifyViewModel
{
	/// <summary>VALIDO | FALSO | SCADUTO | GIA_USATO | ERRORE</summary>
	public string Stato { get; set; } = string.Empty;

	/// <summary>Testo mostrato in grande (es. ✓ VALIDO)</summary>
	public string StatusText { get; set; } = string.Empty;

	/// <summary>Colore sfondo della card (#27ae60, #c0392b, ecc.)</summary>
	public string BgColor { get; set; } = "#555555";

	public string? Negozio { get; set; }
	public string? Cassa { get; set; }
	public string? NTransazione { get; set; }
	public string? TotaleEuro { get; set; }
	public string? DataOra { get; set; }

	/// <summary>true se i dati scontrino vanno mostrati</summary>
	public bool MostraDati => Negozio != null;

	/// <summary>4 colori hex del Sigillo Mondrian</summary>
	public string[] Mondrian { get; set; } = [];
	public DateTime VerificaIl { get; set; } = DateTime.Now;

	public static VerifyViewModel Falso(string[] mondrian) => new()
	{
		Stato = "FALSO",
		StatusText = "✗ NON VALIDO / FALSO",
		BgColor = "#c0392b",
		Mondrian = mondrian
	};

	public static VerifyViewModel Scaduto(string neg, string ti, string tr, string totEuro, string dataOra, string[] mondrian) => new()
	{
		Stato = "SCADUTO",
		StatusText = "⚠ SCADUTO (>4h)",
		BgColor = "#e67e22",
		Negozio =neg,
		Cassa = ti,
		NTransazione = tr,
		TotaleEuro = totEuro,
		DataOra = dataOra,
		Mondrian = mondrian,
	};

	public static VerifyViewModel GiaUsato(string neg, string ti, string tr, string totEuro, string dataOra, string[] mondrian) => new()
	{
		Stato = "GIA_USATO",
		StatusText = "✗ GIÀ UTILIZZATO",
		BgColor = "#c0392b",
		Negozio = neg,
		Cassa = ti,
		NTransazione = tr,
		TotaleEuro = totEuro,
		DataOra = dataOra,
		Mondrian = mondrian,
	};

    public static VerifyViewModel Valido(string neg, string ti, string tr, string totEuro, string dataOra, string[] mondrian) => new()
	{
        Stato = "VALIDO",
        StatusText = "✓ VALIDO",
        BgColor = "#27ae60",
        Negozio = neg,
        Cassa = ti,
        NTransazione = tr,
        TotaleEuro = totEuro,
        DataOra = dataOra,
        Mondrian = mondrian,
    };
}
