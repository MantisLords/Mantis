using Mantis.Core.Calculator;
using Mantis.Core.ScottPlotUtility;
using Mantis.Core.TexIntegration;

namespace Mantis.Workspace.BasicTests;

public static class SimplePlotTest
{
    public static void CreateSimplePlot()
    {
        var plt = ScottPlotExtensions.CreateSciPlot("x axis", "y axis", "Simple plot");

        plt.AddErrorBars(TableTest.DummyList.Select(d => ((ErDouble)d.a, (ErDouble)d.b)),
            label: "This is dummy data");

        plt.SaveFigHere("Example Plot");
    }
}