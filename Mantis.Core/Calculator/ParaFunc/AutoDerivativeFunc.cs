using MathNet.Numerics.LinearAlgebra;

namespace Mantis.Core.Calculator;

public abstract class AutoDerivativeFunc : FuncCore
{

    public int AccuracyOrder = 2;

    public override double CalculateGradient(Vector<double> parameters, double x, int index, double previousValue)
    {
        return SimpleNumericalDerivative.NumericalGradientIndex(p => CalculateResult(p, x), parameters, index,
            AccuracyOrder);
    }

    public override double CalculateXDerivative(Vector<double> parameters, double x)
    {
        return SimpleNumericalDerivative.NumericalDerivative(value => CalculateResult(parameters, value), x,
            AccuracyOrder);
    }
}