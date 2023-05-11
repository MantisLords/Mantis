using Mantis.Core.Calculator;
using Mantis.Core.TexIntegration;

namespace Mantis.Workspace.BasicTests;

public static class SimplePlotTest
{
    public static void CreateSimplePlot()
    {
        Sketchbook sketchbook =
            new Sketchbook(new AxisLayout("X Axis", "Y Axis"), "SimplePlot", "This is a basic Plot");

        sketchbook.Add(new DataMarkSketch()
        {
            Data = TableTest.DummyList.Select(d => ((ErDouble)d.a,(ErDouble)d.b)),
            Legend = "This is Dummy Data"
        });
        
        sketchbook.Add(new StraightPlot()
        {
            Slope = 1,
            YZero = 1,
            Legend = "This is a straight"
        });
        
        sketchbook.SaveLabeled();
    }
}