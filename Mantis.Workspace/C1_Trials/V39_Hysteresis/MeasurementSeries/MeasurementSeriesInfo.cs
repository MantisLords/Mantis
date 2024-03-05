using Mantis.Core.Calculator;
using Mantis.Core.QuickTable;

namespace Mantis.Workspace.C1_Trials.V39_Hysteresis;

[QuickTable("","tab:measurementSeriesInfos")]
public record struct MeasurementSeriesInfo
{
    [QuickTableField("measurementSeries")] public string MeasurementSeriesName = "";
    [QuickTableField("usage")] public string Usage = "";
    [QuickTableField("ringCore")] public string RingCoreName = "";

    [QuickTableField("isHFieldFlipped")] public bool IsHFieldFlipped = false;
    [QuickTableField("isBFieldFlipped")] public bool IsBFieldFlipped = false;

    [QuickTableField("seriesResistance", "Ohm")]
    public ErDouble SeriesResistance = 0;

    [QuickTableField("fluxRange", "VM")] public double FluxRange = 0;
    [QuickTableField("isCurveOneCycle")] public bool IsCurveOneCycle = false;

    [QuickTableField("coercivityEvalMax", "B")]
    public double CoercivityEvalMax = 0;

    [QuickTableField("isRemanenceEvalHAxis")]
    public bool IsRemanenceEvalHaxis = false;

    [QuickTableField("remanenceEvalMin")] public double RemanenceEvalMin = 0;
    [QuickTableField("remanenceEvalMax")] public double RemanenceEvalMax = 0;

    [QuickTableField("saturationEvalMin", "H")]
    public double SaturationEvalMin = 0;

    public MeasurementSeriesInfo(){}
    
}