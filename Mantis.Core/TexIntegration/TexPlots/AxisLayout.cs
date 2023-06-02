using System.Numerics;
using System.Text;
using Mantis.Core.Calculator;
using Mantis.Core.Utility;

namespace Mantis.Core.TexIntegration;

public class AxisLayout
{
    public bool IsLogX = false;
    public bool IsLogY = false;

    public Rect2? Domain;

    public Vector2 PaddingRel = new Vector2(0.1f, 0.1f);

    public float RelWidthToTextSize = 1;
    
    public string XLabel;
    public string YLabel;
    public string? LegendPos;


    public AxisLayout(string xLabel, string yLabel)
    {
        XLabel = xLabel;
        YLabel = yLabel;
    }
    
    public virtual void AppendBegin(StringBuilder builder,Sketchbook sketchbook)
    {
        builder.AppendBegin(GetAxis());
        builder.Append("[ ");
        if(sketchbook.Title != null) builder.AppendLine($"title = {sketchbook.Title},");
        builder.AppendLine($"xlabel = {XLabel},");
        builder.AppendLine($"ylabel = {YLabel},");
        builder.AppendLine($"legend pos = {LegendPos ?? "north west"},");

        var domain = GetDomain(sketchbook);
        builder.AppendLine($"xmin={domain.Min.X}, xmax={domain.Max.X},\nymin={domain.Min.Y}, ymax={domain.Max.Y},");
        builder.AppendLine($"width={RelWidthToTextSize}\\textwidth");
        
        builder.AppendLine("]");

    }

    public void AppendEnd(StringBuilder builder,Sketchbook sketchbook)
    {
        builder.AppendEnd(GetAxis());
    }

    private string GetAxis()
    {
        return IsLogX switch
        {
            false when !IsLogY => "axis",
            true when !IsLogY => "semilogxaxis",
            false when IsLogY => "semilogyaxis",
            true when IsLogY => "loglogaxis",
            _ => ""
        };
    }

    private Rect2 GetDomain(Sketchbook sketchbook)
    {
        if (Domain.HasValue)
            return Domain.Value;

        Vector2 min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
        Vector2 max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);

        foreach (ISketch sketch in sketchbook.Sketches)
        {
            var sketchDom = sketch.GetDomain();

            if (sketchDom.HasValue)
            {
                var domMin = sketchDom.Value.Min;
                min.X = MathF.Min(min.X, domMin.X);
                min.Y = MathF.Min(min.Y, domMin.Y);
                
                var domMax = sketchDom.Value.Max;
                max.X = MathF.Max(max.X, domMax.X);
                max.Y = MathF.Max(max.Y, domMax.Y);
            }
        }
        
        var domain = new Rect2() { Min = min, Max = max };
        var size = domain.Max - domain.Min;
        domain.Max += size * PaddingRel;
        domain.Min -= size * PaddingRel;

        Domain = domain;
        return Domain.Value;
    }
}