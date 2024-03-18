using System.ComponentModel;
using System.Runtime.InteropServices.ComTypes;
using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;

namespace Mantis.Workspace.C1_Trials.V35_Ultrasound;

public class V35_RuntimeMeasurement
{
    
    public static void Process()
    {
        ReadData("RuntimeMeasurements1MHz");
        ReadData("RuntimeMeasurements2MHz");
    }

    public static void ReadData(string fileName)
    {
        var csvReader = new SimpleTableProtocolReader(fileName);
        
        ErDouble PVCLength = csvReader.ExtractSingleValue<ErDouble>("PVCLength");
        ErDouble PVCTime = csvReader.ExtractSingleValue<ErDouble>("PVCTime");
        Console.WriteLine(fileName + " PVC " + CalculateVelocity(PVCLength,PVCTime));
        
        ErDouble PolyLength = csvReader.ExtractSingleValue<ErDouble>("PolyLength");
        ErDouble PolyTime = csvReader.ExtractSingleValue<ErDouble>("PolyTime");
        Console.WriteLine(CalculateVelocity(PolyLength/2,PolyTime));//factor of two missing because of different measurement technique
        
        ErDouble WaterLenght = csvReader.ExtractSingleValue<ErDouble>("WaterLength");
        ErDouble WaterTime = csvReader.ExtractSingleValue<ErDouble>("WaterTime");
        ErDouble Container = csvReader.ExtractSingleValue<ErDouble>("WaterContainerLength");
        Console.WriteLine(CalculateVelocity(Container - WaterLenght,WaterTime));
    }
    public static ErDouble CalculateVelocity(ErDouble length, ErDouble time)
    {
        length = length * Math.Pow(10, -3);
        time = time * Math.Pow(10,-6);
        return  2*length / time;
    }
}