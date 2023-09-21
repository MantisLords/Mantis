using Mantis.Core.TexIntegration;
using Mantis.Workspace.C1_Trials.V41_EMWaveSpeed;

namespace Mantis.Workspace.C1_Trials.V41_EMWaweSpeed;

public static class V41_Main
{
    public static void Process()
    {
        V41_WaveSpeed_Main.Process();
        PartC_SpeedOfLight.Process();
        TexPreamble.GeneratePreamble();
    }
}