using MathNet.Numerics.LinearAlgebra;

namespace Mantis.Core.Calculator;

public class LegendrePolynomialFunc : LinearFuncCore
{
    public override double CalculateXDerivative(Vector<double> parameters, double x)
    {
        if (parameters.Count < 2)
            return 0;
        // p0' = 0
        // p1' = 1
        double res = parameters[1]; 
        double prevDerivative = 1;

        double prevValue = CalculateGradient(x, 1, 1);
        
        for (int i = 2; i < parameters.Count; i++)
        {
            // p(i)' = (i + 1) p(i-1) + x p(i-1)' Ref: Wikipedia Legendre Polynomial
            double derivative = (i + 1) * prevValue + x * prevDerivative;
            res += parameters[i] * derivative;

            prevDerivative = derivative;
            prevValue = CalculateGradient(x, i, prevValue);
        }

        return res;
    }

    private double prevprevValue = 0;

    protected override double CalculateGradient(double x, int n, double prevValue)
    {
        if (n == 0) return 1;
        if (n == 1)
        {
            prevprevValue = prevValue;
            return x;
        }
        
        double v = (((double)2 * n - 1) * x * prevValue -  prevprevValue *((double)n - 1)) / n;
        prevprevValue = prevValue;
        return v;
    }
}