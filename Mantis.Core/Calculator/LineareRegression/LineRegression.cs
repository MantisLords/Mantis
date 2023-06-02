using System.Collections;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearRegression;
using MathNet.Numerics.RootFinding;
using MathNet.Numerics.Statistics;

namespace Mantis.Core.Calculator;

public static class LineRegression
{
    
    /// <summary>
    /// fits a + b x and assumes data points are normal distributed. Data can have y error or no y error
    /// </summary>
    /// <returns>(a,b)</returns>
    public static (ErDouble, ErDouble) LinearRegressionLine<T>(this IEnumerable<T> data,Func<T,(double,ErDouble)> selector,RegressionCommand command)
    {
        ErDouble[] parameters = data.LinearRegression(selector, xi => new double[] { 1, xi },command);
        return (parameters[0], parameters[1]);
    }

    public static (ErDouble, ErDouble) LinearRegressionPoissonDistributed<T>(this IEnumerable<T> data, Func<T, (double, ErDouble)> selector,(ErDouble,ErDouble)? initialGuessGauss = null)
    {
        var rootFindFunction = new RootFindFunctionPoisLinReg(data.Select(e => (selector(e).Item1,selector(e).Item2.Value)).ToArray());
        
        if(!initialGuessGauss.HasValue)
            initialGuessGauss = data.LinearRegressionLine(selector,RegressionCommand.UseYErrors);
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