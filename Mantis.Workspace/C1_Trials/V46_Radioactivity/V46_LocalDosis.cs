using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.QuickTable;
using Mantis.Core.TexIntegration;
using Mantis.Core.Utility;
using ScottPlot.Finance;

namespace Mantis.Workspace.C1_Trials.V46_Radioactivity;

[QuickTable("")]
public record struct NearFarData
{
    [QuickTableField("near")] public ErDouble Near;
    [QuickTableField("far")] public ErDouble Far;
    
    public NearFarData()
    {}
}
public class V46_LocalDosis
{
    public static void Process()
    {
        var csvReader = new SimpleTableProtocolReader("LocalDosisData.csv");
        List<ErDouble> dataListNear = csvReader.ExtractTable<ErDouble>("tab:LocalDosisData");
        List<ErDouble> dataListFar = csvReader.ExtractTable<ErDouble>("tab:1mDosisData");
        
        dataListNear.WeightedMean().AddCommandAndLog("RadiationNear" ,"ySv/h");
    }

    public static ErDouble CalculateDosis(List<ErDouble> dataList)
    {
        dataList.WeightedMean()
    }
}