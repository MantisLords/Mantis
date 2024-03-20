using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.QuickTable;

namespace Mantis.Workspace.C1_Trials.V35_Ultrasound;

[QuickTable("DepthData")]
public record struct DepthData
{
    [QuickTableField("depth")] public ErDouble Depth;

    public DepthData()
    {
        
    }
}
public class V35_DepthMeasurement
{
    public static void Process()
    {
        var csvReader = new SimpleTableProtocolReader("DepthMeasurements.csv");
        List<DepthData> dataList = csvReader.ExtractTable<DepthData>("tab:DepthData");
        
        CalculateDepth(dataList,V35_RuntimeMeasurement.PolyVelocity1MHz);
    }
    
    public static void CalculateDepth(List<DepthData> runtimeList, ErDouble velocity)
    {
        Console.WriteLine("V: " + velocity);
        foreach (var data in runtimeList)
        {
            Console.WriteLine(data.Depth*Math.Pow(10,-6)*velocity/2);
        }
    }
}