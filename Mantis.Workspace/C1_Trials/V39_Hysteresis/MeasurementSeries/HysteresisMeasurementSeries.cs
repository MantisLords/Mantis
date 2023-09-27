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
        DataList = CreateDataList(rawData);

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

    public static HysteresisMeasurementSeries InstantiateSeries(string name,List<PascoData> rawData,
        List<RingCore> ringCores, List<MeasurementSeriesInfo> seriesInfos,double errorVoltage, bool removeDrift = true,
        bool centerData = true)
    {
        var seriesInfo = GetSeriesInfo(seriesInfos, name);
        var ringCore = GetRingCore(ringCores, seriesInfo);

        if (!ringCore.IsFerromagnetic)
            return new NonFerromagneticMeasurementSeries(name, rawData, seriesInfo, ringCore, errorVoltage, removeDrift,
                centerData);
        
        if (seriesInfo.IsCurveOneCycle)
            return new OneCycleMeasurementSeries(name, rawData, seriesInfo, ringCore,errorVoltage, removeDrift, centerData);

        return new HysteresisMeasurementSeries(name, rawData, seriesInfo, ringCore,errorVoltage, removeDrift, centerData);
    }
    
    private HysteresisData[] CreateDataList(List<PascoData> pascoData)
    {
        HysteresisData[] hysteresisData = new HysteresisData[pascoData.Count];
        if (pascoData.Count == 0)
            return hysteresisData;
        
        for (int i = 0; i < hysteresisData.Length; i++)
        {
            var data = new HysteresisData(pascoData[i].Time,pascoData[i].CurrentA / 10.0,pascoData[i].CurrentB / 10.0,0,0);

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
    private static MeasurementSeriesInfo GetSeriesInfo(List<MeasurementSeriesInfo> list,string name)
    {
        foreach (var seriesInfo in list)
        {
            if (seriesInfo.MeasurementSeriesName == name)
            {
                return seriesInfo;
            }
        }

        throw new ArgumentException($"There was no MeasurmentSeriesInfo found by the name '{name}'");
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