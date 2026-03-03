
using System.Text.RegularExpressions;

namespace TcposVerify.Utilities
{
	public static partial class ValidateInputs
	{
        //////////////////////////////////////////////////////////////
        public static bool AreValid(
			string Neg,
			string Ti,
			string Tr,
			string D,
			string T,
			string Tot,
			string Ck,
			string H,
			out string err)
		{
			if (string.IsNullOrWhiteSpace(Neg) || string.IsNullOrWhiteSpace(Ti) ||
				string.IsNullOrWhiteSpace(Tr) || string.IsNullOrWhiteSpace(D) ||
				string.IsNullOrWhiteSpace(T) || string.IsNullOrWhiteSpace(Tot) ||
				string.IsNullOrWhiteSpace(Ck) || string.IsNullOrWhiteSpace(H))
			{
				err = "Parametri mancanti.";
				return false;
			}

			Neg = Neg.ToUpperInvariant().Trim();
			Ti = Ti.ToUpperInvariant().Trim();

			if (!RegexNeg().IsMatch(Neg) || !RegexTi().IsMatch(Ti) ||
				!RegexTr().IsMatch(Tr) || !RegexD().IsMatch(D) ||
				!RegexT().IsMatch(T) || !RegexTot().IsMatch(Tot) ||
				!RegexCk().IsMatch(Ck) || !RegexH().IsMatch(H))
				{
					err = "Parametri in formato non valido.";
					return false;
			}

			err = string.Empty;
			return true;
		}


		// ── Regex validazione (identici al PHP) ──────────────────────────────────
		[GeneratedRegex(@"^[A-Z]{2}[0-9]{4,6}$")] private static partial Regex RegexNeg();
		[GeneratedRegex(@"^[A-Z]{2}[0-9]{4,8}$")] private static partial Regex RegexTi();
		[GeneratedRegex(@"^[0-9]{1,9}$")] private static partial Regex RegexTr();
		[GeneratedRegex(@"^[0-9]{8}$")] private static partial Regex RegexD();
		[GeneratedRegex(@"^[0-9]{6}$")] private static partial Regex RegexT();
		[GeneratedRegex(@"^[0-9]{1,7}$")] private static partial Regex RegexTot();
		[GeneratedRegex(@"^[0-9]{10}$")] private static partial Regex RegexCk();
		[GeneratedRegex(@"^[0-9]{5}$")] private static partial Regex RegexH();
	}
}
