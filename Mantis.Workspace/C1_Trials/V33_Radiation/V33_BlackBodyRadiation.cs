using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.QuickTable;
using Mantis.Core.ScottPlotUtility;
using Mantis.Core.TexIntegration;
using Mantis.Core.Utility;

namespace Mantis.Workspace.C1_Trials.V33_Radiation;

[QuickTable("","")]
public record struct TemperatureVoltageData
{
    [QuickTableField("temperature", "")] public ErDouble temperature;
    
    [QuickTableField("voltage", "")] public ErDouble voltage;
    [QuickTableField("CleanVoltage", "")] public ErDouble CleanVoltage;

    public TemperatureVoltageData()
    {
    }
}
public class V33_BlackBodyRadiation
{
    public static void Process()
    {
        var csvReader = new SimpleTableProtocolReader("BlackBodyData.csv");
        List<TemperatureVoltageData> dataList = csvReader.ExtractTable<TemperatureVoltageData>("tab:BlackBodyData");
        double ZeroTemp = csvReader.ExtractSingleValue<double>("temperatureZero");
        dataList.ForEachRef((ref TemperatureVoltageData data) => 
            data.temperature+=273.15);
        
        DoQuattroFit(dataList,ZeroTemp);
        
    }

    public static void DoQuattroFit(List<TemperatureVoltageData> dataList, double ZeroTemp)
    {
        RegModel model = dataList.CreateRegModel(e => (e.temperature, e.voltage),
            new ParaFunc(2, new QuattroFit(ZeroTemp))
            {
                Units = new []{"",""}
            });
        model.DoRegressionLevenbergMarquardt(new double[] { 1, 1}, false);
        //model.DoLinearRegression(false);
        ErDouble fitParameter = model.ErParameters[1];
        fitParameter.Error = 0.2;
        fitParameter.AddCommand("BlackBodyFitParameter");
        DynPlot plot = new DynPlot("Temperature", "Voltage");
        plot.AddDynErrorBar(dataList.Select(e => (e.temperature, e.voltage)));
        plot.AddRegModel(model);
        plot.SaveAndAddCommand("BlackBodyPlot");
    }
}