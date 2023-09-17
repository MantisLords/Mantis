using Mantis.Core.Calculator;
using Mantis.Core.QuickTable;
using Mantis.Core.TexIntegration;

namespace Mantis.Workspace.C1_Trials.V41_EMWaweSpeed;

[QuickTable("","tab:peakDataForRegression")]
public record struct peakDataForRegression
{
    [QuickTableField("time", "\\seconds")] public ErDouble Angle = 0;

    [QuickTableField("Voltage", "V")] public ErDouble VoltageDiode = 0;
    
    public peakDataForRegression(){}
}

public class Part1_WaveResistence_KoaxCable
{
    
}