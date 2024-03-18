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
        
        CalculateDepth(dataList,GetPolyVelocity());
    }

    public static ErDouble GetPolyVelocity()
    {
        var csvReader = new SimpleTableProtocolReader("RuntimeMeasurements1MHz.csv");
        ErDouble length = csvReader.ExtractSingleValue<ErDouble>("PolyLength");
        ErDouble time = csvReader.ExtractSingleValue<ErDouble>("PolyTime");
        return V35_RuntimeMeasurement.CalculateVelocity(length/2, time);
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