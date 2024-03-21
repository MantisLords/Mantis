using System.Xml.Schema;
using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.QuickTable;
using Mantis.Core.TexIntegration;
using ScottPlot;
using ScottPlot.Finance;

namespace Mantis.Workspace.C1_Trials.V35_Ultrasound;

[QuickTable("")]

public record struct DebyeData
{
     public ErDouble Wavelength;
     public ErDouble Frequency;
     public ErDouble Displacement;
     public ErDouble Order;

    [UseConstructorForParsing]
    public DebyeData(ErDouble Wavelength, ErDouble Frequency, ErDouble Displacement, ErDouble Order)
    {
        this.Wavelength = Wavelength;
        this.Frequency = Frequency;
        this.Displacement = Displacement;
        this.Displacement.Error = 1;
        this.Order = Order;
    }
}
public class V35_DebyeSearsEffect
{
    private static ErDouble _waterVelocity;
    public static ErDouble WaterVelocityDebye => _waterVelocity;
    public static void Process()
    {
        var csvReader = new SimpleTableProtocolReader("DebyeSearsData.csv");
        List<DebyeData> dataList = csvReader.ExtractTable<DebyeData>("tab:DebyeSearsData");
        ErDouble Distance = csvReader.ExtractSingleValue<ErDouble>("Distance") * 10;
        List<ErDouble> velocityList = new List<ErDouble>();
        foreach (var e in dataList)
        {
            CalculateSingleSpeed(e, Distance);//.AddCommandAndLog("SpeedinWater","m/s",LogLevel.OnlyLog);
            velocityList.Add(CalculateSingleSpeed(e, Distance));
        }

        _waterVelocity = velocityList.WeightedMean(false);
        _waterVelocity.AddCommandAndLog("WatervelocityDebye", "m/s");

    }

    public static ErDouble CalculateSingleSpeed(DebyeData dataPoint, ErDouble distance)
    {
        return dataPoint.Order * distance * dataPoint.Frequency * Math.Pow(10, 6) *
            dataPoint.Wavelength * Math.Pow(10, -9) / dataPoint.Displacement;
    }
}