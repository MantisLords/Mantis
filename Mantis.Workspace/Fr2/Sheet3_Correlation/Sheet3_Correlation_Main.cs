using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.QuickTable;
using Mantis.Core.ScottPlotUtility;
using Mantis.Core.TexIntegration;
using Mantis.Core.Utility;
using MathNet.Numerics;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Statistics;

namespace Mantis.Workspace.Fr2.Sheet3_Correlation;

[QuickTable("The measured and averaged environment data of one year by a franconian weather station","tab:EnvironmentData")]
public record struct EnvironmentData
{
    [QuickTableField("Temperature", "°C")] public double Temperature = 0;

    [QuickTableField("Humidity", "g/m^3")] public double Humidity = 0;
    
    public EnvironmentData(){}
    
}

public static class Sheet3_Correlation_Main
{
    public static void Process()
    {
        var reader = new SimpleTableProtocolReader("EnvironmentData");
        List<EnvironmentData> data = reader.ExtractTable<EnvironmentData>();
        
        data.CreateTexTable().SaveLabeled();
        

        double covariance = data.CovarianceBetween(e => (e.Temperature, e.Humidity), true);
        covariance.AddCommandAndLog("TemperatureHumidityCovariance","^{\\circ} C g / m^3");
        
        double correlationCoefficient = data.CorrelationBetween(e => (e.Temperature,e.Humidity));
        correlationCoefficient.AddCommandAndLog("TemperatureHumidityCorrelation");

        RegModel<LineFunc> model = data.CreateRegModel(e => (e.Temperature, e.Humidity),
            new ParaFunc<LineFunc>(2)
            {
                Labels = new[] { "Offset", "Slope" },
                Units = new[] { "g / m^3", "g / m^3 / ^{\\circ} C" }
            });
        
        model.DoLinearRegression(false);
        
        model.AddParametersToPreambleAndLog("Regression");

        
        var plt = ScottPlotExtensions.CreateSciPlot("Temperature in °C", "Humidity in g/m^3");
        plt.AddRegModel(model, labelData: "Measured and averaged wather data", labelFunction: "Best fit");
        plt.SaveAndAddCommand("fig:EnvironmentRegression","Environment data of one year by a franconian weather station Thomas Karb 11.5.23");
        
        
        TexPreamble.GeneratePreamble();
    }
}