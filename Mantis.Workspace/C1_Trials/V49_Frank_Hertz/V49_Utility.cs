using Mantis.Core.Calculator;
using Mantis.Core.QuickTable;

namespace Mantis.Workspace.C1_Trials.V49_Frank_Hertz;

public static class V49_Utility
{
    public static object? ParseWithErrorOnLastDigit(string s)
    {
        return ErDouble.ParseWithErrorLastDigit(s, null, 1);
    }

    public static readonly Dictionary<string, Func<string, object?>> VoltageDataLastDigitErrorParser
        = new Dictionary<string, Func<string, object?>>()
        {
            ["voltage"] = ParseWithErrorOnLastDigit,
            ["current"] = ParseWithErrorOnLastDigit
        };
    
}

[QuickTable("","tab:voltageData")]
public record struct VoltageData
{
    [QuickTableField("voltage", "V")] public ErDouble Voltage;
    [QuickTableField("current", "mA")] public ErDouble Current;
    
    public VoltageData(){}
}