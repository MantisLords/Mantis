using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex32;

namespace Mantis.Core.Calculator;

public static class GeneralLinearRegression
{
    /// <summary>
    /// Most generic one dimensional linear regression
    /// </summary>
    /// <param name="y">y-values</param>
    /// <param name="EY">Error matrix of y-values</param>
    /// <param name="X">X Function matrix</param>
    /// <returns>Returns (a,Ea). With a being the regression parameters and Ea being the corresponding error matrix</returns>
    public static (Vector<double>,Matrix<double>) LinearRegression(Vector<double> y,Matrix<double> EY,Matrix<double> X)
    {
        bool allErrorZero = true;
        bool oneErrorZero = false;
        for (int i = 0; i < EY.ColumnCount; i++)
        {
            allErrorZero &= EY[i, i] == 0;
            oneErrorZero |= EY[i, i] == 0;
        }

        if (!allErrorZero && oneErrorZero)
        {
            throw new ArgumentException("There is a y value with an error of zero. Try giving it an error or " +
                                        "do the regression with completely no errors");
        }
        
        Matrix<double> W;
        if (allErrorZero)
        {
            W = Matrix<double>.Build.DiagonalIdentity(y.Count);
        }
        else
        {
            W = EY.Inverse();
        }

        var XT = X.Transpose();
        
        // XT * W * y = XT * W * X * a
        // Solve for a

        //Fast Method
        //var a = (XT * W * X).Cholesky().Solve(XT * W * y);
        
        // Using the inverse (slow but you get the standard error analytically)
        // a = ((XT * W * X)^-1 * XT * W) * y
        // a = HG * y
        var inverse = (XT * W * X).Inverse();
        var HG = inverse * XT * W;
        var a = HG * y;

        if (allErrorZero)
        {
            var modelValues = X * a;
            var sigma = GoodnessOfFit.StandardError( modelValues, y,a.Count);
            EY = Matrix<double>.Build.Diagonal(y.Count, y.Count, sigma * sigma);
        }
        
        var EA = HG * EY * HG.Transpose();

        return (a, EA);
    }
    
    /// <summary>
    /// Generic one dimensional linear regression
    /// </summary>
    /// <param name="ys">y-Values with error</param>
    /// <param name="X">x-Function Matrix</param>
    /// <returns>Regression parameters</returns>
    public static ErDouble[] LinearRegression(ErDouble[] ys, Matrix<double> X)
    {
        var (y, Ey) = ErDoubleArrayToErrorMatrix(ys);
        var (a, Ea) = LinearRegression(y, Ey, X);
        return ErrorMatrixToErDouble(a, Ea);
    }

    /// <summary>
    /// Generic one dimensional linear regression. Let a = {a1,a2,a3,...,an} be your linear coefficients.
    /// Then is your model a function with creates for every xi a vector f(xi) = {f1(xi), f2(xi), f3(xi), ... fn(xi)}
    /// so then you can compute the y-value like y(xi) = < f(xi) | a >
    /// </summary>
    /// <param name="ys">array of y values</param>
    /// <param name="xs">array of x-values xs = {xi}</param>
    /// <param name="linearModel">linear model f(xi) = {f1(xi), f2(xi), f3(xi), ... fn(xi)}</param>
    /// <returns>Regression parameters a = {a1,a2,a3,...,an}</returns>
    public static ErDouble[] LinearRegression( double[] xs, Func<double, double[]> linearModel,ErDouble[] ys)
    {
        Matrix<double> X = Matrix<double>.Build.DenseOfRows(xs.Select(linearModel));
        return LinearRegression(ys, X);
    }

    /// <summary>
    /// /// Generic one dimensional linear regression. Let a = {a1,a2,a3,...,an} be your linear coefficients.
    /// Then is your model a function with creates for every xi a vector f(xi) = {f1(xi), f2(xi), f3(xi), ... fn(xi)}
    /// so then you can compute the y-value like y(xi) = < f(xi) | a >
    /// Selector syntax for abbreviated usage.
    /// </summary>
    /// <param name="data">Your list of data</param>
    /// <param name="selector">Function witch converts a data element into a (xi,yi) pair</param>
    /// <param name="linearModel">linear model f(xi) = {f1(xi), f2(xi), f3(xi), ... fn(xi)}</param>
    /// <typeparam name="T">Regression parameters a = {a1,a2,a3,...,an}</typeparam>
    /// <returns>Regression parameters a = {a1,a2,a3,...,an</returns>
    public static ErDouble[] LinearRegression<T>(this IEnumerable<T> data, Func<T,(double, ErDouble)> selector,Func<double, double[]> linearModel)
    {
        double[] xs = data.Select(e => selector(e).Item1).ToArray();
        ErDouble[] ys = data.Select(e => selector(e).Item2).ToArray();
        return LinearRegression(xs, linearModel, ys);
    }
    
    

    /// <summary>
    /// Converts a Vector, Error-Matrix Pair to an ErDouble Array
    /// </summary>
    /// <param name="v"></param>
    /// <param name="Ev"></param>
    /// <returns></returns>
    public static ErDouble[] ErrorMatrixToErDouble(Vector<double> v, Matrix<double> Ev)
    {
        ErDouble[] parameters = new ErDouble[v.Count];
        for (int i = 0; i < v.Count; i++)
        {
            parameters[i] = v[i];
            parameters[i].Error = Math.Sqrt(Ev[i, i]);
        }

        return parameters;
    }
    
    /// <summary>
    /// Converts an Array of ErDouble to a Vector, Error-Matrix Pair
    /// </summary>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public static (Vector<double>, Matrix<double>) ErDoubleArrayToErrorMatrix(ErDouble[] parameters)
    {
        Vector<double> v = Vector<double>.Build.DenseOfEnumerable(parameters.Select(e => e.Value));

        double[] errors = new double[parameters.Length];
        for (int i = 0; i < errors.Length; i++)
        {
            errors[i] = parameters[i].Error * parameters[i].Error;
        }

        Matrix<double> Ev = Matrix<double>.Build.Diagonal(errors);
        return (v, Ev);
    }
}