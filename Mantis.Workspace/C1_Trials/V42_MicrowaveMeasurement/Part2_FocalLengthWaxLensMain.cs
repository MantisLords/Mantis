using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.QuickTable;
using Mantis.Core.ScottPlotUtility;
using Mantis.Core.TexIntegration;
using Mantis.Core.Utility;
using ScottPlot;

namespace Mantis.Workspace.C1_Trials.V42_MicrowaveMeasurement;

[QuickTable("","tab:focalLengthMeasurement")]
public record struct FocalLengthWaxLensData
{
    [QuickTableField("receiverPos", "cm")] public ErDouble ReceiverPos = 0;

    [QuickTableField("voltage", "V",lastDigitError:1)] public ErDouble Voltage = 0;

    public ErDouble EffectiveReceiverPos = 0;
    
    public ErDouble ImageDistance = 0;
    
    public FocalLengthWaxLensData()
    {
    }
}

public static class Part2_FocalLengthWaxLensMain
{
    public static void Process()
    {
        var csvReader = new SimpleTableProtocolReader("Part2_FocalLengthWaxLens.csv");

        List<FocalLengthWaxLensData> data = csvReader.ExtractTable<FocalLengthWaxLensData>();

        var emitterPosHornEnd = csvReader.ExtractSingleValue<ErDouble>("emitterPosHornEnd");
        //var emitterPosHornStart = csvReader.ExtractSingleValue<ErDouble>("emitterPosHornStart");
        var distanceHornEndEffectiveEmitterPos =
            csvReader.ExtractSingleValue<ErDouble>("distanceHornEndEffectiveEmitterPos");

        var effectiveEmitterPos = emitterPosHornEnd - distanceHornEndEffectiveEmitterPos;
        
        //var emitterPosDiode = csvReader.ExtractSingleValue<ErDouble>("emitterPosDiode");
        var lensPosLeft = csvReader.ExtractSingleValue<ErDouble>("lensPosLeft");
        var lensPosRight = csvReader.ExtractSingleValue<ErDouble>("lensPosRight");
        var lensPos = (lensPosLeft + lensPosRight) / 2.0;

        var distanceHornEndEffectiveReceiverPos =
            csvReader.ExtractSingleValue<ErDouble>("distanceHornEndEffectiveReceiverPos");
        var distanceReceiverPosHornEnd = csvReader.ExtractSingleValue<ErDouble>("distanceReceiverPosHornEnd");
        var errorReceiverPos = csvReader.ExtractSingleValue<double>("error_receiverPos");

        var voltageOffset = csvReader.ExtractSingleValue<double>("voltageOffset");
        
        data.ForEachRef((ref FocalLengthWaxLensData element) =>
            CalculateErrorAndImageDistance(ref element,lensPos,errorReceiverPos,distanceHornEndEffectiveReceiverPos,distanceReceiverPosHornEnd,1,voltageOffset)
        );
        
        data.Sort(((a, b) => a.ReceiverPos.CompareTo(b.ReceiverPos.Value)));


        RegModel model = data.CreateRegModel(e => (e.ImageDistance, e.Voltage),
            new ParaFunc(6,new GaussFunc())
            {
                Units = new[] {"V","V", "cm", "cm" }
            });


        model.DoRegressionLevenbergMarquardt(new double[] {0,1, 90, 100 },useYErrors:false);
        
        //model.GetGoodnessOfFitLog().LogCommands("Godnees");
        //model.DoRegressionLevenbergMarquardtWithXErrors(initialGuess, 10);
        
        //model.AddParametersToPreambleAndLog("FocalLengthGaussFit");

        var imageDistance = model.ErParameters[2];
        imageDistance.AddCommandAndLog("ImageDistance","cm");
        var objectDistance = lensPos - effectiveEmitterPos;
        objectDistance.AddCommandAndLog("ObjectDistance","cm");
        // 1/f = 1/b + 1/g
        var focalLength = 1 / (1 / imageDistance + 1 / objectDistance);
        focalLength.AddCommandAndLog("FocalLength","cm");

        var plt = new DynPlot("Bildweite b in cm","Spannung in V");//"Image distance b in cm", "voltage U in V");

        var (dynErrorBar,funcPlot) = plt.AddRegModel(model, "Empfänger Signal", "Gauss-Anpassung");
        dynErrorBar.PointConnectedLineStyle.IsVisible = true;
        dynErrorBar.PointConnectedLineStyle.Color = plt.Add.GetNextColor();


        plt.AddVerticalLine(imageDistance.Value);

        plt.Legend.Location = Alignment.UpperLeft;
        
        plt.SaveAndAddCommand("fig:imageDistance");

    }

    private static void CalculateErrorAndImageDistance(ref FocalLengthWaxLensData element, ErDouble lensPos,
        double errorReceiverPos,ErDouble distanceHornEndEffectiveReceiverPos,ErDouble distanceReceiverPosHornEnd, double voltageRange,double voltageOffset)
    {
        // element.Voltage = DeviceErrorsUtil.CalculateDeviceError(Devices.Aglient34405, DataTypes.VoltageDC,
        //     element.Voltage.Value, voltageRange);

        element.Voltage += voltageOffset;
        
        element.ReceiverPos.Error = errorReceiverPos;

        element.EffectiveReceiverPos =
            element.ReceiverPos - distanceReceiverPosHornEnd + distanceHornEndEffectiveReceiverPos;

        element.ImageDistance = element.EffectiveReceiverPos - lensPos;


    }
}