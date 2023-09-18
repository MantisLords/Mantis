using Mantis.Core.Calculator;
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

        // var series = MeasurementSeriesDict["Messreihe #10"];
        // var plt = ScottPlotExtensions.CreateSciPlot("H in A/m", "B in T", pixelWidth: 520);
        // series.PlotData(plt);
        // var dataLimits = plt.GetDataLimits();
        // var xMax = 200;
        // var yMax = dataLimits.YMax / dataLimits.XSpan * xMax;
        // plt.SetAxisLimits(-xMax,xMax,-0.1,0.2);
        // //plt.SetAxisLimits(-xMax,xMax,-yMax,yMax);
        // plt.SaveFigHere(series.Name + "_" + series.RingCore.Name, scale: 4);

        foreach (var s in MeasurementSeriesDict.Values)
        {
            // if (s is NonFerromagneticMeasurementSeries series)
            // {
            //     var plt = ScottPlotExtensions.CreateSciPlot("H in A/m", "B in T", pixelWidth: 520);
            //     series.PlotData(plt,true);
            //     plt.SaveFigHere(s.Name + "_" + s.RingCore.Name, scale: 4);
            // }
        
            if (s is OneCycleMeasurementSeries series)
            {
                var plt = ScottPlotExtensions.CreateSciPlot("H in A/m", "B in T",pixelWidth:520*4);
            
                series.CalculateCoercivity();
                series.CalculateRemanence();
                series.CalculateSaturation();
                series.CalculateHysteresisLoss();
            
                series.PlotData(plt,true,drawRegSaturation:true);
            
                plt.Legend(true, Alignment.UpperLeft);
                
                plt.SaveFigHere(series.Name,scale:4);
            }
        }

    }
    
}