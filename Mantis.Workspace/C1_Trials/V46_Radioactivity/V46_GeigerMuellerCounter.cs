using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.QuickTable;
using Mantis.Core.ScottPlotUtility;
using Mantis.Core.TexIntegration;

namespace Mantis.Workspace.C1_Trials.V46_Radioactivity;

[QuickTable()]
public record struct VoltageCountData
{
    [QuickTableField("voltage")] public ErDouble Voltage;
    [QuickTableField("counts")] public ErDouble Counts;
    
    public VoltageCountData()
    {}
    
}
public class V46_GeigerMuellerCounter
{
    public static void Process()
    {
        var csvReader = new SimpleTableProtocolReader("GeigerMuellerCounter.csv");
        List<VoltageCountData> dataList = csvReader.ExtractTable<VoltageCountData>("tab:GeigerMuellerCounter");
        DynPlot plot = new DynPlot("Voltage [V]","Counts");
        plot.AddDynErrorBar(dataList.Select(e => (e.Voltage, e.Counts)));
        plot.AddVerticalLine(520, "ThresholdVoltage");
        plot.SaveAndAddCommand("GeigerMuellerPlot");
        ErDouble thresholdVoltage = new ErDouble(520, 4);
        thresholdVoltage.AddCommandAndLog("ThresholdVoltage","V");
    }
}