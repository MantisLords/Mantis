using System.Text;
using Mantis.Core.Utility;

namespace Mantis.Core.TexIntegration;

public class TexSmart<T> : ILabel
{
    public string Symbol;
    public string Unit;
    public T Value;

    public TexSmart( string label,string symbol, string unit,T value)
    {
        Symbol = symbol;
        Unit = unit;
        Label = label;
        Value = value;
    }

    public string Label { get; set; }
    
    public void AppendToTex(StringBuilder builder)
    {
        builder.AppendBegin("equation");
        builder.AppendLine($"{Symbol} = {Value.ToString()} {Unit}");
        
        builder.AppendLabel(this);
        
        builder.AppendEnd("equation");
    }
    
}