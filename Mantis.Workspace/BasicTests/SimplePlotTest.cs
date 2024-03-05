using System.Diagnostics;
using System.Reflection;
using Mantis.Core.Calculator;
using Mantis.Core.FileManagement;
using Mantis.Core.LogCommands;
using Mantis.Core.ScottPlotUtility;
using Mantis.Core.TexIntegration;
using ScottPlot;
using ScottPlot.AutoScalers;
using ScottPlot.AxisPanels;
using ScottPlot.Grids;

namespace Mantis.Workspace.BasicTests;

public static class SimplePlotTest
{
    public static void CreateSimplePlot()
    {
        // var plt = new Plot();
        //
        // plt.Axes.SetTopMirrored();
        // plt.Axes.SetYAxisLog();
        // plt.Axes.SetXAxisLog();
        // var right = plt.Axes.SetRightMirrored();
        
        //plt.Axes.SetXAxisLog(false);

        var plt = new DynPlot("X Axis","Y Axis","Plot");
        
        List<(ErDouble,ErDouble)> data = new List<(ErDouble,ErDouble)>();

        for (int i = 1; i < 10; i++)
        {
            data.Add((new ErDouble(i,0.2),new ErDouble(2*i,0.5*i)));
        }

        plt.AddDynErrorBar(data, "Data");
        plt.AddDynFunction(x => 2 * x, "Function");
        

        plt.DynAxes.SetLimitsY(1,20);
        plt.DynAxes.Left.SetToLogarithmicScale();
        //plt.DynAxes.Left.LinearRescaleAxis(2,-10);
        
        plt.SaveFigHere("demo.svg", 400, 300,ImageFormat.Svg);

        double a = 5;
        
        a.AddCommandAndLog("f");


    }
    
}