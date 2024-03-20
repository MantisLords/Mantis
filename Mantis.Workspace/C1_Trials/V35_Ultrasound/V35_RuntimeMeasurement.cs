using System.ComponentModel;
using System.Runtime.InteropServices.ComTypes;
using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.QuickTable;
using Mantis.Core.TexIntegration;
using Mantis.Core.Utility;

namespace Mantis.Workspace.C1_Trials.V35_Ultrasound;

[QuickTable("WaterData")]
public record struct WaterData
{
    [QuickTableField("length")] public ErDouble Length;
    [QuickTableField("time")] public ErDouble Time;
    public WaterData()
    {}
}
public class V35_RuntimeMeasurement
{
    private static ErDouble _polyVelocity1MHz;
    public static ErDouble PolyVelocity1MHz => _polyVelocity1MHz;

    private static ErDouble _waterVelocity;
    public static ErDouble WaterVelocity => _waterVelocity;


    public static void Process()
    {
        ReadData("RuntimeMeasurements1MHz",out ErDouble polyVelocity1MHz);
        _polyVelocity1MHz = polyVelocity1MHz;
        
        ReadData("RuntimeMeasurements2MHz",out ErDouble polyVelocity2MHz);
        var csvReader = new SimpleTableProtocolReader("RuntimeMeasurements1MHz.csv");
        List<WaterData> dataList = csvReader.ExtractTable<WaterData>("tab:WaterData");
        dataList.ForEachRef((ref WaterData data) =>  data.Length.Error = 3);
        CalculateVelocityWater(dataList,out ErDouble WaterVelocity);
        _waterVelocity = WaterVelocity;
    }

    public static void ReadData(string fileName,out ErDouble polyVelocity)
    {
        var csvReader = new SimpleTableProtocolReader(fileName);
        
        ErDouble PVCLength = csvReader.ExtractSingleValue<ErDouble>("PVCLength");
        ErDouble PVCTime = csvReader.ExtractSingleValue<ErDouble>("PVCTime");
        //Console.WriteLine(fileName + " PVC " + CalculateVelocity(PVCLength,PVCTime));
        CalculateVelocity(PVCLength,PVCTime).AddCommandAndLog("PVCVelocity"+fileName,"m/s");
        ErDouble PolyLength = csvReader.ExtractSingleValue<ErDouble>("PolyLength");
        ErDouble PolyTime = csvReader.ExtractSingleValue<ErDouble>("PolyTime");
        polyVelocity = CalculateVelocity(PolyLength / 2, PolyTime);
        polyVelocity.AddCommandAndLog("polyVelocity"+fileName,"m/s");
        //factor of two missing because of different measurement technique

    }
    public static ErDouble CalculateVelocity(ErDouble length, ErDouble time)
    {
        length = length * Math.Pow(10, -3);
        time = time * Math.Pow(10,-6);
        return  2*length / time;
    }

    public static void CalculateVelocityWater(List<WaterData> dataList, out ErDouble WaterVelocity)
    {
        dataList.ForEachRef(((ref WaterData data) => data.Length=data.Length*Math.Pow(10,-3)));
        dataList.ForEachRef(((ref WaterData data) => data.Time=data.Time*Math.Pow(10,-6)));
        RegModel line = dataList.CreateRegModel(e => (e.Time,e.Length),
            new ParaFunc(2, new LineFunc())
            {
                Units = new[] { "", "" }
            }
        );
        line.DoLinearRegression(false);
        WaterVelocity = line.ErParameters[0]*10000;
        WaterVelocity.AddCommandAndLog("WaterVelocity","m/s");
        
    }
}