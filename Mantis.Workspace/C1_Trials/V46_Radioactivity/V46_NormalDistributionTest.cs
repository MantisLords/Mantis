using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.QuickTable;

namespace Mantis.Workspace.C1_Trials.V46_Radioactivity;
public class V46_NormalDistributionTest
{
    public static void Process()
    {
        var csvReader = new SimpleTableProtocolReader("10secGateMeasurement");
        List < CountData > dataList = csvReader.ExtractTable<CountData>("tab:10secMeasurement");
        

    }
}