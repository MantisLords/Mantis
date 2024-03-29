using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.QuickTable;
using Mantis.Core.ScottPlotUtility;
using Mantis.Core.TexIntegration;
using Mantis.Core.Utility;

namespace Mantis.Workspace.C1_Trials.V41_EMWaveSpeed;

[QuickTable("","tab:lightMeasurement")]
public record struct lightSpeedData
{
    [QuickTableField("distance", "\\mm")] public ErDouble distance;

    [QuickTableField("time", "ns")] public ErDouble time;
    
    public lightSpeedData(){}
}

public static class PartC_SpeedOfLight
{
    public static void Process()
    {
        var lightSpeedReader = new SimpleTableProtocolReader("Data\\Measurements");
        List<lightSpeedData> lightSpeedList = lightSpeedReader.ExtractTable<lightSpeedData>();
        lightSpeedList.ForEachRef((ref lightSpeedData e)=>CalculateErrors(ref e,0.5,0.005));//abstandsFehler ist konstant, Zeitfehler war größer für längere Zeiten deshalb 5%
        RegModel model = lightSpeedList.CreateRegModel(e => (e.time, e.distance),
            new ParaFunc(2,new LineFunc())
            {
                Units = new[] { "m/s", "s" }
            });
        model.DoLinearRegressionWithXErrors();
        model.AddParametersToPreambleAndLog("lightSpeedLinearRegression");

        var speedOfLight = model.ErParameters[1].Mul10E(6);
        speedOfLight.AddCommandAndLog("SpeedOfLight");

        DynPlot plot = new DynPlot("time [ns]","distance [mm]");
        plot.AddRegModel(model, "Measured distance-time pairs","Line fitted per gaussian-regression");
        plot.SaveAndAddCommand("regressionPLot");

    }
    

    private static void CalculateErrors(ref lightSpeedData data, double distanceError, double timeError)
    {
        data.distance = data.distance*2;
        data.distance.Error = distanceError;
        data.time.Error = data.time.Value * timeError;
    }
}