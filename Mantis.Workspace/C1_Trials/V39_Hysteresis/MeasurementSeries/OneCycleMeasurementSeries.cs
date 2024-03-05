using Mantis.Core.Calculator;
using MathNet.Numerics;
using MathNet.Numerics.Interpolation;
using ScottPlot;
using ScottPlot.Plottables;

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

public record struct CycleCharacteristicProperties(
    ErDouble? Coercivity, 
    ErDouble? Remanence, 
    ErDouble? Saturation,
    ErDouble? SaturationPermeability,
    ErDouble? HysteresisLoss);

public class OneCycleMeasurementSeries : HysteresisMeasurementSeries
{
    public readonly HBData[] HBPositiveList;
    public readonly HBData[] HBNegativeList;
    
    private PlotEvalDataInfo? _coercifityPlotInfo = null;
    private PlotEvalDataInfo? _remanencePlotInfo = null;
    private PlotEvalDataInfo? _saturationPlotInfo = null;

    public CycleCharacteristicProperties CharacProperties = new CycleCharacteristicProperties();
    
    public bool DrawCalculatedValues = true;
    public bool DrawRegCoercivity = false;
    public bool DrawRegRemanence = false;
    public bool DrawRegSaturation = false;
    public bool DrawRegPoints = false;
    
    internal OneCycleMeasurementSeries(string name,List<PascoData> rawData,MeasurementSeriesInfo seriesInfo,RingCore ringCore,double errorVoltage,bool removeDrift,bool centerData) 
        : base(name, rawData, seriesInfo, ringCore,errorVoltage, removeDrift, centerData)
    {
        var (_HBPositiveList, _HBNegativeList) = FindPositiveAndNegativeData();
        HBPositiveList = _HBPositiveList.ToArray();
        HBNegativeList = _HBNegativeList.ToArray();

        CalculateCoercivity();
        CalculateRemanence();
        CalculateSaturation();
        CalculateHysteresisLoss();
    }
    
    public void CalculateCoercivity()
    {

        var (coercivityPositivePreShif,_) = CalculateCoercivity(HBPositiveList);
        var (coercivityNegativePreShift,_) = CalculateCoercivity(HBNegativeList);
        
        ShiftData(0.5 * (coercivityPositivePreShif.Value + coercivityNegativePreShift.Value),0);
        
        var (coercivityPositive,coercivityPositiveInfo) = CalculateCoercivity(HBPositiveList);
        var (coercivityNegative,coercivityNegativeInfo) = CalculateCoercivity(HBNegativeList);
        Console.WriteLine($"{Label} CorerPos {coercivityPositive} CorerNeg {coercivityNegative}");
        CharacProperties.Coercivity = new[] {-coercivityPositive, coercivityNegative}.WeightedMean(useMaxCovariance:false);
        _coercifityPlotInfo = new PlotEvalDataInfo(coercivityPositiveInfo, coercivityNegativeInfo,
            "Fit zum Bestimmen\nder Koerzitivfeldstärke",
            "Koerzitivfeldstärke"); //"Line fit for\nevaluation coercivity", "Coercivity");

    }

    private (ErDouble,PlotRegInfo) CalculateCoercivity(HBData[] points)
    {

        if (TryFitModel(points, -SeriesInfo.CoercivityEvalMax, SeriesInfo.CoercivityEvalMax, 1,false,
                out RegModel model))
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
        CharacProperties.Remanence = new[] {-remanenceNegative, remanencePositive}.WeightedMean();
        _remanencePlotInfo = new PlotEvalDataInfo(remanencePositiveInfo, remanenceNegativeInfo,
            "Fit zum Bestimmen\ndes Remanenzfelds", "Remanenzfeld"); //"Line fit for\nevaluating remanence", "Remanence");
    }

    private (ErDouble,PlotRegInfo) CalculateRemanence(HBData[] points, int sign)
    {
        if (TryFitModel(points, SeriesInfo.RemanenceEvalMin , SeriesInfo.RemanenceEvalMax ,sign, SeriesInfo.IsRemanenceEvalHaxis,
                out RegModel model))
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
            var satInfoPos = CalculateSaturation(HBPositiveList, 1);
            var satInfoNeg = CalculateSaturation(HBNegativeList, -1);
            
            CharacProperties.Saturation = new[] {- satInfoNeg.Model.ErParameters[0], satInfoPos.Model.ErParameters[0]}.WeightedMean();
            //Console.WriteLine($"MuPos {satInfoPos.Model.ErParameters[1]} MuNeg {satInfoNeg.Model.ErParameters[1]}");
            CharacProperties.SaturationPermeability = new[] {satInfoNeg.Model.ErParameters[1], satInfoPos.Model.ErParameters[1]}.WeightedMean();
            CharacProperties.SaturationPermeability /= Constants.MagneticPermeability;

            _saturationPlotInfo = new PlotEvalDataInfo(satInfoPos, satInfoNeg,
                "Fit zur Bestimmung\nder Sättigung",
                "Sättigungsfeld"); //"Line fit for\nevaluating saturation", "Saturation");

        }catch(ArgumentException){}
    }
    
    private PlotRegInfo CalculateSaturation(HBData[] points, int sign)
    {
        if(SeriesInfo.SaturationEvalMin == 0)
            throw new ArgumentException("You have to set saturation limits");
        
        if (TryFitModel(points, SeriesInfo.SaturationEvalMin , double.PositiveInfinity,sign , true,
                out RegModel model))
        {
            ErDouble saturation = model.ErParameters[0];
            PlotRegInfo info = new PlotRegInfo(model,0, saturation.Value);
            return info;
        }
        else
            throw new ArgumentException("You have to set coercivity limits");
    }

    private bool TryFitModel(HBData[] points, double min, double max,int sign, bool isHAxis,out RegModel model)
    {
        if (min == 0 && max == 0)
        {
            model = null;
            return false;
        }
        
        List<HBData> regressionPoints = SelectDataInRange(points, min, max,sign, isHAxis);

        model = regressionPoints.CreateRegModel(e => (e.H, e.B), new ParaFunc(2,new LineFunc()));
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
        CharacProperties.HysteresisLoss = areaUnderCurve  / RingCore.Density.Mul10E(3) ;
        
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
    


    public override Plot PlotData(Plot plt)
    {
        base.PlotData(plt);


        plt.Legend.Location = Alignment.UpperLeft;
        //AddHBDataP(plt, HBPositiveList, "pos");
        //AddHBDataP(plt, HBNegativeList, "neg");
        
        //return plt;
        
        if(_coercifityPlotInfo != null) _coercifityPlotInfo.Plot(plt,DrawRegPoints,DrawRegCoercivity,DrawCalculatedValues,0);
        if(_remanencePlotInfo != null) _remanencePlotInfo.Plot(plt,DrawRegPoints,DrawRegRemanence,DrawCalculatedValues,1);
        if(_saturationPlotInfo != null) _saturationPlotInfo.Plot(plt,DrawRegPoints,DrawRegSaturation,DrawCalculatedValues,2);
        //AddHBData(plt, HBNegativeList, "Negative HB Data");
        
        return plt;
    }

    private Scatter AddHBDataP(Plot plt,HBData[] data,string legend)
    {
        if (data.Length == 0) return null;
        var xs = data.Select(e => CheckDouble(e.H)).ToArray();
        var ys = data.Select(e => CheckDouble(e.B)).ToArray();
        
        var scatter = plt.Add.Scatter(xs, ys);
        scatter.LineStyle.IsVisible = false;
        scatter.MarkerSize = 1;
        scatter.Label = legend;
        return scatter;
    }
    

    public override void SaveAndLogCalculatedData()
    {
        base.SaveAndLogCalculatedData();
        
        // CharacProperties.Remanence?.AddCommand("Remanence"+Label,"T");
        // CharacProperties.Coercivity?.AddCommand("Coercivity"+Label,"A/m");
        // CharacProperties.Saturation?.AddCommand("Saturation"+Label,"T");
        // CharacProperties.SaturationPermeability?.AddCommand("SaturationPermeability"+Label,"Tm/A");
        // CharacProperties.HysteresisLoss?.AddCommand("HysteresisLoss"+Label,"J/kg");
        
        Console.WriteLine($"{Label}\t{CharacProperties}");
    }

    private Scatter AddHBData(Plot plt,HBData[] data,string legend)
    {
        if (data.Length == 0) return null;
        var xs = data.Select(e => CheckDouble(e.H)).ToArray();
        var ys = data.Select(e => CheckDouble(e.B)).ToArray();
        
        var scatter = plt.Add.Scatter(xs, ys);
        scatter.LineStyle.IsVisible = false;
        scatter.MarkerSize = 1;
        scatter.Label = legend;
        return scatter;
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