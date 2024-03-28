using System.Runtime.InteropServices.ComTypes;
using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.QuickTable;
using Mantis.Core.ScottPlotUtility;
using MathNet.Numerics;
using MathNet.Numerics.Differentiation;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Statistics;
using ScottPlot;
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
        var plotList = PutDataInBoxes(OneSecDataList,OneSecListMean.Value);

        plot.Add.Bars(plotList.Select(e=>(e.Number)).ToArray(),plotList.Select(e=>(e.Commonness)).ToArray());
        
        List < BoxedData > poissonListOneSec = GeneratePoissonDistribution(OneSecListMean.Value, plotList, OneSecDataList.Count);
        plot.Add.Bars(poissonListOneSec.Select(e => e.Number).ToArray(),
           poissonListOneSec.Select(e => e.Commonness).ToArray());
        //BarPlotOneSec.Color = Colors.Red.WithAlpha(0.5);
        //BarPlotOneSec.Label = "Poisson";
        
        plot.SaveAndAddCommand("OneSecDistribution");

        
        
        var secondReader = new SimpleTableProtocolReader("10secGateTimeMeasurement.csv");
        List<CountData> TenSecDataList = secondReader.ExtractTable<CountData>("tab:10secMeasurement");
        
        List<double> meanTenSecDataList = TenSecDataList.Select(e => e.Counts).ToList();
        ErDouble TenSecListMean = meanTenSecDataList.Mean();
        var TenSecPlotList = PutDataInBoxes(TenSecDataList,TenSecListMean.Value);
        DynPlot secondPlot = new DynPlot("Counts", "#");
        secondPlot.Add.Bars(TenSecPlotList.Select(e => e.Number).ToArray(),
            TenSecPlotList.Select(e => e.Commonness).ToArray());
        
        
        List < BoxedData > poissonListTenSec = GeneratePoissonDistribution(TenSecListMean.Value, TenSecPlotList, TenSecDataList.Count);
         secondPlot.Add.Bars(poissonListTenSec.Select(e => e.Number).ToArray(),
            poissonListTenSec.Select(e => e.Commonness).ToArray());
        
        secondPlot.SaveAndAddCommand("TenSecDistribution");
        
        
        double ChiSquaredOneSecond =  PerformChiSquaredForPoisson(plotList,OneSecListMean.Value, OneSecDataList.Count);
        Console.WriteLine("ChiSquaredOneSecond "+ChiSquaredOneSecond);

        double ChiSquaredTenSecond = PerformChiSquaredForPoisson(TenSecPlotList, TenSecListMean.Value, TenSecDataList.Count);
        Console.WriteLine("ChiSquaredTenSecond " + ChiSquaredTenSecond);

        double ChiSquaredTenGauss =
            PerformChiSquaredForGauss(TenSecPlotList, TenSecListMean.Value, TenSecDataList.Count);
        Console.WriteLine("ChiSquaredTenGauss " + ChiSquaredTenGauss);
    }

    public static double PerformChiSquaredForGauss(List<BoxedData> dataList, double meanValue, int NumberofMeasurements)
    {
        double Chi = 0;
        Normal verteilung = new Normal(meanValue, Math.Sqrt(meanValue));
        for (int i = 0; i < dataList.Count; i++)
        {
            double probability = verteilung.Density((int)dataList[i].Number);
            if (i == 0)
            {
                probability = verteilung.CumulativeDistribution((int)dataList[i].Number);
                //Console.WriteLine(probability);
            }

            if (i == dataList.Count - 1)
            {
                probability = 1 - verteilung.CumulativeDistribution((int)dataList[i].Number - 1);
                //Console.WriteLine(probability);
            }

            Chi += Math.Pow(dataList[i].Commonness - probability * NumberofMeasurements, 2) /
                   (probability * NumberofMeasurements);

        }

        return Chi;
    }

    public static List<BoxedData> PutDataInBoxes(List<CountData> dataList, double mean)
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
            e.Number = i + min;
            e.Commonness = groupedCounts[i];
            //Console.WriteLine(e.Number+" "+e.Commonness);
            boxedList.Add(e);
        }

        for (int i = 0; i < boxedList.Count; i++)
        {
            var e = boxedList[i];
            if (e.Commonness < 5)
            {
                if (mean - e.Number > 0)
                {
                    double sum = 0;
                    for (int j = 0; j <= i; j++)
                    {
                        var f = boxedList[j];
                        sum += f.Commonness;
                        f.Commonness = 0;
                        boxedList[j] = f;
                    }

                    var newbox = boxedList[i + 1];
                    newbox.Commonness += sum;
                    boxedList[i + 1] = newbox;
                }

                if (mean - e.Number < 0)
                {
                    double sum = 0;
                    for (int j = i+1; j < boxedList.Count; j++)
                    {
                        var f = boxedList[j];
                        sum += f.Commonness;
                        f.Commonness = 0;
                        boxedList[j] = f;
                    }

                    e.Commonness += sum;
                    boxedList[i] = e;
                }
            }
        }

        for (int i = 0; i < boxedList.Count; i++)
        {
            if (boxedList[i].Commonness == 0)
            {
                boxedList.Remove(boxedList[i]);
                i--;
            }
            
        }
        return boxedList;
    }

    public static double PerformChiSquaredForPoisson(List<BoxedData> dataList,double meanValue,int NumberofMeasurements)
    {
        double ChiSummand = 0;
        Poisson dings = new Poisson(meanValue);
        for (int i = 0; i < dataList.Count; i++)
        {
            double probability = dings.Probability((int)dataList[i].Number);
            if(i==0)
            {
                probability = dings.CumulativeDistribution((int)dataList[i].Number);
                //Console.WriteLine(probability);
            }

            if (i == dataList.Count-1)
            {
                probability = 1-dings.CumulativeDistribution((int)dataList[i].Number-1);
                //Console.WriteLine(probability);
            }
            
            ChiSummand += Math.Pow(dataList[i].Commonness - probability * NumberofMeasurements, 2) /
                                (probability * NumberofMeasurements);
            
        }

        return ChiSummand;
    }

    public static List<BoxedData> GeneratePoissonDistribution(double mean,List<BoxedData> dataList,int NumberofMeasurements)
    {
        Poisson verteilung = new Poisson(mean);
        var returnList = new List<BoxedData>();
        for (int i = 0; i < dataList.Count; i++)
        {
            BoxedData e = dataList[i];
            double probability = verteilung.Probability((int)dataList[i].Number);
            e.Commonness = probability * NumberofMeasurements;
            returnList.Add(e);
        }

        return dataList;
    }
    
}