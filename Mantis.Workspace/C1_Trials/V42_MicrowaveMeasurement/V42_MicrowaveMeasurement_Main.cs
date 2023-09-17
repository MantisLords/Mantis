using Mantis.Core.TexIntegration;
using Mantis.Workspace.C1_Trials.V42_MicrowaveMeasurement;
using Mantis.Workspace.C1_Trials.V42_MicrowaveMeasurement;
using Mantis.Workspace.C1_Trials.V42_MicrowaveMeasurement;
using Mantis.Workspace.C1_Trials.V42_MicrowaveMeasurement;

namespace Mantis.Workspace.C1_Trials.V42_MicrowaveMeasurement;

public static class V42_MicrowaveMeasurement_Main
{
    public static void Process()
    {
        Part1_AngleDispersion.Process();
        Part2_FocalLengthWaxLensMain.Process();
        
        TexPreamble.GeneratePreamble();
    }
}