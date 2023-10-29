using System.Reflection.Metadata;
using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.QuickTable;
using Mantis.Core.ScottPlotUtility;
using Mantis.Core.TexIntegration;
using Mantis.Core.Utility;
using Mantis.Workspace.C1_Trials.Utility;
using MathNet.Numerics;
using ScottPlot;

namespace Mantis.Workspace.C1_Trials.V42_Microwaves_Measurement;

[QuickTable("","tab:Bragg")]
public record struct BraggData
{
    [QuickTableField("angle", "°")] public ErDouble Angle = 0;
    [QuickTableField("voltage", "V",lastDigitError:1)] public ErDouble Voltage = 0;
    
    public BraggData(){}
}

public static class Part6_BraggReflection
{
    public static void Process()
    {
        ProcessBraggReflection("100",1,0,0,"OneZeroZero");
        ProcessBraggReflection("110",1,1,0,"OneOneZero");
    }

    public static void ProcessBraggReflection(string cristalDir,int h,int k,int l,string cristalDirTex)
    {
        var reader = new SimpleTableProtocolReader("Part6_BraggReflection" + cristalDir);

        var errorAngle = reader.ExtractSingleValue<double>("error_angle");
        errorAngle.AddCommand("BraggDiffractionErrorAngle","\\degree");
        var voltmeterRange = reader.ExtractSingleValue<double>("voltmeterRange");
        var maximumAngle = reader.ExtractSingleValue<ErDouble>("maximum");
        var voltageOffset = reader.ExtractSingleValue<double>("voltageOffset");

        var dhkl = Part3_WaveLengths.OfficialWaveLength / 2.0 / ErDouble.Sin(maximumAngle * Constants.Degree);
        var d = dhkl * Math.Sqrt(h * h + k * k + l * l);
        d.AddCommandAndLog("Cristalconstant"+cristalDirTex,"cm");

        List<BraggData> dataList = reader.ExtractTable<BraggData>("tab:BraggReflection");

        var MinVoltage = dataList.Select(e => e.Voltage.Value).Min();
        dataList.ForEachRef((ref BraggData data) =>
        {
            data.Angle.Error = errorAngle;
            //data.Voltage.CalculateDeviceError(Devices.Aglient34405,DataTypes.VoltageDC,voltmeterRange);
            data.Voltage -= voltageOffset;
        });

        var dataSet = dataList.CreateDataSet(e => (e.Angle, e.Voltage));

        var plt = ScottPlotExtensions.CreateSciPlot("Angle in °", "Voltage in V");

        var (_,scatterPlot) =plt.AddErrorBars(dataSet,label:"Bragg diffraction "+cristalDir);
        scatterPlot.LineStyle = LineStyle.Solid;

        var vLine = plt.AddVerticalLine(maximumAngle.Value,style:LineStyle.Dash);
        vLine.PositionLabel = true;
        vLine.PositionLabelOppositeAxis = true;
        vLine.PositionLabelBackground = vLine.Color;

        plt.Legend(true, Alignment.UpperRight);

        plt.SaveAndAddCommand("fig:BraggReflection" + cristalDirTex);


    }
}