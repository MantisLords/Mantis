using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.QuickTable;
using Mantis.Core.TexIntegration;

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
        int i = 0;
        //Console.WriteLine("V: " + velocity);
        foreach (var data in runtimeList)
        {
            i++;
            (data.Depth*Math.Pow(10,-3)*velocity/2).AddCommandAndLog("depth"+i,"");//this value is in mm
        }
    }
}