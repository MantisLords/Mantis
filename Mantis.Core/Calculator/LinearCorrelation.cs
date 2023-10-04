using MathNet.Numerics.Statistics;

namespace Mantis.Core.Calculator;

public static class LinearCorrelation
{
    public static double CovarianceBetween<T>(this IEnumerable<T> data,Func<T,(double,double)> selector, bool populationCovariance = false)
    {
        //return data.Select(e => e.Item1).Covariance(data.Select(e => e.Item2));
        IEnumerable<(double, double)> xyData = data.Select(selector);
        
        var (meanX, meanY) = xyData.XYMean();

        double covariance = 0;
        int count = 0;
        
        foreach (var e in xyData)
        {
            covariance += (e.Item1 - meanX) * (e.Item2 - meanY);
            count++;
        }

        covariance /= populationCovariance ? count : count - 1;

        return covariance;
    }

    public static double CorrelationBetween<T>(this IEnumerable<T> data,Func<T,(double,double)> selector)
    {
        IEnumerable<(double, double)> xyData = data.Select(selector);
        var (meanX, meanY) = xyData.XYMean();

        double sigmaXY = 0;
        double sigmaXX = 0;
        double sigmaYY = 0;


        foreach (var (x,y) in xyData)
        {
            sigmaXY += (x - meanX) * (y - meanY);
            sigmaXX += (x - meanX) * (x - meanX);
            sigmaYY += (y - meanY) * (y - meanY);
        }

        double cof = sigmaXY / Double.Sqrt(sigmaXX * sigmaYY);
        return cof;
    }
    
    private static (double, double) XYMean(this IEnumerable<(double, double)> data)
    {
        return (data.Select(e => e.Item1).Mean(), data.Select(e => e.Item2).Mean());
    }
}