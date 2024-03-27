using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.QuickTable;
using Mantis.Core.TexIntegration;
using Mantis.Workspace.C1_Trials.V46_Radioactivity.Data_Smailagic_Karb;

namespace Mantis.Workspace.C1_Trials.V46_Radioactivity;

[QuickTable()]
public record struct RespData
{
    [QuickTableField("counts")] public ErDouble Counts;
    public RespData(){}
}
public class V46_Responsivity
{
    public static void Process()
    {
        var csvReader = new SimpleTableProtocolReader("Ansprechverhalten.csv");
        List < RespData > countsList = csvReader.ExtractTable<RespData>("tab:ResponsivityData");
        List<double> errorList = countsList.Select(data => data.Counts.Value).ToList();
        ErDouble meanCount = errorList.WeightedMean();
        ErDouble distance = csvReader.ExtractSingleValue<ErDouble>("val:distance");
        ErDouble detectorArea = csvReader.ExtractSingleValue<ErDouble>("val:area");
        //converting area into meters^2
        detectorArea *= Math.Pow(10, -4);
        //converting distance into meters
        distance *= Math.Pow(10, -3);
        ErDouble responsitivity =CalculateResponsivity(meanCount,distance,detectorArea);
        responsitivity.AddCommandAndLog("DetectorResponse");
    }

    public static ErDouble CalculateResponsivity(ErDouble meanCount,ErDouble distance, ErDouble detectorArea)
    {
        return (meanCount * 4 * Math.PI * distance.Pow(2)) / (V46_NullEffectAndActivity._sampleActivity * detectorArea);
    }
}