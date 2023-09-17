using Mantis.Core.Calculator;
using Mantis.Core.QuickTable;
using Mantis.Core.ScottPlotUtility;
using Mantis.Core.TexIntegration;
using Mantis.Workspace.C1_Trials.Utility;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using ScottPlot;
using ScottPlot.Plottable;

namespace Mantis.Workspace.C1_Trials.V39_Hysteresis;

public record struct HysteresisData(double Time,double VoltageA,double VoltageB,double H, double B);

public record PlotRegInfo(RegModel<LineFunc> Model, double PointH, double PointB);

public record PlotEvalDataInfo(PlotRegInfo InfoPositive, PlotRegInfo InfoNegative, string LabelRegression,
    string LabelPoint)
{
    public void Plot(Plot plt, bool plotRegPoints, bool plotLine, bool plotPoint)
    {
        if (plotRegPoints)
        {
            var (_,scatter) = plt.AddErrorBars(InfoPositive.Model.Data, markerSize: 2);
            plt.AddErrorBars(InfoNegative.Model.Data, markerSize: 2,color:scatter.Color);
        }

        if (plotLine)
        {
            var funcPlot = plt.AddFunction(InfoPositive.Model.ParaFunction, lineWidth: 1,label:LabelRegression,lineStyle:LineStyle.DashDot);
            plt.AddFunction(InfoNegative.Model.ParaFunction, lineWidth: 1,color: funcPlot.Color,lineStyle:LineStyle.DashDot);
        }

        if (plotPoint)
        {
            var pointPlt = plt.AddPoint(InfoPositive.PointH, InfoPositive.PointB, shape: MarkerShape.openCircle,
                label: LabelPoint);
            plt.AddPoint(InfoNegative.PointH, InfoNegative.PointB, shape: MarkerShape.openCircle,
                color: pointPlt.Color);
        }
    }
}
    
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

public class HysteresisMeasurementSeries
{
    public readonly string Name;

    public readonly HysteresisData[] DataList;

    public readonly bool IsCurveOneCycle;
    public readonly HBData[]? HBPositiveList = null;
    public readonly HBData[]? HBNegativeList = null;

    //public RegModel<TanhFit>? PositiveModel { get; private set; } = null;
    //public RegModel<TanhFit>? NegativeModel { get; private set; } = null;

    public readonly RingCore RingCore;

    public readonly MeasurementSeriesInfo SeriesInfo;

    public readonly ErDouble SeriesResince; // Ohm

    public readonly double FluxRange; // VM
    public bool IsDriftRemoved { get; private set; } = false;
    public bool IsDataCentered { get; private set; } = false;

    private int _minHIndex = 0;

    public ErDouble? Coercivity = null;
    private PlotEvalDataInfo? _coercifityPlotInfo = null;
    public ErDouble? Remanence = null;
    private PlotEvalDataInfo? _remanencePlotInfo = null;
    public ErDouble? Saturation = null;
    private PlotEvalDataInfo? _saturationPlotInfo = null;


    public HysteresisMeasurementSeries(string name,List<PascoData> rawData,List<RingCore> ringCores,List<MeasurementSeriesInfo> seriesInfos,bool removeDrift = true,bool centerData = true)
    {
        Name = name;
        DataList = CreateDataList(rawData);

        SeriesInfo = GetSeriesInfo(seriesInfos);
        RingCore = GetRingCore(ringCores);
        SeriesResince = SeriesInfo.SeriesResistance;
        SeriesResince.CalculateDeviceError(Devices.VC170,DataTypes.Resistance);
        FluxRange = SeriesInfo.FluxRange;
        
        if(removeDrift) RemoveDrift();
        if(centerData) CenterData();
        
        CalculateHAndB();

        IsCurveOneCycle = SeriesInfo.IsCurveOneCycle;
        if(IsCurveOneCycle)
            (HBPositiveList, HBNegativeList) = FindPositiveAndNegativeData();
    }
    
    private HysteresisData[] CreateDataList(List<PascoData> pascoData)
    {
        HysteresisData[] hysteresisData = new HysteresisData[pascoData.Count];
        if (pascoData.Count == 0)
            return hysteresisData;
        
        for (int i = 0; i < hysteresisData.Length; i++)
        {
            var data = new HysteresisData(pascoData[i].Time,pascoData[i].VoltageA,pascoData[i].VoltageB,0,0);

            hysteresisData[i] = data;
        }
        
        return hysteresisData;
    }
    

    public void CalculateHAndB()
    {
        
        double hFactor = RingCore.N1 /
                         (Constants.PiOver2 * (RingCore.Da + RingCore.Db) *0.001 * SeriesResince.Value);
        double bFactor = FluxRange /
                         (RingCore.N2 * 0.5 * (RingCore.Da - RingCore.Db)*0.001 * RingCore.Height*0.001 * RingCore.FillFactor);

        for (int i = 0; i < DataList.Length; i++)
        {
            var data = DataList[i];
            data.H = hFactor * data.VoltageA;
            data.B = bFactor * data.VoltageB;
            DataList[i] = data;
        }
    }

    public void CalculateCoercivity()
    {
        if(!IsCurveOneCycle && HBNegativeList != null && HBPositiveList != null) return;

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
        if(!IsCurveOneCycle && HBNegativeList != null && HBPositiveList != null) return;

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
        if(!IsCurveOneCycle && HBNegativeList != null && HBPositiveList != null) return;
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
        
    }

    public (HBData[],HBData[]) FindPositiveAndNegativeData()
    {
        double minH = DataList[0].H;
        _minHIndex = 0;
        for (int i = 0; i < DataList.Length; i++)
        {
            if (DataList[i].H < minH)
            {
                minH = DataList[i].H;
                _minHIndex = i;
            }
        }

        HBData[] negativeData = new HBData[_minHIndex+1];
        for (int i = 0; i < _minHIndex+1; i++)
        {
            negativeData[i] = new HBData(DataList[i].H, DataList[i].B);
        }
        
        HBData[] positiveData = new HBData[DataList.Length - _minHIndex];
        for (int i = _minHIndex; i < DataList.Length; i++)
        {
            positiveData[i-_minHIndex] = new HBData(DataList[i].H, DataList[i].B);
        }
        
        return (negativeData, positiveData);
    }

    public Plot PlotData(Plot plt,bool debug)
    {
        
            //var plt = ScottPlotExtensions.CreateSciPlot("H in A/m", "B in T");
            
            AddHBData(plt, DataList, "");
            
            if(_coercifityPlotInfo != null) _coercifityPlotInfo.Plot(plt,false,false,true);
            if(_remanencePlotInfo != null) _remanencePlotInfo.Plot(plt,false,false,true);
            if(_saturationPlotInfo != null) _saturationPlotInfo.Plot(plt,false,true,true);
            //AddHBData(plt, HBNegativeList, "Negative HB Data");
            
            Console.WriteLine($"{Name}\tRem {Remanence.ToString()}\tCoer {Coercivity.ToString()}\tSat {Saturation.ToString()}");
            
            return plt;
            //plt.SaveFigHere(Name, scale: 8);
    }

    private ScatterPlot AddHBData(Plot plt,HysteresisData[] data,string legend)
    {
        if (data.Length == 0) return null;
        var xs = data.Select(e => e.H).ToArray();
        var ys = data.Select(e => e.B).ToArray();
        
        return plt.AddScatter(xs, ys, markerSize: 1, lineStyle: LineStyle.None,label:legend);
    }
    
    private void RemoveDrift()
    {
        if (IsDriftRemoved) return;
        IsDriftRemoved = true;

        if (DataList.Length == 0)
            return;
        
        var firstData = DataList[0];
        var lastData = DataList[DataList.Length - 1];
        
        double voltageDif = lastData.VoltageB - firstData.VoltageB;
        double timeDif = lastData.Time - firstData.Time;
        double drift = voltageDif / timeDif;

        for (int i = 0; i < DataList.Length; i++)
        {
            var data = DataList[i];
            // Remove drift
            data.VoltageB -= data.Time * drift;
            DataList[i] = data;
        }
    }

    private void CenterData()
    {
        if(IsDataCentered) return;
        IsDataCentered = true;

        double voltageMin = DataList[0].VoltageB;
        double voltageMax = DataList[0].VoltageB;
        
        foreach (var t in DataList)
        {
            voltageMax = Math.Max(voltageMax, t.VoltageB);
            voltageMin = Math.Min(voltageMin, t.VoltageB);
        }
        
        // Center
        double shift = 0.5 * (voltageMax + voltageMin);
        for (int i = 0; i < DataList.Length; i++)
        {
            var data = DataList[i];
            data.VoltageB -= shift;
            DataList[i] = data;
        }
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

    private MeasurementSeriesInfo GetSeriesInfo(List<MeasurementSeriesInfo> list)
    {
        foreach (var seriesInfo in list)
        {
            if (seriesInfo.MeasurementSeriesName == Name)
            {
                return seriesInfo;
            }
        }

        throw new ArgumentException($"There was no MeasurmentSeriesInfo found by the name '{Name}'");
    }

    private RingCore GetRingCore(List<RingCore> cores)
    {
        foreach (var core in cores)
        {
            if (SeriesInfo.RingCoreName == core.Type)
            {
                return core;
            }
        }

        throw new ArgumentException($"There was no RingCore found of the type '{SeriesInfo.RingCoreName}'");
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