using Mantis.Core.Calculator;
using Mantis.Core.ScottPlotUtility;
using ScottPlot;

namespace Mantis.Workspace.C1_Trials.V39_Hysteresis;

public record PlotRegInfo(RegModel<LineFunc> Model, double PointH, double PointB);

public record PlotEvalDataInfo(PlotRegInfo InfoPositive, PlotRegInfo InfoNegative, string LabelRegression,
    string LabelPoint)
{
    public void Plot(Plot plt, bool plotRegPoints, bool plotLine, bool plotPoint)
    {
        if (plotRegPoints)
        {
            var (_,scatter) = plt.AddErrorBars(InfoPositive.Model.Data, markerSize: 2);
            plt.AddErrorBars(InfoNegative.Model.Data, markerSize: 2,color:scatter.Color);
        }

        if (plotLine)
        {
            var funcPlot = plt.AddFunction(InfoPositive.Model.ParaFunction, lineWidth: 1,label:LabelRegression,lineStyle:LineStyle.DashDot);
            plt.AddFunction(InfoNegative.Model.ParaFunction, lineWidth: 1,color: funcPlot.Color,lineStyle:LineStyle.DashDot);
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