using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Vector = MathNet.Numerics.LinearAlgebra.Single.Vector;

namespace Mantis.Core.Calculator;

public static class LinearRegression
{
    public static void DoLinearRegression<T>(this RegModel<T> model,bool useYErrors = true) where T : LinearFuncCore,new()
    {
        if (useYErrors)
        {
            if (model.Data.YErrors.AbsoluteMinimum()  == 0)
            {
                    throw new ArgumentException("There is a y value with an error of zero. Try giving it an error or " +
                                                "do the regression and ignoring the y errors");
            }
        }
        else
        {
            model.Weights = Matrix<double>.Build.DiagonalIdentity(model.Data.Count);
        }
        
        DoLinearRegressionWithoutCheck(model,useYErrors);

    }

    public static void DoLinearRegressionWithXErrors<T>(this RegModel<T> model, int iterations = 5)
        where T : LinearFuncCore,new()
    {
        Matrix<double> yErrSq = Matrix<double>.Build.DiagonalOfDiagonalVector(model.Data.YErrors).Power(2);
        Matrix<double> xErrSq = Matrix<double>.Build.DiagonalOfDiagonalVector(model.Data.XErrors).Power(2);
        
        // Find a starting guess for the correct parameters p by ignoring the x errors
        // If there is a zero y error use identity for weight matrix

        DoLinearRegressionWithoutCheck(model,true);

        for (int i = 0; i < iterations; i++)
        {

            Vector<double> xDerivative = model.ParaFunction.CalculateXDerivativePointWise(model.Data.XValues);
            Matrix<double> xDerivativeSq = Matrix<double>.Build.DiagonalOfDiagonalVector(xDerivative).Power(2);
            // s^2 = sy^2 + p^2 * sx^2
            Matrix<double> newErrorSq = (yErrSq + xDerivativeSq *   xErrSq);
            if(newErrorSq.Determinant() == 0)
                throw new ArgumentException("There is a data point with an error of zero. ");

            model.Weights = newErrorSq.Inverse();
            
            DoLinearRegressionWithoutCheck(model,true); // Find the correct result iteratively
        }
    }
    
    private static void DoLinearRegressionWithoutCheck<T>(RegModel<T> model,bool useYErrors) where T : LinearFuncCore,new()
    {
        Matrix<double> x = model.ParaFunction.FuncCore.CalculateGradientPointWise(model.Data.XValues,model.ParaFunction.ParameterCount);
        Vector<double> y = model.Data.YValues;

        Matrix<double> covariance = (x.Transpose() * model.Weights * x).Inverse();
        Vector<double> parameters = covariance * x.Transpose() * model.Weights * y;
        
        if (!useYErrors)
        {
            var modelValues = x * parameters;
            double sigmaSqrt = (y - modelValues).DotProduct(y - modelValues) / (y.Count - parameters.Count);
            covariance *= sigmaSqrt;
        }
        
        model.ParaFunction.ParaSet.SetParametersAndErrorsWithApprox(parameters, covariance);

    }
}