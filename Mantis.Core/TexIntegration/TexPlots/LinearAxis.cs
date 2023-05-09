using System.Text;
using Mantis.Core.Utility;

namespace Mantis.Core.TexIntegration;

public record LinearAxis : ITexWritableEnclosed
{
    public string XLabel;
    public string YLabel;
    public string? LegendPos;
    
    public void AppendBegin(StringBuilder builder)
    {
        builder.AppendBegin("axis");
        builder.Append("[ ");
        builder.AppendLine($"xLabel = {XLabel},");
        builder.AppendLine($"yLabel = {YLabel},");
        builder.AppendLine($"legend pos = {LegendPos ?? "north west"}");
        builder.AppendLine("]");

    }

    public void AppendEnd(StringBuilder builder)
    {
        builder.AppendEnd("axis");
    }
}