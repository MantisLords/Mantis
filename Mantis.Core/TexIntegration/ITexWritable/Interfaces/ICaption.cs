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
        builder.AppendCommand($"caption{{{caption.Caption}}}");
    }
}