using Mantis.Core.Calculator;
using Mantis.Core.ScottPlotUtility;
using ScottPlot;
using ScottPlot.Palettes;

namespace Mantis.Workspace.C1_Trials.V39_Hysteresis;

public record PlotRegInfo(RegModel<LineFunc> Model, double PointH, double PointB);

public record PlotEvalDataInfo(PlotRegInfo InfoPositive, PlotRegInfo InfoNegative, string LabelRegression,
    string LabelPoint)
{
    private static IPalette microcharts = new Microcharts();
    private static IPalette category10 = new Category10();
    
    public void Plot(Plot plt, bool plotRegPoints, bool plotLine, bool plotPoint,int colorIndex)
    {
        
        if (plotRegPoints)
        {
            var pointColor = microcharts.GetColor(1+colorIndex);
            plt.AddDynErrorBar(InfoNegative.Model.Data, "", pointColor).MarkerStyle.Size = 2;
            plt.AddDynErrorBar(InfoNegative.Model.Data, color: pointColor).MarkerStyle.Size = 2;
        }

        if (plotLine)
        {
            
            var lineColor = plt.Add.Palette.GetColor( colorIndex);
            var posFunc = plt.AddDynFunction(InfoPositive.Model.ParaFunction, color:lineColor);
            posFunc.LineStyle.Width = 1;
            posFunc.LineStyle.Pattern = LinePattern.Dotted;
            var negFunc = plt.AddDynFunction(InfoNegative.Model.ParaFunction);
            negFunc.LineStyle.Width = 1;
            negFunc.LineStyle.Pattern = LinePattern.Dotted;
        }

        if (plotPoint)
        {
            var lineColor = category10.GetColor( colorIndex);
            var markers = plt.Add.Markers(
                new Coordinates[]
                {
                    new Coordinates(InfoPositive.PointH, InfoPositive.PointB),
                    new Coordinates(InfoNegative.PointH, InfoNegative.PointB)
                },
                shape: MarkerShape.OpenCircle,
                color: lineColor);
            markers.Label = LabelPoint;
        }
    }
}