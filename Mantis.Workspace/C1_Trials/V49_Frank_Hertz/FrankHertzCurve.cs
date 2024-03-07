using Mantis.Core.Calculator;
using Mantis.Core.QuickTable;
using Mantis.Core.ScottPlotUtility;
using Mantis.Core.TexIntegration;
using Mantis.Core.Utility;
using Mantis.Workspace.C1_Trials.V39_Hysteresis;
using ScottPlot;

namespace Mantis.Workspace.C1_Trials.V49_Frank_Hertz;

[QuickTable("","")]
public record struct FrankHertzInfo
{
    [QuickTableField("name")] public string Name;
    [QuickTableField("columnName")] public string ColumnName;
    [QuickTableField("U3")] public double U3;
    [QuickTableField("U1")] public double U1;
    [QuickTableField("U2Multiplier")] public double U2Multiplier;
    [QuickTableField("U2MaximumsMin")] public double U2MaximumsMin;
    [QuickTableField("U2ManualMaximums")] public string U2ManualMaximums;
    [QuickTableField("ErrorMaximums")] public double ErrorMaximums;
    [QuickTableField("Transitions")] public string Transitions;
    
    public FrankHertzInfo(){}
}

public static class FrankHertzCurve
{
    public static void Process()
    {
        var reader = V49_Frank_Hertz_Main.Reader;
        var infoList = reader.ExtractTable<FrankHertzInfo>("tab:frankHertzInfos");

        var pascoReader = new PascoCsvReader("Capstone Data.csv");
        pascoReader.ReadFile();

        foreach (var info in infoList)  
        {
            ProcessMeasurementSeries(info,pascoReader.MeasurementSeries[info.ColumnName]);
        }
        
    }

    private static void ProcessMeasurementSeries(FrankHertzInfo info, List<PascoData> rawData)
    {
        rawData.ForEachRef((ref PascoData data) => data.ValueA *= info.U2Multiplier);
        
        DynPlot dynPlot = new DynPlot("accelerating voltage in V", "anode current in nA");
        
        var scatter = dynPlot.Add.Scatter(rawData.Select(e => new Coordinates(e.ValueA, e.ValueB)).ToList());
        scatter.MarkerStyle.IsVisible = false;
        scatter.Label = $"Frank Hertz {info.Name} counter U3 = {info.U3}V suction U1 = {info.U1}V";

        var maximums = FindMaximums(10, rawData,info);

        var maximumsPlot = dynPlot.Add.Scatter(maximums.Select(e => new Coordinates(e.Item1.Value, e.Item1.Value)).ToList());
        maximumsPlot.LineStyle.IsVisible = false;
        maximumsPlot.MarkerStyle.Shape = MarkerShape.OpenCircle;
        maximumsPlot.MarkerSize = 3;

        foreach (var maximum in maximums)
        {
            if (info.Name == "Hg")
            {
                var text = dynPlot.Add.Text(maximum.Item1.Value.ToString("G4"), maximum.Item1.Value, maximum.Item2.Value + 0.05);
                text.Label.Alignment = Alignment.LowerCenter;
            }
            else
            {
                var text = dynPlot.Add.Text(maximum.Item1.Value.ToString("G4"), maximum.Item1.Value, maximum.Item2.Value + 0.2);
                text.Label.Alignment = Alignment.LowerCenter;
            }
        }

        //var marker = dynPlot.Add.Marker();
        
        PrintTransitions(maximums,info);


        dynPlot.Legend.Location = Alignment.UpperLeft;
        dynPlot.SaveAndAddCommand("FrankHertzPlot"+info.Name);
        
    }

    private static void PrintTransitions((ErDouble, ErDouble)[] maximums, FrankHertzInfo info)
    {
        string[] transStrings =
            info.Transitions.Split(" ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        Dictionary<string, List<ErDouble>> categoryTransitions = new Dictionary<string, List<ErDouble>>();

        foreach (var transString in transStrings)
        {
            string[] args = transString.Split("-",
                StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            if (args.Length < 2)
                throw new ArgumentException(
                    "A valid Transition must be in the format: <startIndex>-<endIndex>-<category>");

            int start = int.Parse(args[0]) - 1;
            int end = int.Parse(args[1]) - 1;


            if (end >= maximums.Length || start >= maximums.Length)
                throw new ArgumentException(
                    $"The maxima count is {maximums.Length}. Transition {transString} is not valid");
                
            ErDouble transition = maximums[end].Item1 - maximums[start].Item1;
            transition.AddCommandAndLog($"Transition {info.Name} {transString}");

            if (args.Length >= 3)
            {
                string category = args[2];
                if(!categoryTransitions.ContainsKey(category))
                    categoryTransitions.Add(category,new List<ErDouble>());
                
                categoryTransitions[category].Add(transition);
            }
        }

        foreach (var (category,transitions) in categoryTransitions)
        {
            var mean = transitions.WeightedMean();
            mean.AddCommandAndLog($"Transition {info.Name} Mean {category}","ev");
        }
    }

    private static (ErDouble,ErDouble)[] FindMaximums(int indexRegion,List<PascoData> rawData,FrankHertzInfo info)
    {
        List<int> maximums = new List<int>();

        for (int i = indexRegion; i < rawData.Count - indexRegion; i++)
        {
            if (rawData[i].ValueA < info.U2MaximumsMin) continue;
            
            if (IsIndexMaximum(i, indexRegion, rawData))
            {
                if(! (maximums.Count > 0 && maximums[^1] >= i - indexRegion))
                    maximums.Add(i);
            }
        }
        
        AddManualMaximums(maximums,info.U2ManualMaximums,rawData);
        
        maximums.Sort();

        return maximums.Select(i => (new ErDouble(rawData[i].ValueA,info.ErrorMaximums),
                new ErDouble(rawData[i].ValueB,info.ErrorMaximums))).ToArray();
    }

    private static void AddManualMaximums(List<int> maximums, string manualMaximumsString, List<PascoData> rawData)
    {
        double[] manualMaximums = manualMaximumsString.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => double.Parse(s)).ToArray();

        foreach (var manualMaximum in manualMaximums)
        {
            for (int i = 0; i < rawData.Count; i++)
            {
                if (rawData[i].ValueA >= manualMaximum)
                {
                    maximums.Add(i);
                    break;
                }
            }
        }
    }

    private static bool IsIndexMaximum(int index, int indexRegion, List<PascoData> rawData)
    {
        double v = rawData[index].ValueB;
        for (int j = index - indexRegion; j < index + indexRegion; j++)
        {
            if(rawData[j].ValueB > v) return false; // Index i is no maxima
        }

        return true;
    }
}