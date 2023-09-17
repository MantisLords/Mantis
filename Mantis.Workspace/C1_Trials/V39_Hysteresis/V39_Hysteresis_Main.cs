using Mantis.Core.FileImporting;
using Mantis.Core.ScottPlotUtility;
using ScottPlot;

namespace Mantis.Workspace.C1_Trials.V39_Hysteresis;

public static class V39_Hysteresis_Main
{
    private readonly static Dictionary<string, HysteresisMeasurementSeries> MeasurementSeriesDict 
        = new Dictionary<string, HysteresisMeasurementSeries>();
    
    
    public static void Process()
    {
        var pascoCsvReader = new PascoCsvReader("C39_Pasco_RawData");
        
        pascoCsvReader.ReadFile();
        
        Console.WriteLine(pascoCsvReader);

        var seriesInfoReader = new SimpleTableProtocolReader("MeasurementSeriesInfo");

        var ringCores = seriesInfoReader.ExtractTable<RingCore>();
        var seriesInfos = seriesInfoReader.ExtractTable<MeasurementSeriesInfo>();

        //int[] fittableModels = new int[] {1, 7, 15};//{1, 3, 7, 13, 14, 15, 16};

        foreach (var pascoSeries in pascoCsvReader.MeasurementSeries)
        {
            var measurementSeries =
                new HysteresisMeasurementSeries(pascoSeries.Key, pascoSeries.Value, ringCores, seriesInfos);

            MeasurementSeriesDict[measurementSeries.Name] = measurementSeries;
        }

        foreach (var series in MeasurementSeriesDict.Values)
        {
            if (series.IsCurveOneCycle)
            {
                var plt = ScottPlotExtensions.CreateSciPlot("H in A/m", "B in T");

                series.CalculateCoercivity();
                series.CalculateRemanence();
                series.CalculateSaturation();

                series.PlotData(plt,true);

                plt.Legend(true, Alignment.UpperLeft);
                
                plt.SaveFigHere(series.Name,scale:8);
            }
        }


    }
    

    
    
}