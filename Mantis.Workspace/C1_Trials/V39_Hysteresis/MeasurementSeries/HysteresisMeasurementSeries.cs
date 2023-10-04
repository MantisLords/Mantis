using Mantis.Core.Calculator;
using Mantis.Core.QuickTable;
using Mantis.Core.ScottPlotUtility;
using Mantis.Core.TexIntegration;
using Mantis.Workspace.C1_Trials.Utility;
using MathNet.Numerics;
using MathNet.Numerics.Interpolation;
using MathNet.Numerics.LinearAlgebra;
using ScottPlot;
using ScottPlot.Plottable;

namespace Mantis.Workspace.C1_Trials.V39_Hysteresis;

public record struct HysteresisData(double Time,double VoltageA,double VoltageB,double H, double B);



public class HysteresisMeasurementSeries
{
    public readonly string Name;

    public readonly HysteresisData[] DataList;

    public readonly double ErrorB;
    
    //public RegModel<TanhFit>? PositiveModel { get; private set; } = null;
    //public RegModel<TanhFit>? NegativeModel { get; private set; } = null;

    public readonly RingCore RingCore;

    public readonly MeasurementSeriesInfo SeriesInfo;

    public readonly ErDouble SeriesResince; // Ohm

    public readonly double FluxRange; // VM
    public bool IsDriftRemoved { get; private set; } = false;
    public bool IsDataCentered { get; private set; } = false;
    
    public readonly string Label;


    protected HysteresisMeasurementSeries(string name,List<PascoData> rawData,MeasurementSeriesInfo seriesInfo,RingCore ringCore,double errorVoltage,bool removeDrift = true,bool centerData = true)
    {
        Name = name;
        DataList = CreateDataList(rawData,seriesInfo.IsHFieldFlipped,seriesInfo.IsBFieldFlipped);

        SeriesInfo = seriesInfo;
        RingCore = ringCore;
        RingCore.Density.Error = RingCore.ErrorDensity;
        SeriesResince = SeriesInfo.SeriesResistance;
        SeriesResince.CalculateDeviceError(Devices.VC170,DataTypes.Resistance);
        FluxRange = SeriesInfo.FluxRange;
        
        if(removeDrift) RemoveDrift();
        if(centerData) CenterData();
        
        CalculateHAndB();
        ErrorB = errorVoltage * CalculateBFactor();

        if (SeriesInfo.Usage != "")
            Label = SeriesInfo.Usage + RingCore.Type;
        else
            Label = SeriesInfo.MeasurementSeriesName + RingCore.Type;

    }

    public static bool TryInstantiateSeries(string name,List<PascoData> rawData,
        List<RingCore> ringCores, List<MeasurementSeriesInfo> seriesInfos,double errorVoltage,out HysteresisMeasurementSeries measurementSeries, bool removeDrift = true,
        bool centerData = true)
    {
        
        if(TryGetSeriesInfo(seriesInfos, name,out MeasurementSeriesInfo seriesInfo)){
            var ringCore = GetRingCore(ringCores, seriesInfo);

            if (!ringCore.IsFerromagnetic && removeDrift && centerData)
                measurementSeries = new NonFerromagneticMeasurementSeries(name, rawData, seriesInfo, ringCore, errorVoltage, removeDrift, centerData);
            else if (seriesInfo.IsCurveOneCycle && removeDrift && centerData)
                measurementSeries = new OneCycleMeasurementSeries(name, rawData, seriesInfo, ringCore, errorVoltage, removeDrift, centerData);
            else
                measurementSeries = new HysteresisMeasurementSeries(name, rawData, seriesInfo, ringCore, errorVoltage, removeDrift, centerData);
            
            return true;
        }

        measurementSeries = null;
        return false;
    }
    
    private HysteresisData[] CreateDataList(List<PascoData> pascoData,bool isHFieldFlipped,bool isBFieldFlipped)
    {
        double signH = isHFieldFlipped ? -1:1;
        double signB = isBFieldFlipped ? -1:1;
        HysteresisData[] hysteresisData = new HysteresisData[pascoData.Count];
        if (pascoData.Count == 0)
            return hysteresisData;
        
        for (int i = 0; i < hysteresisData.Length; i++)
        {
            var data = new HysteresisData(pascoData[i].Time,signH * pascoData[i].VoltageA,signB* pascoData[i].VoltageB,0,0);

            hysteresisData[i] = data;
        }
        
        return hysteresisData;
    }
    
    private double CalculateHFactor() => RingCore.N1 / (Constants.PiOver2 * (RingCore.Da + RingCore.Db) *0.001 * SeriesResince.Value);
    private double CalculateBFactor() => FluxRange / (RingCore.N2 * 0.5 * (RingCore.Da - RingCore.Db)*0.001 * RingCore.Height*0.001 * RingCore.FillFactor);

    public void CalculateHAndB()
    {

        double hFactor = CalculateHFactor();
        double bFactor = CalculateBFactor();

        for (int i = 0; i < DataList.Length; i++)
        {
            var data = DataList[i];
            data.H = hFactor * data.VoltageA;
            data.B = bFactor * data.VoltageB;
            DataList[i] = data;
        }
    }

    

    public virtual Plot PlotData(Plot plt)
    {
            //var plt = ScottPlotExtensions.CreateSciPlot("H in A/m", "B in T");
            
            AddHBData(plt, DataList, RingCore.Name);
            
            return plt;
            //plt.SaveFigHere(Name, scale: 8);
    }
    
    public virtual void SaveAndLogCalculatedData(){}

    private ScatterPlot AddHBData(Plot plt,HysteresisData[] data,string legend)
    {
        if (data.Length == 0) return null;
        var xs = data.Select(e => CheckDouble(e.H)).ToArray();
        var ys = data.Select(e => CheckDouble(e.B)).ToArray();
        
        return plt.AddScatter(xs, ys, markerSize: 1, lineStyle: LineStyle.None,label:legend);
    }
    
    protected static double CheckDouble(double v)
    {

        return double.IsFinite(v) && !double.IsNaN(v) ? v : 0;
    }
    
    private void RemoveDrift()
    {
        if (IsDriftRemoved) return;
        IsDriftRemoved = true;

        if (DataList.Length == 0)
            return;

        var (firstData, lastData) = GetStartEndPointForDrift(0.01);
        
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

    private (HysteresisData, HysteresisData) GetStartEndPointForDrift(double voltageRange)
    {
        var firstData = DataList[0];

        double voltageARange = 1;

        var candidatesForLastPoint = (from d in DataList
            where Math.Abs(firstData.VoltageA - d.VoltageA) <= voltageARange
            select d).ToArray();

        double maxTimeDiff = 0;
        int timeJumpIndex = 0;
        for (int i = 1; i < candidatesForLastPoint.Length; i++)
        {
            if (maxTimeDiff < candidatesForLastPoint[i].Time - candidatesForLastPoint[i - 1].Time)
            {
                maxTimeDiff = candidatesForLastPoint[i].Time - candidatesForLastPoint[i - 1].Time;
                timeJumpIndex = i;
            }
        }

        double smallestDistance = double.PositiveInfinity;
        int smallesDistanceIndex = timeJumpIndex;
        for (int i = timeJumpIndex; i < candidatesForLastPoint.Length; i++)
        {
            var diff = Math.Abs(candidatesForLastPoint[i].VoltageA - firstData.VoltageA);
            if (smallestDistance > diff)
            {
                smallestDistance = diff;
                smallesDistanceIndex = i;
            }
        }

        var endData = candidatesForLastPoint[smallesDistanceIndex];
        
        //Console.WriteLine($"{Label} \t Start {firstData} EndData {endData} MaxTimeDif {maxTimeDiff}" +
        //                  $"\n\t LastCandidate {candidatesForLastPoint[^1]} Last {DataList[^1]}");
        
        return (firstData, endData);
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
    private static bool TryGetSeriesInfo(List<MeasurementSeriesInfo> list,string name,out MeasurementSeriesInfo info)
    {
        foreach (var seriesInfo in list)
        {
            if (seriesInfo.MeasurementSeriesName == name)
            {
                info = seriesInfo;
                return true;
            }
        }
        
        Console.WriteLine($"There was no MeasurmentSeriesInfo found by the name '{name}'");
        //throw new ArgumentException($"There was no MeasurmentSeriesInfo found by the name '{name}'");
        info = default;
        return false;
    }

    private static RingCore GetRingCore(List<RingCore> cores,MeasurementSeriesInfo seriesInfo)
    {
        foreach (var core in cores)
        {
            if (seriesInfo.RingCoreName == core.Type)
            {
                return core;
            }
        }

        throw new ArgumentException($"There was no RingCore found of the type '{seriesInfo.RingCoreName}'");
    }
}