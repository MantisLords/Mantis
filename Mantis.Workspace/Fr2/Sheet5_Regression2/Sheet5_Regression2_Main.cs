using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.QuickTable;
using Mantis.Core.TexIntegration;
using Mantis.Core.Utility;

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
        
        holeDistance.AddCommand("holeDistance","cm");
        
        Console.WriteLine($"Hole Distance {holeDistance.ToString()}");
        
        // Calculate the distance x
        data.ForEachRef((ref MetalSheetFallData e) => e.Distance = new ErDouble(e.HoleCount * holeDistance.Value));
        
        //Save the data as TexTable
        data.CreateTexTable().SaveLabeled();


        var (alpha, beta, gamma) = data.LinearRegressionQuadratic(e => (e.Time, e.Distance),RegressionCommand.IgnoreYErrors);
        alpha.AddCommand("alphaPoly","cm");
        beta.AddCommand("betaPoly","cm / s");
        gamma.AddCommand("gammaPoly","cm / s^2");
        
        Console.WriteLine($"Regression: a: {alpha.ToString()}, b: {beta.ToString()}, c: {gamma.ToString()}");
        
        //Calculate falling g factor
        ErDouble g = gamma.Mul10E(-2) * 2;
        Console.WriteLine($"g Factor {g.ToString()}");
        g.AddCommand("gFactor","m / s^2");
        
        // Sketch
        Sketchbook sketchbook = new Sketchbook(
            axis: new AxisLayout( "Time t in s","Distance x in cm"),
            label: "fig:MetalSheetFallGraph",
            caption: "Measure g-factor via a simple falling-experiment"
        );
        
        sketchbook.Add(new DataMarkSketch()
        {
            Data = data.Select(e => ((ErDouble)e.Time,(ErDouble)e.Distance)),
            Legend = "Travelled distance of a falling metal sheet with holes"
        });
        
        sketchbook.Add(new FunctionPlot()
        {
            FunctionString = $"{alpha.Value} + {beta.Value} * x + {gamma.Value} * x^2",
            Legend = "Best fit quadratic polynomial"
        });
        
        sketchbook.SaveLabeled();

        TexPreamble.GeneratePreamble();
    }
}