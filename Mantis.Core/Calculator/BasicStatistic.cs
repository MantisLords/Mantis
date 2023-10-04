using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Statistics;

namespace Mantis.Core.Calculator;

public static class BasicStatistic
{

    public static ErDouble WeightedMean(this IEnumerable<double> data)
    {
        return data.Select(v => new ErDouble(v)).WeightedMean(false);
    }
    public static ErDouble WeightedMean(this IEnumerable<ErDouble> data, bool useYErrors = true,bool useMaxCovariance = true)
    {
        Vector<double> y = Vector<double>.Build.DenseOfEnumerable(data.Select(e => e.Value));

        Vector<double> weight = useYErrors
            ? Vector<double>.Build.DenseOfEnumerable(data.Select(e => 1 / (e.Error * e.Error)))
            : Vector<double>.Build.Dense(data.Count(), 1);

        if (useYErrors && weight.Any(v => !double.IsFinite(v)))
            throw new ArgumentException("The data contains value with no error! Set 'useYErrors' false if you want" +
                                        "to ignore the errors or add an error to the value");

        double covariance = 1 / weight.Sum();

        double mean = covariance * weight.DotProduct(y);
        
        
        
        var yCentered = y - mean;
        double sigmaSqrt = yCentered.DotProduct(yCentered) / (y.Count - 1);
        var unWeightedCovariance = sigmaSqrt / y.Count;
        if (y.Count <= 1)
            unWeightedCovariance = 0;

        if (!useYErrors)
            covariance = unWeightedCovariance;
        else if (useMaxCovariance)
            covariance = Math.Max(covariance, unWeightedCovariance);

        return new ErDouble(mean, Math.Sqrt(covariance));
    }
    
    
}