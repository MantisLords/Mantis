using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.QuickTable;
using Mantis.Core.ScottPlotUtility;
using Mantis.Core.TexIntegration;
using Mantis.Core.Utility;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;

namespace Mantis.Workspace.C1_Trials.V40_Polarisation;



public static class V40_PolarisationMain
{
    public static SimpleTableProtocolReader Reader = new SimpleTableProtocolReader("PolarisationData.csv");
    
    public static void Process()
    {
        // MalusLaw.Process();
        // SugarSolution.Process();
        FaradayEffect.Process();
    }
}