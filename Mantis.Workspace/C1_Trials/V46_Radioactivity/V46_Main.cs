using Mantis.Core.TexIntegration;
using Mantis.Workspace.C1_Trials.V46_Radioactivity.Data_Smailagic_Karb;

namespace Mantis.Workspace.C1_Trials.V46_Radioactivity;

public class V46_Main
{
    public static void Process()
    {
        V46_LocalDosis.Process();
        V46_GeigerMuellerCounter.Process();
        V46_NullEffectAndActivity.Process();
        V46_BariumHalfLife.Process();
        TexPreamble.GeneratePreamble();
    }
}