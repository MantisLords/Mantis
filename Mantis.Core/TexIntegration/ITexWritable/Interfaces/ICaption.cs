using System.Text;

namespace Mantis.Core.TexIntegration.Utility;

public interface ICaption : ITexWritable
{
    public string Caption { get; }
}

public static class CaptionExtension
{
    public static void AppendCaption(this StringBuilder builder, ICaption caption)
    {
        builder.ApCommand($"caption{{{caption.Caption}}}");
    }
}