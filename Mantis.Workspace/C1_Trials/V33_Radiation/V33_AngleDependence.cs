using System.Runtime.InteropServices.ComTypes;
using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.QuickTable;
using Mantis.Core.ScottPlotUtility;
using Mantis.Core.Utility;
using ScottPlot;
using ScottPlot.Extensions;

namespace Mantis.Workspace.C1_Trials.V33_Radiation;

[QuickTable("", "")]
public record struct AngleVoltageData
{
    [QuickTableField("angle", "")] public ErDouble angle;
    [QuickTableField("cosAngle", "")] public ErDouble cosAngle;
    [QuickTableField("voltage", "")] public ErDouble voltage;
    
    public AngleVoltageData()
    {}
}
public class V33_AngleDependence
{
    public static void Process()
    {
        var csvReader = new SimpleTableProtocolReader("AngleData");
        List<AngleVoltageData> dataList = csvReader.ExtractTable<AngleVoltageData>("tab:AngleData");
        
        DynPlot plot = new DynPlot();
        plot.AddDynErrorBar(dataList.Select(e => (e.angle, e.voltage)));
        
        var theoreticalFunction = new Func<double, double>(x => dataList[0].voltage.Value * Math.Cos(x.ToRadians()));
        plot.AddDynFunction(theoreticalFunction);
        plot.SaveAndAddCommand("AnglePlot");
        
        DynPlot plotTwo = new DynPlot();
        plotTwo.AddDynErrorBar(dataList.Select(e => (e.cosAngle, e.voltage)));
        
        RegModel model = dataList.CreateRegModel(e => (e.cosAngle, e.voltage),
        new ParaFunc(2,new LineFunc())
        {
            Units = new []{"",""}
        }
        );
        model.DoLinearRegression(false);
        plotTwo.AddRegModel(model);
        plotTwo.SaveAndAddCommand("LinearizedAnglePlot");


    }
}