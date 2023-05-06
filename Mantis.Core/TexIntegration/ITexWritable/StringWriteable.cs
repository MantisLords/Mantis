using System.Text;

namespace Mantis.Core.TexIntegration;

public record struct StringWriteable (string Text) : ITexWritable
{
    public void AppendToTex(StringBuilder builder)
    {
        builder.Append(Text);
    }
    
    public static implicit operator StringWriteable(string text)
    {
        return new StringWriteable(text);
    }
}