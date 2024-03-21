using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.QuickTable;
using Mantis.Core.ScottPlotUtility;
using Mantis.Core.TexIntegration;
using Mantis.Core.Utility;
using MathNet.Numerics.Differentiation;
using MathNet.Numerics.Integration;
using MathNet.Numerics.LinearAlgebra;
using ScottPlot;
using Constants = MathNet.Numerics.Constants;

namespace Mantis.Workspace.C1_Trials.V40_Polarisation;

[QuickTable]
public record struct LocalBFieldData
{
    public const double INITIAL_X_SHIFT = 842;
    
    public ErDouble BField;
    public ErDouble X;

    [UseConstructorForParsing]
    public LocalBFieldData(double bField, string x)
    {
        BField = new ErDouble(bField,bField * FaradayEffect.ErrorBField);
        X = ErDouble.ParseWithErrorLastDigit(x,null,0.1);
        X -= INITIAL_X_SHIFT;
    }
}

[QuickTable]
public record struct BFieldCurrentData
{
    public ErDouble MaxBField;
    public ErDouble MeanBField;
    public ErDouble Current;

    [UseConstructorForParsing]
    public BFieldCurrentData(string bField, string current)
    {
        MaxBField = bField.Split(' ',StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(double.Parse).ToArray()
            .WeightedMean();

        MaxBField.Error = Math.Max(MaxBField.Error, FaradayEffect.ErrorBField * MaxBField.Value);
        
        Current = ErDouble.ParseWithErrorLastDigit(current,null,1);
    }
}

[QuickTable]
public record struct FyAngleData
{
    public ErDouble Current;
    public ErDouble MeanBField;
    public ErDouble Angle;

    [UseConstructorForParsing]
    public FyAngleData(string current, double angle)
    {
        Current = ErDouble.ParseWithErrorLastDigit(current,null,1);
        Angle = new ErDouble(angle,FaradayEffect.ErrorAngle);
    }
}

[QuickTable]
[UseConstructorForParsing]
public record struct GlasDispersionConstantData(double B1,double B2,double B3,double C1,double C2,double C3);

public class TwoCoilsBFieldFunc : AutoDerivativeFunc,IFixedParameterCount
{
    private readonly double _current;

    public TwoCoilsBFieldFunc(double current)
    {
        this._current = current;
    }

    public override double CalculateResult(Vector<double> p, double x)
    {
        var rsq = p[1] * p[1];
        
        var c = FieldOfCoil(x, p[3] + 0.5* p[2], p[4], rsq);
        var c2 = FieldOfCoil(x, p[3] - 0.5 * p[2], p[4], rsq);
        
        return  p[0]*_current * MathNet.Numerics.Constants.MagneticPermeability * (c + c2) * 0.5*Constants.Mega;

        // x -= p[4];
        // return p[0] + p[1] * (Math.Pow(p[2] + Math.Pow(x - p[3] * 0.5, 2), -1.5) +
        // Math.Pow(p[2] + Math.Pow(x + p[3] * 0.5, 2), -1.5));

    }

    private double FieldOfCoil(double x, double a, double d, double r)
    {
        // var xa = x + a;
        // return Math.Pow(r + xa * xa, -1.5);
        var ax = x + a + 0.5 * d;
        var adx = x + a - 0.5 *d;
        return -OneFactor(r, x + a + 0.5 * d) + OneFactor(r, x + a - 0.5 * d); 
        var s = -ax / Math.Sqrt(r + ax * ax) + adx / Math.Sqrt(r + adx * adx);
        return s ;
    }

    private double OneFactor(double r, double b)
        => b / Math.Sqrt(r + b * b);
    
    public int ParameterCount => 5;
}

public static class FaradayEffect
{
    public static double ErrorBField;
    public static double ErrorAngle;

    private static BFieldCurrentData _bcDataAtXMaximum;
    private static double _glasCylinderLength;

    private static ParaFunc _meanFieldCurrentFunc;

    private static Func<double, double> _idealVerdetDispersionFunc;

    private static SimpleTableProtocolReader _reader;

    private static readonly MarkerShape[] _verdetMarkers = new MarkerShape[]
        {MarkerShape.Cross, MarkerShape.OpenDiamond, MarkerShape.OpenCircle};
    
    public static void Process()
    {
        _reader = V40_PolarisationMain.Reader;
        ErrorBField = _reader.ExtractSingleValue<double>("val:errorBField");
        ErrorAngle = _reader.ExtractSingleValue<double>("val:fyErrorAngle");
        ErrorAngle.AddCommandAndLog("fyErrorAngle","\\degree");
        
        CalculateMeanBField();
        CalculateBFieldCurrentDependence();
        CalculateFaradayEffect();
        
    }

    private static void CalculateFaradayEffect()
    {

        DynPlot angleDependencePlot = new DynPlot("mean B-field in mT", "angle in °");
        DynPlot verdetPlot = new DynPlot("wavelength in nm", "verdet's constant in rad/T/m");
        
        CalculateIdealVerdetDispersion();
        
        List<(IErDoubleBase, IErDoubleBase)> verdetPoints = new List<(IErDoubleBase, IErDoubleBase)>();

        var indices = _reader.ExtractSingleValue("val:fyDataIndices").Where(s => !string.IsNullOrEmpty(s)).Select(int.Parse).ToArray();
        foreach (int i in indices)
        {
            CalculateFaradayEffectForWaveLength(i,angleDependencePlot,verdetPoints);
        }

        angleDependencePlot.Legend.Location = Alignment.UpperLeft;
        angleDependencePlot.SaveAndAddCommand("fig:fyAngleDependencePlot");


        verdetPlot.AddDynErrorBar(verdetPoints, "Measured verdet's constants");
        verdetPlot.AddDynFunction(_idealVerdetDispersionFunc, "Ideal Verdet Constants");
        
        verdetPlot.DynAxes.SetLimitsY(16,60);
        verdetPlot.Legend.Location = Alignment.UpperRight;
        verdetPlot.SaveAndAddCommand("fig:fyVerdetPlot");

    }

    private static void CalculateFaradayEffectForWaveLength(int index,Plot angleDepPlot,List<(IErDoubleBase,IErDoubleBase)> verdetPoints)
    {
        var wLArgs = _reader.ExtractSingleValue("val:fyWavelength"+index);
        GErDouble waveLength = GErDouble.Init(double.Parse(wLArgs[0]), double.Parse(wLArgs[1]), double.Parse(wLArgs[2]));
        waveLength.AddCommandAndLog("fyWavelength"+index,"nm");
        waveLength.MinBorder.AddCommandAndLog($"fyWavelength{index}Min","nm");
        waveLength.MaxBorder.AddCommandAndLog($"fyWavelength{index}Max","nm");

        List<FyAngleData> angleData = _reader.ExtractTable<FyAngleData>("tab:fyData" + index);
        var offsetAngle = -angleData[0].Angle.Value;
        angleData.ForEachRef((ref FyAngleData data) =>
        {
            data.Current *= -1;
            data.Angle *= -1;
            data.MeanBField = _meanFieldCurrentFunc.EvaluateAt(data.Current.Value);
            data.Angle -= offsetAngle;
        });
        

        RegModel faradayModel = angleData.CreateRegModel(e => (e.MeanBField, e.Angle), new ParaFunc(2, new LineFunc()));
        faradayModel.DoLinearRegressionWithXErrors();
        // faradayModel.DoLinearRegression();

        ErDouble verdetConstant = faradayModel.ErParameters[1] / _glasCylinderLength;
        verdetConstant *= Constants.Degree;
        verdetConstant *= Constants.Mega;
        verdetConstant.AddCommandAndLog("verdetConstant"+index,"\\frac{rad}{T m}");
        verdetConstant.AddCommandAndLog("verdetConstant"+index+"NoUnit","",LogLevel.OnlyCommand);
        GErDouble.Evaluate(waveLength,_idealVerdetDispersionFunc).AddCommandAndLog("idealVerdetConstant"+index+"NoUnit","");

        var (erBar,dynFunc) = angleDepPlot.AddRegModel(faradayModel, $"Angle rotation \u03BB = {waveLength} nm",
            $"Linear fit: V = {verdetConstant} rad/T/m");
        erBar.MarkerStyle.Shape = _verdetMarkers[index - 1];
        erBar.Color = angleDepPlot.Add.Palette.GetColor(index - 1);
        dynFunc.LineStyle.Color = angleDepPlot.Add.Palette.GetColor(index - 1);
        
        verdetPoints.Add((waveLength,verdetConstant));
        
    }

    private const double EMC = Constants.ElementaryCharge / Constants.ElectronMass / Constants.SpeedOfLight ; 

    private static void CalculateIdealVerdetDispersion()
    {
        var dispersionConstants = _reader.ExtractTable<GlasDispersionConstantData>("tab:glasDispersionConstants");
        var glasConstant = dispersionConstants.First();


        MathNet.Numerics.Differentiation.NumericalDerivative derivative = new NumericalDerivative();
        var nderiv = derivative.CreateDerivativeFunctionHandle(l => Dispersion(l, glasConstant), 1);

        _idealVerdetDispersionFunc = (double l) => - 0.5 * EMC * l * nderiv(l);
        
        Console.WriteLine($"N for 589.3nm: n= {Dispersion(589.3 ,glasConstant)}" +
                          $"NDeriv dn = {nderiv(589.3)}" +
                          $"IdealV V = {_idealVerdetDispersionFunc(589.3)}");
        Console.WriteLine($"N for 435.8nm: n= {Dispersion(435.8 ,glasConstant)}" +
                          $"NDeriv dn = {nderiv(435.8)}" +
                          $"IdealV V = {_idealVerdetDispersionFunc(435.8)}");
        
    }

    private static double Dispersion(double l, GlasDispersionConstantData c)
    {
        l /= 1000;
        double lsq = l * l;
        double nsq = 1 + lsq * (DispersionSummand(lsq, c.B1, c.C1) + DispersionSummand(lsq, c.B2, c.C2) +
                            DispersionSummand(lsq, c.B3, c.C3));
        return Math.Sqrt(nsq);
    }

    private static double DispersionSummand(double lsq, double b, double c) => b / (lsq - c);

    private static void CalculateBFieldCurrentDependence()
    {
        var fieldData = _reader.ExtractTable<BFieldCurrentData>("tab:maxBFieldData");
        
        fieldData.ForEachRef((ref BFieldCurrentData data) => 
            data.MeanBField = data.MaxBField * _bcDataAtXMaximum.MeanBField / _bcDataAtXMaximum.MaxBField);
        
        fieldData.Add(_bcDataAtXMaximum);

        RegModel meanFieldModel = fieldData.CreateRegModel(e => (e.Current, e.MeanBField), new ParaFunc(2, new LineFunc())
        {
            Labels = new []{"m0","mu"},
            Units = new []{"mT","\\frac{mT}{A}"}
        });
        meanFieldModel.DoLinearRegression(true);
        _meanFieldCurrentFunc = meanFieldModel.ParaFunction;
        
        meanFieldModel.AddParametersToPreambleAndLog("MeanBFieldModel");


        DynPlot plot = new DynPlot("Current in A", "Mean B-Field in mT");
        plot.AddRegModel(meanFieldModel, "Mean B-Field in cylinder volume", $"Linear fit: mu = {meanFieldModel.ErParameters[1]} mT/A");
        plot.SaveAndAddCommand("fig:fyMeanBField");

    }

    private static void CalculateMeanBField()
    {
        List<LocalBFieldData> fieldData = _reader.ExtractTable<LocalBFieldData>("tab:localBField");
        var current = _reader.ExtractSingleValue<double>("val:currentForLocalField");
        current.AddCommandAndLog("CurrentForLocalBField","A");

        double xShift = - CenterFieldData(fieldData,current) + LocalBFieldData.INITIAL_X_SHIFT;
        
        RegModel model = fieldData.CreateRegModel(
            e => (e.X, e.BField), new ParaFunc(5, new TwoCoilsBFieldFunc(current))
            {
                Labels = new []{"N","R","a","x0","d"},
                Units = new []{"","mm","mm","mm","mm"}
            });
        
        model.DoRegressionLevenbergMarquardt(new double[] { 100, 5, 10, 0,10},false);
        model.AddParametersToPreambleAndLog("localBFieldModel");

        
        // Calculate Mean field

        _glasCylinderLength = _reader.ExtractSingleValue<double>("val:glasCylinderLength") ;
        _glasCylinderLength.AddCommandAndLog("glasCylinderLength","mm");

        ErDouble cylinderOffset = _reader.ExtractSingleValue<ErDouble>("val:glasCylinderCenterOffset");
        
        double negBound = cylinderOffset.Value -0.5 * _glasCylinderLength;
        double posBound = cylinderOffset.Value + 0.5 * _glasCylinderLength;
        
        cylinderOffset.AddCommandAndLog("cylinderCenterOffset","mm",LogLevel.OnlyLog);

        ErDouble meanField = IntegrateLocalFieldModelOverCylinder(model, cylinderOffset.Value);
        double centerMeanField = IntegrateLocalFieldModelOverCylinder(model, 0);
        double minMeanField = IntegrateLocalFieldModelOverCylinder(model, cylinderOffset.MaxBorder);
            
        double erSq = DoubleExponentialTransformation.Integrate(
            x =>
            {
                ErDouble v = model.ParaFunction.EvaluateAt(x);
                return v.Error * v.Error;
            },
            negBound,
            posBound,
            1e-5);

        meanField.Error = Math.Max( Math.Max(Math.Sqrt(erSq), Math.Abs(meanField.Value - centerMeanField) ) ,Math.Abs(meanField.Value - minMeanField));
        meanField /= _glasCylinderLength;
        meanField.AddCommandAndLog("MeanBField","mT");

        double maximumReadPos = _reader.ExtractSingleValue<double>("val:maximumReadPos") ;
        maximumReadPos -= xShift;

        _bcDataAtXMaximum = new BFieldCurrentData()
        {
            Current = current,
            MeanBField = meanField,
            MaxBField = model.ParaFunction.EvaluateAt(maximumReadPos)
        };
        
        // Plot
        
        var plot = new DynPlot("x in mm", "B-field in mT");

        plot.AddRegModel(model, "Magnetic field measured with hall sensor",
            "Fit: B-fields of two coils");
        var hSpan = plot.Add.HorizontalSpan(negBound, posBound);
        hSpan.Label.Text = $"Cylinder volume; Mean field {meanField} mT";

        plot.Legend.Location = Alignment.LowerCenter;
        plot.DynAxes.SetLimitsX(-22,22);
        plot.SaveAndAddCommand("fig:fyLocalBField");

    }

    private static double IntegrateLocalFieldModelOverCylinder(RegModel model, double offset) =>
        DoubleExponentialTransformation.Integrate(model.ParaFunction.EvaluateAtDouble,
            offset -0.5 * _glasCylinderLength,
            offset + 0.5 * _glasCylinderLength,
            1e-5);

    private static double CenterFieldData(List<LocalBFieldData> fieldData,double current)
    {
        RegModel model = fieldData.CreateRegModel(
            e => (e.X, e.BField), new ParaFunc(5, new TwoCoilsBFieldFunc(current)));

        model.DoRegressionLevenbergMarquardt(new double[] { 100, 5, 10, 0,10},false);
        
        var center = model.ErParameters[3].Value;
        fieldData.ForEachRef((ref LocalBFieldData data) => data.X += center);
        return center;
    }
}