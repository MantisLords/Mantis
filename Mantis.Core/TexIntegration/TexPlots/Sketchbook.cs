using System.Text;
using Mantis.Core.TexIntegration;
using Mantis.Core.Utility;

namespace Mantis.Core.TexIntegration;

public class Sketchbook : ITexWritable, ILabel,ICaption
{
    public List<ITexWritable> Sketches { get; } = new List<ITexWritable>();
    
    public ITexWritableEnclosed Axis { get; set; }
    
    public void AppendToTex(StringBuilder builder)
    {
        builder.AppendBegin("figure","h!");
        
        builder.AppendCommand("centering");
        
        builder.AppendBegin("tikzpicture");
        
        Axis.AppendBegin(builder);

        foreach (var sketch in Sketches)
        {
            sketch.AppendToTex(builder);
        }
        
        Axis.AppendEnd(builder);

        builder.AppendEnd("tikzpicture");
        
        builder.AppendCaption(this);
        builder.AppendLabel(this);
        
        builder.AppendEnd("figure");
    }

    public void Add(ITexWritable sketch)
    {
        Sketches.Add(sketch);
    }

    public string Label { get; set; }
    public string Caption { get; set; }
}