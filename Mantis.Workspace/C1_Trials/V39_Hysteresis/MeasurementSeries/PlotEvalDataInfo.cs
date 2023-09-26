using Mantis.Core.Calculator;
using Mantis.Core.ScottPlotUtility;
using ScottPlot;

namespace Mantis.Workspace.C1_Trials.V39_Hysteresis;

public record PlotRegInfo(RegModel<LineFunc> Model, double PointH, double PointB);

public record PlotEvalDataInfo(PlotRegInfo InfoPositive, PlotRegInfo InfoNegative, string LabelRegression,
    string LabelPoint)
{
    public void Plot(Plot plt, bool plotRegPoints, bool plotLine, bool plotPoint,int colorIndex)
    {
        
        if (plotRegPoints)
        {
            var pointColor = Palette.Microcharts.GetColor(1+colorIndex);
            plt.AddErrorBars(InfoPositive.Model.Data, markerSize: 2,color:pointColor);
            plt.AddErrorBars(InfoNegative.Model.Data, markerSize: 2,color:pointColor);
        }

        if (plotLine)
        {
            var lineColor = plt.Palette.GetColor(1 + colorIndex);
            plt.AddFunction(InfoPositive.Model.ParaFunction, lineWidth: 1,label:LabelRegression,lineStyle:LineStyle.DashDot,color:lineColor);
            plt.AddFunction(InfoNegative.Model.ParaFunction, lineWidth: 1,color: lineColor,lineStyle:LineStyle.DashDot);
        }

        if (plotPoint)
        {
            var pointPlt = plt.AddPoint(InfoPositive.PointH, InfoPositive.PointB, shape: MarkerShape.openCircle,
                label: LabelPoint);
            plt.AddPoint(InfoNegative.PointH, InfoNegative.PointB, shape: MarkerShape.openCircle,
                color: pointPlt.Color);
        }
    }
}