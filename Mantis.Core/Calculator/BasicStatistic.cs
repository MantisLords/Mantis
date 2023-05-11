using MathNet.Numerics.Statistics;

namespace Mantis.Core.Calculator;

public static class BasicStatistic
{
    public static (double, double) XYMean(this IEnumerable<(double, double)> data)
    {
        return (data.Select(e => e.Item1).Mean(), data.Select(e => e.Item2).Mean());
    }
}