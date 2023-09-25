using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.QuickTable;
using Mantis.Core.ScottPlotUtility;
using Mantis.Core.TexIntegration;
using Mantis.Core.Utility;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;

namespace Mantis.Workspace.C1_Trials.V40_Polarisation;

[QuickTable("","tab:malusData")]
public record struct MalusData
{
    [QuickTableField("current", "muA")] public ErDouble Current = 0;
    [QuickTableField("angle", "°")] public ErDouble Angle = 0;

    public MalusData()
    {
        
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

public class V40_PolarisationMain
{
    public static void Process()
    {
        var reader = new SimpleTableProtocolReader("MalusData.csv");

        var dataList = reader.ExtractTable<MalusData>();
        
        //dataList.ForEachRef((ref MalusData data) => data.Angle -= 0);

        var model = dataList.CreateRegModel(e => (e.Angle, e.Current), new ParaFunc<CosFunc>(2));


        model.DoRegressionLevenbergMarquardt(new double[] {0, 1}, false);
        
        model.AddParametersToPreambleAndLog("");
        
        //model.AddGoodnessOfFitToPreambleAndLog("");

        var plt = ScottPlotExtensions.CreateSciPlot("Angle in °", "Current in muA");

        plt.AddRegModel(model);
        
        plt.SaveAndAddCommand("fig:MalusFit","");
    }
}