using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.ScottPlotUtility;
using Mantis.Core.TexIntegration;
using Mantis.Core.Utility;
using Mantis.Workspace.C1_Trials.V42_MicrowaveMeasurement;
using MathNet.Numerics;
using ScottPlot;

namespace Mantis.Workspace.C1_Trials.V42_Microwaves_Measurement;


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
        
        var maximumAngle = reader.ExtractSingleValue<ErDouble>("maximum");
        maximumAngle.AddCommand("BraggDifAngle"+cristalDirTex,"\\degree");
        maximumAngle.Error.AddCommand("BraggDiffractionErrorAngle","\\degree");
        var voltageOffset = reader.ExtractSingleValue<double>("voltageOffset");

        var dhkl = Part3_WaveLengths.OfficialWaveLength / 2.0 / ErDouble.Sin(maximumAngle * Constants.Degree);
        var d = dhkl * Math.Sqrt(h * h + k * k + l * l);
        d.AddCommandAndLog("Cristalconstant"+cristalDirTex,"cm");

        List<AngleVoltageData> dataList = reader.ExtractTable<AngleVoltageData>("tab:BraggReflection");

        var MinVoltage = dataList.Select(e => e.Voltage.Value).Min();
        dataList.ForEachRef((ref AngleVoltageData data) =>
        {
            data.Angle.Error = errorAngle;
            //data.Voltage.CalculateDeviceError(Devices.Aglient34405,DataTypes.VoltageDC,voltmeterRange);
            data.Voltage -= voltageOffset;
        });

        var dataSet = dataList.CreateDataSet(e => (e.Angle, e.Voltage));

        var plt = new DynPlot("Winkel in °", "Spannung in V");//"Angle in °", "Voltage in V");

        var errorBar = plt.AddDynErrorBar(dataSet, label:$"Bragg-Reflexion für die {cristalDir}-Ebene");//label:"Bragg diffraction "+cristalDir);
        errorBar.PointConnectedLineStyle.IsVisible = true;

        plt.AddVerticalLine(maximumAngle.Value);
        
        
        plt.Legend.Location = k == 1 ? Alignment.UpperCenter : Alignment.UpperRight;
        
        plt.SaveAndAddCommand("fig:BraggReflection" + cristalDirTex);


    }
}