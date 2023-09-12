using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.QuickTable;
using Mantis.Core.ScottPlotUtility;
using Mantis.Core.Utility;
using Mantis.Workspace.C1_Trials.Utility;
using ScottPlot;

namespace Mantis.Workspace.C1_Trials.V42_Microwaves_Measurement;

[QuickTable("","tab:Bragg")]
public record struct BraggData
{
    [QuickTableField("angle", "°")] public ErDouble Angle = 0;
    [QuickTableField("voltage", "V")] public ErDouble Voltage = 0;
    
    public BraggData(){}
}

public static class Part6_BraggReflection
{
    public static void Process()
    {
        ProcessBraggReflection("100");
        ProcessBraggReflection("110");
    }

    public static void ProcessBraggReflection(string cristalDir)
    {
        var reader = new SimpleTableProtocolReader("BraggReflection" + cristalDir);

        var errorAngle = reader.ExtractSingleValue<double>("error_angle");
        var voltmeterRange = reader.ExtractSingleValue<double>("voltmeterRange");
        var maximumAngle = reader.ExtractSingleValue<ErDouble>("maximum");

        List<BraggData> dataList = reader.ExtractTable<BraggData>("tab:BraggReflection");

        var MinVoltage = dataList.Select(e => e.Voltage.Value).Min();
        dataList.ForEachRef((ref BraggData data) =>
        {
            data.Angle.Error = errorAngle;
            data.Voltage.CalculateDeviceError(Devices.Aglient34405,DataTypes.VoltageDC,voltmeterRange);
            data.Voltage -= MinVoltage;
        });

        var dataSet = dataList.CreateDataSet(e => (e.Angle, e.Voltage));

        var plt = ScottPlotExtensions.CreateSciPlot("Angle in °", "Voltage in V");

        var (_,scatterPlot) =plt.AddErrorBars(dataSet,label:"Bragg Reflection "+cristalDir);
        scatterPlot.LineStyle = LineStyle.Solid;

        var vLine = plt.AddVerticalLine(maximumAngle.Value,style:LineStyle.Dash);
        vLine.PositionLabel = true;
        vLine.PositionLabelOppositeAxis = true;
        vLine.PositionLabelBackground = vLine.Color;

        plt.Legend(true, Alignment.UpperRight);

        plt.SaveFigHere("fig_BraggReflection" + cristalDir);


    }
}