using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.QuickTable;
using Mantis.Core.ScottPlotUtility;
using Mantis.Core.Utility;

namespace Mantis.Workspace.C1_Trials.V33_Radiation;
[QuickTable("", "LeslieData")]
public record struct TempRadiationData
{
    [QuickTableField("temperature", "C")] public ErDouble temperature;
    [QuickTableField("polished","")] public ErDouble polished;
    [QuickTableField("white", "")] public ErDouble white;
    [QuickTableField("matt", "")] public ErDouble matt;
    [QuickTableField("black", "")] public ErDouble black;
        
    public TempRadiationData()
    {
    }
}

public class V33_Leslie_Cube
{
    public static void Process()
    {

        var csvReader = new SimpleTableProtocolReader("Data/LeslieCubeData.csv");
        List<TempRadiationData> dataList = csvReader.ExtractTable<TempRadiationData>("tab:LeslieCubeData");
        dataList.ForEachRef(((ref TempRadiationData data) =>
                data.temperature.Error = 0.01));
        GenerateFirstPlot(dataList);
    
    }
    public static void GenerateFirstPlot(List<TempRadiationData> dataList)
    {
            DynPlot plot = new DynPlot("temp", "Radiation");
            plot.AddDynErrorBar(dataList.Select(e => (e.temperature, e.polished)));
            plot.AddDynErrorBar(dataList.Select(e => (e.temperature, e.white)));
            plot.AddDynErrorBar(dataList.Select(e => (e.temperature, e.matt)));
            plot.AddDynErrorBar(dataList.Select(e => (e.temperature, e.black)));
            plot.SaveAndAddCommand("StefanBolzmannPlot");
    }
}