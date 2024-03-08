using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.TexIntegration;

namespace Mantis.Workspace.C1_Trials.V33_Radiation;

public class V33_Transistivity
{
    public static void Process()
    {
        CalculateTransistivities();
        CalculateWavelenghts();
        
    }

    public static void CalculateTransistivities()
    {
        var csvReader = new SimpleTableProtocolReader("Smailagic_Karb_Data/TransistivityData");
        List<ErDouble> dataList = new List<ErDouble>();
        dataList.Add(csvReader.ExtractSingleValue<ErDouble>("NoFilter"));
        dataList.Add(csvReader.ExtractSingleValue<ErDouble>("Nickel"));
        dataList.Add(csvReader.ExtractSingleValue<ErDouble>("Window"));
        dataList.Add(csvReader.ExtractSingleValue<ErDouble>("SiliconeFront"));
        dataList.Add(csvReader.ExtractSingleValue<ErDouble>("NaCl"));
        dataList.Add(csvReader.ExtractSingleValue<ErDouble>("InfraredGlass"));
        dataList.Add(csvReader.ExtractSingleValue<ErDouble>("SiliconeBack"));
        dataList.Add(csvReader.ExtractSingleValue<ErDouble>("SiliconeMid"));
        dataList.Add(csvReader.ExtractSingleValue<ErDouble>("ZeroPoint"));
        for (int i = 0; i < 9; i++)
        {
            ErDouble wert = dataList[i];
            wert.Error = dataList[i].Value * 0.025 + 0.8 * Math.Pow(10, -3);
            ((wert-dataList[8]) / dataList[0]).AddCommand("TransistivityValue"+i);
            dataList[i] = wert;
        }
    }

    public static void CalculateWavelenghts()
    {
        double b =  2.89777 * Math.Pow(10,-3);
        double RoomTemp = 20 + 273.15;
        double CubeTemp = 80 + 273.15;
        (b/CubeTemp).AddCommand("CubeWavelength");
        (b/RoomTemp).AddCommand("RoomWavelenght");
    }
}