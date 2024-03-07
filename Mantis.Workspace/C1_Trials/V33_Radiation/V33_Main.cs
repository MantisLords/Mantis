using Mantis.Core.TexIntegration;

namespace Mantis.Workspace.C1_Trials.V33_Radiation;

public class V33_Main
{
    public static void Process()
    {
        V33_Leslie_Cube.Process();
        V33_Transistivity.Process();
        TexPreamble.GeneratePreamble();
        
    }
}