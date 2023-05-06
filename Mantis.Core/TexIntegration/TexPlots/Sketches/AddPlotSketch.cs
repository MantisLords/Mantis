using System.Text;

namespace Mantis.Core.TexIntegration;

public abstract record AddPlotSketch : ITexWritable
{
    public string? Legend;
    
    public void AppendToTex(StringBuilder builder)
    {
        builder.AppendCommand("addplot[");
        
        AppendInsideSquareBrackets(builder);

        builder.AppendLine("]");
        AppendAfterBeforeSemi(builder);
        builder.AppendLine(";");
        AppendAtEnd(builder);
    }

    public virtual void AppendInsideSquareBrackets(StringBuilder builder){}

    public virtual void AppendAfterBeforeSemi(StringBuilder builder)
    {
    }

    public virtual void AppendAtEnd(StringBuilder builder)
    {
        if(Legend != null)
            builder.AppendCommand($"addlegendentry{{{Legend}}}");
    }


}