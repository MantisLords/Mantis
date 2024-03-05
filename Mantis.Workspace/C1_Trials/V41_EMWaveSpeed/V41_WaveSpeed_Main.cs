using System.Drawing;
using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.QuickTable;
using Mantis.Core.ScottPlotUtility;
using Mantis.Core.TexIntegration;
using ScottPlot;

namespace Mantis.Workspace.C1_Trials.V41_EMWaweSpeed;

public record struct OsziPulseData
{
    public ErDouble time;
    public ErDouble voltage;
}//OsziData anscheinend schon da

[QuickTable("","tab:standingWaveData")]
public record struct StandingWaveData
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
    public ErDouble nodeVoltage;
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
       lengthRetardation.AddCommand("lengthRetardation","m");
        
       var calculatedMeanList =
           tempGroupedListList.Select(listWithSameNodeCount => CalculateDataMean(listWithSameNodeCount)).ToList();
       ErDouble v = CalculateVelocityRuntime(peakDifference, 50);
       Console.WriteLine(v);
       calculatedMeanList[0].nodeVoltage.AddCommand("nodeVoltageOne");
       calculatedMeanList[1].nodeVoltage.AddCommand("nodeVoltageTwo");
       calculatedMeanList[2].nodeVoltage.AddCommand("nodeVoltageThree");
       v.AddCommand("velocityRuntime","m/s");
       Console.WriteLine("EpsilonR runtime"+CalculateEpsR(v));
       CalculateEpsR(v).AddCommand("epsilonRRuntime","");
       ErDouble vRet = CalculateVelocityRuntime(peakDifferenceRetardation, lengthRetardation);
       vRet.AddCommand("vRetRuntime","m/s");
       (lengthRetardation * 2*(19.0 * Math.Pow(10, -7))).AddCommand("expectedRetardation");
       Console.WriteLine("Velocity retardation"+vRet);

       
       List<CalculatedData> dataForTables =  CalculateValuesForTables(calculatedMeanList);
       dataForTables[0].frequency.AddCommand("frequencyFirst");
       (dataForTables[0].frequency*Math.Pow(10,-6)).AddCommand("frequencyTableFirst");
       dataForTables[0].vStanding.AddCommand("vStandingFirst");
       (dataForTables[0].vStanding*Math.Pow(10,-8)).AddCommand("vStandingTableFirst");
       dataForTables[0].EpsilonR.AddCommand("epsilonRFirst","");
       dataForTables[0].damping.AddCommand("dampingFirst");
       dataForTables[1].frequency.AddCommand("frequencySecond");
       (dataForTables[1].frequency*Math.Pow(10,-6)).AddCommand("frequencyTableSecond");
       dataForTables[1].vStanding.AddCommand("vStandingSecond");
       (dataForTables[1].vStanding*Math.Pow(10,-8)).AddCommand("vStandingTableSecond");
       dataForTables[1].EpsilonR.AddCommand("epsilonRSecond");
       dataForTables[1].damping.AddCommand("dampingSecond");
       dataForTables[2].frequency.AddCommand("frequencyThird");
       (dataForTables[2].frequency*Math.Pow(10,-6)).AddCommand("frequencyTableThird");
       dataForTables[2].vStanding.AddCommand("vStandingThird");
       (dataForTables[2].vStanding*Math.Pow(10,-8)).AddCommand("vStandingTableThird");
       dataForTables[2].EpsilonR.AddCommand("epsilonRThird");
       dataForTables[2].damping.AddCommand("dampingThird");

       Console.WriteLine(dataForTables[0].frequency + " " + dataForTables[0].vStanding + " " + dataForTables[0].EpsilonR + " " + dataForTables[0].damping);
       Console.WriteLine(dataForTables[1].frequency + " " + dataForTables[1].vStanding + " " + dataForTables[1].EpsilonR + " " + dataForTables[1].damping);
       Console.WriteLine(dataForTables[2].frequency + " " + dataForTables[2].vStanding + " " + dataForTables[2].EpsilonR + " " + dataForTables[2].damping);
    CalculateResistanceMean();
    double[] xFordamping = new[] { dataForTables[0].frequency.Value, dataForTables[2].frequency.Value };
    double[] xError = new[] { dataForTables[0].frequency.Error, dataForTables[2].frequency.Error };
    double[] yFordamping = new[] { dataForTables[0].damping.Value, dataForTables[2].damping.Value };
    double[] yError = new[] { dataForTables[0].damping.Error, dataForTables[2].damping.Error };
    double[] xSecondForDamping = new[] { dataForTables[1].frequency.Value };
    double[] ySecondForDamping = new[] { dataForTables[1].damping.Value };
    double[] xErrorSecond = new[] { dataForTables[1].frequency.Error };
    double[] yErrorSecond = new[] { dataForTables[1].damping.Error };
    DynPlot plot = new DynPlot("Frequency [Hz]","damping[dB/m]");
    plot.AddDynErrorBar(dataForTables.Select(e=>(e.frequency,e.damping)));
    
    var sc1 = plot.Add.Scatter(xSecondForDamping, ySecondForDamping,color: Colors.Black);
    sc1.LineStyle.IsVisible = false;
    sc1.MarkerSize = 7F;
    sc1.MarkerStyle.Shape = MarkerShape.FilledSquare;
    sc1.Label = "Standing waves with open far end";
    
    var sc2 = plot.Add.Scatter(xFordamping, yFordamping,color:Colors.Red );
    sc2.LineStyle.IsVisible = false;
    sc2.MarkerSize = 9F;
    sc2.MarkerStyle.Shape = MarkerShape.FilledTriangleDown;
    sc2.Label = "Standing waves with short circuit at far end";
    plot.SaveAndAddCommand("dampingPlot");
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
        StandingWaveData newData = new StandingWaveData()
        {
            frequency = new ErDouble(frequencyMean.Value*1000,5000),
            incommingVoltage = incommingVMean*1000,
            nodeVoltage = nodeVMean,
            isEndFixed = listWithSameNodeCount.Key.Item2,
            nodeCount = listWithSameNodeCount.Key.Item1
        };
        
        Console.WriteLine($"New Wave data {newData.ToString()}");
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
        ErDouble U2l = U0 - deltaU ;
        return Math.Pow(10,3)* ErDouble.Log((U0 / U2l)) / Math.Log(10) * 20.0 / 100.0;
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
            if(nodeCount==2)
            {
                lambda = 200.0/3.0;
            }
        }
        if(openEnd==true)
        { 
            lambda = 50 * 2;
        }
        
        return lambda*f;
    }

    public static void CalculateResistanceMean()
    {
        double sum = 0;
        int count = 0;
        List<ErDouble> list = new List<ErDouble>(){52.1, 50.0};
        for (int i = 0; i < list.Count; i++)
        {
            sum += list[i].Value;
            count++;
        }

        ErDouble mean = sum / count;
        ErDouble sumformean=0;
        for (int i = 0; i < list.Count; i++)
        {
            sumformean += (list[i].Value - mean)*(list[i].Value-mean);
        }
        mean.Error = Math.Sqrt((1/(double)(count-1)) * sumformean.Value)/Math.Sqrt(count);
        mean.AddCommand("resistanceMean","\\Omega");
    }
}