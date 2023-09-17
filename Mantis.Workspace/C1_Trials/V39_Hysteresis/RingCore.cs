using Mantis.Core.QuickTable;

namespace Mantis.Workspace.C1_Trials.V39_Hysteresis;

[QuickTable("","tab:ringCores")]
public record struct RingCore
{
    [QuickTableField("type")]
    public string Type = "";

    [QuickTableField("name")] public string Name = "";
    [QuickTableField("da", "mm")] public double Da = 0;
    [QuickTableField("db", "mm")] public double Db = 0;
    [QuickTableField("n1")] public int N1 = 0;
    [QuickTableField("n2")] public int N2 = 0;
    [QuickTableField("height", "mm")] public double Height = 0;
    [QuickTableField("fillfactor")] public double FillFactor = 0;
    
    public RingCore(){}
}