using System.Collections;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearRegression;
using MathNet.Numerics.RootFinding;
using MathNet.Numerics.Statistics;

namespace Mantis.Core.Calculator;

public static class SimpleRegression
{
    /// <summary>
    /// fits a + b x and assumes data points are normal distributed and have no errors
    /// </summary>
    /// <returns>(a,b)</returns>
    public static (ErDouble, ErDouble) LinearRegressionNoErrors<T>(this IEnumerable<T> data,Func<T,(double,double)> selector)
    {
        IEnumerable<(double, double)> xyData = data.Select(selector);
        (double meanX, double meanY) = xyData.XYMean();

        double sigmaXY = 0,sigmaXX = 0,sumXX = 0;
        int count = 0;

        foreach ((double x, double y)  in xyData)
        {
            sigmaXY += (x - meanX) * (y - meanY);
            sigmaXX += (x - meanX) * (x - meanX);
            sumXX += x * x;
            count++;
        }

        ErDouble beta = sigmaXY / sigmaXX;
        ErDouble alpha = meanY - beta * meanX;

        double sigmaYfX=0;

        foreach ((double x,double y) in xyData)
        {
            double dif = y - alpha.Value - beta.Value * x;
            sigmaYfX += dif * dif;
        }

        double regressionErrorSq = sigmaYfX / (count - 2);

        alpha.Error = Math.Sqrt(regressionErrorSq * sumXX / count / sigmaXX);
        beta.Error = Math.Sqrt(regressionErrorSq / sigmaXX);

        return (alpha, beta);

    }
    
    /// <summary>
    /// fits a + b x and assumes data points are normal distributed and uses y error
    /// </summary>
    /// <returns>(a,b)</returns>
    public static (ErDouble, ErDouble) LinearRegression<T>(this IEnumerable<T> data,Func<T,(double,ErDouble)> selector)
    {
        var weightArray = data.Select(e => 1 / selector(e).Item2.Error /selector(e).Item2.Error).ToArray();
        var W = Matrix<double>.Build.Diagonal(weightArray);
        var y = Vector<double>.Build.DenseOfEnumerable(data.Select(e => selector(e).Item2.Value));
        var X = Matrix<double>.Build.DenseOfColumnArrays(data.Select(e => new double[]
        {
            1,
            selector(e).Item1
        })).Transpose();

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

        var YErrorSqrt = W.Inverse();
        var AErrorSqrt = HG * YErrorSqrt * HG.Transpose();

        ErDouble alpha = a[0];
        alpha.Error = Math.Sqrt(AErrorSqrt[0, 0]);

        ErDouble beta = a[1];
        beta.Error = Math.Sqrt(AErrorSqrt[1, 1]);

        return (alpha, beta);
    }

    public static (ErDouble, ErDouble) LinearRegressionPoissonDistributed<T>(this IEnumerable<T> data, Func<T, (double, ErDouble)> selector,(ErDouble,ErDouble)? initialGuessGauss = null)
    {
        var rootFindFunction = new RootFindFunctionPoisLinReg(data.Select(e => (selector(e).Item1,selector(e).Item2.Value)).ToArray());
        
        if(!initialGuessGauss.HasValue)
            initialGuessGauss = data.LinearRegression(selector);
        (ErDouble alphaInit, ErDouble betaInit) = initialGuessGauss.Value;

        double[] roots = Broyden.FindRoot(rootFindFunction.Evaluate, new double[] { alphaInit.Value, betaInit.Value });

        ErDouble alpha = roots[0];
        ErDouble beta = roots[1];

        alpha.Error = alphaInit.Error;
        beta.Error = betaInit.Error;
        
        return (alpha, beta);
    }

    private class RootFindFunctionPoisLinReg
    {
        private int n = 0;
        private double sumX = 0;
        private (double, double)[] xyData;

        public RootFindFunctionPoisLinReg((double,double)[] data)
        {
            xyData = data;
            n = xyData.Length;
            foreach ((double x, double y)  in xyData)
            {
                sumX += x;
            }
        }

        /// <summary>
        /// The function where the roots shall be determind
        /// </summary>
        /// <param name="parameters">parameters = {a,b}</param>
        /// <returns>{f1, f2} look at fr2</returns>
        public double[] Evaluate(double[] parameters)
        {
            double a = parameters[0], b = parameters[1];
            double f1 = 0;
            double f2 = 0;
            foreach ((double x,double y) in xyData)
            {
                f1 += y / (a + b * x);
                f2 += x * y / (a + b * x);
            }

            f1 -= n;
            f2 -= sumX;

            return new double[] { f1, f2 };
        }
    }
    
}