// using MathNet.Numerics.LinearAlgebra;
// using MathNet.Numerics.Optimization;
//
// namespace Mantis.Core.Calculator.Regression;
//
// public class RegressionExecutor<T> : IObjectiveModel where T : ParaFunc
// {
//     public readonly RegModel<T> RegModel;
//
//     public RegressionExecutor(RegModel<T> regModel)
//     {
//         RegModel = regModel;
//         
//         Matrix<double> yErrorMatrix = Matrix<double>.Build.DiagonalOfDiagonalVector(RegModel.Data.YErrors).Power(2);
//         Weights = yErrorMatrix.Determinant() != 0
//             ? yErrorMatrix.Inverse()
//             : Matrix<double>.Build.DiagonalIdentity(RegModel.Data.Count);
//     }
//
//     public IObjectiveModel CreateNew()
//     {
//         throw new NotImplementedException();
//     }
//
//
//     private ParaFunc ParaFunction => RegModel.ParaFunction;
//     public Vector<double> ObservedY => RegModel.Data.YValues;
//     private Vector<double> ObservedX => RegModel.Data.XValues;
//     private int DataCount => RegModel.Data.Count;
//     public Matrix<double> Weights { get; set; }
//     public Vector<double> ModelValues { get; private set; }
//     public Vector<double> Point => RegModel.ParaFunction.ParaSet.Parameters;
//     public double Value { get; private set; }
//     public double RedResidual{ get; private set; }
//     public Vector<double> Gradient { get; private set; }
//     public Matrix<double> Hessian { get; private set; }
//     public int FunctionEvaluations { get; set; }
//     public int JacobianEvaluations { get; set; }
//     public int DegreeOfFreedom => RegModel.Data.Count - RegModel.ParaFunction.ParameterCount;
//     public bool IsGradientSupported => true;
//     public bool IsHessianSupported => true;
//
//     public Matrix<double>? Jacobean { get; private set; }
//
//
//     public void UpdateModelValues()
//     {
//         ModelValues = RegModel.ParaFunction.EvaluatePointWise(ObservedX);
//     }
//
//     public void UpdateResidual()
//     {
//         UpdateModelValues();
//         Vector<double> distance = ModelValues - ObservedY;
//         Value = distance.DotProduct(Weights * distance);
//         RedResidual = Value / DegreeOfFreedom;
//     }
//
//     public void UpdateJacobean()
//     {
//         Jacobean ??= Matrix<double>.Build.Dense(DataCount, ParaFunction.ParameterCount);
//
//         for (int i = 0; i < DataCount; i++)
//         {
//             for (int j = 0; j < ParaFunction.ParameterCount; j++)
//             {
//                 Jacobean[i, j] = ParaFunction.CalculateGradient(ObservedX[i], ParaFunction.ParaSet.Parameters, j);
//             }
//         }
//     }
//
//     public void UpdateCovarianceAndStandardError()
//     {
//         UpdateGradientHessian();
//         ParaFunction.ParaSet.SetCovarianceAndErrorsWithApprox(Hessian.Inverse());
//     }
//     
//     public void SetParameters(Vector<double> initialGuess, List<bool> isFixed = null)
//     {
//         RegModel.ParaFunction.ParaSet.Parameters = initialGuess;
//     }
//
//     /// <summary>
//     /// Updates the parameters and residuals.
//     /// Does not update the covariance and errors!
//     /// </summary>
//     /// <param name="parameters"></param>
//     public void EvaluateAt(Vector<double> parameters)
//     {
//         RegModel.ParaFunction.ParaSet.Parameters = parameters;
//         UpdateGradientHessian();
//     }
//
//     public void UpdateGradientHessian()
//     {
//         UpdateResidual();
//         UpdateJacobean();
//
//         Matrix<double> JT = Jacobean.Transpose();
//
//         Gradient = -JT * Weights * (ObservedY - ModelValues);
//
//         Hessian = JT * Weights * Jacobean;
//
//         FunctionEvaluations++;
//         JacobianEvaluations++;
//     }
//
//     public IObjectiveModel Fork()
//     {
//         return RegModel.Fork().Executor;
//     }
//
//     public IObjectiveFunction ToObjectiveFunction()
//     {
//         throw new NotImplementedException();
//     }
// }