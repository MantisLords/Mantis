using System.Text;
using MathNet.Numerics.LinearAlgebra;

namespace Mantis.Core.Calculator;

public class DataSet
{
    public readonly Vector<double> YValues;

    public readonly Vector<double> YErrors;

    public readonly Vector<double> XValues;

    public readonly Vector<double> XErrors;

    public readonly int Count;
    

    public DataSet(Vector<double> yValues, Vector<double> yErrors, Vector<double> xValues, Vector<double> xErrors, int count)
    {
        YValues = yValues;
        YErrors = yErrors;
        XValues = xValues;
        XErrors = xErrors;
        Count = count;
    }

    public override string ToString()
    {
        StringBuilder builder = new StringBuilder(); 
        builder.Append($"DataSet ({Count})\n");
        for (int i = 0; i < Count; i++)
        {
            builder.Append($"x: {new ErDouble(XValues[i], XErrors[i]).ToString()}\t\ty: {new ErDouble(YValues[i], YErrors[i]).ToString()}\n");
        }

        return builder.ToString();
    }
}

public static class FastDataSetConstructor
{
    public static DataSet CreateDataSet<T>(this IEnumerable<T> data, Func<T, (ErDouble, ErDouble)> selector)
    {
        return new DataSet(
            yValues: Vector<double>.Build.DenseOfEnumerable(data.Select(e => selector(e).Item2.Value)),
            yErrors: Vector<double>.Build.DenseOfEnumerable(data.Select(e => selector(e).Item2.Error)),
            xValues: Vector<double>.Build.DenseOfEnumerable(data.Select(e => selector(e).Item1.Value)),
            xErrors: Vector<double>.Build.DenseOfEnumerable(data.Select(e => selector(e).Item1.Error)),
            count: data.Count()
        );
    }
}