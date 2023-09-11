using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.QuickTable;
using Mantis.Core.ScottPlotUtility;
using Mantis.Core.Utility;
using Mantis.Workspace.C1_Trials.Utility;
using ScottPlot.Renderable;

namespace Mantis.Workspace.C1_Trials.V42_Microwaves_Measurement;

[QuickTable("","tab:angleDispersion")]
public record struct AngleDispersionData
{
    [QuickTableField("angle", "\\degree")] public ErDouble Angle;

    [QuickTableField("diodeVoltage", "V")] public ErDouble VoltageDiode;
    
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

        var plt = ScottPlotExtensions.CreateSciPlot("DiodeVoltage V", "",relHeight:1);
        
        dataList.ForEach(e => Console.WriteLine(e));
        
        double[] angleRadians = dataList.Select(e => (e.Angle.Value + 45.0) * Math.PI / 180.0).ToArray();
        double[] voltage = dataList.Select(e => e.VoltageDiode.Value).ToArray();

        for (int i = 0; i < angleRadians.Length; i++)
        {
            Console.WriteLine($"angle: {angleRadians[i]}");
        }

        var (xs, ys) = ScottPlot.Tools.ConvertPolarCoordinates(voltage, angleRadians);
        

        plt.AddScatter(xs, ys);

        plt.AddRadar(new double[,] {{1.5,1.5} }, false,disableFrameAndGrid:false);
        
        plt.Grid(false);
        
        plt.SetAxisLimits(0,1.5,0,1.5);
        plt.SaveAndAddCommand("fig:AngleDispersionRadial","");
        
    }

    private static void CalculateErrors(ref AngleDispersionData data, double voltmeterRange, double errorAngle)
    {
        data.Angle.Error = errorAngle;

        data.VoltageDiode = DeviceErrorsUtil.CalculateError(Devices.Aglient34405, DataTypes.VoltageDC, voltmeterRange,
            data.VoltageDiode.Value);
    }
}