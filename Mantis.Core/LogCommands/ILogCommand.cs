namespace Mantis.Core.LogCommands;

public interface ILogCommand
{
    public (string, string) GetLabeledContent(bool isLatex = false);
}