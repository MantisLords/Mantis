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
    [QuickTableField("temperature", "")] public ErDouble Temperature;
    
    [QuickTableField("voltage", "")] public ErDouble Voltage;
    [QuickTableField("CleanVoltage", "")] public ErDouble CleanVoltage;
    

    [UseConstructorForParsing]
    public TemperatureVoltageData(string temperature, string voltage, ErDouble CleanVoltage)
    {
        this.Temperature = ErDouble.ParseWithErrorLastDigit(temperature,null,0.2);
        Temperature += 273;
        this.Voltage = ErDouble.ParseWithErrorLastDigit(voltage,null,0.000025);
        
        this.CleanVoltage = CleanVoltage;
    }
}
public class V33_BlackBodyRadiation
{
    public static void Process()
    {
        var csvReader = new SimpleTableProtocolReader("BlackBodyData.csv");
        List<TemperatureVoltageData> dataList = csvReader.ExtractTable<TemperatureVoltageData>("tab:BlackBodyData");
        double ZeroTemp = csvReader.ExtractSingleValue<double>("temperatureZero");
        
        Console.WriteLine(dataList[^1]);

        DoQuattroFit(dataList,ZeroTemp);
        
    }

    public static void DoQuattroFit(List<TemperatureVoltageData> dataList, double ZeroTemp)
    {
        RegModel model = dataList.CreateRegModel(e => (temperature: e.Temperature, voltage: e.Voltage),
            new ParaFunc(3, new QuattroFitPlusConstant(ZeroTemp))
            {
                Units = new []{"","",""}
            });
        model.DoRegressionLevenbergMarquardt(new double[] { 1, 1,1}, false);
        //model.DoLinearRegression(false);
        model.AddParametersToPreambleAndLog("Quatro",LogLevel.OnlyLog);
        ErDouble fitParameter = model.ErParameters[1];
        fitParameter.Error = 0.2;
        fitParameter.AddCommand("BlackBodyFitParameter");
        DynPlot plot = new DynPlot("Temperature", "Voltage");
        plot.AddDynErrorBar(dataList.Select(e => (temperature: e.Temperature, voltage: e.Voltage)));
        plot.AddRegModel(model);
        plot.SaveAndAddCommand("BlackBodyPlot");
    }
}