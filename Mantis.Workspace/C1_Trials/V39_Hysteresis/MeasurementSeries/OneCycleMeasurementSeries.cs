﻿using Mantis.Core.Calculator;
using Mantis.Core.ScottPlotUtility;
using MathNet.Numerics.Interpolation;
using ScottPlot;
using ScottPlot.Plottable;

namespace Mantis.Workspace.C1_Trials.V39_Hysteresis;


public record struct HBData(double H, double B) : IComparable<HBData>
{
    public int CompareTo(HBData other)
    {
        int c = H.CompareTo(other.H);
        if (c == 0)
            c = B.CompareTo(other.B);
        return c;
    }
}

public class OneCycleMeasurementSeries : HysteresisMeasurementSeries
{
    public readonly HBData[] HBPositiveList;
    public readonly HBData[] HBNegativeList;
    
    public ErDouble? Coercivity = null;
    private PlotEvalDataInfo? _coercifityPlotInfo = null;
    public ErDouble? Remanence = null;
    private PlotEvalDataInfo? _remanencePlotInfo = null;
    public ErDouble? Saturation = null;
    private PlotEvalDataInfo? _saturationPlotInfo = null;

    public ErDouble? HysteresisLoss = null;
    
    internal OneCycleMeasurementSeries(string name,List<PascoData> rawData,MeasurementSeriesInfo seriesInfo,RingCore ringCore,double errorVoltage,bool removeDrift,bool centerData) 
        : base(name, rawData, seriesInfo, ringCore,errorVoltage, removeDrift, centerData)
    {
        var (_HBPositiveList, _HBNegativeList) = FindPositiveAndNegativeData();
        HBPositiveList = _HBPositiveList.ToArray();
        HBNegativeList = _HBNegativeList.ToArray();
    }
    
    public void CalculateCoercivity()
    {

        var (coercivityPositivePreShif,_) = CalculateCoercivity(HBPositiveList);
        var (coercivityNegativePreShift,_) = CalculateCoercivity(HBNegativeList);
        
        ShiftData(0.5 * (coercivityPositivePreShif.Value + coercivityNegativePreShift.Value),0);
        
        var (coercivityPositive,coercivityPositiveInfo) = CalculateCoercivity(HBPositiveList);
        var (coercivityNegative,coercivityNegativeInfo) = CalculateCoercivity(HBNegativeList);
        Coercivity = (-coercivityPositive + coercivityNegative) / 2;
        _coercifityPlotInfo = new PlotEvalDataInfo(coercivityPositiveInfo, coercivityNegativeInfo,
            "Line fit for\nevaluation coercivity", "Coercivity");
        
    }

    private (ErDouble,PlotRegInfo) CalculateCoercivity(HBData[] points)
    {

        if (TryFitModel(points, -SeriesInfo.CoercivityEvalMax, SeriesInfo.CoercivityEvalMax, 1,false,
                out RegModel<LineFunc> model))
        {
            ErDouble coercivity = -model.ErParameters[0] / model.ErParameters[1];
            PlotRegInfo info = new PlotRegInfo(model, coercivity.Value, 0);
            return (coercivity, info);
        }
        else
            throw new ArgumentException("You have to set coercivity limits");

    }

    public void CalculateRemanence()
    {
        var (remanencePositive,remanencePositiveInfo) = CalculateRemanence(HBPositiveList,1);
        var (remanenceNegative,remanenceNegativeInfo) = CalculateRemanence(HBNegativeList,-1);
        Remanence = (remanencePositive - remanenceNegative) / 2;
        _remanencePlotInfo = new PlotEvalDataInfo(remanencePositiveInfo, remanenceNegativeInfo,
            "Line fit for\nevaluating remanence", "Remanence");
    }

    private (ErDouble,PlotRegInfo) CalculateRemanence(HBData[] points, int sign)
    {
        if (TryFitModel(points, SeriesInfo.RemanenceEvalMin , SeriesInfo.RemanenceEvalMax ,sign, SeriesInfo.IsRemanenceEvalHaxis,
                out RegModel<LineFunc> model))
        {
            ErDouble remanence = model.ErParameters[0];
            PlotRegInfo info = new PlotRegInfo(model,0,  remanence.Value);
            return (remanence, info);
        }
        else
            throw new ArgumentException("You have to set coercivity limits");
    }

    public void CalculateSaturation()
    {
        try
        {
            var (saturationPositive, saturationPositiveInfo) = CalculateSaturation(HBPositiveList, 1);
            var (saturationNegative, saturationNegativeInfo) = CalculateSaturation(HBNegativeList, -1);
            Saturation = (saturationPositive - saturationNegative) / 2;
            _saturationPlotInfo = new PlotEvalDataInfo(saturationPositiveInfo, saturationNegativeInfo,
                "Line fit for\nevaluating saturation", "Saturation");

        }catch(ArgumentException){}
    }
    
    private (ErDouble,PlotRegInfo) CalculateSaturation(HBData[] points, int sign)
    {
        if(SeriesInfo.SaturationEvalMin == 0)
            throw new ArgumentException("You have to set saturation limits");
        
        if (TryFitModel(points, SeriesInfo.SaturationEvalMin , double.PositiveInfinity,sign , true,
                out RegModel<LineFunc> model))
        {
            ErDouble saturation = model.ErParameters[0];
            Console.WriteLine("mu "+model.ErParameters[1]);
            PlotRegInfo info = new PlotRegInfo(model,0, saturation.Value);
            return (saturation, info);
        }
        else
            throw new ArgumentException("You have to set coercivity limits");
    }

    private bool TryFitModel(HBData[] points, double min, double max,int sign, bool isHAxis,out RegModel<LineFunc> model)
    {
        if (min == 0 && max == 0)
        {
            model = null;
            return false;
        }
        
        List<HBData> regressionPoints = SelectDataInRange(points, min, max,sign, isHAxis);

        model = regressionPoints.CreateRegModel(e => (e.H, e.B), new ParaFunc<LineFunc>(2));
        model.DoLinearRegression(false);
        return true;
    }

    public void CalculateHysteresisLoss()
    {
        double positiveIntegralMin = CalculateStepIntegralLower(HBPositiveList);
        double positiveIntegralMax = CalculateStepIntegralUpper(HBPositiveList);
        double negativeIntegralMin = CalculateStepIntegralLower(HBNegativeList);
        double negativeIntegralMax = CalculateStepIntegralUpper(HBNegativeList);

        double areaUnderCurveMin = Math.Abs(positiveIntegralMin - negativeIntegralMax);
        double areaUnderCurveMax = Math.Abs(positiveIntegralMax - negativeIntegralMin);
        
        ErDouble areaUnderCurve = 0.5 * (areaUnderCurveMax+areaUnderCurveMin);

        areaUnderCurve.Error = 0.5 * Math.Abs(areaUnderCurveMax - areaUnderCurveMin);
        double errorFromErrorB = ErrorB * (HBPositiveList[^1].H - HBPositiveList[0].H);
        areaUnderCurve.Error = Math.Max(areaUnderCurve.Error, errorFromErrorB);
        HysteresisLoss = areaUnderCurve  / RingCore.Density.Mul10E(3) ;
        
    }

    private double CalculateStepIntegralUpper(HBData[] data)
    {
        var stepInterpolation = SelectInterpolationMonotonousAscendingB(data);
        return stepInterpolation.Integrate(data[0].H, data[^1].H);
    }

    private double CalculateStepIntegralLower(HBData[] data)
    {
        var stepInterpolation = SelectInterpolationMonotonousDescendingReverseB(data);
        return stepInterpolation.Integrate(-data[^1].H, -data[0].H);
    }

    private StepInterpolation SelectInterpolationMonotonousAscendingB(HBData[] data)
    {
        double[] hs = new double[data.Length];
        double[] bs = new double[data.Length];

        double b = double.NegativeInfinity;
        for (int i = 0; i < data.Length; i++)
        {
            hs[i] = data[i].H;
            b = Math.Max(b, data[i].B);
            bs[i] = b;
        }
        
        
        return StepInterpolation.InterpolateSorted(hs,bs);
    }
    
    private StepInterpolation SelectInterpolationMonotonousDescendingReverseB(HBData[] data)
    {
        double[] hs = new double[data.Length];
        double[] bs = new double[data.Length];

        double b = double.PositiveInfinity;
        for (int i = 0; i < data.Length;i++)
        {
            hs[i] = -data[data.Length - i - 1].H;
            b = Math.Min(b, data[data.Length - i - 1].B);
            bs[i] = b;
        }

        return StepInterpolation.InterpolateSorted(hs,bs);
    }
    


    public void PlotData(Plot plt, bool drawCalculatedValues = true, bool drawRegCoercivity = false,
        bool drawRegRemanence = false, bool drawRegSaturation = false, bool drawRegPoints = false)
    {
        base.PlotData(plt);
        
        if(_coercifityPlotInfo != null) _coercifityPlotInfo.Plot(plt,drawRegPoints,drawRegCoercivity,drawCalculatedValues);
        if(_remanencePlotInfo != null) _remanencePlotInfo.Plot(plt,drawRegPoints,drawRegRemanence,drawCalculatedValues);
        if(_saturationPlotInfo != null) _saturationPlotInfo.Plot(plt,drawRegPoints,drawRegSaturation,drawCalculatedValues);
        //AddHBData(plt, HBNegativeList, "Negative HB Data");
            
        Console.WriteLine($"{Name}-{base.RingCore.Name}\tRem {Remanence.ToString()}\tCoer {Coercivity.ToString()}\tSat {Saturation.ToString()} Los: {HysteresisLoss.ToString()}");


    }
    
    private ScatterPlot AddHBData(Plot plt,HBData[] data,string legend)
    {
        if (data.Length == 0) return null;
        var xs = data.Select(e => CheckDouble(e.H)).ToArray();
        var ys = data.Select(e => CheckDouble(e.B)).ToArray();
        
        return plt.AddScatter(xs, ys, markerSize: 1, lineStyle: LineStyle.None,label:legend);
    }

    private static double CheckDouble(double v)
    {

        return double.IsFinite(v) && !double.IsNaN(v) ? v : 0;
    }

    private (List<HBData>,List<HBData>) FindPositiveAndNegativeData()
    {
        double minH = DataList[0].H;
        int _minHIndex = 0;
        for (int i = 0; i < DataList.Length; i++)
        {
            if (DataList[i].H < minH)
            {
                minH = DataList[i].H;
                _minHIndex = i;
            }
        }


        List<HBData> negativeData = new List<HBData>(_minHIndex + 2);
        for (int i = _minHIndex; i >= 0; i--)
        {
            negativeData.Add( new HBData(DataList[i].H, DataList[i].B));
        }

        negativeData.Sort();

        List<HBData> positiveData = new List<HBData>(DataList.Length - _minHIndex + 1);
        for (int i = _minHIndex; i < DataList.Length; i++)
        {
            positiveData.Add(new HBData(DataList[i].H, DataList[i].B));
        }

        positiveData.Sort();
        
        if(negativeData[^1].H > positiveData[^1].H)
            positiveData.Add(negativeData[^1]);
        else
            negativeData.Add(positiveData[^1]);
        
        return (negativeData, positiveData);
    }
    
    private void ShiftData(double shiftH, double shiftB)
    {
        for (int i = 0; i < DataList.Length; i++)
        {
            var data = DataList[i];
            data.H -= shiftH;
            data.B += shiftB;
            DataList[i] = data;
        }
        
        if(HBPositiveList != null)
            for (int i = 0; i < HBPositiveList.Length; i++)
            {
                var data = HBPositiveList[i];
                data.H -= shiftH;
                data.B += shiftB;
                HBPositiveList[i] = data;
            }
        
        if(HBNegativeList != null)
            for (int i = 0; i < HBNegativeList.Length; i++)
            {
                var data = HBNegativeList[i];
                data.H -= shiftH;
                data.B += shiftB;
                HBNegativeList[i] = data;
            }
    }
    
    private List<HBData> SelectDataInRange(HBData[] dataList, double min, double max,int sign, bool isHAxis)
    {
        List<HBData> selected = new List<HBData>();
        foreach (var data in dataList)
        {
            var v = isHAxis ? data.H : data.B;
            v *= sign;
            if(min <= v && v <= max)
                selected.Add(data);
        }

        return selected;
    }
}