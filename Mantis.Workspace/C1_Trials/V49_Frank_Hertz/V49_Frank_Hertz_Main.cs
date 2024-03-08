using Mantis.Core.FileImporting;
using Mantis.Core.TexIntegration;
using Mantis.Workspace.C1_Trials.V49_Frank_Hertz.Data_Smailagic_Karb;

namespace Mantis.Workspace.C1_Trials.V49_Frank_Hertz;

public static class V49_Frank_Hertz_Main
{
    public readonly static SimpleTableProtocolReader Reader = new SimpleTableProtocolReader("VacuumDiodeData.csv");
    
    public static void Process()
    {
        CharacteristicCurves.Process();
        SaturationCurve.Process();
        FrankHertzCurve.Process();
        
        TexPreamble.GeneratePreamble();
    }
}