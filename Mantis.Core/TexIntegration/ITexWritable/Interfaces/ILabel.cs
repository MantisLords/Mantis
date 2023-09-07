using System.Text;
using Mantis.Core.Utility;

namespace Mantis.Core.TexIntegration;

public interface ILabel : ITexWritable
{
    public string Label { get; }
}

public static class LabelExtension
{
    public static void AppendLabel(this StringBuilder builder, ILabel label)
    {
        builder.AppendLabel(label.Label);
    }
    
    public static void AppendLabel(this StringBuilder builder, string label)
    {
        builder.AppendCommand($"label{{{label}}}");
    }

    public static void SaveLabeled(this ILabel writable)
    {
        writable.Save(writable.Label.Replace(':','_'));
    }
}