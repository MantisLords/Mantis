using System.Text;

namespace Mantis.Core.TexIntegration;

public record StraightPlot : AddPlotSketch
{
    public double Slope;
    public double YZero;
    
    public override void AppendAfterBeforeSemi(StringBuilder builder)
    {
        base.AppendAfterBeforeSemi(builder);

        builder.AppendLine($"{{x*{Slope} + {YZero}}}");
    }
}