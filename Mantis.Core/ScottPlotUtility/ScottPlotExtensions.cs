using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using Mantis.Core.Calculator;
using Mantis.Core.FileManagement;
using Mantis.Core.TexIntegration;
using Mantis.Core.Utility;
using ScottPlot;
using ScottPlot.Plottable;
using ScottPlot.Renderable;
using FunctionPlot = ScottPlot.Plottable.FunctionPlot;


namespace Mantis.Core.ScottPlotUtility;

public static class ScottPlotExtensions
{
    public static (ErrorBar,ScatterPlot) AddErrorBars(this ScottPlot.Plot plt, Calculator.DataSet dataSet,Color? color = null,
        float markerSize = 4F, string label = "",bool logY = false,bool logX = false,bool errorBars = true)
    {
        var (xs, xErP, xErN) = ConvertDataPoints(dataSet.XValues, dataSet.XErrors, logX);
        var (ys, yErP, yErN) = ConvertDataPoints(dataSet.YValues, dataSet.YErrors, logY);

        var scatter = plt.AddScatter(xs, ys, color, markerSize: markerSize, lineStyle: LineStyle.None, label: label);
        scatter.MarkerShape = MarkerShape.eks;
        

        ErrorBar errorBar = null;
        if (errorBars)
        {
            errorBar = plt.AddErrorBars(xs, ys, xErP, xErN, yErP, yErN, scatter.Color, 0f);
            errorBar.LineWidth = 0.75f;
        }

        return (errorBar,scatter);
    }

    public static (ErrorBar,ScatterPlot) AddErrorBars(this Plot plt, IEnumerable<(ErDouble, ErDouble)> data,
        Color? color = null, float markerSize = 4F,string label = "",bool logY = false,bool logX = false,bool errorBars = true)
    {
        var (xs, xErP, xErN) = ConvertDataPoints(data.Select(e => e.Item1.Value), data.Select(e => e.Item1.Error), logX);
        var (ys, yErP, yErN) = ConvertDataPoints(data.Select(e => e.Item2.Value), data.Select(e => e.Item2.Error), logY);

        var scatter = plt.AddScatter(xs, ys,color:color, markerSize: markerSize, lineStyle: LineStyle.None, label: label);
        scatter.MarkerShape = MarkerShape.eks;
        ErrorBar errorBar = null;
        if (errorBars)
        {
            errorBar = plt.AddErrorBars(xs, ys, xErP, xErN, yErP, yErN, scatter.Color, 0f);
            errorBar.LineWidth = 0.75f;
        }

        return (errorBar,scatter);
    }

    public static FunctionPlot AddFunction<T>(this Plot plt, ParaFunc<T> paraFunc,
        Color? color = null,double lineWidth = 1.5D,LineStyle lineStyle = LineStyle.Solid,string label = "",
        bool logX = false,bool logY = false)
        where T : FuncCore, new()
    {
        Func<double, double?> function;
        if(!logX && !logY)
            function = x => paraFunc.EvaluateAtDouble(x);
        else if (logY && !logX)
            function = x => Math.Log10(paraFunc.EvaluateAtDouble(x));
        else if (logX && !logY)
            function = x => paraFunc.EvaluateAtDouble(Math.Pow(10, x));
        else
        {
            function = x => Math.Log10(paraFunc.EvaluateAtDouble(Math.Pow(10, x)));
        }
        var graph = plt.AddFunction(function, color, lineWidth, lineStyle);
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
        string labelFunction = "",bool logX = false,bool logY = false,bool errorBars = true)
        where T : FuncCore, new()
    {
        var (graphErrors,scatterPlot) = plt.AddErrorBars(model.Data, label: labelData,logX:logX,logY:logY,errorBars:errorBars);
        var graphFunction = plt.AddFunction(model.ParaFunction, label: labelFunction,logX:logX,logY:logY);
        return (graphErrors,scatterPlot, graphFunction);
    }

    public static void SaveAndAddCommand(this Plot plt,string label,string? caption = null,double scale = 4)
    {
        int argumentCount = 0;
        
        string path = label.Replace(':', '_').Replace(' ', '_');
        string commandLabel = path.Replace("_", null);
        plt.SaveFigHere(path, scale: scale);

        StringBuilder builder = new StringBuilder();
        builder.AppendCommand("begin{figure}[htbp]");
        builder.AppendBegin("center");        
        builder.AppendCommand($"includegraphics[width = 0.9\\linewidth]{{{FileManager.GetRelativeCurrentOutputDir(true) +  path}}}");

        if(!string.IsNullOrEmpty(caption))
            builder.AppendCaption(caption);
        else
        {
            argumentCount++;
            builder.AppendCaption("#"+argumentCount);
        }

        builder.AppendLabel(label);
        builder.AppendEnd("center");
        
        builder.AppendEnd("figure");
        
        TexPreamble.AddCommand(builder.ToString(),commandLabel,argumentCount);
    }

    public static string SaveFigHere(this Plot plt,string filePath,
        int? width = null,int? height = null,bool lowQuality = false,double scale = 1D)
    {
        string path = PathUtility.TryCombineAndAddExtension(FileManager.CurrentOutputDir, filePath,"png");

        Directory.CreateDirectory(Path.GetDirectoryName(path));
        return plt.SaveFig(path, width, height, lowQuality, scale);
    }

    // pixelWidth = 520
    public static Plot CreateSciPlot(string xLabel, string yLabel, string title = "", double relHeight = 0.75,
        int pixelWidth = 438)
    {
        var plt = new Plot(pixelWidth,(int)(pixelWidth * relHeight ));
        plt.ConfigureToSciStyle(xLabel,yLabel,title);
        return plt;
    }

    public static void ConfigureToSciStyle(this Plot plt,string xLabel,string yLabel,string title = "")
    {
        string[] customColors = {"#000000","#d63638","#135e96","#005c12"};

        // create a custom palette and set it in the plot module
        plt.Palette = ScottPlot.Palette.FromHtmlColors(customColors);
        //plt.Palette = Palette.Microcharts;
        
        //Console.WriteLine(ScottPlot.Drawing.InstalledFont.ValidFontName("CMU Serif"));
        plt.Style(new SciStyle());
        
        plt.XAxis.TickMarkDirection(false);
        plt.YAxis.TickMarkDirection(false);

        var rightAxis = plt.RightAxis;
        rightAxis.Ticks(true,true,false);
        rightAxis.TickMarkDirection(false);
        
        plt.TopAxis.Ticks(true,true,false);
        plt.TopAxis.TickMarkDirection(false);
        plt.Legend();

        plt.XAxis.Label(xLabel);
        plt.YAxis.Label(yLabel);
        plt.Title(title);
    }

    public static void SetLabelsToLog(this Axis axis)
    {
        static string logTickLabels(double y) => Math.Pow(10, y).ToString();
        axis.TickLabelFormat(logTickLabels);

// Use log-spaced minor tick marks and grid lines to make it more convincing
        axis.MinorLogScale(true);
        axis.MajorGrid(true, Color.FromArgb(80, Color.Black));
        axis.MinorGrid(true, Color.FromArgb(20, Color.Black));
    }

    private static (double[] xs,  double[] xErP, double[] xErN)
        ConvertDataPoints(
            IEnumerable<double> xList,
            IEnumerable<double> xErList,
            bool logX)
    {
        double[] xLinear = xList.ToArray();
        double[] xErLinear = xErList.ToArray();
        
        if (!logX)
        {
            
            return (xLinear, xErLinear, xErLinear);
        }
        else
        {
            double[] xLog = xLinear.Select(Math.Log10).ToArray();
            double[] xErLogP = new double[xLinear.Length];
            double[] xErLogN = new double[xLinear.Length];
            
            for (int i = 0; i < xLinear.Length; i++)
            {
                xErLogP[i] = Math.Log10(xLinear[i] + xErLinear[i]) - xLog[i];
                xErLogN[i] = xLog[i] - Math.Log10(xLinear[i] - xErLinear[i]);
            }
            
            // for (int i = 0; i < xLinear.Length; i++)
            // {
            //     Console.WriteLine($"xLin: {xLinear[i]} xLog {xLog[i]} + {xErLogP[i]} - {xErLogN[i]}");
            // }

            return (xLog, xErLogP, xErLogN);

        }
        
    }
}