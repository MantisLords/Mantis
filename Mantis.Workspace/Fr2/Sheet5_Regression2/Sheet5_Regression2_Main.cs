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
        
        // This will read the table '# tab:MetalSheetFallData' from the csv file. Tables are marked by '# '
        // You have to make sure that the 'MetalSheetFallData' Type has a 'QuickTable'-attribute.
        // Also each fields need a 'QuickTableField'-attribute if they are imported from the csv file
        List<MetalSheetFallData> data = reader.ExtractTable<MetalSheetFallData>("tab:MetalSheetFallData");
        
        // This will extract the '* Hole Distance' field from the csv file. Single Value Fields have to be marked with '*'
        ErDouble holeDistance = reader.ExtractSingleValue<ErDouble>("Hole Distance");
        // This will add the ErDouble to the Tex file and it will print it out to the console
        holeDistance.AddCommandAndLog("holeDistance","cm");

        // Calculate the distance x
        data.ForEachRef((ref MetalSheetFallData e) => e.Distance = new ErDouble(e.HoleCount * holeDistance.Value));
        
        //Save the data as TexTable
        data.CreateTexTable().SaveLabeled();
        
        
        // This function will create a regression model with a 'PolynomialFunc'tion
        // The date is read in via the selector. First is x, second is y. Here x: e.Time, y: e.Distance
        var model = data.CreateRegModel(e => (e.Time, e.Distance),
            new ParaFunc<PolynomialFunc>(3)
            {
                // These are the units of the regression parameters
                // For a ponomial here f(x) = A + B x + C x^2 these are the units
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