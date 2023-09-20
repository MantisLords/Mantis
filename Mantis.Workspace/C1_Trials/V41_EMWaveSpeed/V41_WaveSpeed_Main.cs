using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Xml;
using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.QuickTable;
using Mantis.Core.ScottPlotUtility;
using Mantis.Core.TexIntegration;
using Mantis.Core.Utility;
using Mantis.Workspace.BasicTests;
using Mantis.Workspace.C1_Trials.Utility;
using Microsoft.VisualBasic;
using ScottPlot;
using ScottPlot.Drawing.Colormaps;

namespace Mantis.Workspace.C1_Trials.V41_EMWaweSpeed;

public record struct OsziPulseData
{
    public ErDouble time;
    public ErDouble voltage;
}//OsziData anscheinend schon da

[QuickTable("","tab:standingWaveData")]
public struct StandingWaveData
{
    
    [QuickTableField("frequency","Hz")]
    public ErDouble frequency;

    [QuickTableField("nodeCount")] 
    public int nodeCount;
    [QuickTableField("nodeVoltage","V")]
    public ErDouble nodeVoltage;
    [QuickTableField("incomingVoltage","V")]
    public ErDouble incommingVoltage;
    [QuickTableField("isEndFixed")]
    public bool isEndFixed;
    
    public StandingWaveData(){}
};

public static class V41_WaveSpeed_Main
{
    public static double frequencyError = 0.01;
    public static double voltageErrorOszi = 0.01;
    public static void Process()
    {
        Console.WriteLine("Ronny");
        var osziCsvReader = new OsziRowWiseCsvReader("Data\\F0005CH1.csv");
        osziCsvReader.ReadFile();
        List < OsziData > dataList= osziCsvReader.Data.ToList();
        Console.WriteLine(dataList[0].Time);
        
        dataList.ForEachRef(((ref OsziData data) => CalculateErrors(ref data)));
        
        RegModel<LineFunc> model = dataList.CreateRegModel(e => (e.Time, e.Voltage),
            new ParaFunc<LineFunc>(2)
            {
                Units = new[] { "Gradient", "Voltage" }
            }
        );
        model.DoLinearRegression(false);
        model.AddParametersToPreambleAndLog("VoltagePulseLineFit");
       ScottPlot.Plot plot = ScottPlotExtensions.CreateSciPlot("time in s", "Voltage in V",pixelWidth:520 * 4);
       var (errorBar,scatterPlot,functionPlot) = plot.AddRegModel(model, "daten", "fit");
       scatterPlot.MarkerSize = 5;
       plot.SaveAndAddCommand("fig:VoltagePulse","caption");
       
       var standingWaveReader = new SimpleTableProtocolReader("Data\\Measurements");
       List<StandingWaveData> standingWaveList = InitializeErrors( standingWaveReader.ExtractTable<StandingWaveData>());

       var tempGroupedListList = standingWaveList.GroupBy(e => (e.nodeCount,e.isEndFixed)).ToList();

       var calculatedMeanList =
           tempGroupedListList.Select(listWithSameNodeCount => CalculateDataMean(listWithSameNodeCount)).ToList();
       
       calculatedMeanList
           
    }

    public static StandingWaveData CalculateDataMean(IGrouping<(int,bool),StandingWaveData> listWithSameNodeCount)
    {
        ErDouble sumFreq=0;
        ErDouble sumInV = 0;
        ErDouble sumNodeV = 0;
        int count = 0;
        

        foreach (StandingWaveData data in listWithSameNodeCount)
        {
            count++;
            sumFreq += data.frequency;
            sumInV += data.incommingVoltage;
            sumNodeV += data.nodeVoltage;
        }

        ErDouble frequencyMean = sumFreq / count;
        ErDouble sumForStandardDeviationFreq = new ErDouble(0,0);
        ErDouble sumForStandardDeviationIncomV = new ErDouble();
        ErDouble sumForStandardDeviationNodeV = new ErDouble();
        ErDouble nodeVMean = sumNodeV/count;
        ErDouble incommingVMean = sumInV/count;
        foreach (var data in listWithSameNodeCount)
        {
            sumForStandardDeviationFreq += (frequencyMean - data.frequency).Pow(2);
            sumForStandardDeviationNodeV += (nodeVMean - data.nodeVoltage).Pow(2);
            sumForStandardDeviationIncomV += (incommingVMean - data.incommingVoltage).Pow(2);
        }

        incommingVMean.Error = Math.Sqrt((1/(double)(count-1)) * sumForStandardDeviationIncomV.Value)/Math.Sqrt(count);
        nodeVMean.Error = Math.Sqrt(sumForStandardDeviationNodeV.Value * 1/(count-1))/Math.Sqrt(count);
        frequencyMean.Error = Math.Sqrt( sumForStandardDeviationFreq.Value * 1/(count-1))/Math.Sqrt(count);
        StandingWaveData newData = new StandingWaveData()
        {
            frequency = frequencyMean,
            incommingVoltage = incommingVMean,
            nodeVoltage = nodeVMean,
            isEndFixed = listWithSameNodeCount.Key.Item2,
            nodeCount = listWithSameNodeCount.Key.Item1
        };
        return newData;
    }

    

    public static List<StandingWaveData> InitializeErrors(List<StandingWaveData> data)
    {
        StandingWaveData e = new StandingWaveData();
        for (int i = 0; i < data.Count; i++)
        {
            e = data[i];
            e.frequency = new ErDouble(e.frequency.Value, e.frequency.Value * frequencyError);
            e.nodeVoltage = new ErDouble(e.nodeVoltage.Value, e.nodeVoltage.Value * voltageErrorOszi);
            e.incommingVoltage = new ErDouble(e.incommingVoltage.Value, e.incommingVoltage.Value * voltageErrorOszi);
            data[i] = e;
        }

        return data;
    }

    public static OsziData CalculateErrors(ref OsziData data)
    {
        double osziVoltageError = 0.01;
        double osziTimeError = 0.01;
        data.Voltage = new ErDouble(data.Voltage.Value, data.Voltage.Value * osziVoltageError);
        data.Time = new ErDouble(data.Time.Value, data.Time.Value * osziTimeError);
        return data;
    }
    public static ErDouble CalculateVelocityRuntime(double time, ErDouble length)
    {
        return 2 * length / time;
    }

    public static ErDouble CalculateEpsR(ErDouble v)
    {
        return (3 * Math.Pow(10, 8) / v).Pow(2);
    }

    public static ErDouble CalculateDamping(ErDouble U0,ErDouble deltaU)
    {
        ErDouble U2l = deltaU - U0;
        return 20 * 1 / 100 * Math.Log((U0 / U2l).Value, 10);
    }

    public static ErDouble CalculateVelocityStanding(ErDouble f, bool openEnd, int nodeCount)
    {
        ErDouble lambda = new ErDouble(0, 0);
        if (openEnd)
        {
            lambda = 50 * 4/(2 * nodeCount-1);
        }
        else
        { lambda = 50 * 2/nodeCount;
        }
        
        return lambda*f;
    }
}