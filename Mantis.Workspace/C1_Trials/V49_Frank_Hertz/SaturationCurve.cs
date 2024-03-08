using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.ScottPlotUtility;
using Mantis.Core.TexIntegration;
using MathNet.Numerics.LinearAlgebra;
using Microsoft.VisualBasic;
using ScottPlot;

namespace Mantis.Workspace.C1_Trials.V49_Frank_Hertz.Data_Smailagic_Karb;

public class RichardsonFunc : AutoDerivativeFunc, IFixedParameterCount
{
    private const double Kb = 0.0000861733262;
    
    public override double CalculateResult(Vector<double> p, double x)
    {
        double t = VoltageToTemp(x);
        return p[0] * t * t * Math.Exp(-p[1] / Kb / t) ;
    }

    public static double VoltageToTemp(double u)
        => 1616 + 97 * u;

    public int ParameterCount => 2;
}

public static class SaturationCurve
{
    public static void Process()
    {
        var reader = V49_Frank_Hertz_Main.Reader;

        var anodeVoltage =
            reader.ExtractSingleValue<ErDouble>("val:saturationAnodeVoltage", V49_Utility.ParseWithErrorOnLastDigit);
        anodeVoltage.AddCommandAndLog("saturationAnodeVoltage");


        List<VoltageData> saturationData = reader.ExtractTable<VoltageData>("tab:saturationCurve",V49_Utility.VoltageDataLastDigitErrorParser);

        RegModel richardsonModel = saturationData.CreateRegModel(e => (e.Voltage, e.Current),
            new ParaFunc(4, new RichardsonFunc())
            {
                Labels = new[] {"A", "Wa",""},
                Units = new[] {"", "eV",""}
            });

        richardsonModel.DoRegressionLevenbergMarquardt(new double[] {1, 1},false);
        richardsonModel.AddParametersToPreambleAndLog("richardsonModel");


        DynPlot plot = new DynPlot("Heating voltage in v", "Anode current in mA");

        plot.AddRegModel(richardsonModel, $"Saturation curve for acceleration Ua = {anodeVoltage} V", 
            $"Fit of the richardson model Wa = {richardsonModel.ErParameters[1]} eV");

        var idealFunc = richardsonModel.ParaFunction.Fork();
        var original = idealFunc.ErParameters;
        original[1] = 4.55;

        idealFunc.ParaSet.SetParameters(original);
        
        Console.WriteLine(idealFunc.ParaSet);

        plot.AddDynFunction(idealFunc, "Ideal curve");

        plot.Legend.Location = Alignment.UpperLeft;
        plot.SaveAndAddCommand("richardsonPlot");



    }
}