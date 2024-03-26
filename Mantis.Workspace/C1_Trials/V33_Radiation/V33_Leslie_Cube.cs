﻿using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.QuickTable;
using Mantis.Core.ScottPlotUtility;
using Mantis.Core.TexIntegration;
using Mantis.Core.Utility;
using MathNet.Numerics.LinearAlgebra;
using ScottPlot;
using ScottPlot.Legends;
using SkiaSharp;

namespace Mantis.Workspace.C1_Trials.V33_Radiation;
[QuickTable("", "LeslieData")]

public record struct TempRadiationData
{
    [QuickTableField("temperature", "C")] public ErDouble temperature;
    [QuickTableField("polished","")] public ErDouble polished;
    [QuickTableField("white", "")] public ErDouble white;
    [QuickTableField("matt", "")] public ErDouble matt;
    [QuickTableField("black", "")] public ErDouble black;
        
    public TempRadiationData()
    {
    }
}

public class QuattroFit : AutoDerivativeFunc, IFixedParameterCount
{
    private readonly double temperatureZero;

    public QuattroFit(double temperatureZero)
    {
        this.temperatureZero = temperatureZero;
    }

    public override double CalculateResult(Vector<double> p, double x)
    {
        return p[0] * (Math.Pow(x, p[1]) - Math.Pow(temperatureZero, p[1]));
    }

    public int ParameterCount => 2;
}
public class QuattroFitPlusConstant : AutoDerivativeFunc, IFixedParameterCount
{
    private readonly double temperatureZero;

    public QuattroFitPlusConstant(double temperatureZero)
    {
        this.temperatureZero = temperatureZero;
    }

    public override double CalculateResult(Vector<double> p, double x)
    {
        return p[0] * (Math.Pow(x, p[1]) - Math.Pow(temperatureZero+ p[2], p[1])) ;
    }

    public int ParameterCount => 3;
}
public class V33_Leslie_Cube
{
    public static void Process()
    {

        var csvReader = new SimpleTableProtocolReader("LeslieCubeData.csv");
        List<TempRadiationData> dataList = csvReader.ExtractTable<TempRadiationData>("tab:LeslieCubeData");
        ErDouble t0 = csvReader.ExtractSingleValue<ErDouble>("temperature0");
        
        dataList.ForEachRef(((ref TempRadiationData data) =>
            data.temperature.Value += 273.15));
        
        
        // Here we generate the first two plots
        DynPlot plot = new DynPlot("Temperature [K]", "Voltage [mV]");
        GenerateFirstPlot(dataList,plot);
        GenerateT4Plot(dataList);
        // Now we fit the Exponent in order to verify the Stefan Bolzmann radiation Law
        PleaseFitTheQuattroFit(dataList,plot);
        

    }
    public static void GenerateFirstPlot(List<TempRadiationData> dataList,DynPlot plot)
    {
            
            plot.AddDynErrorBar(dataList.Select(e => (e.temperature, e.polished)));
            plot.AddDynErrorBar(dataList.Select(e => (e.temperature, e.white)));
            plot.AddDynErrorBar(dataList.Select(e => (e.temperature, e.matt)));
            plot.AddDynErrorBar(dataList.Select(e => (e.temperature, e.black)));
            //plot.SaveAndAddCommand("StefanBolzmannPlot");
    }
    public static void GenerateT4Plot(List<TempRadiationData> dataList)
    {
        var csvReader = new SimpleTableProtocolReader("LeslieCubeData.csv");
        ErDouble t0 = csvReader.ExtractSingleValue<ErDouble>("temperature0"); 
        DynPlot plot = new DynPlot("temp", "Radiation");
        Console.WriteLine("T0 read from csv file: "+t0);
        plot.AddDynErrorBar(dataList.Select(e => (e.temperature.Pow(4)-t0.Pow(4), e.polished)));
        plot.AddDynErrorBar(dataList.Select(e => (e.temperature.Pow(4)-t0.Pow(4), e.white)));
        plot.AddDynErrorBar(dataList.Select(e => (e.temperature.Pow(4)-t0.Pow(4), e.matt)));
        plot.AddDynErrorBar(dataList.Select(e => (e.temperature.Pow(4)-t0.Pow(4), e.black)));
        plot.SaveAndAddCommand("T4Plot");
    }
    public static void PleaseFitTheQuattroFit(List<TempRadiationData> dataList,DynPlot plot)
    {
        var reader = new SimpleTableProtocolReader("LeslieCubeData");
        double temperatureZero = reader.ExtractSingleValue<double>("temperature0");
        
        RegModel QuattroFunc = dataList.CreateRegModel(e=>(e.temperature, e.white),
            new ParaFunc(2,new QuattroFit(temperatureZero))
            {
                Units = new []{"",""}
            }
        );
        QuattroFunc.DoRegressionLevenbergMarquardt(new double[] { 4, 4 }, false);
        QuattroFunc.ErParameters[1].AddCommand("ExponentWhite");
        Console.WriteLine(QuattroFunc.ErParameters[1]);
        plot.AddRegModel(QuattroFunc,null,"White surface",Color.FromSKColor(SKColors.Yellow));
        QuattroFunc = dataList.CreateRegModel(e=>(e.temperature, e.matt),
            new ParaFunc(2,new QuattroFit(temperatureZero))
            {
                Units = new []{"",""}
            }
        );
        QuattroFunc.DoRegressionLevenbergMarquardt(new double[] { 4, 4 }, false);
        QuattroFunc.ErParameters[1].AddCommand("ExponentMatt");
        Console.WriteLine(QuattroFunc.ErParameters[1]);
        plot.AddRegModel(QuattroFunc,null,"Matte surface",Color.FromSKColor(SKColors.Green));

        QuattroFunc = dataList.CreateRegModel(e=>(e.temperature, e.black),
            new ParaFunc(2,new QuattroFit(temperatureZero))
            {
                Units = new []{"",""}
            }
        );
        QuattroFunc.DoRegressionLevenbergMarquardt(new double[] { 4, 4 }, false);
        QuattroFunc.ErParameters[1].AddCommand("ExponentBlack");
        Console.WriteLine(QuattroFunc.ErParameters[1]);
        plot.AddRegModel(QuattroFunc,null,"Black surface",Color.FromSKColor(SKColors.Blue));

        QuattroFunc = dataList.CreateRegModel(e=>(e.temperature, e.polished),
            new ParaFunc(3,new QuattroFitPlusConstant(temperatureZero))
            {
                Units = new []{"","",""}
            }
        );
        QuattroFunc.DoRegressionLevenbergMarquardt(new double[] { 1,1,1}, false);
        QuattroFunc.ErParameters[1].AddCommand("ExponentPolished");
        Console.WriteLine(QuattroFunc.ErParameters[1]);
        plot.AddRegModel(QuattroFunc,null,"Polished surface",Color.FromSKColor(SKColors.Red));
        var legend = plot.Legend;
        legend.Location = Alignment.UpperLeft;
        
        plot.SaveAndAddCommand("StefanBolzmannPlot");
    }
}