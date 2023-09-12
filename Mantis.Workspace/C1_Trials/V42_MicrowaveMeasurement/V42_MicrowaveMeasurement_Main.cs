using Mantis.Core.TexIntegration;

namespace Mantis.Workspace.C1_Trials.V42_Microwaves_Measurement;

public static class V42_MicrowaveMeasurement_Main
{
    public static void Process()
    {
        //Part1_AngleDispersion.Process();
        //Part2_FocalLengthWaxLensMain.Process();
        //Part3_WaveLengths.Process();
        //Part4_TotalReflection.Process();
        //Part5_Polarisation.Process();
        Part6_BraggReflection.Process();
        
        TexPreamble.GeneratePreamble();
    }
}