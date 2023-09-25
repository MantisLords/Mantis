using Mantis.Core.TexIntegration;
using Mantis.Workspace.C1_Trials.V31_RealGasStateVariables;

namespace Mantis.Workspace.C1_Trials.V31_RealGasStateVariables;

public class V31_Main
{
    public static void Process()
    {
        Part1_IsothermsAndCriticalPoints.Process();
        Homework.Process();
        TexPreamble.GeneratePreamble();
    }
}