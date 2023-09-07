using MathNet.Numerics.LinearAlgebra;

namespace Mantis.Core.Calculator;

public abstract class FuncCore
{
    #region Calculate Result

    public Vector<double> CalculateResultPointWise(Vector<double> parameters, Vector<double> x)
    {
        var res = Vector<double>.Build.Dense(x.Count);
        for (int i = 0; i < res.Count; i++)
        {
            res[i] = CalculateResult(parameters, x[i]);
        }

        return res;
    }

    public abstract double CalculateResult(Vector<double> parameters, double x);

    #endregion

    #region Calculate Gradient

    public Matrix<double> CalculateGradientPointWise(Vector<double> parameters, Vector<double> x)
    {

        var gradient = Matrix<double>.Build.Dense(x.Count, parameters.Count);

        for (int row = 0; row < x.Count; row++)
        {
            double value = 0;
            for (int column = 0; column < parameters.Count; column++)
            {
                value = CalculateGradient(parameters, x[row], column, value);
                gradient[row, column] = value;
            }
        }

        return gradient;
    }

    public Vector<double> CalculateGradientVector(Vector<double> parameters, double x)
    {
        Vector<double> gradient = Vector<double>.Build.Dense(parameters.Count);
        double value = 0;
        for (int i = 0; i < parameters.Count; i++)
        {
            value = CalculateGradient(parameters, x, i, value);
            gradient[i] = value;
        }

        return gradient;
    }

    public abstract double CalculateGradient(Vector<double> parameters, double x, int index, double previousValue);

    #endregion

    #region Calculate X Derivative

    public Vector<double> CalculateXDerivativePointWise(Vector<double> parameters, Vector<double> x)
    {
        var res = Vector<double>.Build.Dense(x.Count);
        for (int i = 0; i < res.Count; i++)
        {
            res[i] = CalculateXDerivative(parameters, x[i]);
        }
        return res;
    }

    public abstract double CalculateXDerivative(Vector<double> parameters, double x);

    #endregion
}