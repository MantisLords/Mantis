using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.QuickTable;
using Mantis.Core.TexIntegration;
using ScottPlot.Extensions;
using ScottPlot.Finance;

namespace Mantis.Workspace.C1_Trials.V35_Ultrasound;

[QuickTable("")]
public record struct AlphaBetaData
{
    [QuickTableField("alpha")] public ErDouble Alpha;
    [QuickTableField("beta")] public ErDouble Beta;
    public AlphaBetaData()
    {}
}
public class V35_WaveModeMeasurement
{
    public static void Process()
    {
        var csvReader = new SimpleTableProtocolReader("LongTransWaveData.csv");
        List<AlphaBetaData> LongCopperList = csvReader.ExtractTable<AlphaBetaData>("tab:LongDataCopper");
        List<AlphaBetaData> TransCopperList = csvReader.ExtractTable<AlphaBetaData>("tab:TransDataCopper");
        List<AlphaBetaData> LongDataAlu = csvReader.ExtractTable<AlphaBetaData>("tab:LongDataAlu");
        List<AlphaBetaData> TransDataAlu = csvReader.ExtractTable<AlphaBetaData>("tab:TransDataAlu");
        CalculateVelocities(LongCopperList,"LongCopper");
        CalculateVelocities(TransCopperList,"TransCopper");
        CalculateVelocities(LongDataAlu,"LongAlu");
        CalculateVelocities(TransDataAlu,"TransAlu");
    }

    public static void CalculateVelocities(List<AlphaBetaData> dataList,string name)
    {
        
        for (int i = 0; i < dataList.Count; i++)
        {
            var e = dataList[i];
            (V35_DebyeSearsEffect.WaterVelocityDebye*Math.Sin(e.Beta.Value.ToRadians()) / Math.Sin(e.Alpha.Value.ToRadians())).AddCommandAndLog(name + i,"");
        }
    }
}