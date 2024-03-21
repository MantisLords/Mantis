using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.QuickTable;
using Mantis.Core.TexIntegration;

namespace Mantis.Workspace.C1_Trials.V35_Ultrasound.Data_Smailagic_Karb;

[QuickTable("")]
public record struct AbsorbtionData
{
    [QuickTableField("self")] public ErDouble Self;
    [QuickTableField("length")] public ErDouble Damped;
    
    public AbsorbtionData(){}
}
public class V35_Absorbtion
{
    public static void Process()
    {
        var csvReader = new SimpleTableProtocolReader("AbsorbtionData.csv");
        List < AbsorbtionData > OneMHzList = csvReader.ExtractTable<AbsorbtionData>("tab:AmplitudeData1MHz");
        List<AbsorbtionData> TwoMHzList = csvReader.ExtractTable<AbsorbtionData>("tab:AmplitudeData2MHz");
        ErDouble cylinderLength = csvReader.ExtractSingleValue<ErDouble>("Length");
        CalculateDampening(OneMHzList, cylinderLength).AddCommandAndLog("Dampening1Mhz","1/cm");
        CalculateDampening(TwoMHzList,cylinderLength).AddCommandAndLog("Dampening2MHz","1/cm");
    }

    public static ErDouble CalculateDampening(List<AbsorbtionData> dataList, ErDouble length)
    {
        return -ErDouble.Log(dataList[0].Damped / dataList[0].Self) / (length/10);
    }
}