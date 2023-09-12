using Mantis.Core.TexIntegration;

namespace Mantis.Workspace.C1_Trials.V42_Microwaves_Measurement;

public static class V42_MicrowaveMeasurement_Main
{
    public static void Process()
    {
        Part1_AngleDispersion.Process();
        Part2_FocalLengthWaxLensMain.Process();
        
        TexPreamble.GeneratePreamble();
    }
}