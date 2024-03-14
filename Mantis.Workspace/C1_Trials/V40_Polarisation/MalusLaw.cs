using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.QuickTable;
using Mantis.Core.ScottPlotUtility;
using Mantis.Core.TexIntegration;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;

namespace Mantis.Workspace.C1_Trials.V40_Polarisation;

[QuickTable("","tab:malusData")]
public record struct MalusData
{
    [QuickTableField("current", "muA")] public ErDouble Current = 0;
    [QuickTableField("angle", "°")] public ErDouble Angle = 0;


    [UseConstructorForParsing]
    public MalusData(string current, string angle)
    {
        Current = ErDouble.ParseWithErrorLastDigit(current,null,1);
        Angle = ErDouble.ParseWithErrorLastDigit(angle,null,0.5);
    }
}

public class CosFunc : AutoDerivativeFunc
{
    public override double CalculateResult(Vector<double> ps, double x)
    {
        // f(x) = A + B cos^2( x )
        return ps[1] * Math.Pow(Math.Cos((x + ps[0]) * Constants.Degree), 2);
    }
}

public static class MalusLaw
{
    public static void Process()
    {
        var reader = V40_PolarisationMain.Reader;

        var dataList = reader.ExtractTable<MalusData>("tab:malusData");
        
        //dataList.ForEachRef((ref MalusData data) => data.Angle -= 0);

        var model = dataList.CreateRegModel(e => (e.Angle, e.Current), new ParaFunc(2,new CosFunc()));


        model.DoRegressionLevenbergMarquardt(new double[] {0, 1}, false);
        
        model.AddParametersToPreambleAndLog("Malus");
        
        //model.AddGoodnessOfFitToPreambleAndLog("");

        var plt = new DynPlot("Angle in °", "Current in muA");

        plt.AddRegModel(model,"signal from the photodiode","Fit of Malus' law");
        
        plt.SaveAndAddCommand("fig:MalusFit","");
    }
}