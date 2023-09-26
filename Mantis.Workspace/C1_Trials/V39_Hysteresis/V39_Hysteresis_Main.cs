using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.ScottPlotUtility;
using Mantis.Core.TexIntegration;
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

        var seriesInfoReader = new SimpleTableProtocolReader("MeasurementSeriesInfo");

        double errorVoltage = seriesInfoReader.ExtractSingleValue<double>("errorVoltage");
        var ringCores = seriesInfoReader.ExtractTable<RingCore>();
        var seriesInfos = seriesInfoReader.ExtractTable<MeasurementSeriesInfo>();
        
        

        //int[] fittableModels = new int[] {1, 7, 15};//{1, 3, 7, 13, 14, 15, 16};

        foreach (var pascoSeries in pascoCsvReader.MeasurementSeries)
        {
            var measurementSeries =
                HysteresisMeasurementSeries.InstantiateSeries(pascoSeries.Key, pascoSeries.Value, ringCores, seriesInfos,errorVoltage);

            MeasurementSeriesDict[measurementSeries.Name] = measurementSeries;
        }

        if (MeasurementSeriesDict["Messreihe #1"] is OneCycleMeasurementSeries series1)
        {
            series1.DrawRegRemanence = true;
            series1.DrawRegCoercivity = true;
        }
        ((MeasurementSeriesDict["Messreihe #13"] as OneCycleMeasurementSeries)!).DrawRegSaturation = true;
        
        foreach (var series in MeasurementSeriesDict.Values)
        {
            series.SaveAndLogCalculatedData();
            
            var plt = ScottPlotExtensions.CreateSciPlot("H in A/m", "B in T",pixelWidth:520);
            series.PlotData(plt);
            plt.SaveFigHere(series.Name+series.RingCore.Type,scale:4);
            
        }
        
        TexPreamble.GeneratePreamble();

    }
    
}