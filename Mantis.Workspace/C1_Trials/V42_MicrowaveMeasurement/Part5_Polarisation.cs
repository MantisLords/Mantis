using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.QuickTable;
using Mantis.Core.ScottPlotUtility;
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
    [QuickTableField("voltage", "V")] public ErDouble Voltage = 0;

    public PolarisationData()
    {
    }
}

/// <summary>
/// f(x) = A + B * Sin( x * PI/180)
/// </summary>
public class Sin4Func : AutoDerivativeFunc,IFixedParameterCount
{
    
    public override double CalculateResult(Vector<double> p, double x)
    {
        return p[0] + p[1] * Math.Pow(Math.Sin(x * Constants.Degree), 4);
    }

    public int ParameterCount => 2;
}

public static class Part5_Polarisation
{
    public static void Process()
    {
        var reader = new SimpleTableProtocolReader("Polarisation.csv");

        var errorAngle = reader.ExtractSingleValue<double>("error_angle");
        var rangeVoltmeter = reader.ExtractSingleValue<double>("rangeVoltmeter");

        List<PolarisationData> dataList = reader.ExtractTable<PolarisationData>("tab:Polarisation");
        
        dataList.ForEachRef((ref PolarisationData data) =>
        {
            data.Angle.Error = errorAngle;
            data.Voltage.CalculateDeviceError(Devices.Aglient34405,DataTypes.VoltageDC,rangeVoltmeter);
        });

        RegModel<Sin4Func> model = dataList.CreateRegModel(e => (e.Angle, e.Voltage),
            new ParaFunc<Sin4Func>(2)
            {
                Units = new []{"V","V"}
            });

        model.DoRegressionLevenbergMarquardtWithXErrors(new double[] {0, 1}, 5);

        var plt = ScottPlotExtensions.CreateSciPlot("Angle in °", "Voltage V");

        plt.AddRegModel(model, "Received Intensity", "Malus Fit");
        plt.Legend(true, Alignment.UpperRight);
        
        plt.SaveAndAddCommand("fig:Polarisation","caption");
    }
}