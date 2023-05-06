using Mantis.Core.TexIntegration;

namespace Mantis.Workspace.BasicTests;

public static class SimplePlotTest
{
    public static void CreateSimplePlot()
    {
        Sketchbook sketchbook = new Sketchbook()
        {
            Label = "SimplePlot",
            Caption = "This is a basic Plot",
            Axis = new LinearAxis()
            {
                XLabel = "X Axis",
                YLabel = "Y Axis"
            }
        };
        
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