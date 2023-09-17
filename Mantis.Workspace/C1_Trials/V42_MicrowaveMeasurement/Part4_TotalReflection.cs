using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.QuickTable;
using Mantis.Core.ScottPlotUtility;
using Mantis.Core.Utility;
using Mantis.Workspace.C1_Trials.Utility;
using ScottPlot;

namespace Mantis.Workspace.C1_Trials.V42_Microwaves_Measurement;

[QuickTable("","tab:totalReflection")]
public record struct TotalReflectionData
{
    [QuickTableField("width","cm")]
    public ErDouble Width = 0;

    [QuickTableField("voltageTransmitted", "V")]
    public ErDouble VoltageTransmitted = 0;

    [QuickTableField("voltageReflected", "V")]
    public ErDouble VoltageReflected = 0;
    
    public TotalReflectionData(){}
}

public static class Part4_TotalReflection
{
    public static void Process()
    {
        var reader = new SimpleTableProtocolReader("TotalReflection.csv");

        var voltmeterRange = reader.ExtractSingleValue<double>("voltmeterRange");
        var errorWidth = reader.ExtractSingleValue<double>("error_width");

        List<TotalReflectionData> dataList = reader.ExtractTable<TotalReflectionData>("tab:totalReflection");
        
        dataList.ForEachRef(((ref TotalReflectionData data) => CalculateErrors(ref data,errorWidth,voltmeterRange)));

        RegModel<ExpFunc> transmittedExpModel = dataList.CreateRegModel(e => (e.Width, e.VoltageTransmitted),
            new ParaFunc<ExpFunc>(2));

        transmittedExpModel.DoRegressionLevenbergMarquardtWithXErrors(new double[] {1, -1},5);

        RegModel<ExpFunc> reflectedExpModel = dataList.CreateRegModel(e => (e.Width, e.VoltageReflected),
            new ParaFunc<ExpFunc>(2));

        reflectedExpModel.DoRegressionLevenbergMarquardtWithXErrors(new double[] {1, -1},5);

        var plt = ScottPlotExtensions.CreateSciPlot("Width in cm", "Voltage in V");

        plt.AddRegModel(transmittedExpModel, "Transmitted Voltage", "Exp Fit transmitted", logY: true);
        plt.AddRegModel(reflectedExpModel, "ReflectedVoltage", "Exp Fit Reflected", logY: true);
        
        plt.YAxis.SetLabelsToLog();

        plt.Legend(true, Alignment.LowerCenter);
        
        plt.SaveAndAddCommand("fig:TotalReflection","caption");




    }

    private static void CalculateErrors(ref TotalReflectionData element, double errorWidth, double voltmeterRange)
    {
        element.Width.Error = errorWidth;
        element.VoltageReflected.CalculateDeviceError(Devices.Aglient34405, DataTypes.VoltageDC,
            voltmeterRange);

        element.VoltageReflected += 0.121;

        element.VoltageTransmitted.CalculateDeviceError(Devices.Aglient34405, DataTypes.VoltageDC, voltmeterRange);

        element.VoltageTransmitted += 0.121;
    }
}