
using System.Globalization;

namespace TcposVerify.Utilities
{
	public static class GenericChekers
	{
		private static readonly CultureInfo culture = new CultureInfo("it-IT");


        //////////////////////////////////////////////////////////////
        public static bool IsScaduto(DateTime emissioneScontrino, int scadenzaSecondi)
		{
			try
			{
                double diff = (DateTime.Now - emissioneScontrino).TotalSeconds;
				return
                    // Scaduto da più di scadenzaSecondi
                    diff > scadenzaSecondi
					// Significa che lo scontrino è nel futuro di più di 10 minuti rispetto all'ora del server.
					// È un controllo anti-manomissione: se qualcuno costruisce un QR con un'ora futura (es. tra 2 ore)
					// per allungare artificialmente la finestra di validità, viene rifiutato.
					// Il margine di 600 secondi (10 minuti) serve come tolleranza per piccoli disallineamenti
					// di orario tra il registratore di cassa e il server.
                    || diff < -600;
			}
			catch
			{
				return true;
			}
		}

        //////////////////////////////////////////////////////////////
        public static string FormatTotale(string tot)
		{
			if (!long.TryParse(tot, out long c)) return "---";
			return (c / 100m).ToString("N2", culture);
		}

        //////////////////////////////////////////////////////////////
        public static string FormatDataOra(string d, string t)
			=> $"{d[..2]}/{d[2..4]}/{d[4..]} alle {t[..2]}:{t[2..4]}:{t[4..]}";
	}
}
