using System.Text;
using Mantis.Core.Calculator;

namespace Mantis.Core.TexIntegration;

public record StraightPlot : FunctionPlot
{
    public double Slope;
    public double YZero;

    public override string FunctionString => $"x*{Slope} + {YZero}";

}