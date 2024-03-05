using Mantis.Core.Calculator;
using Mantis.Core.ScottPlotUtility;

namespace Mantis.Examples.DynPlotExamples;

public static class DynPlot_Main
{
    public static void Process()
    {
        DynPlot plot = new DynPlot("X", "Y");

        List<(ErDouble, ErDouble)> dataPoints = new List<(ErDouble, ErDouble)>();
        for (int i = 0; i < 20; i++)
        {
            dataPoints.Add((new ErDouble(i,i*0.2),new ErDouble(2*i,0.5)));
        }

        //plot.AddDynErrorBar(dataPoints, "Example Data");
        
        

    }
}