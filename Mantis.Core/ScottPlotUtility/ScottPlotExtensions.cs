using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using Mantis.Core.Calculator;
using Mantis.Core.FileManagement;
using Mantis.Core.TexIntegration;
using Mantis.Core.Utility;
using ScottPlot;
using ScottPlot.Plottable;
using FunctionPlot = ScottPlot.Plottable.FunctionPlot;


namespace Mantis.Core.ScottPlotUtility;

public static class ScottPlotExtensions
{
    public static (ErrorBar,ScatterPlot) AddErrorBars(this ScottPlot.Plot plt, Calculator.DataSet dataSet,Color? color = null,
        float markerSize = 7F, string label = "")
    {
        double[] xs = dataSet.XValues.ToArray();
        double[] ys = dataSet.YValues.ToArray();
        var xErrors = dataSet.XErrors.ToArray();
        double[] yErrors = dataSet.YErrors.ToArray();
        var errorBar = plt.AddErrorBars(xs, ys, xErrors, yErrors, color, 0f);
        errorBar.LineWidth = 1.5f;
        var scatter = plt.AddScatter(xs, ys, errorBar.Color, markerSize: markerSize, lineStyle: LineStyle.None, label: label);

        return (errorBar,scatter);
    }

    public static (ErrorBar,ScatterPlot) AddErrorBars(this Plot plt, IEnumerable<(ErDouble, ErDouble)> data,
        Color? color = null, float markerSize = 7F,string label = "")
    {
        var xs = data.Select(e => e.Item1.Value).ToArray();
        var xErrors = data.Select(e => e.Item1.Error).ToArray();
        var ys = data.Select(e => e.Item2.Value).ToArray();
        var yErrors = data.Select(e => e.Item2.Error).ToArray();

        var errorBar = plt.AddErrorBars(xs, ys, xErrors, yErrors, color, 0f);
        errorBar.LineWidth = 1.5f;
        var scatter = plt.AddScatter(xs, ys, errorBar.Color, markerSize: markerSize, lineStyle: LineStyle.None, label: label);
        return (errorBar,scatter);
    }

    public static FunctionPlot AddFunction<T>(this Plot plt, ParaFunc<T> paraFunc,
        Color? color = null,double lineWidth = 1.5D,LineStyle lineStyle = LineStyle.Solid,string label = "")
        where T : FuncCore, new()
    {
        double? Function(double x) => paraFunc.EvaluateAtDouble(x);
        var graph = plt.AddFunction(Function, color, lineWidth, lineStyle);
        graph.Label = label;
        return graph;
    }

    public static (ScatterPlot,Polygon) AddFunctionWithError<T>(this Plot plt, ParaFunc<T> paraFunc,int points = 100,
        Color? color = null, float lineWidth = 2f,double errorAlpha = 0.5,string label = "")
        where T : FuncCore, new()
    {
        var limits = plt.GetAxisLimits();

        var xs = new double[points];
        var ys = new double[points];
        var yErrors = new double[points];

        for (int i = 0; i < points; i++)
        {
            var x = limits.XMin + limits.XSpan * i / 100d;
            xs[i] = x;

            var res = paraFunc.EvaluateAt(x);
            ys[i] = res.Value;
            yErrors[i] = res.Error;
        }
        
        var graphScatter = plt.AddScatter(xs, ys, color: color, lineWidth: lineWidth, markerSize: 0f, label: label);
        var graphError = plt.AddFillError(xs, ys, yErrors, Color.FromArgb((int)(errorAlpha * 256), graphScatter.Color));

        return (graphScatter, graphError);
    }

    public static (ErrorBar,ScatterPlot, FunctionPlot) AddRegModel<T>(this Plot plt, RegModel<T> model, string labelData = "",
        string labelFunction = "")
        where T : FuncCore, new()
    {
        var (graphErrors,scatterPlot) = plt.AddErrorBars(model.Data, label: labelData);
        var graphFunction = plt.AddFunction(model.ParaFunction, label: labelFunction);
        return (graphErrors,scatterPlot, graphFunction);
    }

    public static void SaveAndAddCommand(this Plot plt,string label,string caption,double scale = 4)
    {
        string path = label.Replace(':', '_').Replace(' ', '_');
        string commandLabel = path.Replace("_", null);
        plt.SaveFigHere(path, scale: scale);

        StringBuilder builder = new StringBuilder();
        builder.AppendCommand("begin{figure}[h]");
        
        builder.AppendCommand($"includegraphics[width = \\textwidth]{{{FileManager.GetRelativeCurrentOutputDir(true) +  path}}}");
        builder.AppendCommand("centering");
        if(!string.IsNullOrEmpty(caption))
            builder.AppendCaption(caption);
        
        builder.AppendLabel(label);
        
        builder.AppendEnd("figure");
        
        TexPreamble.AddCommand(builder.ToString(),commandLabel);
    }

    public static string SaveFigHere(this Plot plt,string filePath,
        int? width = null,int? height = null,bool lowQuality = false,double scale = 1D)
    {
        string path = PathUtility.TryCombineAndAddExtension(FileManager.CurrentOutputDir, filePath,"png");

        Directory.CreateDirectory(Path.GetDirectoryName(path));

        return plt.SaveFig(path, width, height, lowQuality, scale);
    }

    public static Plot CreateSciPlot(string xLabel, string yLabel, string title = "", double relHeight = 0.75,
        int pixelWidth = 520)
    {
        var plt = new Plot(pixelWidth,(int)(pixelWidth * relHeight ));
        plt.ConfigureToSciStyle(xLabel,yLabel,title);
        return plt;
    }

    public static void ConfigureToSciStyle(this Plot plt,string xLabel,string yLabel,string title = "")
    {
        plt.Palette = Palette.Microcharts;
        //Console.WriteLine(ScottPlot.Drawing.InstalledFont.ValidFontName("CMU Serif"));
        plt.Style(new SciStyle());
        
        plt.XAxis.TickMarkDirection(false);
        plt.YAxis.TickMarkDirection(false);
        plt.RightAxis.Ticks(true,true,false);
        plt.RightAxis.TickMarkDirection(false);
        plt.TopAxis.Ticks(true,true,false);
        plt.TopAxis.TickMarkDirection(false);
        plt.Legend();

        plt.XAxis.Label(xLabel);
        plt.YAxis.Label(yLabel);
        plt.Title(title);
    }
}