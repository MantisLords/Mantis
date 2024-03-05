using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.QuickTable;
using Mantis.Core.ScottPlotUtility;
using Mantis.Core.TexIntegration;
using Mantis.Core.Utility;
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

        var errorWidth = reader.ExtractSingleValue<double>("error_width");
        var voltageOffset = reader.ExtractSingleValue<double>("voltageOffset");

        List<TotalReflectionData> dataList = reader.ExtractTable<TotalReflectionData>("tab:totalReflection");

        double maxWidth = dataList.Max(e => e.Width.Value);
        dataList.ForEachRef(((ref TotalReflectionData data) => CalculateErrors(ref data,errorWidth,voltageOffset,maxWidth)));

        
        RegModel transmittedExpModel = dataList.Where(e => !e.VoltageTransmitted.Value.AlmostEqual(-voltageOffset)).CreateRegModel(e => (e.Width, e.VoltageTransmitted),
            new ParaFunc(3,new ExpFunc()));

        transmittedExpModel.DoRegressionLevenbergMarquardtWithXErrors(new double[] {1, -1},5);
        transmittedExpModel.AddParametersToPreambleAndLog("TransmittedExpModel",LogLevel.OnlyLog);
        transmittedExpModel.GetGoodnessOfFitLog().AddCommandAndLog("TransmittedExpModel",LogLevel.OnlyLog);
        
        RegModel reflectedExpModel = dataList.Where(e => !e.VoltageReflected.Value.AlmostEqual(-voltageOffset)).CreateRegModel(e => (e.Width, e.VoltageReflected),
            new ParaFunc(3,new ShiftExpFunc()));
  
        
        
        Console.WriteLine($"Count Trans {transmittedExpModel.Data.Count} Reflect {reflectedExpModel.Data.Count}");

        reflectedExpModel.DoRegressionLevenbergMarquardtWithXErrors(new double[] {0.3,-1, -1},5);
        reflectedExpModel.AddParametersToPreambleAndLog("ReflectedExpModel",LogLevel.OnlyCommand);

        var plt = new DynPlot("Spaltabstand x in cm","Spannung in V");//"Width x in cm", "Voltage U in V");
        plt.DynAxes.LogY();

        plt.AddRegModel(transmittedExpModel, "Transmittiertes Signal", "Fit: A + exp(Bx)");//"Transmitted signal", "Fit: A + exp(Bx)", logY: true);
        var (errorBar, _) =
            plt.AddRegModel(reflectedExpModel, "Reflektiertes Signal", "Fit: A + B exp(Cx)");//"Reflected signal", "Fit: A + B exp(Cx)", logY: true);
        errorBar.MarkerStyle.Shape = MarkerShape.OpenDiamond;

        plt.Legend.Location = Alignment.LowerRight;
        
        plt.SaveAndAddCommand("fig:TotalReflection");

    }

    private static void CalculateErrors(ref TotalReflectionData element, double errorWidth,double voltageOffset,double maxWidth)
    {
        element.Width = maxWidth - element.Width;
        
        element.Width.Error = errorWidth;
        element.VoltageReflected -= voltageOffset;
        element.VoltageTransmitted -= voltageOffset;
    }
}