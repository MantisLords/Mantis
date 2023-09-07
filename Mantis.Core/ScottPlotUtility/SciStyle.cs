using ScottPlot.Styles;
using System.Drawing;

namespace Mantis.Core.ScottPlotUtility;

public class SciStyle : Default
{
    private string TexFontName = "CMU Serif";

    private string? actualFont = null;

    private string GetTexIfExits => actualFont ??= CheckIfFontExists();

    public override string TitleFontName => GetTexIfExits;
    public override string AxisLabelFontName => GetTexIfExits;
    public override string TickLabelFontName => GetTexIfExits;

    private string CheckIfFontExists()
    {
        string font = ScottPlot.Drawing.InstalledFont.ValidFontName(TexFontName);

        if (font != TexFontName)
        {
            Console.WriteLine($"You have not installed the font '{TexFontName}' on your system. Using default instead");
        }

        return font;
    }
}