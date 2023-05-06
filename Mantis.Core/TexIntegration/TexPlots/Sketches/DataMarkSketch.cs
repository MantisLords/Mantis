using System.Globalization;
using System.Text;

namespace Mantis.Core.TexIntegration;

public record DataMarkSketch : AddPlotSketch
{
    public IEnumerable<(ErDouble, ErDouble)> Data;
    
    public override void AppendInsideSquareBrackets(StringBuilder builder)
    {
        base.AppendInsideSquareBrackets(builder);
        builder.AppendLine("only marks");
    }

    public override void AppendAfterBeforeSemi(StringBuilder builder)
    {
        builder.AppendLine("coordinates{");
        foreach ((ErDouble x,ErDouble y) in Data)
        {
            builder.AppendLine($"({x.Value},{y.Value}) +- ({x.Error},{y.Error})");
        }

        builder.Append("}");
    }
}