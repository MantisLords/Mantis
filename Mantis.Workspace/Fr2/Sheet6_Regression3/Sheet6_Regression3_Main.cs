using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.QuickTable;
using Mantis.Core.ScottPlotUtility;
using Mantis.Core.TexIntegration;
using Mantis.Core.Utility;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;
using ScottPlot;

namespace Mantis.Workspace.Fr2.Sheet6_Regression3;

[QuickTable("Angle dependence of scattering antiprotons at hydrogen","tab:HScatteringData")]
public record struct HScatteringData
{
    [QuickTableField("detector angle", "\\degree","\\phi")]
    public double Angle;

    [QuickTableField("scattering count at full target", "", "N_{full}")]
    public ErDouble ScatteringCount;

    [QuickTableField("scattering count at empty target", "", "N_{empty}")]
    public ErDouble BackgroundScatteringCount;

    [QuickTableField("cleansed scattering count", "", "N", false)]
    public ErDouble CleansedScatteringCount;

    public double CosOfAngle;
    
    public HScatteringData(){}

}

public static class Sheet6_Regression3_Main
{
    public static void Process()
    {
        List<HScatteringData> data =
            new SimpleTableProtocolReader("HScatteringData.csv").ExtractTable<HScatteringData>();
        
        data.ForEachRef(AddErrorAndCleanseData);
        
        data.CreateTexTable().SaveLabeled();
        

        RegModel<LegendrePolynomialFunc> model = data.CreateRegModel(e => (e.CosOfAngle, e.CleansedScatteringCount),
            new ParaFunc<LegendrePolynomialFunc>(3));
        
        model.DoLinearRegression();
        
        model.AddParametersToPreambleAndLog("LegendreFit");
        model.AddGoodnessOfFitToPreambleAndLog("LegendreFit");
        
        
        var plt = ScottPlotExtensions.CreateSciPlot("Cos of detector angle",
            "Scattering Count",
            "Scattering graph with legendre polynomials");

        plt.AddRegModel(model, "Cleansed scattering count", "Best fit legendre polynomial");

        plt.SaveAndAddCommand("fig:LegendreFit","Scattering of antiprotons at hydrogen atoms");
        

        TexPreamble.AddCommand("^{\\circ}","degree");
            
        TexPreamble.GeneratePreamble();
    }

    private static void AddErrorAndCleanseData(ref HScatteringData e)
    {
        e.ScatteringCount.Error = Math.Sqrt(e.ScatteringCount.Value);
        e.BackgroundScatteringCount.Error = Math.Sqrt(e.BackgroundScatteringCount.Value);

        e.CleansedScatteringCount = e.ScatteringCount - 2 * e.BackgroundScatteringCount;

        e.CosOfAngle = Math.Cos(e.Angle * Math.PI / 180.0);
    }
}