using System.Drawing;
using System.Reflection.Emit;
using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.QuickTable;
using Mantis.Core.ScottPlotUtility;
using Mantis.Core.Utility;
using ScottPlot;

namespace Mantis.Workspace.C1_Trials.V31_RealGasStateVariables;

[QuickTable("", "ChamberData")]

public class Homework
{
    public static void Process()
    {
        double n = 1;//mol
        double R = 8.314*10;//
        double T = 288.7;//K
        double a = 3.658 * Math.Pow(10, 6);//mol
        double b = 42.75;

        var function1 = new Func<double, double?>(V => R * T / V);
        var function2 = new Func<double, double?>(V => R*T/(V-b)-a/Math.Pow(V,2));
        var constantFunction = new Func<double, double?>(v => 59.334);
        ScottPlot.Plot plot = ScottPlotExtensions.CreateSciPlot("volume V", "pressure p");
        plot.SetAxisLimits(45,600,20,100);
        var plot1 = plot.AddFunction(function1,Color.Black);
        plot1.Label = "Ideal gas";
        var plot2 = plot.AddFunction(function2,Color.Red);
        plot2.Label = "Van der Waals gas";
        var plot3 = plot.AddFunction(constantFunction, Color.Blue);
        plot3.Label = "Maxwell line";
        var legend = plot.Legend(true,Alignment.UpperRight);
        legend.FontSize = 9;
        plot.SaveAndAddCommand("HomeworkPlotFirst","caption");
        
    }
    
}