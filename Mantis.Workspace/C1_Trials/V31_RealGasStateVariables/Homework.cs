using System.Drawing;
using Mantis.Core.ScottPlotUtility;

namespace Mantis.Workspace.C1_Trials.V31_RealGasStateVariables;

public class Homework
{
    public static void Process()
    {
        double n = 1;//mol
        double R = 8.314;//
        double T = 288.7;//K
        double a = 365.8 * Math.Pow(10, -3);//mol
        double b = 42.75 * Math.Pow(10,-6);

        var function1 = new Func<double, double?>(V => n * R * T / V);
        var function2 = new Func<double, double?>(V => n*R*T/(V-n*b)-n*n*a/(V*V));
        ScottPlot.Plot plot = ScottPlotExtensions.CreateSciPlot("volume", "pressure");
        plot.SetAxisLimits(0,0.600,0,100);
        plot.AddFunction(function1,Color.Black);
        plot.AddFunction(function2, Color.Red);
        plot.SaveAndAddCommand("HomeworkPlotFirst","caption");
    }
}