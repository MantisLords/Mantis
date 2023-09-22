using MathNet.Numerics.LinearAlgebra;

namespace Mantis.Core.Calculator;

/// <summary>
/// Linear function f(x) = A + B x
/// </summary>
public class LineFunc : LinearFuncCore,IFixedParameterCount
{
    public int ParameterCount => 2;

    protected override double CalculateGradient(double x, int n,double prevValue)
    {
        return Math.Pow(x, n);
    }

    public override double CalculateXDerivative(Vector<double> parameters, double x)
    {
        return parameters[1];
    }
}