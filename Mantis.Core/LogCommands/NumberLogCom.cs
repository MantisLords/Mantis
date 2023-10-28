using System.Globalization;
using System.Numerics;

namespace Mantis.Core.LogCommands;

public record NumberLogCom<T>(string Label, INumber<T> Number, string Unit = "") : ILogCommand where T : INumber<T>
{
    public (string, string) GetLabeledContent(bool isLatex)
    {
        string res = Number.ToString(isLatex?"G4":"g4",CultureInfo.InvariantCulture);
        if (!string.IsNullOrEmpty(Unit))
        {
            if (isLatex) res += $"\\, \\mathrm{{{Unit}}}";
            else res += " " + Unit;
        }

        return (Label, res);
    }
}

public static class NumberLogComExtension
{
    public static void Add<T>(this List<ILogCommand> commands, string label, INumber<T> number, string unit = "")
        where T : INumber<T>
    {
        commands.Add(new NumberLogCom<T>(label,number,unit));
    }
}