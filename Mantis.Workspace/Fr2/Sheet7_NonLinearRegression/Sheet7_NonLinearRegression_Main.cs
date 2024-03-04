using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.QuickTable;
using Mantis.Core.ScottPlotUtility;
using Mantis.Core.TexIntegration;
using Mantis.Core.Utility;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using ScottPlot;

namespace Mantis.Workspace.Fr2.Sheet7_NonLinearRegression;

[QuickTable("Decay data of unknown radioactive substance","tab:RadioDecayData")]
public record struct RadioDecayData
{
    [QuickTableField("time", "s", "t")] public double Time = 0;

    [QuickTableField("decay count", "", "N")]
    public ErDouble DecayCount = 0;

    [QuickTableField("", "", "ln(N)", false)]
    public ErDouble LogDecayCount = 0;
    
    public RadioDecayData(){}
}

public static class Sheet7_NonLinearRegression_Main
{
    public static void Process()
    {
        List<RadioDecayData> data = new SimpleTableProtocolReader("RadioDecayData.csv").ExtractTable<RadioDecayData>();
        
        data.ForEachRef(SetErrorAndCalcLog);
        
        data.CreateTexTable().SaveLabeled();
        

        RegModel logModel =
            data.CreateRegModel(e => (e.Time, e.LogDecayCount), new ParaFunc(2,new LineFunc()));
        
        logModel.DoLinearRegression();
        
        RegModel linearizedModel =
            data.CreateRegModel(e => (e.Time, e.DecayCount), new ParaFunc(
                    new []{ErDouble.Exp(logModel.ErParameters[0]),logModel.ErParameters[1]},new ExpFunc()));
        linearizedModel.AddParametersToPreambleAndLog("LinearizedModel");
        linearizedModel.GetGoodnessOfFitLog().AddCommandAndLog("LinearizedModel");

        RegModel nonLinearModel =
            data.CreateRegModel(e => (e.Time, e.DecayCount), new ParaFunc(2,new ExpFunc()));

        Vector<double> initialGuess = linearizedModel.ParaFunction.ParaSet.Parameters;
        nonLinearModel.DoRegressionLevenbergMarquardt(initialGuess);

        var halfTime = -Constants.Ln2 / nonLinearModel.ErParameters[1];
        halfTime.AddCommandAndLog("HalfTime","s");
        
        nonLinearModel.AddParametersToPreambleAndLog("NonlinearModel");
        nonLinearModel.GetGoodnessOfFitLog().AddCommandAndLog("NonlinearModel");

        var plt = new DynPlot(
            "Time in s",
            "Decay Count");
        plt.Legend.Location = Alignment.UpperRight;
        plt.AddRegModel(nonLinearModel,"Measured Decay with poisson uncertainty","Levenberg Marquardt");
        plt.AddDynFunction(linearizedModel.ParaFunction, label: "Linearized Model");
        plt.SaveAndAddCommand("fig:NonLinearRegression","Fit of exponential function via Levenberg Marquardt");

        TexPreamble.GeneratePreamble();


    }

    private static void SetErrorAndCalcLog(ref RadioDecayData e)
    {
        e.DecayCount.Error = Math.Sqrt(e.DecayCount.Value);
        e.LogDecayCount = ErDouble.Log(e.DecayCount);
    }


}