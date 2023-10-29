using System.Reflection.Emit;
using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.QuickTable;
using Mantis.Core.ScottPlotUtility;
using Mantis.Core.TexIntegration;
using Mantis.Core.Utility;
using Mantis.Workspace.C1_Trials.Utility;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using ScottPlot;

namespace Mantis.Workspace.C1_Trials.V42_Microwaves_Measurement;

[QuickTable("","tab:Polarisation")]
public record struct PolarisationData
{
    [QuickTableField("angle", "\\degree")] public ErDouble Angle = 0;
    [QuickTableField("voltage", "V",lastDigitError:1)] public ErDouble Voltage = 0;

    public PolarisationData()
    {
    }
}

/// <summary>
/// f(x) = A + B * Sin( x * PI/180)
/// </summary>
public class Sin4ExpFunc : AutoDerivativeFunc,IFixedParameterCount
{
    
    public override double CalculateResult(Vector<double> p, double x)
    {
        double nx =  x * Constants.Degree;
        double y = Math.Cos(nx);
        return p[0] + p[1] * Math.Pow(y,4*p[2]);
    }

    public int ParameterCount => 3;
}

public static class Part5_Polarisation
{
    public static void Process()
    {
        var reader = new SimpleTableProtocolReader("Part5_Polarisation.csv");

        var errorAngle = reader.ExtractSingleValue<double>("error_angle");
        var rangeVoltmeter = reader.ExtractSingleValue<double>("rangeVoltmeter");
        var voltageOffset = reader.ExtractSingleValue<double>("voltageOffset");

        List<PolarisationData> dataList = reader.ExtractTable<PolarisationData>("tab:Polarisation");
        
        dataList.ForEachRef((ref PolarisationData data) =>
        {
            data.Angle -= 90; // shift by 90°
            data.Angle.Error = errorAngle;
            data.Voltage -= voltageOffset;
            //data.Voltage.CalculateDeviceError(Devices.Aglient34405,DataTypes.VoltageDC,rangeVoltmeter);
        });

        // RegModel<Sin4ExpFunc> model = dataList.CreateRegModel(e => (e.Angle, e.Voltage),
        //     new ParaFunc<Sin4ExpFunc>(4)
        //     {
        //         Labels = new[]{"U0","Ur","a"},
        //         Units = new []{"V","V",""}
        //     });
        //
        // model.DoRegressionLevenbergMarquardt(new double[] {0, 1,0,1});
        //
        // model.AddParametersToPreambleAndLog("PolarisationFit");
        // model.AddGoodnessOfFitToPreamble("PolarisationFit");

        string phi = "\u03d5";
        var plt = ScottPlotExtensions.CreateSciPlot($"Winkel {phi} in °","Spannung U in V");//"Angle phi in °", "Voltage U in V");

        var (erBars, _) = plt.AddErrorBars(dataList.Select(e => (e.Angle, e.Voltage)),
            label: "Signal des\nEmpfängers", errorBars: true);//label: "Output voltage\nof receiver",errorBars:true);
        var max = dataList.Max(e => e.Voltage.Value);
        Func<double, double?> cos4 = x => max * Math.Pow(Math.Cos(x * Constants.Degree), 4);
        var funcPlt = plt.AddFunction(cos4);
        funcPlt.Label = $"Theoretischer\nVerlauf:\ncos^4({phi})";//"Theoretical\ncos^4(x)";

        // plt.AddFunction(model.ParaFunction, label: "U0 + Ur * cos(phi)^(4a)");
        
        //plt.AddRegModel(model, "Output voltage of receiver", "Fit U = a + b cos^4(phi)",errorBars:false);
        plt.Legend(true, Alignment.UpperLeft);
        
        plt.SaveAndAddCommand("fig:Polarisation");
    }
}