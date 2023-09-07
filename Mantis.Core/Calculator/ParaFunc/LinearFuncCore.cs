using MathNet.Numerics.LinearAlgebra;

namespace Mantis.Core.Calculator;

public abstract class LinearFuncCore : FuncCore
{
    public override double CalculateGradient(Vector<double> parameters, double x, int index, double prevValue)
    {
        return CalculateGradient(x, index,prevValue);
    }

    public override double CalculateResult(Vector<double> parameters, double x)
    {
        double res = 0;
        double value = 0;
        for (int i = 0; i < parameters.Count; i++)
        {
            value = CalculateGradient(x, i, value);
            res += parameters[i] * value;
        }

        return res;
    }
    
    public Matrix<double> CalculateGradientPointWise(Vector<double> x,int parameterCount)
    {

        var gradient = Matrix<double>.Build.Dense(x.Count, parameterCount);

        for (int row = 0; row < x.Count; row++)
        {
            double value = 0;
            for (int column = 0; column < parameterCount; column++)
            {
                value = CalculateGradient(x[row], column, value);
                gradient[row, column] = value;
            }
        }

        return gradient;
    }

    protected abstract double CalculateGradient(double x, int n,double prevValue);
}