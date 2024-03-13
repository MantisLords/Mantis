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
    
    [UseConstructorForParsing]
    public AngleVoltageData(ErDouble angle, ErDouble cosAngle, ErDouble voltage)
    {
        this.angle = angle;
        this.angle.Error = 0.5;
        this.voltage = voltage;
        this.voltage.Error = this.voltage.Value * 0.025;
        this.cosAngle = cosAngle;

    }
}
public class V33_AngleDependence
{
    public static void Process()
    {
        var csvReader = new SimpleTableProtocolReader("AngleData");
        List<AngleVoltageData> dataList = csvReader.ExtractTable<AngleVoltageData>("tab:AngleData");
        
        DynPlot plot = new DynPlot("Angle [deg]","Voltage [mV]");
        plot.AddDynErrorBar(dataList.Select(e => (e.angle, e.voltage)),label:"Measured voltage proportional to the radiation");
        
        var theoreticalFunction = new Func<double, double>(x => dataList[0].voltage.Value * Math.Cos(x.ToRadians()));
        plot.AddDynFunction(theoreticalFunction,label:"Radiation curve of a perfect black body according Lambert");
        plot.SaveAndAddCommand("AnglePlot");
        
        DynPlot plotTwo = new DynPlot("Cosine of angle","Voltage [mV]");
        plotTwo.AddDynErrorBar(dataList.Select(e => (e.cosAngle, e.voltage)),label:"Measured voltage proportional to the radiation");
        
        RegModel model = dataList.CreateRegModel(e => (e.cosAngle, e.voltage),
        new ParaFunc(2,new LineFunc())
        {
            Units = new []{"",""}
        }
        );
        model.DoLinearRegression(true);
        plotTwo.AddRegModel(model,labelFunction:"Fitted linear function through the data");
        plotTwo.SaveAndAddCommand("LinearizedAnglePlot");


    }
}