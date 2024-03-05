using Mantis.Core.Calculator;
using Mantis.Core.Calculator.Regression;
using Mantis.Core.FileImporting;
using Mantis.Core.QuickTable;
using Mantis.Core.ScottPlotUtility;
using Mantis.Core.TexIntegration;
using Mantis.Core.Utility;
using MathNet.Numerics;

namespace Mantis.Workspace.Fr2.Sheet4_Regression1;

[QuickTable("Decay Data of Ba-137","tab:BaDecayData")]
public record struct BaDecayData
{
    [QuickTableField("Time", "s", "t",true)] public double Time = 0;

    [QuickTableField("Decay Count", "","\\symN", true)]
    public ErDouble DecayCount = 0;

    [QuickTableField("", "", "ln(\\symN)", false)]
    public ErDouble LogDecayCount = 0;
    
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

        var modelNoErrors = data.CreateRegModel(e => (e.Time, e.LogDecayCount),
            new ParaFunc(2,new LineFunc())
            {
                Units = new string[] { "", "s^{-1}" }
            });
        
        modelNoErrors.DoLinearRegression(false);
        modelNoErrors.AddParametersToPreambleAndLog("NoErrors");
        CalcHalfTimeAndAddCommand(modelNoErrors,"NoErrors");

        var modelGauss = modelNoErrors.Fork();
        modelGauss.DoLinearRegression(true);
        modelGauss.AddParametersToPreambleAndLog("Gauss");
        CalcHalfTimeAndAddCommand(modelGauss,"Gauss");

        var modelPoisson = modelNoErrors.Fork();
        //modelPoisson.DoLinearRegressionPoissonDistributed();
        modelPoisson.AddParametersToPreambleAndLog("Poisson");
        CalcHalfTimeAndAddCommand(modelPoisson,"Poisson");

        var plt = new DynPlot("Time in s", "log( deltaN )");
        plt.AddDynErrorBar(modelNoErrors.Data, label: "Measured decay");

        plt.AddDynFunction(modelNoErrors.ParaFunction, label: "Regression while ignoring errors");
        plt.AddDynFunction(modelGauss.ParaFunction, label: "Regression with gaussian distribution");
        plt.AddDynFunction(modelPoisson.ParaFunction, label: "Regression with poissonian distribution");
        
        plt.SaveAndAddCommand("fig:BaDecay","Decay of Ba-137");
        
        
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

    private static void CalcHalfTimeAndAddCommand(RegModel logModel,string preFix)
    {
        ErDouble halfTime = -Constants.Ln2 / logModel.ErParameters[1];
        halfTime.AddCommandAndLog(preFix + "HalfTime","s");
    }

    public static void CalculateLog(ref BaDecayData e)
    {
        e.LogDecayCount = ErDouble.Log(e.DecayCount,Math.E) ;
    }
}