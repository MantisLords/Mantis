using System.Text;
using Mantis.Core.Calculator;
using Mantis.Core.TexIntegration;
using Mantis.Core.Utility;

namespace Mantis.Core.TexIntegration;

public class Sketchbook : ITexWritable, ILabel,ICaption
{
    public string? Title;

    public Sketchbook(AxisLayout axis, string label, string caption,string? title = null)
    {
        Axis = axis;
        Label = label;
        Caption = caption;
        Title = title;
    }

    public List<ISketch> Sketches { get; } = new List<ISketch>();
    
    public AxisLayout Axis { get; set; }

    public Rect2? Domain
    {
        get => Axis.Domain;
        set => Axis.Domain = value;
    }
    
    public void AppendToTex(StringBuilder builder)
    {
        builder.AppendBegin("figure","h!");
        
        builder.AppendCommand("centering");
        
        builder.AppendBegin("tikzpicture");
        
        Axis.AppendBegin(builder,this);

        foreach (var sketch in Sketches)
        {
            sketch.AppendToTex(builder,this);
        }
        
        Axis.AppendEnd(builder,this);

        builder.AppendEnd("tikzpicture");
        
        builder.AppendCaption(this);
        builder.AppendLabel(this);
        
        builder.AppendEnd("figure");
    }

    public void Add(ISketch sketch)
    {
        Sketches.Add(sketch);
    }

    public string Label { get; set; }
    public string Caption { get; set; }
}