using System.Globalization;
using System.Numerics;
using System.Text;
using Mantis.Core.Utility;

namespace Mantis.Core.TexIntegration;

public static class TexPreamble
{
    public static TexWriter PreambleWriter = new TexWriter();

    public static List<string> Packages = new List<string>()
    {
        "amsmath",
        "tikz",
        "pgfplots"
    };

    public static void AddCommand(this string content,string label)
    {
        PreambleWriter.Builder.AppendCommand($"newcommand{{\\{label}}}{{{content}}}");
    }

    public static void AddCommand<T>(this INumber<T> number, string label, string unit = "") where T : INumber<T>
    {
        string res = number.ToString("G4",CultureInfo.CurrentCulture);
        if (!string.IsNullOrEmpty(unit))
            res += "\\ " + unit;
        AddCommand(res,label);
    }

    public static void GeneratePreamble()
    {
        StringBuilder packageBuilder = new StringBuilder();
        packageBuilder.AppendLine();

        foreach (string package in Packages)
        {
            packageBuilder.AppendCommand($"usepackage{{{package}}}");
        }
        packageBuilder.AppendLine();

        PreambleWriter.Builder.Insert(0, packageBuilder.ToString());
        
        PreambleWriter.Save("Preamble");
    }
}