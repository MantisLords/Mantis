using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.TexIntegration;
using MathNet.Numerics.Statistics;

namespace Mantis.Workspace.C1_Trials.V40_Polarisation;

public static class SugarSolution
{
    public static void Process()
    {
        var reader = V40_PolarisationMain.Reader;

        double wavelength = reader.ExtractSingleValue<double>("val:hsWavelength");

        var angleFirstPos = ExtractMean("val:firstPosAngle");
        var angleSecondPos = ExtractMean("val:secondPosAngle");

        var halfShadow = (angleFirstPos + angleSecondPos) / 2;
        halfShadow.AddCommandAndLog("halfShadowAngle","^\\circ");

        var glasTubeLength = reader.ExtractSingleValue<ErDouble>("val:glasTubeLength");
        var referenceConcentration = reader.ExtractSingleValue<ErDouble>("val:referenceConcentration");
        var angleReference = reader.ExtractSingleValue<ErDouble>("val:angleReference");
        var angleTest = reader.ExtractSingleValue<ErDouble>("val:angleTest");

        var specificOpticalRotation = (halfShadow - angleReference) / referenceConcentration / glasTubeLength;
        specificOpticalRotation.AddCommandAndLog("sugarSpecificOpticalRotation","\\frac{^\\circ cm^2}{g}");

        var testConcentration = (halfShadow - angleTest) / (halfShadow - angleReference) * referenceConcentration;
        testConcentration.AddCommandAndLog("sugarTestConcentration","\\frac{mg}{ml}");
        
    }

    private static ErDouble ExtractMean(string name)
    {
        string[] args = V40_PolarisationMain.Reader.ExtractSingleValue(name);

        var values = args.Select(e => double.Parse(e)).ToArray();
        return values.Mean();
    }
}