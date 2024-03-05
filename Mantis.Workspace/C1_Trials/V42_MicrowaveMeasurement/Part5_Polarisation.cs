using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.QuickTable;
using Mantis.Core.ScottPlotUtility;
using Mantis.Core.Utility;
using Mantis.Workspace.C1_Trials.V42_MicrowaveMeasurement;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using ScottPlot;

namespace Mantis.Workspace.C1_Trials.V42_Microwaves_Measurement;


public static class Part5_Polarisation
{
    public static void Process()
    {
        var reader = new SimpleTableProtocolReader("Part5_Polarisation.csv");

        var errorAngle = reader.ExtractSingleValue<double>("error_angle");
        var voltageOffset = reader.ExtractSingleValue<double>("voltageOffset");

        List<AngleVoltageData> dataList = reader.ExtractTable<AngleVoltageData>("tab:Polarisation");
        
        dataList.ForEachRef((ref AngleVoltageData data) =>
        {
            data.Angle -= 90; // shift by 90°
            data.Angle.Error = errorAngle;
            data.Voltage -= voltageOffset;
        });



        string phi = "\u03d5";
        var plt = new DynPlot($"Winkel {phi} in °","Spannung U in V");

        plt.AddDynErrorBar(dataList.Select(e => ((IErDoubleBase)e.Angle, (IErDoubleBase)e.Voltage)),false, "Signal des Empfängers");//label: "Output voltage\nof receiver",errorBars:true);
        var max = dataList.Max(e => e.Voltage.Value);
        Func<double, double> cos4 = x => max * Math.Pow(Math.Cos(x * Constants.Degree), 4);
        plt.AddDynFunction(cos4,$"Theoretischer Verlauf: cos^4({phi})");

        plt.Legend.Location = Alignment.UpperLeft;
        
        plt.SaveAndAddCommand("fig:Polarisation");
    }
}