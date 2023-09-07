namespace Mantis.Core.LogCommands;

public record BasicLogCom(string Label,string Content) : ILogCommand
{
    public (string, string) GetLabeledContent(bool isLatex)
    {
        return (Label, Content);
    }
}