using System.Text;

namespace Mantis.Core.TexIntegration.Utility;

public static class StringBuilderExtension
{
    public static void ApCommand(this StringBuilder builder, string command)
    {
        builder.AppendLine($"\\{command}");
    }

    public static void ApBegin(this StringBuilder builder, string environment)
    {
        builder.ApCommand($"begin{{{environment}}}");
    }

    public static void ApEnd(this StringBuilder builder, string environment)
    {
        builder.ApCommand($"end{{{environment}}}");
    }
}