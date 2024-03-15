using Mantis.Core.Calculator;
using Mantis.Core.QuickTable;
using Mantis.Core.ScottPlotUtility;
using Mantis.Core.TexIntegration;
using MathNet.Numerics.LinearAlgebra;
using Microsoft.VisualBasic;
using Constants = MathNet.Numerics.Constants;

namespace Mantis.Workspace.C1_Trials.V40_Polarisation;

[QuickTable]
public record struct LocalBFieldData
{
    public ErDouble BField;
    public ErDouble X;

    [UseConstructorForParsing]
    public LocalBFieldData(double bField, string x)
    {
        BField = new ErDouble(bField,FaradayEffect.ErrorBField);
        X = ErDouble.ParseWithErrorLastDigit(x,null,0.1);
        X -= 841.08;
    }
}

public class TwoCoilsBFieldFunc : AutoDerivativeFunc,IFixedParameterCount
{
    private double current;

    public TwoCoilsBFieldFunc(double current)
    {
        this.current = current;
    }

    public override double CalculateResult(Vector<double> p, double x)
    {
        var rsq = p[1] * p[1];
        
        var c = FieldOfCoil(x, p[3] + p[2], -p[4], rsq);
        var c2 = FieldOfCoil(x, p[3] - p[2], p[4], rsq);
        
        return  p[0]*current * MathNet.Numerics.Constants.MagneticPermeability * (c - c2) * 0.5*Constants.Mega;

        // x -= p[4];
        // return p[0] + p[1] * (Math.Pow(p[2] + Math.Pow(x - p[3] * 0.5, 2), -1.5) +
        // Math.Pow(p[2] + Math.Pow(x + p[3] * 0.5, 2), -1.5));

    }

    private double FieldOfCoil(double x, double a, double d, double r)
    {
        // var xa = x + a;
        // return Math.Pow(r + xa * xa, -1.5);
        var ax = x + 0.5 * a + 0.5 * d;
        var adx = x + 0.5*a - 0.5 *d;
        var s = -ax * Math.Sqrt(r + ax * ax) + adx / Math.Sqrt(r + adx * adx);
        return s ;
    }
    
    public int ParameterCount => 5;
}

public static class FaradayEffect
{
    public static double ErrorBField;
    
    public static void Process()
    {
        var reader = V40_PolarisationMain.Reader;
        ErrorBField = reader.ExtractSingleValue<double>("val:errorBField");
        
        CalculateMeanBField();
    }

    private static void CalculateMeanBField()
    {
        var reader = V40_PolarisationMain.Reader;

        List<LocalBFieldData> fieldData = reader.ExtractTable<LocalBFieldData>("tab:localBField");
        var current = reader.ExtractSingleValue<double>("val:currentForLocalField");

        RegModel model = fieldData.CreateRegModel(
            e => (e.X, e.BField), new ParaFunc(5, new TwoCoilsBFieldFunc(current)));

        model.DoRegressionLevenbergMarquardt(new double[] { 100, 5, 10, 0,10},false);
        
        model.AddParametersToPreambleAndLog("fyLocalBField");

        var plot = new DynPlot("x in mm", "B-field in mT");

        plot.AddRegModel(model, "With hall sensor measured data",
            "Fit of magnet fields of two coils");
        
        plot.SaveAndAddCommand("fyLocalBField");

    }
}