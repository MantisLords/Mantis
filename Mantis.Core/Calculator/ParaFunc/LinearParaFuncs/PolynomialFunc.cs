using MathNet.Numerics.LinearAlgebra;

namespace Mantis.Core.Calculator;

public class PolynomialFunc : LinearFuncCore
{
    public override double CalculateXDerivative(Vector<double> parameters, double x)
    {
        double res = 0;
        for (int i = 1; i < parameters.Count; i++)
        {
            res += Math.Pow(x, i - 1) * i;
        }

        return res;
    }

    protected override double CalculateGradient(double x, int n,double prevValue)
    {
        return Math.Pow(x, n);
    }
}