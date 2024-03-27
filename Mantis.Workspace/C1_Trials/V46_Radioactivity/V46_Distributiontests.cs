using System.Runtime.InteropServices.ComTypes;
using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.QuickTable;
using Mantis.Core.ScottPlotUtility;
using MathNet.Numerics;
using MathNet.Numerics.Differentiation;
using MathNet.Numerics.Distributions;
using ScottPlot.Finance;

namespace Mantis.Workspace.C1_Trials.V46_Radioactivity;

[QuickTable()]
public record struct CountData
{
    [QuickTableField("count")] public double Counts;
    [QuickTableField("time")] public ErDouble Time;
    public CountData(){}
}

public record struct BoxedData
{
    public double Number;
    public double Commonness;

    public BoxedData()
    {
    }
}
public class V46_Distributiontests
{
    public static void Process()
    {
        var csvReader = new SimpleTableProtocolReader("1secGateTimeMeasurement.csv");
        List<CountData> OneSecDataList = csvReader.ExtractTable<CountData>("tab:1secMeasurement");
        List<double> meanOneSecList = OneSecDataList.Select(e => e.Counts).ToList();
        ErDouble OneSecListMean = meanOneSecList.WeightedMean();
        DynPlot plot = new DynPlot("counts", "#");
        var plotList = PutDataInBoxes(OneSecDataList);
        plot.Add.Bars(plotList.Select(e=>(e.Number)).ToArray(),plotList.Select(e=>(e.Commonness)).ToArray());
        plot.SaveAndAddCommand("OneSecDistribution");

        
        
        var secondReader = new SimpleTableProtocolReader("10secGateTimeMeasurement.csv");
        List<CountData> TenSecDataList = secondReader.ExtractTable<CountData>("tab:10secMeasurement");
        var TenSecPlotList = PutDataInBoxes(TenSecDataList);
        List<double> meanTenSecDataList = TenSecDataList.Select(e => e.Counts).ToList();
        ErDouble TenSecListMean = meanTenSecDataList.WeightedMean();
        DynPlot secondPlot = new DynPlot("Counts", "#");
        secondPlot.Add.Bars(TenSecPlotList.Select(e => e.Number).ToArray(),
            TenSecPlotList.Select(e => e.Commonness).ToArray());
        secondPlot.SaveAndAddCommand("TenSecDistribution");
        
        Console.WriteLine(OneSecListMean);
        Console.WriteLine(OneSecDataList.Count);
        Console.WriteLine(TenSecListMean);
        Console.WriteLine(TenSecDataList.Count);
        
        
        double ChiSquaredOneSecond =  PerformChiSquaredForPoisson(plotList,OneSecListMean.Value, OneSecDataList.Count);
        Console.WriteLine(ChiSquaredOneSecond);

        double ChiSquaredTenSecond = PerformChiSquaredForPoisson(TenSecPlotList, TenSecListMean.Value, TenSecDataList.Count);
        Console.WriteLine(ChiSquaredTenSecond);
    }

    public static List<BoxedData> PutDataInBoxes(List<CountData> dataList)
    {
        int max = dataList.Max(e => (int)e.Counts);
        int min = dataList.Min(e => (int)e.Counts);
        Console.WriteLine(max + " " + min);

        int[] groupedCounts = new int[max-min+1];
        
        foreach (var e in dataList)
        {
            groupedCounts[(int)e.Counts-min] += 1;
        }

        var boxedList = new List<BoxedData>();
        for (int i = 0; i < groupedCounts.Length; i++)
        {
            BoxedData e = new BoxedData();
            e.Number = i;
            e.Commonness = groupedCounts[i];
            //Console.WriteLine(e.Number+" "+e.Commonness);
            boxedList.Add(e);
        }

        return boxedList;
    }

    public static double PerformChiSquaredForPoisson(List<BoxedData> dataList,double meanValue,int NumberofMeasurements)
    {
        double ChiSummand = 0;
        for (int i = 0; i < dataList.Count; i++)
        {
            Poisson dings = new Poisson(meanValue);
            double probability = dings.Probability((int)dataList[i].Number);
            Console.WriteLine(dataList[i].Number + " " +meanValue + " " + probability);
            Console.WriteLine(dataList[i].Number +" "+dataList[i].Commonness+" " + probability * NumberofMeasurements );
            ChiSummand += Math.Pow(dataList[i].Commonness - probability * NumberofMeasurements, 2) /
                                (probability * NumberofMeasurements);
        }

        return ChiSummand;
    }

    public static double PoissonProbability(double mean, double k)
    {
        return Math.Pow(mean, k) * Math.Exp(-mean) / SpecialFunctions.Factorial((int)k);
    }
    
}