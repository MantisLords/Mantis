using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.ScottPlotUtility;
using Mantis.Core.TexIntegration;
using ScottPlot;
using ScottPlot.Plottables;

namespace Mantis.Workspace.C1_Trials.V39_Hysteresis;

public static class V39_Hysteresis_Main
{
    private readonly static Dictionary<string, HysteresisMeasurementSeries> MeasurementSeriesDict 
        = new Dictionary<string, HysteresisMeasurementSeries>();
    
    
    public static void Process()
    {
        var pascoCsvReader = new PascoCsvReader("RawData_Karb_Smailagic");
        
        pascoCsvReader.ReadFile();

        var seriesInfoReader = new SimpleTableProtocolReader("MeasurementSeriesInfo");

        double errorVoltage = seriesInfoReader.ExtractSingleValue<double>("errorVoltage");
        var ringCores = seriesInfoReader.ExtractTable<RingCore>();
        var seriesInfos = seriesInfoReader.ExtractTable<MeasurementSeriesInfo>();
        
        

        //int[] fittableModels = new int[] {1, 7, 15};//{1, 3, 7, 13, 14, 15, 16};

        foreach (var pascoSeries in pascoCsvReader.MeasurementSeries)
        {
            if (HysteresisMeasurementSeries.TryInstantiateSeries(pascoSeries.Key, pascoSeries.Value, ringCores,
                    seriesInfos, errorVoltage, out HysteresisMeasurementSeries measurementSeries))
            {

                MeasurementSeriesDict[measurementSeries.Label] = measurementSeries;
            }
        }
        
                
        foreach (var series in MeasurementSeriesDict.Values)
        {
            //if (series is OneCycleMeasurementSeries oneCycleMeasurementSeries)
            //    oneCycleMeasurementSeries.DrawRegPoints = true;

            if (series is OneCycleMeasurementSeries oneCycleMeasurementSeries)
            {
                if (series.SeriesInfo.Usage == "ExCoercivityRemanence")
                {
                    oneCycleMeasurementSeries.DrawRegRemanence = true;
                    oneCycleMeasurementSeries.DrawRegCoercivity = true;
                }
                else if (series.SeriesInfo.Usage == "ExSaturation")
                {
                    oneCycleMeasurementSeries.DrawRegSaturation = true;
                }
            }
            
            series.SaveAndLogCalculatedData();
            
            var plt = new DynPlot("H in A/m", "B in T");
            series.PlotData(plt);
            
            if(series.SeriesInfo.Usage == "ExDemagnetization") PlotExDemagnetization(plt,series);
            if(series.SeriesInfo.Usage == "ExIrreversibility") PlotExIrreversibility(plt,series);
            
            plt.SaveAndAddCommand("fig:"+series.Label);
        }
        
        var oneCyclePropertiesGroupByRingCore = from e in MeasurementSeriesDict
            where e.Value is OneCycleMeasurementSeries
            group (e.Value as OneCycleMeasurementSeries).CharacProperties by e.Value.RingCore;
        
        var weightedMeanCoreList = from grouping in oneCyclePropertiesGroupByRingCore
            select new
            {
                Core = grouping.Key,
                Properties = grouping.WeightedMean()
            };
        
        Console.WriteLine("### Weighted Means ###");
        foreach (var e in weightedMeanCoreList)
        {
            e.Properties.Remanence?.AddCommand("Remanence"+e.Core.Type,"T");
            e.Properties.Coercivity?.AddCommand("Coercivity"+e.Core.Type,"A/m");
            e.Properties.Saturation?.AddCommand("Saturation"+e.Core.Type,"T");
            e.Properties.SaturationPermeability?.AddCommand("SaturationPermeability"+e.Core.Type);
            e.Properties.HysteresisLoss?.AddCommand("HysteresisLoss"+e.Core.Type,"J/kg");
            Console.WriteLine($"Core: {e.Core.Type} \n{e.Properties}");
        }
        
        
        var paraMagneticGroupByRingCore = from e in MeasurementSeriesDict
            where e.Value is NonFerromagneticMeasurementSeries
            group (e.Value as NonFerromagneticMeasurementSeries).MagneticPermeability by e.Value.RingCore;
        
        var weightedMeanPermeabilityList = from grouping in paraMagneticGroupByRingCore
            select new
            {
                Core = grouping.Key,
                MagneticPermeablity = grouping.NullableWeightedMean()
            };

        foreach (var e in weightedMeanPermeabilityList)
        {
            if (e.MagneticPermeablity != null)
            {
                e.MagneticPermeablity?.AddCommandAndLog($"{e.Core.Type} MagneticPermeability");
                (e.MagneticPermeablity!.Value - 1).AddCommandAndLog($"{e.Core.Type} MagneticSusceptibility");
            }
            //Console.WriteLine($"Core: {e.Core.Name} Permeability: {e.MagneticPermeablity}");
        }
        
        
        
        // -------- Drift
        string driftSeriesName = "Messreihe_Kern4_17_2_R";
        try
        {
            var woodPascoSeries = pascoCsvReader.MeasurementSeries.First(e => e.Key == driftSeriesName);

            if (HysteresisMeasurementSeries.TryInstantiateSeries(woodPascoSeries.Key, woodPascoSeries.Value, ringCores,
                    seriesInfos, errorVoltage, out HysteresisMeasurementSeries series, removeDrift: false))
            {
                var plt = new DynPlot("H in A/m", "B in T");
                series.PlotData(plt);
            
                plt.SaveAndAddCommand("fig:NoDriftRemovalWood");
            }else Console.WriteLine($"Error: Could not instantiate {driftSeriesName}");
            
        }catch(ArgumentException e){Console.WriteLine($"Series {driftSeriesName} was not found");}
        
        
        
        
        TexPreamble.GeneratePreamble();

    }

    private static void PlotExDemagnetization(Plot plt, HysteresisMeasurementSeries series)
    {
        var black = plt.Add.Palette.GetColor(0);
        var red = plt.Add.Palette.GetColor(1);
        var blue = plt.Add.Palette.GetColor(2);
        
        var inletPlot = new DynPlot("H in A/m", "B in T");

        inletPlot.Add.HorizontalLine(0, 0.5f,black);
        inletPlot.Add.VerticalLine(0, 0.5f,black);
        
        series.PlotData(inletPlot);
        // if (inletPlot.PlottableList[2] is Scatter scatter)
        // {
        //     scatter.MarkerSize = 3;
        //     scatter.Color = black;
        // }
        inletPlot.DynAxes.SetLimits(-200,200,-0.15,0.05);
        
        inletPlot.AddArrow(-100,-0.05, -50, -0.025, color: red,lineWidth:3);
        inletPlot.AddArrow(100, -0.025, 50, -0.05, color: blue,lineWidth:3);

        inletPlot.ScaleFactor = 0.6f;
        plt.AddDynInlet(inletPlot, new PixelRect(300,480,310,150));

        plt.Legend.Location = Alignment.UpperLeft;
        
        plt.ShowGrid();

        plt.AddArrow(-1500, -0.7, -1000, 0.3, color: red,lineWidth:3);
        plt.AddArrow(1500, 0.7, 1000, -0.3, color: blue,lineWidth:3);
    }
    
    private static void PlotExIrreversibility(Plot plt, HysteresisMeasurementSeries series)
    {
        var red = plt.Add.Palette.GetColor(1);
        var blue = plt.Add.Palette.GetColor(2);
        
        var inletPlot = new DynPlot("H in A/m", "B in T");
        series.PlotData(inletPlot);
        if (inletPlot.GetPlottables().First() is Scatter scatter)
        {
            scatter.MarkerSize = 3;
        }
        inletPlot.DynAxes.SetLimits(800,2200,0.9,1.2);
        
        inletPlot.AddArrow(1530, 1.05, 1800, 1.1, color: red,lineWidth:3);
        inletPlot.AddArrow(1620, 1, 1300, 0.95, color: blue,lineWidth:3);

        inletPlot.ScaleFactor = 0.6f;
        
        plt.AddDynInlet(inletPlot, new PixelRect(300,480,310,150));
        
        //plt.HideGrid();
        plt.Legend.Location = Alignment.UpperLeft;

    }

    private static CycleCharacteristicProperties WeightedMean(this IEnumerable<CycleCharacteristicProperties> data)
    {
        var dataArray = data.ToArray();
        return new CycleCharacteristicProperties(
            dataArray.Select(e => e.Coercivity).NullableWeightedMean(),
            dataArray.Select(e => e.Remanence).NullableWeightedMean(),
            dataArray.Select(e => e.Saturation).NullableWeightedMean(),
            dataArray.Select(e => e.SaturationPermeability).NullableWeightedMean(),
            dataArray.Select(e => e.HysteresisLoss).NullableWeightedMean()
        );
    }

    private static ErDouble? NullableWeightedMean(this IEnumerable<ErDouble?> data)
    {
        if (data.Any(v => v == null))
            return null;
        return data.Select(e => e.Value).WeightedMean();
    }
    
}