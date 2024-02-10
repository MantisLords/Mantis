using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.QuickTable;
using Mantis.Core.ScottPlotUtility;
using Mantis.Core.TexIntegration;
using Mantis.Core.Utility;
using MathNet.Numerics;
using MathNet.Numerics.Integration;
using MathNet.Numerics.LinearAlgebra;
using ScottPlot;

namespace Mantis.Workspace.CondensedMatter1.Exercise2_DebyeModel;


[QuickTable("","")]
public record struct HeatCapacityData
{
    [QuickTableField("T", "K")] public double Temperature;
    [QuickTableField("Cp", "J/K/kg")] public double MassSpecificHeatCapacity;

    [QuickTableField("Cv", "J/K atom", doesImport: false)]
    public double HeatCapacity;

    public HeatCapacityData(){}
}

public class LowTT3Func : AutoDerivativeFunc, IFixedParameterCount
{
    public const double R = 8.31446261815324;
    public const double Factor = 12.0 / 5.0 * Math.PI * Math.PI * Math.PI * Math.PI * R;
    
    public override double CalculateResult(Vector<double> parameters, double x)
    {
        double v = x / parameters[0];
        return Factor * v * v * v;
    }

    public int ParameterCount => 1;
}

public class DebyeModelFunc : AutoDerivativeFunc, IFixedParameterCount
{
    public override double CalculateResult(Vector<double> p, double x)
    {
        return CalcResult(x, p[0]);
    }

    public static double CalcResult(double T, double ThetaD)
    {
        return SimpsonRule.IntegrateComposite((double theta) => IntegralKernel(T, theta, ThetaD), 0.1, ThetaD,16);
    }

    public static double IntegralKernel(double T, double Theta,double ThetaD)
    {
        if (Theta == 0)
            return 0;
        var exp = Math.Exp(Theta / T);
        var expMin = exp - 1;
        return LowTT3Func.R * 9 * exp / expMin / expMin * Theta * Theta * Theta * Theta / T / T / ThetaD / ThetaD / ThetaD;

    }

    public int ParameterCount => 1;
}

public class CM1_Ex2_DebyeModel_Main
{
    public static void Process()
    {
        var reader = new SimpleTableProtocolReader("HeatCapacitySilver.csv");

        double atomicMass = reader.ExtractSingleValue<double>("AtomicMassSilver"); // g / mol
        
        var heatCapacityListLowT = reader.ExtractTable<HeatCapacityData>("tab:HeatCapacitySilverLowT");
        heatCapacityListLowT.ForEachRef((ref HeatCapacityData data) => data.HeatCapacity = data.MassSpecificHeatCapacity * atomicMass * 0.001);

        RegModel<LowTT3Func> lowTModel = heatCapacityListLowT.CreateRegModel(
            e => (new ErDouble(e.Temperature), new ErDouble(e.HeatCapacity)),
            new ParaFunc<LowTT3Func>(1)
            {
                Labels = new string[]{"ThetaD"},
                Units = new string[]{"K"}
            });

        lowTModel.DoRegressionLevenbergMarquardt(new double[] {290}, false);
        lowTModel.AddParametersToPreambleAndLog("LowTModel",LogLevel.OnlyLog);

        Plot(lowTModel,"HeatCapacityLowT");

        var heatCapacityList = reader.ExtractTable<HeatCapacityData>("tab:HeatCapacitySilverFullRange");
        heatCapacityList.ForEachRef((ref HeatCapacityData data) => data.HeatCapacity = data.MassSpecificHeatCapacity * atomicMass * 0.001);
        
        RegModel<DebyeModelFunc> debyeModel = heatCapacityList.CreateRegModel(
            e => (new ErDouble(e.Temperature), new ErDouble(e.HeatCapacity)),
            new ParaFunc<DebyeModelFunc>(1)
            {
                Labels = new string[]{"ThetaD"},
                Units = new string[]{"K"}
            });
        debyeModel.ParaFunction.ParaSet.SetParameters(lowTModel.ErParameters);
        
        debyeModel.DoRegressionLevenbergMarquardt(new double[] {290}, false);
        debyeModel.AddParametersToPreambleAndLog("DebyeModel",LogLevel.OnlyLog);

        var thetaD = lowTModel.ErParameters[0].Value;

        for (int t = 0; t < 340; t+=20)
        {
            Console.WriteLine($"T: {t} Cv: {debyeModel.ParaFunction.EvaluateAtDouble(t)}" +
                              $"IK: {DebyeModelFunc.IntegralKernel(t,0,thetaD)} {DebyeModelFunc.IntegralKernel(t,thetaD,thetaD)}");
        }
        
        Plot(debyeModel,"HeatCapacityFullRange");
        
    }

    private static void Plot<T>(RegModel<T> model, string name) where T : FuncCore,new()
    {
        var plot = new DynPlot("T in K", "Cv in J/K");
        
        var errorBar = plot.AddDynErrorBar(model.Data);

        errorBar.MarkerStyle.Shape = MarkerShape.FilledSquare;
        errorBar.MarkerStyle.Size = 1;

        plot.AddDynFunction(model.ParaFunction);

        plot.SaveFigHere(name, 500, 400, ImageFormat.Svg);
    }
    
    
}