
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Optimization;
using MathNet.Numerics.Optimization.ObjectiveFunctions;

namespace Mantis.Core.Calculator;

public static class NonLinearRegression
{

    public static NonlinearMinimizationResult DoRegressionLevenbergMarquardt<T>(this RegModel<T> model,
        double[] initialGuess, bool useYErrors = true) where T : FuncCore, new()
    {
        Vector<double> initialGuessVector = Vector<double>.Build.DenseOfArray(initialGuess);
        return DoRegressionLevenbergMarquardt(model, initialGuessVector, useYErrors);
    }

    public static NonlinearMinimizationResult DoRegressionLevenbergMarquardt<T>(this RegModel<T> model,Vector<double> initialGuess,bool useYErrors = true) where T : FuncCore,new()
    {
        var objective = GetNonlinearObjectiveModel(model,useYErrors);
        var res = new LevenbergMarquardtMinimizer().FindMinimum(objective, initialGuess);

        if (res.ReasonForExit != ExitCondition.Converged )
            Console.WriteLine($"Finished LM! Reason for exit: {res.ReasonForExit} Iterations: {res.Iterations}");

        var covariance = objective.Hessian.Inverse();
        if (!useYErrors)
        {
            double sigmaSqrt = objective.Value / objective.DegreeOfFreedom;
            covariance *= sigmaSqrt;
        }
        
        model.ParaFunction.ParaSet.SetParametersAndErrorsWithApprox(objective.Point,covariance);
        
        // Console.WriteLine($"StandarErrors: MathNet: {res.StandardErrors} Me: {model.ParaFunction.ParaSet} \n R: {objective.Value}" +
        //                   $" RAlt: {model.CalculateReducedResidual()}");

        return res;
    }

    public static NonlinearMinimizationResult DoRegressionLevenbergMarquardtWithXErrors<T>(this RegModel<T> model,
        double[] initialGuess, int xIterations)
        where T : FuncCore, new()
    {
        Vector<double> initialGuessVector = Vector<double>.Build.DenseOfArray(initialGuess);
        return DoRegressionLevenbergMarquardtWithXErrors(model, initialGuessVector, xIterations);
    }

    public static NonlinearMinimizationResult DoRegressionLevenbergMarquardtWithXErrors<T>(this RegModel<T> model,
        Vector<double> initialGuess, int xIterations)
        where T : FuncCore, new()
    {
        Matrix<double> yErrSq = Matrix<double>.Build.DiagonalOfDiagonalVector(model.Data.YErrors).Power(2);
        Matrix<double> xErrSq = Matrix<double>.Build.DiagonalOfDiagonalVector(model.Data.XErrors).Power(2);
        
        var objective = GetNonlinearObjectiveModel(model,true);

        var minimizer = new LevenbergMarquardtMinimizer();

        var res = minimizer.FindMinimum(objective, initialGuess);
        if (res.ReasonForExit != ExitCondition.Converged )
            Console.WriteLine($"Finished LM! Reason for exit: {res.ReasonForExit} Iterations: {res.Iterations}");

        
        
        for (int i = 0; i < xIterations; i++)
        {
            Vector<double> xDerivative = model.ParaFunction.FuncCore.CalculateXDerivativePointWise(objective.Point,model.Data.XValues);
            Matrix<double> xDerivativeSq = Matrix<double>.Build.DiagonalOfDiagonalVector(xDerivative).Power(2);
            // s^2 = sy^2 + p^2 * sx^2
            Matrix<double> newErrorSq = (yErrSq + xDerivativeSq *   xErrSq);
            if(newErrorSq.Determinant() == 0)
                throw new ArgumentException("There is a data point with an error of zero. ");
            
            objective.SetObserved(model.Data.XValues, model.Data.YValues, newErrorSq.Inverse().Diagonal());
            
            res = minimizer.FindMinimum(objective, objective.Point);
            if (res.ReasonForExit != ExitCondition.Converged && res.ReasonForExit != ExitCondition.RelativePoints)
                Console.WriteLine($"Finished LM! Reason for exit: {res.ReasonForExit} Iterations: {res.Iterations}");
        }
        
        model.ParaFunction.ParaSet.SetParametersAndErrorsWithApprox(objective.Point,objective.Hessian.Inverse());

        return res;
    }

    internal static NonlinearObjectiveFunction GetNonlinearObjectiveModel<T>(RegModel<T> model,bool useYErrors) where T : FuncCore,new()
    {
        Func<Vector<double>,Vector<double>,Vector<double>> function = model.ParaFunction.FuncCore.CalculateResultPointWise;

        NonlinearObjectiveFunction objective;

        if (model.ParaFunction.FuncCore is AutoDerivativeFunc)
        {
            objective = new NonlinearObjectiveFunction(function);
        }
        else
        {
            var derivatives = model.ParaFunction.FuncCore.CalculateGradientPointWise;
            objective = new NonlinearObjectiveFunction(function, derivatives);
        }
        
        var weigths = useYErrors ? model.Weights.Diagonal() : Vector<double>.Build.Dense(model.Data.Count, 1);
        if (weigths.AbsoluteMinimum()  == 0)
        {
            throw new ArgumentException("There is a y value with an error of zero. Try giving it an error or " +
                                        "do the regression and ignoring the y errors");
        }
        
        objective.SetObserved(model.Data.XValues, model.Data.YValues, weigths);
        return objective;
    }
}