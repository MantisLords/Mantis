using Mantis.Core.ScottPlotUtility;
using ScottPlot;

namespace Mantis.Workspace.C1_Trials.V39_Hysteresis;

public record struct HysteresisData(double VoltageA,double VoltageB,double H, double B);
public static class V39_Hysteresis_Main
{
    private readonly static Dictionary<string, HysteresisData[]> MeasurementSeries = new Dictionary<string, HysteresisData[]>();
    
    public static void Process()
    {
        var reader = new PascoCsvReader("C39_Pasco_RawData");
        
        reader.ReadFile();
        
        Console.WriteLine(reader);
        
        InitAllSeries(reader.MeasurementSeries);
        
        for (int i = 1; i < 16; i++)
        {
            PlotSeries(i,reader);
        }
        
    }

    private static void InitAllSeries(Dictionary<string, List<PascoData>> readSeries)
    {
        foreach (var readData in readSeries)
        {
            MeasurementSeries[readData.Key] = InitialiseData(readData.Value);
        }
    }

    private static HysteresisData[] InitialiseData(List<PascoData> pascoData)
    {
        HysteresisData[] hysteresisData = new HysteresisData[pascoData.Count];
        if (pascoData.Count == 0)
            return hysteresisData;
        
        double voltageDif = pascoData[pascoData.Count-1].VoltageB - pascoData[0].VoltageB;
        double timeDif = pascoData[pascoData.Count-1].Time - pascoData[0].Time;
        double drift = voltageDif / timeDif;

        double voltageMin = pascoData[0].VoltageB;
        double voltageMax = pascoData[0].VoltageB;
        
        for (int i = 0; i < hysteresisData.Length; i++)
        {
            var data = new HysteresisData(pascoData[i].VoltageA,pascoData[i].VoltageB,0,0);
            
            // Remove drift
            data.VoltageB -= pascoData[i].Time * drift;

            voltageMax = Math.Max(voltageMax, data.VoltageB);
            voltageMin = Math.Min(voltageMin, data.VoltageB);

            hysteresisData[i] = data;
        }
        
        // Center
        double shift = 0.5 * (voltageMax + voltageMin);
        for (int i = 0; i < hysteresisData.Length; i++)
        {
            var data = hysteresisData[i];
            data.VoltageB -= shift;
            hysteresisData[i] = data;
        }
        
        return hysteresisData;
    }

    public static void PlotSeries(int index, PascoCsvReader reader)
    {
        var series = MeasurementSeries["Messreihe #"+index];

        double[] xs = new double[series.Length];
        double[] ys = new double[series.Length];
        for (int i = 0; i < series.Length; i++)
        {
            xs[i] = series[i].VoltageA;
            ys[i] = series[i].VoltageB;
        }

        var plt = ScottPlotExtensions.CreateSciPlot("Voltage A", "Voltage B");

        plt.AddScatter(xs, ys,lineStyle:LineStyle.None,markerSize:0.5f);

        plt.SaveFigHere("TestHysteresisPlot"+index,scale:8);
    
    }
}