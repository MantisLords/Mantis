using Mantis.Core.Calculator;
using Mantis.Core.LogCommands;

namespace Mantis.Core.TexIntegration;

public static class CalculatorLogExtension
{
    public static void AddCommandsToPreamble(this IEnumerable<ILogCommand> commands,string labelPrefix = "")
    {
        foreach (var command in commands)
        {
            var (label, content) = command.GetLabeledContent(true);
            label = labelPrefix + label;
            label = label.Replace(" ", null).Replace(":", null);
            TexPreamble.AddCommand(content,label);
        }
    }

    public static void AddCommandsToPreambleAndLog(this IEnumerable<ILogCommand> commands, string labelPrefix = "")
    {
        commands.AddCommandsToPreamble(labelPrefix);

        string res = $"{labelPrefix}:\t";
        res += commands.ToLogString();
        Console.WriteLine(res);
    }
    
    public static void AddParametersToPreamble<T>(this RegModel<T> model, string labelPrefix = "")
        where T : FuncCore, new()
    {
        model.ParaFunction.ParaSet.GetParametersLog().AddCommandsToPreamble(labelPrefix);
    }
    
    public static void AddParametersToPreambleAndLog<T>(this RegModel<T> model, string labelPrefix = "")
        where T : FuncCore, new()
    {
        model.ParaFunction.ParaSet.GetParametersLog().AddCommandsToPreambleAndLog(labelPrefix);
    }
    
    public static void AddParametersToPreamble<T>(this ParaFunc<T> model, string labelPrefix = "")
        where T : FuncCore, new()
    {
        model.ParaSet.GetParametersLog().AddCommandsToPreamble(labelPrefix);
    }
    
    public static void AddParametersToPreambleAndLog<T>(this ParaFunc<T> model, string labelPrefix = "")
        where T : FuncCore, new()
    {
        model.ParaSet.GetParametersLog().AddCommandsToPreambleAndLog(labelPrefix);
    }
    
    public static void AddGoodnessOfFitToPreamble<T>(this RegModel<T> model, string labelPrefix = "")
        where T : FuncCore, new()
    {
        model.GetGoodnessOfFitLog().AddCommandsToPreamble(labelPrefix);
    }
    
    public static void AddGoodnessOfFitToPreambleAndLog<T>(this RegModel<T> model, string labelPrefix = "")
        where T : FuncCore, new()
    {
        model.GetGoodnessOfFitLog().AddCommandsToPreambleAndLog(labelPrefix);
    }
}