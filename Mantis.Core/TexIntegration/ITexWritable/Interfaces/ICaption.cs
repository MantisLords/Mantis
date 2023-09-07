using System.Text;
using Mantis.Core.Utility;

namespace Mantis.Core.TexIntegration;

public interface ICaption : ITexWritable
{
    public string Caption { get; }
}

public static class CaptionExtension
{
    public static void AppendCaption(this StringBuilder builder, ICaption caption)
    {
        builder.AppendCaption(caption.Caption);
    }
    
    public static void AppendCaption(this StringBuilder builder, string caption)
    {
        builder.AppendCommand($"caption{{{caption}}}");
    }
}