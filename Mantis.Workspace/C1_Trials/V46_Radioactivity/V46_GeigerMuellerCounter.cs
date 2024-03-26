using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.QuickTable;

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
        
    }
}