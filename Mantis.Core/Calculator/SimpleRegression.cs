namespace Mantis.Core.Calculator;

public static class SimpleRegression
{
    public static (ErDouble, ErDouble) LinearRegression<T>(this IEnumerable<T> data,Func<T,(double,double)> selector)
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
    
}