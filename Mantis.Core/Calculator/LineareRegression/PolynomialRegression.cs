namespace Mantis.Core.Calculator;

public static class PolynomialRegression
{
    /// <summary>
    /// Do a polynomial regression of oder n. With y(x) = Sum ak * x^k = a0 + a1 x + a2 x^2 + a3 x^3 +... an x^n
    /// </summary>
    /// <param name="data">Array of data points</param>
    /// <param name="selector">Converter of a data point to (x,y) pair</param>
    /// <param name="order">polynom order</param>
    /// <typeparam name="T"></typeparam>
    /// <returns>Regression parameters a = {a0,a1,a2,a3,...an}</returns>
    /// <exception cref="ArgumentException"></exception>
    public static ErDouble[] LinearRegressionPolynomial<T>(this IEnumerable<T> data,
        Func<T, (double, ErDouble)> selector, int order,RegressionCommand command)
    {
        if (order <= 0)
            throw new ArgumentException("The order of the polynomial must be greater than zero");

        return data.LinearRegression(selector, xi => PolynomialEvaluation(xi, order),command);
    }
    
    /// <summary>
    /// Fits a quadratic polynomial y(x) = a + b x + c x^2
    /// </summary>
    /// <param name="data">Array of data points</param>
    /// <param name="selector">Converter of a data point to (x,y) pair</param>
    /// <typeparam name="T"></typeparam>
    /// <returns>Regression parameters (a,b,c) </returns>
    public static (ErDouble, ErDouble, ErDouble) LinearRegressionQuadratic<T>(this IEnumerable<T> data,
        Func<T, (double, ErDouble)> selector,RegressionCommand command)
    {
        ErDouble[] parameters = data.LinearRegression(selector, x => new double[]{1, x, x * x}, command);
        return (parameters[0], parameters[1], parameters[2]);
    }

    public static IEnumerable<double> PolynomialEvaluation(double x, int order)
    {
        double xpowi = 1;
        for (int i = 0; i < order+1; i++)
        {
            yield return xpowi;
            xpowi *= x;
        }
    }

    public static ErDouble[] LinearRegressionLegendrePolynomial<T>(this IEnumerable<T> data,
        Func<T, (double, ErDouble)> selector, int order, RegressionCommand command)
    {
        if (order <= 0)
            throw new ArgumentException("The order of the polynomial must be greater than zero");
        
        return data.LinearRegression(selector, xi => LegendrePolynomialEvaluation(xi, order),command);
    }

    public static IEnumerable<double> LegendrePolynomialEvaluation(double x, int order)
    {
        double[] res = new double[order];
        if (order > 0) res[0] = 1;
        if (order > 1) res[1] = x;

        for (int n = 2; n < order; n++)
        {
            res[n] = (((double)2 * n - 1) * x * res[n - 1] -  res[n - 2] *((double)n - 1)) / n;
        }

        return res;
    }
}