using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.Net.Sockets;
using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.QuickTable;
using Mantis.Core.ScottPlotUtility;
using Mantis.Core.Utility;
using ScottPlot;

namespace Mantis.Workspace.C1_Trials.V31_RealGasStateVariables;

[QuickTable("", "ChamberData")]
public record struct VolumePressureData
{
    [QuickTableField("pressure", "Pa")] public ErDouble pressure;

    [QuickTableField("volume", "cm^3")] public ErDouble volume;

    public VolumePressureData(){}
        
}

public static class Part1_IsothermsAndCriticalPoints
{
    
    public static void Process()
    {
        var csvReader1 = new SimpleTableProtocolReader("ChamberDataTemp1.csv");
        List<VolumePressureData> dataListTemp1 = csvReader1.ExtractTable<VolumePressureData>("tab:ChamberData");
        
        var csvReader2 = new SimpleTableProtocolReader("ChamberDataTemp2.csv");
        List<VolumePressureData> dataListTemp2 = csvReader2.ExtractTable<VolumePressureData>("tab:ChamberData");
        
        var csvReader3 = new SimpleTableProtocolReader("ChamberDataTemp3.csv");
        List<VolumePressureData> dataListTemp3 = csvReader3.ExtractTable<VolumePressureData>("tab:ChamberData");
        
        var csvReader4 = new SimpleTableProtocolReader("ChamberDataTemp4.csv");
        List<VolumePressureData> dataListTemp4 = csvReader4.ExtractTable<VolumePressureData>("tab:ChamberData");
        
        var csvReader5 = new SimpleTableProtocolReader("ChamberDataTemp5.csv");
        List<VolumePressureData> dataListTemp5 = csvReader5.ExtractTable<VolumePressureData>("tab:ChamberData");
        
        var csvReader6 = new SimpleTableProtocolReader("ChamberDataTemp6.csv");
        List<VolumePressureData> dataListTemp6 = csvReader6.ExtractTable<VolumePressureData>("tab:ChamberData");
        
        var csvReader7 = new SimpleTableProtocolReader("ChamberDataTemp7.csv");
        List<VolumePressureData> dataListTemp7 = csvReader7.ExtractTable<VolumePressureData>("tab:ChamberData");
        
        var csvReader8 = new SimpleTableProtocolReader("ChamberDataTemp8.csv");
        List<VolumePressureData> dataListTemp8 = csvReader8.ExtractTable<VolumePressureData>("tab:ChamberData");
        
        var csvReader9 = new SimpleTableProtocolReader("ChamberDataTemp9.csv");
        List<VolumePressureData> dataListTemp9 = csvReader9.ExtractTable<VolumePressureData>("tab:ChamberData");

        ScottPlot.Plot plot = ScottPlotExtensions.CreateSciPlot("volume", "pressure");
        plot.AddScatter(dataListTemp1.Select(e => e.volume.Value).ToArray(),
            dataListTemp1.Select(e => e.pressure.Value).ToArray());
        plot.AddScatter(dataListTemp2.Select(e => e.volume.Value).ToArray(),
            dataListTemp2.Select(e => e.pressure.Value).ToArray());
        plot.AddScatter(dataListTemp3.Select(e => e.volume.Value).ToArray(),
            dataListTemp3.Select(e => e.pressure.Value).ToArray());
        plot.AddScatter(dataListTemp4.Select(e => e.volume.Value).ToArray(),
            dataListTemp4.Select(e => e.pressure.Value).ToArray());
        plot.AddScatter(dataListTemp5.Select(e => e.volume.Value).ToArray(),
            dataListTemp5.Select(e => e.pressure.Value).ToArray());
        plot.AddScatter(dataListTemp6.Select(e => e.volume.Value).ToArray(),
            dataListTemp6.Select(e => e.pressure.Value).ToArray());
        plot.AddScatter(dataListTemp7.Select(e => e.volume.Value).ToArray(),
            dataListTemp7.Select(e => e.pressure.Value).ToArray());
        plot.AddScatter(dataListTemp8.Select(e => e.volume.Value).ToArray(),
            dataListTemp8.Select(e => e.pressure.Value).ToArray());
        plot.AddScatter(dataListTemp9.Select(e => e.volume.Value).ToArray(),
            dataListTemp9.Select(e => e.pressure.Value).ToArray());
       
        CalculateMaxwellLine(dataListTemp1,plot,0.2);
        CalculateMaxwellLine(dataListTemp2,plot,0.2);
        CalculateMaxwellLine(dataListTemp3,plot,0.15);
        CalculateMaxwellLine(dataListTemp4,plot,0.1);
        CalculateMaxwellLine(dataListTemp5,plot,0);
        CalculateMaxwellLine(dataListTemp6,plot,0.01);
        
        

    }

    public static void CalculateMaxwellLine(List<VolumePressureData> data, ScottPlot.Plot plot,double sensitivity)
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
            if (data[i+1].pressure.Value - data[i].pressure.Value <= sensitivity)
            {
                sum += e.pressure.Value;
                returnList.Add(e);
                returnList.Add(m);
                count++;
            }
            Console.WriteLine(sum/count);
        }
        List<VolumePressureData> maxwellLine1 = CreateMaxwellLine(sum / count, returnList[0].volume.Value, returnList[returnList.Count-1].volume.Value);
        plot.AddScatter(maxwellLine1.Select(e => e.volume.Value).ToArray(),
            maxwellLine1.Select(e => e.pressure.Value).ToArray(), Color.Black, 2F,5F,MarkerShape.cross);
        plot.SaveAndAddCommand("fig:plot","caption");
        
    }

    private static List<VolumePressureData> CreateMaxwellLine(double height,double position1, double position2)
    {
        List<VolumePressureData> twoPointList = new List<VolumePressureData>();
        VolumePressureData dataPoint1 = new VolumePressureData();
        dataPoint1.pressure = height;
        dataPoint1.volume = position1;
        Console.WriteLine(dataPoint1.volume +"and"+ dataPoint1.pressure);
        VolumePressureData dataPoint2 = new VolumePressureData();
        dataPoint2.pressure = height;
        dataPoint2.volume = position2;
        Console.WriteLine(dataPoint2.volume + "and" + dataPoint2.pressure);
        twoPointList.Add(dataPoint1);
        twoPointList.Add(dataPoint2);
        return twoPointList;
    }
}