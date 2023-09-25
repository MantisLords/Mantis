using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.QuickTable;
using Mantis.Core.ScottPlotUtility;
using Mantis.Core.TexIntegration;
using Mantis.Core.Utility;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;
using MathNet.Numerics.Optimization;
using Microsoft.VisualBasic;
using ScottPlot;
using Constants = MathNet.Numerics.Constants;
using Vector = MathNet.Numerics.LinearAlgebra.Double.Vector;

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
        

        RegModel<LineFunc> logModel =
            data.CreateRegModel(e => (e.Time, e.LogDecayCount), new ParaFunc<LineFunc>(2));
        
        logModel.DoLinearRegression();
        
        RegModel<ExpFunc> linearizedModel =
            data.CreateRegModel(e => (e.Time, e.DecayCount), new ParaFunc<ExpFunc>(
                    new []{ErDouble.Exp(logModel.ErParameters[0]),logModel.ErParameters[1]}));
        linearizedModel.AddParametersToPreambleAndLog("LinearizedModel");
        linearizedModel.AddGoodnessOfFitToPreambleAndLog("LinearizedModel");

        RegModel<ExpFunc> nonLinearModel =
            data.CreateRegModel(e => (e.Time, e.DecayCount), new ParaFunc<ExpFunc>(2));

        Vector<double> initialGuess = linearizedModel.ParaFunction.ParaSet.Parameters;
        nonLinearModel.DoRegressionLevenbergMarquardt(initialGuess);

        var halfTime = -Constants.Ln2 / nonLinearModel.ErParameters[1];
        halfTime.AddCommandAndLog("HalfTime","s");
        
        nonLinearModel.AddParametersToPreambleAndLog("NonlinearModel");
        nonLinearModel.AddGoodnessOfFitToPreambleAndLog("NonlinearModel");

        var plt = ScottPlotExtensions.CreateSciPlot(
            "Time in s",
            "Decay Count");
        plt.Legend(true, Alignment.UpperRight);
        plt.AddRegModel(nonLinearModel,"Measured Decay with poisson uncertainty","Levenberg Marquardt");
        plt.AddFunction(linearizedModel.ParaFunction, label: "Linearized Model");
        plt.SaveAndAddCommand("fig:NonLinearRegression","Fit of exponential function via Levenberg Marquardt");

        TexPreamble.GeneratePreamble();


    }

    private static void SetErrorAndCalcLog(ref RadioDecayData e)
    {
        e.DecayCount.Error = Math.Sqrt(e.DecayCount.Value);
        e.LogDecayCount = ErDouble.Log(e.DecayCount,Math.E);
    }


}