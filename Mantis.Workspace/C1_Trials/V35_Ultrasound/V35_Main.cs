using Mantis.Core.TexIntegration;

namespace Mantis.Workspace.C1_Trials.V35_Ultrasound;

public class V35_Main
{
    public static void Process()
    {
        V35_RuntimeMeasurement.Process();
        V35_SpeedInMetals.Process();
        V35_DepthMeasurement.Process();
        TexPreamble.GeneratePreamble();
    }
    
}