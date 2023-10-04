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
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Interpolation;
using MathNet.Numerics.LinearAlgebra;
using ScottPlot;
using ScottPlot.Plottable;
using ScottPlot.Renderable;

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

public class InvExpFunc : AutoDerivativeFunc,IFixedParameterCount
{
    public override double CalculateResult(Vector<double> p, double x)
    {
        return p[0] * Math.Exp(p[1] / x);
    }

    public int ParameterCount => 2;
}

public static class Part1_IsothermsAndCriticalPoints
{
    
    public static void Process()
    {
        var csvReader1 = new SimpleTableProtocolReader("ChamberDataTemp1.csv");
        List<VolumePressureData> dataListTemp1 = csvReader1.ExtractTable<VolumePressureData>("tab:ChamberData");
        dataListTemp1.ForEachRef((ref VolumePressureData data) =>
        {
            data.pressure.Error = 0.25;
            data.volume.Error = 0.02;
        });
        var csvReader2 = new SimpleTableProtocolReader("ChamberDataTemp2.csv");
        List<VolumePressureData> dataListTemp2 = csvReader2.ExtractTable<VolumePressureData>("tab:ChamberData");
        dataListTemp2.ForEachRef((ref VolumePressureData data) =>
        {
            data.pressure.Error = 0.25;
            data.volume.Error = 0.02;
        });
        var csvReader3 = new SimpleTableProtocolReader("ChamberDataTemp3.csv");
        List<VolumePressureData> dataListTemp3 = csvReader3.ExtractTable<VolumePressureData>("tab:ChamberData");
        dataListTemp3.ForEachRef((ref VolumePressureData data) =>
        {
            data.pressure.Error = 0.25;
            data.volume.Error = 0.02;
        });
        var csvReader4 = new SimpleTableProtocolReader("ChamberDataTemp4.csv");
        List<VolumePressureData> dataListTemp4 = csvReader4.ExtractTable<VolumePressureData>("tab:ChamberData");
        dataListTemp4.ForEachRef((ref VolumePressureData data) =>
        {
            data.pressure.Error = 0.25;
            data.volume.Error = 0.02;
        });
        var csvReader5 = new SimpleTableProtocolReader("ChamberDataTemp5.csv");
        List<VolumePressureData> dataListTemp5 = csvReader5.ExtractTable<VolumePressureData>("tab:ChamberData");
        dataListTemp5.ForEachRef((ref VolumePressureData data) =>
        {
            data.pressure.Error = 0.25;
            data.volume.Error = 0.02;
        });
        var csvReader6 = new SimpleTableProtocolReader("ChamberDataTemp6.csv");
        List<VolumePressureData> dataListTemp6 = csvReader6.ExtractTable<VolumePressureData>("tab:ChamberData");
        dataListTemp6.ForEachRef((ref VolumePressureData data) =>
        {
            data.pressure.Error = 0.25;
            data.volume.Error = 0.02;
        });
        var csvReader7 = new SimpleTableProtocolReader("ChamberDataTemp7.csv");
        List<VolumePressureData> dataListTemp7 = csvReader7.ExtractTable<VolumePressureData>("tab:ChamberData");
        dataListTemp7.ForEachRef((ref VolumePressureData data) =>
        {
            data.pressure.Error = 0.25;
            data.volume.Error = 0.02;
        });
        var csvReader8 = new SimpleTableProtocolReader("ChamberDataTemp8.csv");
        List<VolumePressureData> dataListTemp8 = csvReader8.ExtractTable<VolumePressureData>("tab:ChamberData");
        dataListTemp8.ForEachRef((ref VolumePressureData data) =>
        {
            data.pressure.Error = 0.25;
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
        plot.AddErrorBars(dataListTemp1.Select(e => (e.volume, e.pressure)),label:"30.2");
        plot.AddErrorBars(dataListTemp2.Select(e => (e.volume, e.pressure)),label:"35.1");
         plot.AddErrorBars(dataListTemp3.Select(e => (e.volume, e.pressure)),label:"40.1");
         plot.AddErrorBars(dataListTemp4.Select(e => (e.volume, e.pressure)),label:"43.1");
         plot.AddErrorBars(dataListTemp5.Select(e => (e.volume, e.pressure)),label:"45.1");
         plot.AddErrorBars(dataListTemp6.Select(e => (e.volume, e.pressure)),label:"46.1");
         plot.AddErrorBars(dataListTemp7.Select(e => (e.volume, e.pressure)),label:"46.6");
         plot.AddErrorBars(dataListTemp8.Select(e => (e.volume, e.pressure)),label:"47.2");
         plot.AddErrorBars(dataListTemp9.Select(e => (e.volume, e.pressure)),label:"48.2");
        
        List<VolumePressureData> dataForFit = new List<VolumePressureData>();
        CalculateMaxwellLine(dataListTemp1,plot,0.25,0.5,dataForFit,100);
         CalculateMaxwellLine(dataListTemp2,plot,0.25,0.5,dataForFit,1000);
         CalculateMaxwellLine(dataListTemp3,plot,0.25,0.4,dataForFit,1000);
         CalculateMaxwellLine(dataListTemp4,plot,0.5,0.5,dataForFit,1.0);
         CalculateMaxwellLine(dataListTemp5,plot,0.5,0.35,dataForFit,0.9);
        
        CalculateMaxwellLine(dataListTemp6,plot,0.2,0.3,dataForFit,100);
         var spline = CubicSpline.InterpolateAkima(dataForFit.Select(e => e.volume.Value), dataForFit.Select(e => e.pressure.Value));
         var f = plot.AddFunction(x => spline.Interpolate(x), Color.Black,2D);
         f.Label = "interpolated binodal";
         f.XMax = 1.650;
         f.XMin = 0.375;
         Console.WriteLine("extrema " + spline.Extrema());
         Console.WriteLine(spline.Interpolate(spline.Extrema().Item2));
         plot.AddPoint(spline.Extrema().Item2, spline.Interpolate(spline.Extrema().Item2),Color.Red,5F,MarkerShape.filledTriangleUp,label:"Critical point");

         ErDouble criticalVolume = new ErDouble(spline.Extrema().Item2, 0.05);
         ErDouble criticalPressure = new ErDouble(spline.Interpolate(spline.Extrema().Item2), 0.5);
         ErDouble criticalTemperature = new ErDouble(46.3, 0.3);
         criticalPressure.AddCommand("criticalPressure","bar");
         criticalVolume.AddCommand("criticalVolume","cm^3");
         
         CalculateA(criticalPressure,criticalVolume,criticalTemperature).AddCommand("parameterA");
         CalculateB(criticalPressure,criticalVolume,criticalTemperature).AddCommand("parameterB");
         CalculateNu(CalculateB(criticalPressure, criticalVolume, criticalTemperature), criticalVolume).AddCommand("amountOfSubstance");
        //plot.AddRegModel(model, "data", "fitted function");
        plot.Legend(true,Alignment.UpperRight);
        plot.SaveAndAddCommand("fig:plot","caption");
        plot.SetAxisLimits(0.5,0.8,33,38);
        plot.SaveAndAddCommand("plotZoom","caption");
        
        
        List<TemperaturePressureData> temperaturePressureForLogPlot = new List<TemperaturePressureData>();
        CreateTemperaturePressureData(dataForFit, csvReader1,temperaturePressureForLogPlot,0);
        CreateTemperaturePressureData(dataForFit, csvReader2,temperaturePressureForLogPlot,2);
        CreateTemperaturePressureData(dataForFit, csvReader3,temperaturePressureForLogPlot,4);
        CreateTemperaturePressureData(dataForFit, csvReader4,temperaturePressureForLogPlot,6);
        CreateTemperaturePressureData(dataForFit, csvReader5,temperaturePressureForLogPlot,8);
        
        ScottPlot.Plot temperaturePlot = ScottPlotExtensions.CreateSciPlot("temperature", "pressure");
        temperaturePlot.AddErrorBars(temperaturePressureForLogPlot.Select(e=>(e.temperature,e.pressure)));
        temperaturePlot.SaveAndAddCommand("fig:temperaturePlot","caption");
        
        ScottPlot.Plot temperatureLogPlot = ScottPlotExtensions.CreateSciPlot("inverse temperature", "pressure");
        
        //temperatureLogPlot.AddErrorBars(
        //    temperaturePressureForLogPlot.Select(e => ((e.temperature+273.15).Pow(-1),e.pressure)));

        
        
        RegModel<InvExpFunc> expoModel = temperaturePressureForLogPlot.CreateRegModel(e=>((273.15+e.temperature),e.pressure),
            new ParaFunc<InvExpFunc>(2)
        {
            Units = new[]{"",""}
        }
            );
        expoModel.DoRegressionLevenbergMarquardt(new double[] { 1,1 }, false);
        expoModel.AddParametersToPreambleAndLog("ExpoData");
        temperatureLogPlot.AddRegModel(expoModel, "inverse temp", "log of pressure ", false);
      // temperatureLogPlot.SetAxisLimits(0,0.2,22,38);

        temperatureLogPlot.SaveAndAddCommand("fig:tempLogPlot","caption");
        expoModel.ErParameters[1].AddCommand("regressionExponent");
        CalculateEvaporationEnergy(expoModel.ErParameters[1]).AddCommand("evaporationEnergy");
        CalculateEvaporationEnergy(expoModel.ErParameters[1]*6.22*Math.Pow(10,23)*(-1)).AddCommand("evaporationEnergyPerMole");
        CalculateCriticalTemp(expoModel.ErParameters[1], criticalPressure,
            expoModel.ErParameters[0]).AddCommand("criticalTemp");
        //comparing with theory
        double T = 30.2 + 273.15;//K
        double R = 83.144;//bar*cm^3/K*mol
        double b = CalculateB(criticalPressure, criticalVolume, criticalTemperature).Value * Math.Pow(10, 6);//cm^3/mol
        double a = CalculateA(criticalPressure, criticalVolume, criticalTemperature).Value * Math.Pow(10, 7);//bar*cm^6/mol^2
        double nu = CalculateNu(CalculateB(criticalPressure, criticalVolume, criticalTemperature), criticalVolume).Value;//mol
        Console.WriteLine("a=" + a + " b= " + b + " nu= " + nu );
        
        ScottPlot.Plot comparationPlot = ScottPlotExtensions.CreateSciPlot("volume[cm^3]", "pressure[bar]");
        var idealGas = new Func<double, double?>((v) => R * T * nu / v);
        var vdWGas = new Func<double, double?>((v) => nu * R * T / (v - nu * b) - nu * nu * a / (v * v));
        comparationPlot.AddFunction(idealGas, Color.Red);
        var funkiton1 = comparationPlot.AddFunction(vdWGas, Color.Blue);
        comparationPlot.AddErrorBars(dataListTemp1.Select(e => (e.volume, e.pressure)),Color.Black);
        comparationPlot.SetAxisLimits(0,3.5,12.5,47.5);
        funkiton1.XMin = 0.3;
        comparationPlot.SaveAndAddCommand("comparationPlot","caption");
    }

    private static List<TemperaturePressureData> CreateTemperaturePressureData(List<VolumePressureData> data,
        SimpleTableProtocolReader reader,List<TemperaturePressureData> returnList,int index)
    {
        TemperaturePressureData e = new TemperaturePressureData();
        
            e.pressure = data[index].pressure;
            e.pressure.Error=0.5;
            e.temperature = reader.ExtractSingleValue<double>("temperature");
            e.temperature.Error = 0.2;
            returnList.Add(e);
            return returnList;
    }

    private static ErDouble CalculateCriticalTemp(ErDouble b, ErDouble p, ErDouble expoRegParameter0)
    {
        return (b / ErDouble.Log(p / expoRegParameter0,Math.E))-273.15;//degC
    }
    private static ErDouble CalculateEvaporationEnergy(ErDouble regressionParameter)
    {
        return regressionParameter * 1.380694 * Math.Pow(10, -23);
    }

    public static ErDouble CalculateB(ErDouble pk, ErDouble vk, ErDouble tk)
    {
        Console.WriteLine(pk+ " "+vk+" "+tk);
        return ((tk+273.15) / pk) * (83.144 / 8) * Math.Pow(10, -6);//m^3/mol
    }

    public static ErDouble CalculateNu(ErDouble b, ErDouble vk)
    {
        return vk*Math.Pow(10,-6) / (b*3);//mol
    }
    public static ErDouble CalculateA(ErDouble pk, ErDouble vk, ErDouble tk)
    {
        ErDouble b = CalculateB(pk, vk, tk);
        return pk * 27 * b.Pow(2)*Math.Pow(10,5);//Pa*m^6/mol^2
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