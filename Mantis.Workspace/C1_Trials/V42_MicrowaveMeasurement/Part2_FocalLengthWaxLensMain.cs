using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.QuickTable;
using Mantis.Core.ScottPlotUtility;
using Mantis.Core.TexIntegration;
using Mantis.Core.Utility;
using Mantis.Workspace.C1_Trials.Utility;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;
using ScottPlot;

namespace Mantis.Workspace.C1_Trials.V42_Microwaves_Measurement;

[QuickTable("","tab:focalLengthMeasurement")]
public record struct FocalLengthWaxLensData
{
    [QuickTableField("receiverPos", "cm")] public ErDouble ReceiverPos;

    [QuickTableField("voltage", "V")] public ErDouble Voltage;

    public ErDouble ImageDistance;
    
    public FocalLengthWaxLensData(){}
}

public static class Part2_FocalLengthWaxLensMain
{
    public static void Process()
    {
        var csvReader = new SimpleTableProtocolReader("FocalLengthWaxLens.csv");

        List<FocalLengthWaxLensData> data = csvReader.ExtractTable<FocalLengthWaxLensData>();

        var emitterPosHornEnd = csvReader.ExtractSingleValue<ErDouble>("emitterPosHornEnd");
        var emitterPosHornStart = csvReader.ExtractSingleValue<ErDouble>("emitterPosHornStart");
        var emitterPosDiode = csvReader.ExtractSingleValue<ErDouble>("emitterPosDiode");
        var lensPosLeft = csvReader.ExtractSingleValue<ErDouble>("lensPosLeft");
        var lensPosRight = csvReader.ExtractSingleValue<ErDouble>("lensPosRight");
        var errorReceiverPos = csvReader.ExtractSingleValue<double>("error_receiverPos");

        var lensPos = (lensPosLeft + lensPosRight) / 2.0;
        
        data.ForEachRef((ref FocalLengthWaxLensData element) =>
                CalculateErrorAndImageDistance(ref element,lensPos,errorReceiverPos,1)
            );


        RegModel<GaussFunc> model = data.CreateRegModel(e => (e.ImageDistance, e.Voltage),
            new ParaFunc<GaussFunc>(6)
            {
                Units = new[] {"V","V", "cm", "cm" }
            });

        Vector<double> initialGuess = Vector<double>.Build.DenseOfArray(new double[] {0,1, 90, 100 });

        model.DoRegressionLevenbergMarquardtWithXErrors(initialGuess,5);
        //model.DoRegressionLevenbergMarquardtWithXErrors(initialGuess, 10);
        
        model.AddParametersToPreambleAndLog();
        
        var plt = ScottPlotExtensions.CreateSciPlot("image distance b in cm", "voltage U in V");

        var (errorBar,scatterPlot,funcPlot) = plt.AddRegModel(model, "received Signal", "Fit normal distribution");
        scatterPlot.LineStyle = LineStyle.Solid;
        
        plt.SaveAndAddCommand("fig:imageDistance","Gauss fit");

    }

    private static void CalculateErrorAndImageDistance(ref FocalLengthWaxLensData element, ErDouble lensPos,
        double errorReceiverPos, double voltageRange)
    {
        element.Voltage = DeviceErrorsUtil.CalculateError(Devices.Aglient34405, DataTypes.VoltageDC, voltageRange,
            element.Voltage.Value);
        

        element.ReceiverPos.Error = errorReceiverPos;

        element.ImageDistance = element.ReceiverPos - lensPos;


    }
}