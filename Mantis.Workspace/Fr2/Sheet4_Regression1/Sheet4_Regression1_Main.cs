using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.QuickTable;
using Mantis.Core.TexIntegration;
using Mantis.Core.Utility;
using MathNet.Numerics;

namespace Mantis.Workspace.Fr2.Sheet4_Regression1;

[QuickTable("Decay Data of Ba-137","tab:BaDecayData")]
public record struct BaDecayData
{
    [QuickTableField("Time", "s", "t",true)] public double Time;

    [QuickTableField("Decay Count", "","\\symN", true)]
    public ErDouble DecayCount;

    [QuickTableField("", "", "ln(\\symN)", false)]
    public ErDouble LogDecayCount;
    
    public BaDecayData(){}
}

public static class Sheet4_Regression1_Main
{
    public static void Process()
    {
        // Import data
        List<BaDecayData> data = new SimpleTableProtocolReader("BaDecayData.csv").ExtractTable<BaDecayData>();

        // Linearize and add errors
        data.ForEachRef((ref BaDecayData e) => e.DecayCount.Error = Math.Sqrt(e.DecayCount.Value));
        data.ForEachRef(CalculateLog);
        
        data.CreateTexTable().SaveLabeled();
        
        // Gaussian Linear regression while ignoring the errors
        (ErDouble alphaNoError,ErDouble betaNoError) = data.LinearRegressionNoErrors(e => (e.Time, e.LogDecayCount.Value));
        CalcHalfTimeAndAddCommands(alphaNoError,betaNoError,"NoError");
        
        // Gaussian linear regression with y errors
        (ErDouble alphaGauss, ErDouble betaGauss) = data.LinearRegressionLine(e => (e.Time, e.LogDecayCount));
        CalcHalfTimeAndAddCommands(alphaGauss,betaGauss,"Gauss");
        
        // Poisson linear regression with y errors
        (ErDouble alphaPoisson, ErDouble betaPoisson) = data.LinearRegressionPoissonDistributed(e => (e.Time, e.LogDecayCount),
            initialGuessGauss:(alphaGauss,betaGauss));
        CalcHalfTimeAndAddCommands(alphaPoisson,betaPoisson,"Poisson");
        
        // Plot

        Sketchbook sketchbook = new Sketchbook(
            axis: new AxisLayout("Time in s", "log( \\symN )"),
            label: "fig:BaDecay",
            caption: "Decay of Ba-137");
        
        sketchbook.Add(new DataMarkSketch()
        {
            Data = data.Select(e => ((ErDouble)e.Time,(ErDouble)e.LogDecayCount)),
            Legend = "Measured decay"
        });
        
        sketchbook.Add(new StraightPlot()
        {
            Slope = betaNoError.Value,
            YZero = alphaNoError.Value,
            Legend = "Regression with no errors"
        });

        sketchbook.SaveLabeled();
        
        TexPreamble.AddCommand("\\delta N","symN");
        
        TexPreamble.GeneratePreamble();

    }

    private static void CalcHalfTimeAndAddCommands(ErDouble alpha, ErDouble beta, string postfix)
    {
        ErDouble halfTime = - Constants.Ln2 / beta;
        
        alpha.AddCommand("alpha"+postfix);
        beta.AddCommand("beta"+postfix,"s^{-1}");
        halfTime.AddCommand("halfTime"+postfix,"s");
        Console.WriteLine($"Regression with {postfix}: a = {alpha.ToString()} b = {beta.ToString()} Ts = {halfTime.ToString()}");
    }

    public static void CalculateLog(ref BaDecayData e)
    {
        e.LogDecayCount = ErDouble.Log(e.DecayCount) ;
    }
}