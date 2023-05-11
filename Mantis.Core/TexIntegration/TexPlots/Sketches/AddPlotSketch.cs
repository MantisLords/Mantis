using System.Text;
using Mantis.Core.Calculator;
using Mantis.Core.Utility;

namespace Mantis.Core.TexIntegration;

public abstract record AddPlotSketch : ISketch
{
    public string? Legend;

    public virtual void AppendInsideSquareBrackets(StringBuilder builder,Sketchbook sketchbook)
    {
        
        
        
    }

    public virtual void AppendAfterBeforeSemi(StringBuilder builder,Sketchbook sketchbook)
    {
    }

    public virtual void AppendAtEnd(StringBuilder builder,Sketchbook sketchbook)
    {
        if(Legend != null)
            builder.AppendCommand($"addlegendentry{{{Legend}}}");
    }


    public abstract Rect2? GetDomain();

    public void AppendToTex(StringBuilder builder, Sketchbook sketchbook)
    {
        builder.AppendCommand("addplot[");
        
        AppendInsideSquareBrackets(builder,sketchbook);

        builder.AppendLine("]");
        AppendAfterBeforeSemi(builder,sketchbook);
        builder.AppendLine(";");
        AppendAtEnd(builder,sketchbook);
    }
}