using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.TexIntegration;
using MathNet.Numerics.Statistics;
using Microsoft.VisualBasic;

namespace Mantis.Workspace.C1_Trials.V40_Polarisation;

public static class SugarSolution
{
    public static void Process()
    {
        var reader = V40_PolarisationMain.Reader;

        double wavelength = reader.ExtractSingleValue<double>("val:hsWavelength");

        var angleFirstPos = ExtractMean("val:firstPosAngle");
        var angleSecondPos = ExtractMean("val:secondPosAngle");

        var halfShadow = (angleSecondPos - angleFirstPos) ;
        halfShadow.AddCommandAndLog("halfShadowAngle","\\degree");

        var glasTubeLength = reader.ExtractSingleValue<ErDouble>("val:glasTubeLength");
        glasTubeLength.AddCommandAndLog("sugarGlasTubeLength","mm");
        //var defaultAngle = reader.ExtractSingleValue<ErDouble>("val:defaultAngle");
        var defaultAngle = (angleFirstPos + angleSecondPos) / 2;
        defaultAngle.AddCommandAndLog("defaultAngle","\\degree");
        var referenceConcentration = reader.ExtractSingleValue<ErDouble>("val:referenceConcentration");
        referenceConcentration.AddCommandAndLog("referenceConcentration","mg/ml");
        var angleReference = reader.ExtractSingleValue<ErDouble>("val:angleReference");
        angleReference.AddCommandAndLog("angleReference","\\degree");
        var angleTest = reader.ExtractSingleValue<ErDouble>("val:angleTest");
        angleTest.AddCommandAndLog("angleTest","\\degree");

        var refRotationAngle = angleReference - defaultAngle;
        refRotationAngle.AddCommandAndLog("referenceRotationAngle","\\degree");
        var specificOpticalRotation = refRotationAngle / referenceConcentration / glasTubeLength;
        specificOpticalRotation *= 10000;
        specificOpticalRotation.AddCommandAndLog("sugarSpecificOpticalRotation","\\frac{\\degree \\, cm^2}{g}");

        var testRotationAngle = angleTest - defaultAngle;
        testRotationAngle.AddCommandAndLog("testRotationAngle","\\degree");
        var testConcentration = testRotationAngle / refRotationAngle * referenceConcentration;
        testConcentration.AddCommandAndLog("sugarTestConcentration","\\frac{mg}{ml}");
        
    }

    private static ErDouble ExtractMean(string name)
    {
        string[] args = V40_PolarisationMain.Reader.ExtractSingleValue(name);

        var values = args.Where(s => !string.IsNullOrWhiteSpace(s)).Select(e => double.Parse(e)).ToArray();
        return values.WeightedMean();
    }
}