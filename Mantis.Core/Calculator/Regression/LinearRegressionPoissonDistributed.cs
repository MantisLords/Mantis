using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;

namespace Mantis.Core.Calculator.Regression;

public static class LinearRegressionPoissonDistributed
{
    public static void DoLinearRegressionPoissonDistributed<T>(this RegModel<T> model,bool useYErrors = true)
        where T : LinearFuncCore, new()
    {
        model.DoLinearRegression(useYErrors);
        
        var xs = model.Data.XValues;
        var ys = model.Data.YValues;

        Func<Vector<double>, double> poissonResidual =
            (parameters =>
            {
                var modelYs = model.ParaFunction.FuncCore.CalculateResultPointWise(parameters, xs);
                return -ys.DotProduct(modelYs.PointwiseLog()) + modelYs.Sum();
            });
        
        // Find poisson parameters with initial guess of the gaussian regression
        var poissonParameters = FindMinimum.OfFunction(poissonResidual, model.ParaFunction.ParaSet.Parameters);
        
        // Use the covariance of the gaussian regression as approximation for the poissonian
        // For better error calculation you require a Monte Carlo approach
        model.ParaFunction.ParaSet.SetParametersAndErrorsWithApprox(
            poissonParameters, model.ParaFunction.ParaSet.CovarianceMatrix);
    }
    
    
}