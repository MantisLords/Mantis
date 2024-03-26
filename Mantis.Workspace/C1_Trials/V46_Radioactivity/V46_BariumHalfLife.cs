using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.QuickTable;
using Mantis.Core.ScottPlotUtility;
using Mantis.Core.TexIntegration;
using Mantis.Core.Utility;
using ScottPlot.Finance;

namespace Mantis.Workspace.C1_Trials.V46_Radioactivity;

[QuickTable()]
public record struct HalfLifeData
{
    [QuickTableField("time")] public ErDouble Time;
    [QuickTableField("counts")] public ErDouble Counts;
    public HalfLifeData(){}
}
public class V46_BariumHalfLife
{
    public static void Process()
    {
        var csvReader = new SimpleTableProtocolReader("BariumDecayData");
        List<HalfLifeData> dataList =  csvReader.ExtractTable<HalfLifeData>("tab:BariumDecayData");
        DynPlot plot = new DynPlot("Time in s", "counts");
        dataList.ForEachRef(((ref HalfLifeData data) => data.Time *= Math.Pow(10,-6)));
        dataList.ForEachRef(((ref HalfLifeData data) => data.Counts.Error = Math.Sqrt(data.Counts.Value)));
        for (int i = 1; i < dataList.Count; i++)
        {
            var e = dataList[i];
            var f = dataList[i - 1];
            e.Time += f.Time;
            dataList[i] = e;
        }
        plot.AddDynErrorBar(dataList.Select(e => (e.Time, e.Counts)));
        RegModel expoFunc = dataList.CreateRegModel(e => (e.Time, e.Counts),
            new ParaFunc(2,new ExpFunc())
        {
            Units = new []{"",""}
        });
        expoFunc.DoRegressionLevenbergMarquardt(new double[]{2,-2});
        plot.AddRegModel(expoFunc);
        (Math.Log(2)/expoFunc.ErParameters[1]).AddCommandAndLog("BariumHalfLifeFromFit","1/s");
        plot.SaveAndAddCommand("BariumPlot");
    }
}