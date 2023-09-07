using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.QuickTable;
using Mantis.Core.ScottPlotUtility;
using Mantis.Core.TexIntegration;
using Mantis.Core.Utility;
using MathNet.Numerics.LinearAlgebra;
using Vector = MathNet.Numerics.LinearAlgebra.Single.Vector;

namespace Mantis.Workspace.Fr2.Sheet5_Regression2;

[QuickTable("Measured time of a falling metal sheet with drill holes", "tab:MetalSheetFallData")]
public record struct MetalSheetFallData
{
    [QuickTableField("Hole Count", "", "N")]
    public int HoleCount;

    [QuickTableField("Distance", "cm", "x", false)]
    public ErDouble Distance;

    [QuickTableField("Time", "s", "t")] public double Time;

    public MetalSheetFallData(){}
}

public static class Sheet5_Regression2_Main
{
    public static void Process()
    {
        //Import data
        var reader = new SimpleTableProtocolReader("MetalSheetFallData");
        List<MetalSheetFallData> data = reader.ExtractTable<MetalSheetFallData>();
        ErDouble holeDistance = reader.ExtractSingleValue<ErDouble>("Hole Distance");
        
        holeDistance.AddCommandAndLog("holeDistance","cm");

        // Calculate the distance x
        data.ForEachRef((ref MetalSheetFallData e) => e.Distance = new ErDouble(e.HoleCount * holeDistance.Value));
        
        //Save the data as TexTable
        data.CreateTexTable().SaveLabeled();
        

        var model = data.CreateRegModel(e => (e.Time, e.Distance),
            new ParaFunc<PolynomialFunc>(3)
            {
                Units = new []{"cm","cm / s","cm / s^2"}
            });

        model.DoLinearRegression(false);

        model.AddParametersToPreambleAndLog("Poly");

        ErDouble g2 = model.ErParameters[2].Mul10E(-2) * 2;
        g2.AddCommandAndLog("GFactor", "m / s^2");
        

        var plt = ScottPlotExtensions.CreateSciPlot(
            "Time t in s",
            "Distance x in cm");

        plt.AddRegModel(model,
            labelData: "Travelled distance of a falling metal sheet with holes",
            labelFunction:"Best fit quadratic polynomial");

        plt.SaveAndAddCommand("fig:MetalSheetFallGraph", "Measure g-factor via a simple falling experiment");
        

        TexPreamble.GeneratePreamble();
    }
}