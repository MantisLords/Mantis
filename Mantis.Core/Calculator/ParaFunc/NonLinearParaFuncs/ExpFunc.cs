using MathNet.Numerics.LinearAlgebra;

namespace Mantis.Core.Calculator;



public class ExpFunc : FuncCore,IFixedParameterCount
{
    public override double CalculateGradient(Vector<double> parameters, double x, int index, double prevValue)
    {
        switch (index)
        {
            case 0: return Math.Exp(x * parameters[1]);
            case 1: return x * parameters[0] * Math.Exp(x * parameters[1]);
            default: throw new ArgumentOutOfRangeException($"Index: {index} is not valid");
        }
    }

    public override double CalculateResult(Vector<double> parameters, double x)
    {
        return parameters[0] * Math.Exp(x * parameters[1]);
    }

    public override double CalculateXDerivative(Vector<double> parameters, double x)
    {
        return parameters[0] * parameters[1] * Math.Exp(parameters[1] * x);
    }

    public int ParameterCount => 2;
}