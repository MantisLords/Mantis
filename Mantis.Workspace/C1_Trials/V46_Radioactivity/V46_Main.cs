using Mantis.Core.TexIntegration;

namespace Mantis.Workspace.C1_Trials.V46_Radioactivity;

public class V46_Main
{
    public static void Process()
    {
        V46_LocalDosis.Process();
        V46_GeigerMuellerCounter.Process();
        TexPreamble.GeneratePreamble();
    }
}