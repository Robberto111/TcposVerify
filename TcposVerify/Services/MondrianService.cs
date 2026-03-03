namespace TcposVerify.Services;

/// <summary>
/// Port 1:1 di generaMondrian() PHP — LCG parametri glibc.
/// </summary>
public class MondrianService
{
    private static readonly string[] Palette =
    [
        "#E63946", "#2A9D8F", "#264653", "#E9C46A", "#F4A261",
        "#A8DADC", "#457B9D", "#1D3557", "#F1FAEE", "#E76F51",
    ];

    public string[] Genera(long seed)
    {
        long s      = seed & 0x7FFFFFFF;
        var  colors = new string[4];
        for (int i = 0; i < 4; i++)
        {
            s        = (s * 1_664_525L + 1_013_904_223L) & 0x7FFFFFFF;
            colors[i] = Palette[s % Palette.Length];
        }
        return colors;
    }
}
