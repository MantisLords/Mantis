using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.QuickTable;
using Mantis.Core.TexIntegration;
using Mantis.Core.Utility;
using MathNet.Numerics;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Statistics;

namespace Mantis.Workspace.Fr2.Sheet3_Correlation;

[QuickTable("The measured and averaged environment data of one year by a franconian weather station","tab:EnvironmentData")]
public record struct EnvironmentData
{
    [QuickTableField("Temperature", "°C")] public double Temperature;

    [QuickTableField("Humidity", "g/m^3")] public double Humidity;
    
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
        covariance.AddCommand("TemperatureHumidityCovariance","^{\\circ} C g / m^3");
        
        double correlationCoefficient = data.CorrelationBetween(e => (e.Temperature,e.Humidity));
        correlationCoefficient.AddCommand("TemperatureHumidityCorrelation");

        (ErDouble offset, ErDouble slope) = data.LinearRegressionLine(e => (e.Temperature, e.Humidity),RegressionCommand.IgnoreYErrors);
        offset.AddCommand("RegressionOffset"," g / m^3");
        slope.AddCommand("RegressionSlope","g / m^3 / ^{\\circ} C");


        Console.WriteLine($"Covariance: {covariance} Population Correlation: {correlationCoefficient}");
        Console.WriteLine($"offset {offset.ToString()} slope {slope.ToString()}");

        Sketchbook regressionPlot = new Sketchbook(
            axis: new AxisLayout( "Temperature in $ ^{\\circ} C$","Humidity in $g/m^3$"),
            label: "fig:EnvironmentRegression",
            caption: "Environment data of one year by a franconian weather station Thomas Karb 11.5.23");

        
        regressionPlot.Add(new DataMarkSketch()
        {
            Data = data.Select(e => ((ErDouble) e.Temperature,(ErDouble) e.Humidity)),
            Legend = "Measured and averaged weather data"
        });
        
        regressionPlot.Add(new StraightPlot()
        {
            Slope = slope.Value,
            YZero = offset.Value,
            Legend = "Best fit"
        });
        
        regressionPlot.SaveLabeled();
        
        TexPreamble.GeneratePreamble();
    }
}