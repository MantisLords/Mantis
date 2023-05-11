using System.Text;
using Mantis.Core.Calculator;

namespace Mantis.Core.TexIntegration;

public interface ISketch
{
    public Rect2? GetDomain();
    
    public void AppendToTex(StringBuilder builder, Sketchbook sketchbook);
}