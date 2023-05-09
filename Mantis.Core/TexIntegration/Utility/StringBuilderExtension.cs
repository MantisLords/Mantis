using System.Text;

namespace Mantis.Core.Utility;

public static class StringBuilderExtension
{
    public static void AppendCommand(this StringBuilder builder, string command)
    {
        builder.AppendLine($"\\{command}");
    }

    public static void AppendBegin(this StringBuilder builder, string environment)
    {
        builder.AppendCommand($"begin{{{environment}}}");
    }
    
    public static void AppendBegin(this StringBuilder builder, string environment,string squarebrackets)
    {
        builder.AppendCommand($"begin{{{environment}}}[{squarebrackets}]");
    }

    public static void AppendEnd(this StringBuilder builder, string environment)
    {
        builder.AppendCommand($"end{{{environment}}}");
    }
}