using System.Formats.Tar;
using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.QuickTable;
using Mantis.Core.ScottPlotUtility;
using ScottPlot;
using SkiaSharp;
namespace Mantis.Workspace.C1_Trials.V35_Ultrasound;

[QuickTable("CopperMeasurement")]
public record struct AngleIntensityData
{
    [QuickTableField("Angle")] public ErDouble Angle;
    [QuickTableField("Long")] public ErDouble Long;
    [QuickTableField("Trans")] public ErDouble Trans;

    public AngleIntensityData()
    {
    }
}
public static class V35_SpeedInMetals
{
    public static void Process()
    {
        var csvReader = new SimpleTableProtocolReader("WavemodeMeasurement");
        List<AngleIntensityData> dataList = csvReader.ExtractTable<AngleIntensityData>("tab:CopperMeasurement");
        DynPlot plot = new DynPlot("Angle []", "Intensity [V]");
        plot.AddDynErrorBar(dataList.Select(e => (e.Angle, e.Long)),"Longitudinal",Color.FromSKColor(SKColors.Blue));
        plot.AddDynErrorBar(dataList.Select(e => (e.Angle, e.Trans)),"Transverse", Color.FromSKColor(SKColors.Red));
       plot.Legend.Location = Alignment.UpperRight;
        
        List<AngleIntensityData> dataList2 = csvReader.ExtractTable<AngleIntensityData>("tab:AluMeasurement");
        DynPlot secondPlot = new DynPlot("Angle []", "Intensity [V]");
        secondPlot.AddDynErrorBar(dataList2.Select(e => (e.Angle, e.Trans)),"Transverse", Color.FromSKColor(SKColors.Red));
        secondPlot.AddDynErrorBar(dataList2.Select(e => (e.Angle, e.Long)), "Longitudinal",
            Color.FromSKColor(SKColors.Blue));
        

        plot.AddVerticalLine(22,null,Color.FromSKColor(SKColors.Blue));
        plot.AddVerticalLine(25, null, Color.FromSKColor(SKColors.Red));
        plot.AddVerticalLine(42, null, Color.FromSKColor(SKColors.Red));

        secondPlot.AddVerticalLine(15, null, Color.FromSKColor(SKColors.Blue));
        secondPlot.AddVerticalLine(17, null, Color.FromSKColor(SKColors.Red));
        secondPlot.AddVerticalLine(30, null, Color.FromSKColor(SKColors.Red));
        
        plot.SaveAndAddCommand("LongTransPlotCopper");
        secondPlot.SaveAndAddCommand("LongTransPlotAluminum");
    }
}