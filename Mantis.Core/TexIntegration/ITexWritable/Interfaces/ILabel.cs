using System.Text;

namespace Mantis.Core.TexIntegration;

public interface ILabel : ITexWritable
{
    public string Label { get; }
}

public static class LabelExtension
{
    public static void AppendLabel(this StringBuilder builder, ILabel label)
    {
        builder.AppendCommand($"label{{{label.Label}}}");
    }

    public static void SaveLabeled(this ILabel writable)
    {
        writable.Save(writable.Label);
    }
}