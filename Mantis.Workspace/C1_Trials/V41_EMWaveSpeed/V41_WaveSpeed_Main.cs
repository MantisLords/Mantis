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

public struct CalculatedData
{
    public ErDouble frequency;
    public ErDouble EpsilonR;
    public ErDouble vStanding;
    public ErDouble damping;
}

public static class V41_WaveSpeed_Main
{
    public static double frequencyError = 0.01;
    public static double voltageErrorOszi = 0.01;
    
    public static void Process()
    {
        Console.WriteLine("Ronny");
        
       var standingWaveReader = new SimpleTableProtocolReader("Data\\Measurements");
       List<StandingWaveData> standingWaveList = standingWaveReader.ExtractTable<StandingWaveData>();
       var peakDifference = standingWaveReader.ExtractSingleValue<ErDouble>("peakDifference");
       var peakDifferenceRetardation = standingWaveReader.ExtractSingleValue<ErDouble>("peakDifferenceRetardation");
       var lengthRetardation = standingWaveReader.ExtractSingleValue<ErDouble>("lengthRetardation");
       var tempGroupedListList = standingWaveList.GroupBy(e => (e.nodeCount,e.isEndFixed)).ToList();

       var calculatedMeanList =
           tempGroupedListList.Select(listWithSameNodeCount => CalculateDataMean(listWithSameNodeCount)).ToList();
       ErDouble v = CalculateVelocityRuntime(peakDifference, 50);
       Console.WriteLine(v);
       Console.WriteLine("EpsilonR runtime"+CalculateEpsR(v));
       ErDouble vRet = CalculateVelocityRuntime(peakDifferenceRetardation, lengthRetardation);
       Console.WriteLine("Velocity retardation"+vRet);


       List<CalculatedData> dataForTables =  CalculateValuesForTables(calculatedMeanList);
       ;
       Console.WriteLine(dataForTables[0].frequency + " " + dataForTables[0].vStanding + " " + dataForTables[0].EpsilonR + " " + dataForTables[0].damping);

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
            frequency = frequencyMean*1000,
            incommingVoltage = incommingVMean,
            nodeVoltage = nodeVMean,
            isEndFixed = listWithSameNodeCount.Key.Item2,
            nodeCount = listWithSameNodeCount.Key.Item1
        };
        return newData;
    }

    
    
    
    public static ErDouble CalculateVelocityRuntime(ErDouble time, ErDouble length)
    {
        return 2 * length / time;
    }

    public static ErDouble CalculateEpsR(ErDouble v)
    {
        return (3 * Math.Pow(10, 8) / v).Pow(2);
    }

    public static List<CalculatedData> CalculateValuesForTables(List<StandingWaveData> data)
    {
        List<CalculatedData> calculatedData = new List<CalculatedData>();
        for (int i = 0; i < data.Count; i++)
        {
            var e = new CalculatedData();
            e.frequency = data[i].frequency;
            e.vStanding =
                CalculateVelocityStanding(data[i].frequency, data[i].isEndFixed, data[i].nodeCount);
            e.damping = CalculateDamping(data[i].incommingVoltage, data[i].nodeVoltage);
            e.EpsilonR = CalculateEpsR(e.vStanding);
            calculatedData.Add(e);
        }

        return calculatedData;
    }
    public static ErDouble CalculateDamping(ErDouble U0,ErDouble deltaU)
    {
        ErDouble U2l = U0 - deltaU/1000 ;
        return  Math.Log((U0 / U2l).Value, 10)* 20 * 1 / 100;
    }

    public static ErDouble CalculateVelocityStanding(ErDouble f, bool openEnd, int nodeCount)
    {
        ErDouble lambda = new ErDouble();
        if (openEnd==false)
        {
            if (nodeCount == 1)
            {
                lambda = 50 * 4;
            }
            else
            {
                lambda = 50 * 3 / 4;
            }
        }
        else
        { 
            lambda = 50 * 2;
        }
        
        return lambda*f;
    }
}