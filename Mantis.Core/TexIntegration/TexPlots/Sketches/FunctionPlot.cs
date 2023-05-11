using System.Text;
using Mantis.Core.Calculator;

namespace Mantis.Core.TexIntegration;

public record FunctionPlot : AddPlotSketch
{
    public virtual string FunctionString { get; set; }

    public override void AppendInsideSquareBrackets(StringBuilder builder,Sketchbook sketchbook)
    {
        base.AppendInsideSquareBrackets(builder,sketchbook);
        if (sketchbook.Domain.HasValue)
            builder.AppendLine($"domain={sketchbook.Domain.Value.Min.X}:{sketchbook.Domain.Value.Max.X},");
    }

    public override void AppendAfterBeforeSemi(StringBuilder builder,Sketchbook sketchbook)
    {
        base.AppendAfterBeforeSemi(builder,sketchbook);
        
        builder.AppendLine($"{{{FunctionString}}}");
    }

    public override Rect2? GetDomain()
    {
        return null;
    }
}