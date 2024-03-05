using System.Drawing;
using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.QuickTable;
using Mantis.Core.ScottPlotUtility;
using Mantis.Core.TexIntegration;
using Mantis.Core.Utility;
using MathNet.Numerics.LinearAlgebra;
using ScottPlot;
using ScottPlot.Palettes;
using Color = ScottPlot.Color;

namespace Mantis.Workspace.C1_Trials.V31_RealGasStateVariables;

[QuickTable("", "ChamberData")]
public record struct VolumePressureData
{
    [QuickTableField("pressure", "Pa")] public ErDouble pressure;

    [QuickTableField("volume", "cm^3")] public ErDouble volume;

    public VolumePressureData()
    {
    }
}

public record struct TemperaturePressureData
{
    public ErDouble temperature;
    public ErDouble pressure;

    public TemperaturePressureData()
    {
    }
}

public class InvExpFunc : AutoDerivativeFunc, IFixedParameterCount
{
    public override double CalculateResult(Vector<double> p, double x)
    {
        return p[0] * Math.Exp(p[1] / x);
    }

    public int ParameterCount => 2;
}

public class BiExponentialFunc : AutoDerivativeFunc, IFixedParameterCount
{
    public override double CalculateResult(Vector<double> p, double x)
    {
        return p[0] * Math.Log(p[1] * x) + p[2] * Math.Exp(-p[3] * x);
    }

    public int ParameterCount => 4;
}

public static class Part1_IsothermsAndCriticalPoints
{
    public static void Process()
    {
        var csvReader1 = new SimpleTableProtocolReader("ChamberDataTemp1.csv");
        List<VolumePressureData> dataListTemp1 = csvReader1.ExtractTable<VolumePressureData>("tab:ChamberData");
        dataListTemp1.ForEachRef((ref VolumePressureData data) =>
        {
            data.pressure.Error = 0.25;
            data.volume.Error = 0.02;
        });
        var csvReader2 = new SimpleTableProtocolReader("ChamberDataTemp2.csv");
        List<VolumePressureData> dataListTemp2 = csvReader2.ExtractTable<VolumePressureData>("tab:ChamberData");
        dataListTemp2.ForEachRef((ref VolumePressureData data) =>
        {
            data.pressure.Error = 0.25;
            data.volume.Error = 0.02;
        });
        var csvReader3 = new SimpleTableProtocolReader("ChamberDataTemp3.csv");
        List<VolumePressureData> dataListTemp3 = csvReader3.ExtractTable<VolumePressureData>("tab:ChamberData");
        dataListTemp3.ForEachRef((ref VolumePressureData data) =>
        {
            data.pressure.Error = 0.25;
            data.volume.Error = 0.02;
        });
        var csvReader4 = new SimpleTableProtocolReader("ChamberDataTemp4.csv");
        List<VolumePressureData> dataListTemp4 = csvReader4.ExtractTable<VolumePressureData>("tab:ChamberData");
        dataListTemp4.ForEachRef((ref VolumePressureData data) =>
        {
            data.pressure.Error = 0.25;
            data.volume.Error = 0.02;
        });
        var csvReader5 = new SimpleTableProtocolReader("ChamberDataTemp5.csv");
        List<VolumePressureData> dataListTemp5 = csvReader5.ExtractTable<VolumePressureData>("tab:ChamberData");
        dataListTemp5.ForEachRef((ref VolumePressureData data) =>
        {
            data.pressure.Error = 0.25;
            data.volume.Error = 0.02;
        });
        var csvReader6 = new SimpleTableProtocolReader("ChamberDataTemp6.csv");
        List<VolumePressureData> dataListTemp6 = csvReader6.ExtractTable<VolumePressureData>("tab:ChamberData");
        dataListTemp6.ForEachRef((ref VolumePressureData data) =>
        {
            data.pressure.Error = 0.25;
            data.volume.Error = 0.02;
        });
        var csvReader7 = new SimpleTableProtocolReader("ChamberDataTemp7.csv");
        List<VolumePressureData> dataListTemp7 = csvReader7.ExtractTable<VolumePressureData>("tab:ChamberData");
        dataListTemp7.ForEachRef((ref VolumePressureData data) =>
        {
            data.pressure.Error = 0.25;
            data.volume.Error = 0.02;
        });
        var csvReader8 = new SimpleTableProtocolReader("ChamberDataTemp8.csv");
        List<VolumePressureData> dataListTemp8 = csvReader8.ExtractTable<VolumePressureData>("tab:ChamberData");
        dataListTemp8.ForEachRef((ref VolumePressureData data) =>
        {
            data.pressure.Error = 0.25;
            data.volume.Error = 0.02;
        });
        var csvReader9 = new SimpleTableProtocolReader("ChamberDataTemp9.csv");
        List<VolumePressureData> dataListTemp9 = csvReader9.ExtractTable<VolumePressureData>("tab:ChamberData");
        dataListTemp9.ForEachRef((ref VolumePressureData data) =>
        {
            data.pressure.Error = 0.5;
            data.volume.Error = 0.02;
        });

        DynPlot plot = new DynPlot("volume V [cm^3]", "pressure p [bar]");
        plot.Add.Palette = new Category20();
        
        plot.AddDynErrorBar(dataListTemp1.Select(e => (e.volume, e.pressure)), label: "30.2°C");
        plot.AddDynErrorBar(dataListTemp2.Select(e => (e.volume, e.pressure)), label: "35.1°C");
        plot.AddDynErrorBar(dataListTemp3.Select(e => (e.volume, e.pressure)), label: "40.1°C");
        plot.AddDynErrorBar(dataListTemp4.Select(e => (e.volume, e.pressure)), label: "43.1°C");
        plot.AddDynErrorBar(dataListTemp5.Select(e => (e.volume, e.pressure)), label: "45.1°C");
        plot.AddDynErrorBar(dataListTemp6.Select(e => (e.volume, e.pressure)), label: "46.1°C");
        plot.AddDynErrorBar(dataListTemp7.Select(e => (e.volume, e.pressure)), label: "46.6°C");
        plot.AddDynErrorBar(dataListTemp8.Select(e => (e.volume, e.pressure)), label: "47.2°C");
        plot.AddDynErrorBar(dataListTemp9.Select(e => (e.volume, e.pressure)), label: "48.2°C");

        List<VolumePressureData> dataForFit = new List<VolumePressureData>();
        CalculateMaxwellLine(dataListTemp1, plot, 0.25, 0.5, dataForFit, 100);
        CalculateMaxwellLine(dataListTemp2, plot, 0.25, 0.5, dataForFit, 1000);
        CalculateMaxwellLine(dataListTemp3, plot, 0.25, 0.4, dataForFit, 1000);
        CalculateMaxwellLine(dataListTemp4, plot, 0.5, 0.5, dataForFit, 1.0);
        CalculateMaxwellLine(dataListTemp5, plot, 0.5, 0.35, dataForFit, 0.9);
        CalculateMaxwellLine(dataListTemp6, plot, 0.2, 0.3, dataForFit, 100);

        RegModel polyFunc = dataForFit.CreateRegModel(e => (e.volume, e.pressure),
            new ParaFunc(4,new BiExponentialFunc())
            {
                Units = new[] {"", "", "", ""}
            }
        );

        polyFunc.DoRegressionLevenbergMarquardt(new double[] {1, 1, 1, 1}, true);
        polyFunc.ErParameters[0].AddCommand("paramA");
        polyFunc.ErParameters[1].AddCommand("paramB");
        polyFunc.ErParameters[2].AddCommand("paramC");
        polyFunc.ErParameters[3].AddCommand("paramD");
        var regressionToFunction = new Func<double, double>(x =>
        {
            if (x > 0.370 && x < 1.65)
                return polyFunc.ErParameters[0].Value * Math.Log(polyFunc.ErParameters[1].Value * x) +
                       polyFunc.ErParameters[2].Value * Math.Exp(-polyFunc.ErParameters[3].Value * x);
            else return double.NaN;
        });
        var interpolation = plot.AddDynFunction(regressionToFunction,color:Colors.Black);
        interpolation.Label = "fitted function of the form p = a * log(bV) + c* e^{-dV}";
        // interpolation.XMax = 1.65;
        // interpolation.XMin = 0.370;
        ErDouble criticalPressure = new ErDouble(36.58, 1.0);
        ErDouble criticalTemperature = new ErDouble(47.0, 1.5);
        ErDouble criticalVolume = new ErDouble(0.69, 0.05);

        List<VolumePressureData> criticalPoint = new List<VolumePressureData>();
        VolumePressureData critPoint = new VolumePressureData();
        critPoint.pressure = criticalPressure;
        critPoint.volume = criticalVolume;
        criticalPoint.Add(critPoint);
        plot.AddDynErrorBar(criticalPoint.Select(e => (e.volume, e.pressure)),color: Colors.Red,
            label: "Critical point with estimated errors");
        criticalPressure.AddCommand("criticalPressure");
        criticalVolume.AddCommand("criticalVolume");
        double m = 0.370;
        List<VolumePressureData> dataForFill = new List<VolumePressureData>();
        for (int i = 0; i < 250; i++)
        {
            VolumePressureData punkt = new VolumePressureData();
            punkt.pressure = polyFunc.ParaFunction.EvaluateAt(m);
            punkt.volume = m;
            dataForFill.Add(punkt);
            m += 0.005;
        }
        
        var fill = plot.Add.Polygon(dataForFill.Select(e => e.volume.Value).ToArray(),
            dataForFill.Select(e => e.pressure.Value).ToArray());

        fill.FillStyle.Color = new Color(80, 12, 45, 67);
        
        //plot.AddRegModel(model, "data", "fitted function");
        plot.Legend.Location = Alignment.UpperRight;
        plot.Legend.Font.Size = 7;
        plot.SaveAndAddCommand("fig:plot", "caption");
        
        plot.SaveAndAddCommand("fig:plot", "caption");
        plot.DynAxes.SetLimits(0.5, 0.8, 33, 38);
        plot.Legend.IsVisible = false;
        plot.SaveAndAddCommand("plotZoom", "caption");


        List<TemperaturePressureData> temperaturePressureForLogPlot = new List<TemperaturePressureData>();
        CreateTemperaturePressureData(dataForFit, csvReader1, temperaturePressureForLogPlot, 0);
        CreateTemperaturePressureData(dataForFit, csvReader2, temperaturePressureForLogPlot, 2);
        CreateTemperaturePressureData(dataForFit, csvReader3, temperaturePressureForLogPlot, 4);
        CreateTemperaturePressureData(dataForFit, csvReader4, temperaturePressureForLogPlot, 6);
        CreateTemperaturePressureData(dataForFit, csvReader5, temperaturePressureForLogPlot, 8);
        CreateTemperaturePressureData(dataForFit, csvReader6, temperaturePressureForLogPlot, 10);

        DynPlot temperaturePlot = new DynPlot("temperature", "pressure");
        temperaturePlot.AddDynErrorBar(temperaturePressureForLogPlot.Select(e => (e.temperature, e.pressure)));
        temperaturePlot.SaveAndAddCommand("fig:temperaturePlot", "caption");

        DynPlot temperatureLogPlot =new DynPlot("inverse temperature 1/T [1/K]", "pressure p [bar]");
        temperaturePlot.DynAxes.LogY();

        temperatureLogPlot.AddDynErrorBar(
            temperaturePressureForLogPlot.Select(e => ((e.temperature + 273.15).Pow(-1), e.pressure)));


        RegModel lineModel = temperaturePressureForLogPlot.CreateRegModel(e =>
                (ErDouble.Log(e.pressure), (1 / (273.15 + e.temperature))),
            new ParaFunc(2,new LineFunc())
            {
                Units = new[] {"", ""}
            }
        );
        lineModel.DoLinearRegression(false);
        lineModel.AddParametersToPreambleAndLog("lineData");
        //Console.WriteLine("berechnete Temp "+( -273.15 + lineModel.ErParameters[1]/(Math.Log(36.26,Math.E)-lineModel.ErParameters[0])));
        ErDouble criticalTemp = (-273.15 + (1 / lineModel.ParaFunction.EvaluateAt(Math.Log(36.58, Math.E))));
        criticalTemp.AddCommand("criticalTemp");
        CalculateA(criticalPressure, criticalVolume, criticalTemp).AddCommand("parameterA");
        CalculateB(criticalPressure, criticalVolume, criticalTemp).AddCommand("parameterB");
        CalculateNu(CalculateB(criticalPressure, criticalVolume, criticalTemp), criticalVolume)
            .AddCommand("amountOfSubstance");
        Console.WriteLine("evaluate at " + criticalTemp);
        RegModel expoModel = temperaturePressureForLogPlot.CreateRegModel(
            e => (1 / (273.15 + e.temperature), e.pressure),
            new ParaFunc(2,new ExpFunc())
            {
                Units = new[] {"", ""}
            }
        );
        expoModel.DoRegressionLevenbergMarquardt(new double[] {1, 1}, false);
        expoModel.AddParametersToPreambleAndLog("ExpoData");
        temperatureLogPlot.AddRegModel(expoModel, "Inverse temperature with error",
            "Exponential fit for pressure dependence");
        temperatureLogPlot.Legend.Location = Alignment.LowerLeft;
        
        temperatureLogPlot.AddHorizontalLine(36.58, color:Colors.Red, label: "Critical pressure at 36.60 bar");
        //line.Min = 1/(46.56+273.15);
        temperatureLogPlot.DynAxes.SetLimits(0.00312, 0.0033, 22.5, 39);
        List<TemperaturePressureData> onePointList = new List<TemperaturePressureData>();
        TemperaturePressureData point = new TemperaturePressureData();
        point.temperature = 1 / (criticalTemp + 273.15);
        point.pressure = new ErDouble(36.58, 1);
        onePointList.Add(point);
        temperatureLogPlot.AddDynErrorBar(onePointList.Select(e => (e.temperature, e.pressure)), color:Colors.Green);
        //temperatureLogPlot.XAxis.TickLabelFormat("e",false);
        expoModel.CalculateReducedResidual().AddCommandAndLog("residual");
        // temperatureLogPlot.SetAxisLimits(0,0.2,22,38);

        //temperatureLogPlot.YAxis.SetLabelsToLog();
        temperatureLogPlot.SaveAndAddCommand("fig:tempLogPlot", "caption");
        
        
        DynPlot linePlot = new DynPlot("Inverse temperature", "Pressure");
        linePlot.DynAxes.LogY();
        linePlot.AddRegModel(expoModel, "Inverse temperature with error", "Exponential fit for pressure dependence");
        linePlot.SaveAndAddCommand("fig_lineTempPlot");
        expoModel.ErParameters[1].AddCommand("regressionExponent");
        CalculateEvaporationEnergy(expoModel.ErParameters[1]).AddCommand("evaporationEnergy");
        CalculateEvaporationEnergy(expoModel.ErParameters[1] * 6.22 * Math.Pow(10, 23) * (-1))
            .AddCommand("evaporationEnergyPerMole");
        //CalculateCriticalTemp(expoModel.ErParameters[1], criticalPressure,
        //expoModel.ErParameters[0]).AddCommand("criticalTemp");
        //comparing with theory
        double T = 30.2 + 273.15; //K
        double R = 83.144; //bar*cm^3/K*mol
        double b = CalculateB(criticalPressure, criticalVolume, criticalTemperature).Value * Math.Pow(10, 6); //cm^3/mol
        double a = CalculateA(criticalPressure, criticalVolume, criticalTemperature).Value *
                   Math.Pow(10, 7); //bar*cm^6/mol^2
        double nu = CalculateNu(CalculateB(criticalPressure, criticalVolume, criticalTemperature), criticalVolume)
            .Value; //mol
        Console.WriteLine("a=" + a + " b= " + b + " nu= " + nu);

        DynPlot comparationPlot = new DynPlot("volume V [cm^3]", "pressure p [bar]");
        var idealGas = new Func<double, double>((v) => R * T * nu / v);
        var vdWGas = new Func<double, double>((v) => nu * R * T / (v - nu * b) - nu * nu * a / (v * v));
        var funktion2 = comparationPlot.AddDynFunction(idealGas, color:Colors.Red);
        funktion2.Label = "Ideal gas";
        var funktion1 = comparationPlot.AddDynFunction(vdWGas, color:Colors.Blue);
        funktion1.Label = "Van der Waals gas";
        comparationPlot.AddDynErrorBar(dataListTemp1.Select(e => (e.volume, e.pressure)), color:Colors.Black,
            label: "Measurement data");
        comparationPlot.DynAxes.SetLimits(0, 3.5, 12.5, 47.5);
        //funktion1.XMin = 0.3;
        comparationPlot.Legend.Location = Alignment.UpperRight;
        comparationPlot.SaveAndAddCommand("comparationPlot", "caption");
    }

    private static List<TemperaturePressureData> CreateTemperaturePressureData(List<VolumePressureData> data,
        SimpleTableProtocolReader reader, List<TemperaturePressureData> returnList, int index)
    {
        TemperaturePressureData e = new TemperaturePressureData();

        e.pressure = data[index].pressure;
        e.pressure.Error = 0.5;
        e.temperature = reader.ExtractSingleValue<double>("temperature");
        e.temperature.Error = 0.2;
        returnList.Add(e);
        return returnList;
    }

    private static ErDouble CalculateCriticalTemp(ErDouble b, ErDouble p, ErDouble expoRegParameter0)
    {
        return (b / ErDouble.Log(p / expoRegParameter0)) - 273.15; //degC
    }

    private static ErDouble CalculateEvaporationEnergy(ErDouble regressionParameter)
    {
        return regressionParameter * 1.380694 * Math.Pow(10, -23);
    }

    public static ErDouble CalculateB(ErDouble pk, ErDouble vk, ErDouble tk)
    {
        Console.WriteLine(pk + " " + vk + " " + tk);
        return ((tk + 273.15) / pk) * (83.144 / 8) * Math.Pow(10, -6); //m^3/mol
    }

    public static ErDouble CalculateNu(ErDouble b, ErDouble vk)
    {
        return vk * Math.Pow(10, -6) / (b * 3); //mol
    }

    public static ErDouble CalculateA(ErDouble pk, ErDouble vk, ErDouble tk)
    {
        ErDouble b = CalculateB(pk, vk, tk);
        return pk * 27 * b.Pow(2) * Math.Pow(10, 5); //Pa*m^6/mol^2
    }

    public static void CalculateMaxwellLine(List<VolumePressureData> data, DynPlot plot, double sensitivity,
        double variance, List<VolumePressureData> dataForFit, double lowerlimit)
    {
        List<VolumePressureData> returnList = new List<VolumePressureData>();
        double sum = 0;
        double count = 0;
        for (int i = 0; i < data.Count - 1; i++)
        {
            var e = new VolumePressureData();
            var m = new VolumePressureData();
            e = data[i];
            m = data[i + 1];
            if (Math.Abs(data[i + 1].pressure.Value - data[i].pressure.Value) <= sensitivity &&
                data[i].volume.Value <= lowerlimit)
            {
                sum += e.pressure.Value;
                returnList.Add(e);
                returnList.Add(m);
                Console.WriteLine(e.pressure.Value + " and data " + m.pressure.Value);
                count++;
            }

            Console.WriteLine(sum / count);
        }

        ErDouble mean = new ErDouble(sum / count, 0.25);
        List<VolumePressureData> maxwellLine1 = CreateMaxwellLine(mean, returnList, variance, dataForFit);
        var scatter = plot.Add.Scatter(maxwellLine1.Select(e => e.volume.Value).ToArray(),
            maxwellLine1.Select(e => e.pressure.Value).ToArray(), Colors.Black);
        scatter.MarkerStyle.Shape = MarkerShape.Cross;
        scatter.MarkerSize = 5f;
        scatter.LineWidth = 2f;
        
        var errorBar = plot.AddDynErrorBar(maxwellLine1.Select(e => (e.volume, e.pressure)),color: Colors.Magenta);
        errorBar.LineStyle.Width = 2f;
    }

    private static List<VolumePressureData> CreateMaxwellLine(ErDouble height, List<VolumePressureData> data,
        double variance, List<VolumePressureData> dataForFit)
    {
        List<VolumePressureData> twoPointList = new List<VolumePressureData>();
        VolumePressureData dataPoint1 = new VolumePressureData();
        List<VolumePressureData> cleanList = new List<VolumePressureData>();
        for (int i = 0; i < data.Count; i++)
        {
            var e = new VolumePressureData();
            e = data[i];
            if (Math.Abs(data[i].pressure.Value - height.Value) <= variance)
            {
                cleanList.Add(e);
            }
        }

        dataPoint1.pressure = height;
        dataPoint1.volume = cleanList[0].volume;
        Console.WriteLine(dataPoint1.volume + "and" + dataPoint1.pressure);
        VolumePressureData dataPoint2 = new VolumePressureData();
        dataPoint2.pressure = height;
        dataPoint2.volume = cleanList[cleanList.Count - 1].volume;
        Console.WriteLine(dataPoint2.volume + "and" + dataPoint2.pressure);
        twoPointList.Add(dataPoint1);
        twoPointList.Add(dataPoint2);
        //Adding the edge points to the dataForFit List which we will use later
        dataForFit.Add(dataPoint1);
        dataForFit.Add(dataPoint2);
        return twoPointList;
    }
}