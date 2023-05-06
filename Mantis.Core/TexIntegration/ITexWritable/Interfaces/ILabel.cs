using System.Text;

namespace Mantis.Core.TexIntegration.Utility;

public interface ILabel : ITexWritable
{
    public string Label { get; }
}

public static class LabelExtension
{
    public static void ApLabel(this StringBuilder builder, ILabel label)
    {
        builder.ApCommand($"label{{{label.Label}}}");
    }

    public static void Save(this ILabel writable)
    {
        TexWriter writer = new TexWriter();
        writer.Write(writable);
        writer.Save(writable.Label);
    }
}