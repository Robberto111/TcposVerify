namespace TcposVerify.Services;

/// <summary>
/// Port 1:1 di calcolaChecksum() e calcolaH() dal PHP originale.
/// </summary>
public class ChecksumService
{
    private const long MOD1 = 2_147_483_647L;
    private const long MOD2 = 2_147_483_587L;
    private const long MOD3 = 2_147_483_549L;

    private static long SafeMulMod(long a, long b, long mod)
    {
        long bHi   = b / 1000;
        long bLo   = b % 1000;
        long resHi = (a * bHi) % mod;
        return ((resHi * 1000) + (a * bLo)) % mod;
    }

    private static (long k, long s1, long s2, long s3) DeriveSecrets(string d)
    {
        long dateVal = long.Parse(d);
        long k  = (long)((double)dateVal * 7919 % MOD1);
        long s1 = (982_451_653L + dateVal + k) % MOD1;
        long s2 = (15_485_863L  + (dateVal * 7)  + k) % MOD2;
        long s3 = (32_452_843L  + (dateVal * 13) + k) % MOD3;
        return (k, s1, s2, s3);
    }

    private static long NumericOnly(string code)
        => long.Parse(string.Concat(code.Where(char.IsDigit)));

    public string CalcolaChecksum(string neg, string ti, string tr, string d, string t, string tot)
    {
        int giorno = int.Parse(d[..2]);
        var (_, s1, s2, s3) = DeriveSecrets(d);

        long negNum    = NumericOnly(neg);
        long tiNum     = NumericOnly(ti);
        long trNum     = long.Parse(tr);
        long totNum    = long.Parse(tot);
        long modFinale = 900_000_000L + ((long)giorno * 1_000_000L);
        int  branch    = ((giorno * 17) + 3) % 2;

        long res;
        if (branch != 0)
        {
            long step1 = ((negNum * s1) + (tiNum * s2)) % MOD1;
            long tmp2  = (step1 + trNum) % MOD2;
            long step2 = SafeMulMod(tmp2, s3, MOD2);
            res        = ((step2 * 31) + (totNum * 17)) % modFinale;
        }
        else
        {
            long step1 = ((totNum * s1) + (trNum * s2)) % MOD1;
            long tmp2  = (step1 + tiNum) % MOD2;
            long step2 = SafeMulMod(tmp2, s3, MOD2);
            res        = ((step2 * 31) + (negNum * 17)) % modFinale;
        }

        string totClean  = string.Concat(tot.Where(char.IsDigit));
        int    cifraCtrl = totClean.Sum(c => c - '0') % 10;
        return res.ToString("D9") + cifraCtrl;
    }

    public string CalcolaH(string neg, string ti, string tr, string d)
    {
        var (_, s1, s2, _) = DeriveSecrets(d);

        long negNum = NumericOnly(neg);
        long tiNum  = NumericOnly(ti);
        long trNum  = long.Parse(tr);

        long hStep1 = SafeMulMod(trNum, s2, MOD2);
        long hTmp   = (hStep1 + negNum + tiNum) % MOD3;
        long hVal   = SafeMulMod(hTmp, s1, MOD1);
        return (hVal % 100_000L).ToString("D5");
    }

    public long GetKSeed(string d)
    {
        long dateVal = long.Parse(d);
        return (long)((double)dateVal * 7919 % MOD1);
    }
}
