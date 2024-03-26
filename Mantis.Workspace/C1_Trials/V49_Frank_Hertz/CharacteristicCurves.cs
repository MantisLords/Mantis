using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.ScottPlotUtility;
using Mantis.Core.TexIntegration;
using MathNet.Numerics.LinearAlgebra;
using ScottPlot;

namespace Mantis.Workspace.C1_Trials.V49_Frank_Hertz;

/// <summary>
/// Representing function a(x - b)^c
/// </summary>
public class SchottkyFunc : AutoDerivativeFunc,IFixedParameterCount
{
    public override double CalculateResult(Vector<double> p, double x)
    {
        return p[0] * Math.Pow(x - p[1], p[2]);
    }

    public int ParameterCount => 3;
}

public static class CharacteristicCurves
{
    public static void Process()
    {
        DynPlot characteristicPlot = new DynPlot("anode voltage in V", "anode current in mA");
        DynPlot schottkyPlot = new DynPlot("anode voltage in V", "anode current in mA");
        schottkyPlot.DynAxes.LogX();
        schottkyPlot.DynAxes.LogY();

        
        var reader = V49_Frank_Hertz_Main.Reader;
        string[] measurementIndices = reader.ExtractSingleValue("val:measurementIndices");
        // Process the data for heating voltage Uh = 6.5V and Uh = 7.0V 
        foreach (var index in measurementIndices)
        {
            if(!string.IsNullOrWhiteSpace(index))
                ProcessCharacteristicCurve(index,reader,characteristicPlot,schottkyPlot);
        }

        characteristicPlot.Legend.Location = Alignment.UpperLeft;
        schottkyPlot.Legend.Location = Alignment.UpperLeft;
        
        characteristicPlot.SaveAndAddCommand("fig:characteristicCurve");
        schottkyPlot.SaveAndAddCommand("fig:schottky");
    }

    private static void ProcessCharacteristicCurve(string index, SimpleTableProtocolReader reader,
        DynPlot characteristicPlot,DynPlot schottkyPlot)
    {
        // Extract heating voltage and log it
        ErDouble heatingVoltage =
            reader.ExtractSingleValue<ErDouble>("val:heatingVoltage" + index, V49_Utility.ParseWithErrorOnLastDigit);
        heatingVoltage.AddCommandAndLog("heatingVoltage"+index,"V");

        // Extract characteristic Data to plot it
        List<VoltageData> characteristicData = reader.ExtractTable<VoltageData>("tab:characteristicCurve" + index, 
                V49_Utility.VoltageDataLastDigitErrorParser);

        var dynErrorBar = characteristicPlot.AddDynErrorBar(characteristicData.Select(e => (e.Voltage, e.Current)),
            $"Characteristic curve for Uh = {heatingVoltage.Value} V");
        dynErrorBar.PointConnectedLineStyle.IsVisible = true;
        dynErrorBar.MarkerStyle.Size = 3;
        dynErrorBar.MarkerStyle.Shape = index == "65" ? MarkerShape.OpenTriangleUp : MarkerShape.OpenSquare;
        
        
        // Calculate schottky langmuir law
        // first get region and then extract the correct data
        var (schottkyMin, schottkyMax) = ExtractSchottkyRange(reader, index);
        var schottkyData = characteristicData.Where(e =>
            e.Voltage.Value >= schottkyMin && e.Voltage.Value <= schottkyMax);

        RegModel schottkyModel = schottkyData.CreateRegModel(e => (e.Voltage, e.Current),
            new ParaFunc(3, new SchottkyFunc())
            {
                Labels = new[] {"Factor", "Uk", "Power"},
                Units = new[] {"", "V", ""}
            });

        schottkyModel.DoRegressionLevenbergMarquardt(new[] {1.0, 0.0, 3.0 / 2.0},false);
        schottkyModel.AddParametersToPreambleAndLog("schottky"+index);

        var uk = schottkyModel.ErParameters[1].Value;
        var schottkyBar = schottkyPlot.AddDynErrorBar(schottkyData.Select(e => (e.Voltage - uk, e.Current)),
            $"Characteristic curve for Uh = {heatingVoltage.Value} V");
        //var (schottkyBar,_) = schottkyPlot.AddRegModel(schottkyModel, $"Characteristic curve for Uh = {heatingVoltage.Value} V", "Fit of Schottky-law: Ia = a (Ua - Uk) ^ b");
        schottkyBar.MarkerStyle.Size = 2;
        schottkyBar.MarkerStyle.Shape = index == "65" ? MarkerShape.OpenTriangleUp : MarkerShape.OpenSquare;
        schottkyPlot.AddDynFunction(x => schottkyModel.ParaFunction.EvaluateAtDouble(x + uk),
            "Fit of Schottky-law: Ia = a (Ua - Uk) ^ b");

        schottkyBar.Color = schottkyPlot.Add.Palette.Colors[0];
        
        if (index == "70")
        {
            var l = characteristicPlot.AddVerticalLine(schottkyMin,color:Colors.Black,linePattern:LinePattern.Solid,autoLabel:false);
            var l2 = characteristicPlot.AddVerticalLine(schottkyMax,color:Colors.Black,linePattern:LinePattern.Solid,autoLabel:false);
            l.LineWidth = 0.5f;
            l2.LineWidth = 0.5f;
        }
    }

    private static (double, double) ExtractSchottkyRange(SimpleTableProtocolReader reader, string index)
    {
        string[] range = reader.ExtractSingleValue("val:schottkyRange" + index);
        if (range.Length < 2)
            throw new ArgumentException("The schottkyRange as two argument: Min, Max");
        return (double.Parse(range[0]), double.Parse(range[1]));
    }
    
    
}