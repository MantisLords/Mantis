using Mantis.Core.TexIntegration;

namespace Mantis.Workspace.C1_Trials.V41_EMWaweSpeed;

public static class V41_Main
{
    public static void Process()
    {
        V41_WaveSpeed_Main.Process();
        TexPreamble.GeneratePreamble();
    }
}