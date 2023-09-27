using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.QuickTable;
using Mantis.Core.ScottPlotUtility;
using Mantis.Core.TexIntegration;
using Mantis.Core.Utility;
using MathNet.Numerics.Interpolation;
using ScottPlot;
using ScottPlot.Plottable;

namespace Mantis.Workspace.C1_Trials.V31_RealGasStateVariables;

[QuickTable("", "ChamberData")]
public record struct VolumePressureData
{
    [QuickTableField("pressure", "Pa")] public ErDouble pressure;

    [QuickTableField("volume", "cm^3")] public ErDouble volume;

    public VolumePressureData(){}
        
}

public struct TemperaturePressureData
{
    public ErDouble temperature;
    public ErDouble pressure;
}
public static class Part1_IsothermsAndCriticalPoints
{
    
    public static void Process()
    {
        var csvReader1 = new SimpleTableProtocolReader("ChamberDataTemp1.csv");
        List<VolumePressureData> dataListTemp1 = csvReader1.ExtractTable<VolumePressureData>("tab:ChamberData");
        dataListTemp1.ForEachRef((ref VolumePressureData data) =>
        {
            data.pressure.Error = 0.1;
            data.volume.Error = 0.02;
        });
        var csvReader2 = new SimpleTableProtocolReader("ChamberDataTemp2.csv");
        List<VolumePressureData> dataListTemp2 = csvReader2.ExtractTable<VolumePressureData>("tab:ChamberData");
        dataListTemp2.ForEachRef((ref VolumePressureData data) =>
        {
            data.pressure.Error = 0.1;
            data.volume.Error = 0.02;
        });
        var csvReader3 = new SimpleTableProtocolReader("ChamberDataTemp3.csv");
        List<VolumePressureData> dataListTemp3 = csvReader3.ExtractTable<VolumePressureData>("tab:ChamberData");
        dataListTemp3.ForEachRef((ref VolumePressureData data) =>
        {
            data.pressure.Error = 0.1;
            data.volume.Error = 0.02;
        });
        var csvReader4 = new SimpleTableProtocolReader("ChamberDataTemp4.csv");
        List<VolumePressureData> dataListTemp4 = csvReader4.ExtractTable<VolumePressureData>("tab:ChamberData");
        dataListTemp4.ForEachRef((ref VolumePressureData data) =>
        {
            data.pressure.Error = 0.1;
            data.volume.Error = 0.02;
        });
        var csvReader5 = new SimpleTableProtocolReader("ChamberDataTemp5.csv");
        List<VolumePressureData> dataListTemp5 = csvReader5.ExtractTable<VolumePressureData>("tab:ChamberData");
        dataListTemp5.ForEachRef((ref VolumePressureData data) =>
        {
            data.pressure.Error = 0.1;
            data.volume.Error = 0.02;
        });
        var csvReader6 = new SimpleTableProtocolReader("ChamberDataTemp6.csv");
        List<VolumePressureData> dataListTemp6 = csvReader6.ExtractTable<VolumePressureData>("tab:ChamberData");
        dataListTemp6.ForEachRef((ref VolumePressureData data) =>
        {
            data.pressure.Error = 0.1;
            data.volume.Error = 0.02;
        });
        var csvReader7 = new SimpleTableProtocolReader("ChamberDataTemp7.csv");
        List<VolumePressureData> dataListTemp7 = csvReader7.ExtractTable<VolumePressureData>("tab:ChamberData");
        dataListTemp7.ForEachRef((ref VolumePressureData data) =>
        {
            data.pressure.Error = 0.1;
            data.volume.Error = 0.02;
        });
        var csvReader8 = new SimpleTableProtocolReader("ChamberDataTemp8.csv");
        List<VolumePressureData> dataListTemp8 = csvReader8.ExtractTable<VolumePressureData>("tab:ChamberData");
        dataListTemp8.ForEachRef((ref VolumePressureData data) =>
        {
            data.pressure.Error = 0.1;
            data.volume.Error = 0.02;
        });
        var csvReader9 = new SimpleTableProtocolReader("ChamberDataTemp9.csv");
        List<VolumePressureData> dataListTemp9 = csvReader9.ExtractTable<VolumePressureData>("tab:ChamberData");
        dataListTemp9.ForEachRef((ref VolumePressureData data) =>
        {
            data.pressure.Error = 0.5;
            data.volume.Error = 0.02;
        });
        
        ScottPlot.Plot plot = ScottPlotExtensions.CreateSciPlot("volume", "pressure");
        plot.Palette = Palette.Category20;
        plot.AddErrorBars(dataListTemp1.Select(e => (e.volume, e.pressure)));
         plot.AddErrorBars(dataListTemp2.Select(e => (e.volume, e.pressure)));
         plot.AddErrorBars(dataListTemp3.Select(e => (e.volume, e.pressure)));
         plot.AddErrorBars(dataListTemp4.Select(e => (e.volume, e.pressure)));
         plot.AddErrorBars(dataListTemp5.Select(e => (e.volume, e.pressure)));
         plot.AddErrorBars(dataListTemp6.Select(e => (e.volume, e.pressure)));
         plot.AddErrorBars(dataListTemp7.Select(e => (e.volume, e.pressure)));
         plot.AddErrorBars(dataListTemp8.Select(e => (e.volume, e.pressure)));
         plot.AddErrorBars(dataListTemp9.Select(e => (e.volume, e.pressure)));
        
        List<VolumePressureData> dataForFit = new List<VolumePressureData>();
        CalculateMaxwellLine(dataListTemp1,plot,0.25,0.5,dataForFit,100);
         CalculateMaxwellLine(dataListTemp2,plot,0.25,0.5,dataForFit,1000);
         CalculateMaxwellLine(dataListTemp3,plot,0.25,0.4,dataForFit,1000);
         CalculateMaxwellLine(dataListTemp4,plot,0.5,0.5,dataForFit,1.0);
         CalculateMaxwellLine(dataListTemp5,plot,0.5,0.35,dataForFit,0.9);
        
        CalculateMaxwellLine(dataListTemp6,plot,0.2,0.3,dataForFit,100);
         var spline = CubicSpline.InterpolateAkima(dataForFit.Select(e => e.volume.Value), dataForFit.Select(e => e.pressure.Value));
         plot.AddFunction(x => spline.Interpolate(x),Color.Black);
         Console.WriteLine("extrema " + spline.Extrema());
         Console.WriteLine(spline.Interpolate(spline.Extrema().Item2));
         plot.AddPoint(spline.Extrema().Item2, spline.Interpolate(spline.Extrema().Item2),Color.Red,3F,MarkerShape.filledTriangleUp);
        
        
        //plot.AddRegModel(model, "data", "fitted function");
        plot.SaveAndAddCommand("fig:plot","caption");
        
        
        List<TemperaturePressureData> temperaturePressureForLogPlot = new List<TemperaturePressureData>();
        CreateTemperaturePressureData(dataForFit, csvReader1,temperaturePressureForLogPlot,0);
        CreateTemperaturePressureData(dataForFit, csvReader2,temperaturePressureForLogPlot,2);
        CreateTemperaturePressureData(dataForFit, csvReader3,temperaturePressureForLogPlot,4);
        CreateTemperaturePressureData(dataForFit, csvReader4,temperaturePressureForLogPlot,6);
        CreateTemperaturePressureData(dataForFit, csvReader5,temperaturePressureForLogPlot,8);
        
        ScottPlot.Plot temperaturePlot = ScottPlotExtensions.CreateSciPlot("temperature", "pressure");
        temperaturePlot.AddScatter(temperaturePressureForLogPlot.Select(e => e.temperature.Value).ToArray(),
            temperaturePressureForLogPlot.Select(e => e.pressure.Value).ToArray());
        temperaturePlot.SaveAndAddCommand("fig:temperaturePlot","caption");
        
        ScottPlot.Plot temperatureLogPlot = ScottPlotExtensions.CreateSciPlot("temperature", "pressure");
        temperatureLogPlot.AddScatter(
            temperaturePressureForLogPlot.Select(e => Math.Log(e.pressure.Value,Math.E)).ToArray(),
            temperaturePressureForLogPlot.Select(e => 1 / (e.temperature.Value)).ToArray());

        temperatureLogPlot.SaveAndAddCommand("fig:tempLogPlot","caption");
        
        


    }

    private static List<TemperaturePressureData> CreateTemperaturePressureData(List<VolumePressureData> data,
        SimpleTableProtocolReader reader,List<TemperaturePressureData> returnList,int index)
    {
        TemperaturePressureData e = new TemperaturePressureData();
        
            e.pressure = data[index].pressure.Value;
            e.temperature = reader.ExtractSingleValue<double>("temperature");
            returnList.Add(e);
            
        

        return returnList;
    }

    public static void CalculateMaxwellLine(List<VolumePressureData> data, ScottPlot.Plot plot,double sensitivity,double variance,List<VolumePressureData> dataForFit,double lowerlimit)
    {
        List<VolumePressureData> returnList = new List<VolumePressureData>();
        double sum = 0;
        double count = 0;
        for (int i = 0; i < data.Count-1; i++)
        {
            var e = new VolumePressureData();
            var m = new VolumePressureData();
            e = data[i];
            m = data[i + 1];
            if (Math.Abs(data[i+1].pressure.Value - data[i].pressure.Value) <= sensitivity && data[i].volume.Value <= lowerlimit)
            {
                sum += e.pressure.Value;
                returnList.Add(e);
                returnList.Add(m);
                Console.WriteLine(e.pressure.Value+" and data "+m.pressure.Value);
                count++;
            }
            Console.WriteLine(sum/count);
            
        }
        List<VolumePressureData> maxwellLine1 = CreateMaxwellLine(sum / count, returnList,variance,dataForFit);
        plot.AddScatter(maxwellLine1.Select(e => e.volume.Value).ToArray(),
            maxwellLine1.Select(e => e.pressure.Value).ToArray(), Color.Black, 2F,5F,MarkerShape.cross);

    }

    private static List<VolumePressureData> CreateMaxwellLine(double height,List<VolumePressureData> data,double variance,List<VolumePressureData> dataForFit)
    {
        List<VolumePressureData> twoPointList = new List<VolumePressureData>();
        VolumePressureData dataPoint1 = new VolumePressureData();
        List<VolumePressureData> cleanList = new List<VolumePressureData>();
        for (int i = 0; i < data.Count; i++)
        {
            var e = new VolumePressureData();
            e = data[i];
            if (Math.Abs(data[i].pressure.Value - height) <= variance)
            {
                cleanList.Add(e);
            }
        }
        dataPoint1.pressure = height;
        dataPoint1.volume = cleanList[0].volume;
        Console.WriteLine(dataPoint1.volume +"and"+ dataPoint1.pressure);
        VolumePressureData dataPoint2 = new VolumePressureData();
        dataPoint2.pressure = height;
        dataPoint2.volume = cleanList[cleanList.Count-1].volume;
        Console.WriteLine(dataPoint2.volume + "and" + dataPoint2.pressure);
        twoPointList.Add(dataPoint1);
        twoPointList.Add(dataPoint2);
        //Adding the edge points to the dataForFit List which we will use later
        dataForFit.Add(dataPoint1);
        dataForFit.Add(dataPoint2);
        return twoPointList;
    }
}