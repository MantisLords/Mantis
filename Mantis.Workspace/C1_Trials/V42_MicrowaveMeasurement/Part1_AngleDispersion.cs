using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.QuickTable;
using Mantis.Core.ScottPlotUtility;
using Mantis.Core.TexIntegration;
using Mantis.Core.Utility;
using Mantis.Workspace.C1_Trials.Utility;
using MathNet.Numerics;
using ScottPlot;
using ScottPlot.Renderable;

namespace Mantis.Workspace.C1_Trials.V42_Microwaves_Measurement;

[QuickTable("","tab:angleDispersion")]
public record struct AngleDispersionData
{
    [QuickTableField("angle", "\\degree")] public ErDouble Angle = 0;

    [QuickTableField("diodeVoltage", "V")] public ErDouble VoltageDiode = 0;
    
    public AngleDispersionData(){}
}

public static class Part1_AngleDispersion
{
    public static void Process()
    {
        var csvReader = new SimpleTableProtocolReader("AngleDispersion.csv");
        List<AngleDispersionData> dataList = csvReader.ExtractTable<AngleDispersionData>();

        var voltmeterRange = csvReader.ExtractSingleValue<double>("voltmeterRange");
        var errorAngle = csvReader.ExtractSingleValue<double>("error_angle");
        
        dataList.ForEachRef(((ref AngleDispersionData e) => CalculateErrors(ref e,voltmeterRange,errorAngle)));
        
        
        RegModel<GaussFunc> model = dataList.CreateRegModel(e => (e.Angle, e.VoltageDiode),
            new ParaFunc<GaussFunc>(4)
            {
                Units = new[] {"V", "V", "\\degree", "\\degree"}
            });

        model.DoRegressionLevenbergMarquardt(new double[] {0, 1, 0, 1});
        
        model.AddParametersToPreambleAndLog("AngleDispersionGaussFit");
        var fwhm = 2 * Math.Sqrt(2 * Constants.Ln2) * model.ErParameters[3];
        fwhm.AddCommandAndLog("AngleDispersionGaussFWHM","\\degree");

        var plot = ScottPlotExtensions.CreateSciPlot("Angle in °", "Voltage in V");

        plot.AddRegModel(model, "Output of the receiver", "Gauss Fit");

        plot.Legend(true, Alignment.UpperRight);

        plot.SaveAndAddCommand("fig:AngleDispersion","caption");

    }

    private static void CalculateErrors(ref AngleDispersionData data, double voltmeterRange, double errorAngle)
    {
        data.Angle.Error = errorAngle;

        data.VoltageDiode = DeviceErrorsUtil.CalculateError(Devices.Aglient34405, DataTypes.VoltageDC, voltmeterRange,
            data.VoltageDiode.Value);
    }
}