using Mantis.Core.TexIntegration;
using Mantis.Workspace.C1_Trials.V35_Ultrasound.Data_Smailagic_Karb;

namespace Mantis.Workspace.C1_Trials.V35_Ultrasound;

public class V35_Main
{
    public static void Process()
    {
        V35_RuntimeMeasurement.Process();
        V35_SpeedInMetals.Process();
        V35_DepthMeasurement.Process();
        V35_Absorbtion.Process();
        V35_DebyeSearsEffect.Process();
        V35_WaveModeMeasurement.Process();
        TexPreamble.GeneratePreamble();
    }
    
}