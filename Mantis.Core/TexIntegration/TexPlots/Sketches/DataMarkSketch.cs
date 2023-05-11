using System.Globalization;
using System.Numerics;
using System.Text;
using Mantis.Core.Calculator;

namespace Mantis.Core.TexIntegration;

public record DataMarkSketch : AddPlotSketch
{
    public IEnumerable<(ErDouble, ErDouble)> Data;
    
    public override void AppendInsideSquareBrackets(StringBuilder builder,Sketchbook sketchbook)
    {
        base.AppendInsideSquareBrackets(builder,sketchbook);
        builder.AppendLine("only marks");
    }

    public override void AppendAfterBeforeSemi(StringBuilder builder,Sketchbook sketchbook)
    {
        builder.AppendLine("coordinates{");
        foreach ((ErDouble x,ErDouble y) in Data)
        {
            builder.AppendLine($"({x.Value},{y.Value}) +- ({x.Error},{y.Error})");
        }

        builder.Append("}");
    }

    public override Rect2? GetDomain()
    {
        Vector2 min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
        Vector2 max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);

        foreach ((ErDouble x,ErDouble y) in Data)
        {

            min.X = MathF.Min(min.X, (float)x.Min);
            min.Y = MathF.Min(min.Y, (float)y.Min);
            
            max.X = MathF.Max(max.X, (float)x.Max);
            max.Y = MathF.Max(max.Y, (float)y.Max);
            
        }
        Console.WriteLine($"Min{min} Max {max}");
        
        return new Rect2() { Min = min, Max = max };
    }
}