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

[QuickTable("","tab:totalReflection")]
public record struct TotalReflectionData
{
    [QuickTableField("width","cm")]
    public ErDouble Width = 0;

    [QuickTableField("voltageTransmitted", "V",lastDigitError:1)]
    public ErDouble VoltageTransmitted = 0;

    [QuickTableField("voltageReflected", "V",lastDigitError:1)]
    public ErDouble VoltageReflected = 0;
    
    public TotalReflectionData(){}
}

public class ShiftExpFunc : AutoDerivativeFunc,IFixedParameterCount
{
    public override double CalculateResult(Vector<double> p, double x)
    {
        return p[0] + p[1] * Math.Exp(p[2] * x);
    }

    public int ParameterCount => 3;
}

public static class Part4_TotalReflection
{
    public static void Process()
    {
        var reader = new SimpleTableProtocolReader("Part4_TotalReflection.csv");

        var voltmeterRange = reader.ExtractSingleValue<double>("voltmeterRange");
        var errorWidth = reader.ExtractSingleValue<double>("error_width");
        var voltageOffset = reader.ExtractSingleValue<double>("voltageOffset");

        List<TotalReflectionData> dataList = reader.ExtractTable<TotalReflectionData>("tab:totalReflection");

        double maxWidth = dataList.Max(e => e.Width.Value);
        dataList.ForEachRef(((ref TotalReflectionData data) => CalculateErrors(ref data,errorWidth,voltmeterRange,voltageOffset,maxWidth)));

        
        RegModel<ExpFunc> transmittedExpModel = dataList.Where(e => !e.VoltageTransmitted.Value.AlmostEqual(-voltageOffset)).CreateRegModel(e => (e.Width, e.VoltageTransmitted),
            new ParaFunc<ExpFunc>(3));

        transmittedExpModel.DoRegressionLevenbergMarquardtWithXErrors(new double[] {1, -1},5);
        transmittedExpModel.LogParameters("TransmittedExpModel");
        
        RegModel<ShiftExpFunc> reflectedExpModel = dataList.Where(e => !e.VoltageReflected.Value.AlmostEqual(-voltageOffset)).CreateRegModel(e => (e.Width, e.VoltageReflected),
            new ParaFunc<ShiftExpFunc>(3));
        
        Console.WriteLine($"Count Trans {transmittedExpModel.Data.Count} Reflect {reflectedExpModel.Data.Count}");

        reflectedExpModel.DoRegressionLevenbergMarquardtWithXErrors(new double[] {0.3,-1, -1},5);
        reflectedExpModel.LogParameters("ReflectedExpModel");
        
        var plt = ScottPlotExtensions.CreateSciPlot("Width x in cm", "Voltage U in V");

        plt.AddRegModel(transmittedExpModel, "Transmitted signal", "Fit: A + exp(Bx)", logY: true);
        var(errorBar,scatterPlot,_)=plt.AddRegModel(reflectedExpModel, "Reflected signal", "Fit: A + B exp(Cx)", logY: true);
        errorBar.Color = plt.Palette.GetColor(0);
        scatterPlot.Color = plt.Palette.GetColor(0);
        scatterPlot.MarkerShape = MarkerShape.openDiamond;
        
        plt.YAxis.SetLabelsToLog();

        plt.Legend(true, Alignment.LowerRight);
        plt.Grid(true);
        
        plt.SaveAndAddCommand("fig:TotalReflection");




    }

    private static void CalculateErrors(ref TotalReflectionData element, double errorWidth, double voltmeterRange,double voltageOffset,double maxWidth)
    {
        element.Width = maxWidth - element.Width;
        
        element.Width.Error = errorWidth;
        // element.VoltageReflected.CalculateDeviceError(Devices.Aglient34405, DataTypes.VoltageDC,
        //     voltmeterRange);

        element.VoltageReflected -= voltageOffset;
        
        // element.VoltageTransmitted.CalculateDeviceError(Devices.Aglient34405, DataTypes.VoltageDC, voltmeterRange);

        element.VoltageTransmitted -= voltageOffset;
    }
}