using System.Reflection.Emit;
using Mantis.Core.TexIntegration;

namespace Mantis.Core.LogCommands;


public static class CommandLoggerExtension
{
    public static string ToLogString(this IEnumerable<ILogCommand> commands)
    {
        string res = "";
        foreach (var command in commands)
        {
            var (label, content) = command.GetLabeledContent(false);
            res += $"{label} = {content}\t";
        }

        return res;
    }
}