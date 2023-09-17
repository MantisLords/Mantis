using System.Diagnostics;
using System.Reflection.Metadata;
using Mantis.Core.Calculator;
using Mantis.Core.QuickTable;
using Mantis.Core.ScottPlotUtility;
using Mantis.Core.TexIntegration;
using Mantis.Core.Utility;
using Mantis.Workspace.BasicTests;
using Mantis.Workspace.C1_Trials.Utility;
using Microsoft.VisualBasic;
using ScottPlot.Drawing.Colormaps;

namespace Mantis.Workspace.C1_Trials.V41_EMWaweSpeed;

public record struct OsziPulseData
{
    public ErDouble time;
    public ErDouble voltage;
}//OsziData anscheinend schon da

public static class V41_WaveSpeed_Main
{
    public static void Process()
    {
        Console.WriteLine("Ronny");
        var osziCsvReader = new OsziRowWiseCsvReader("Data\\F005CH1.csv");
        List < OsziData > dataList= osziCsvReader.Data.ToList();
        Console.WriteLine(dataList[0].Time);
        RegModel<LineFunc> model = dataList.CreateRegModel(e => (e.Time, e.Voltage),
            new ParaFunc<LineFunc>(2)
            {
                Units = new[] { "Gradient", "Voltage" }
            }
        );
        model.DoLinearRegression(true);
        model.AddParametersToPreambleAndLog("VoltagePulseLineFit");
       ScottPlot.Plot plot = ScottPlotExtensions.CreateSciPlot("time in s", "Voltage in V");
       plot.AddRegModel(model, "daten", "fit");
       plot.SaveAndAddCommand("fig:VoltagePulse","caption");
    }

    private static void CalculateErrors(ref OsziData data)
    {
        double osziTimeError = 0.1;
        double osziVoltageError = 0.1;
        
    }

    public static ErDouble CalculateVelocityRuntime(double time, ErDouble length)
    {
        return 2 * length / time;
    }

    public static ErDouble CalculateEpsR(ErDouble v)
    {
        return (3 * Math.Pow(10, 8) / v).Pow(2);
    }
}