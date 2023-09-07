// using MathNet.Numerics.LinearAlgebra;
// using MathNet.Numerics.LinearAlgebra.Single;
// using MathNet.Numerics.Optimization;
//
// namespace Mantis.Core.Calculator;
//
// public static class MyLevenbergMarquardtMinimizer
// {
//     public static Vector<double> FindMinimum(IObjectiveModel model, Vector<double> initialGuess)
//     {
//         double tau = 1E-6;
//         double epsilon1 = 1E-15;
//         double epsilon2 = 1E-15;
//         int maxIterations = 200 * (initialGuess.Count() + 1);
//         double nu = 2;
//         Vector<double> p = initialGuess;
//         
//         model.SetParameters(p);
//
//         Matrix<double> hessian = model.Hessian;
//         Vector<double> gradient = model.Gradient;
//
//         double mu = tau * hessian.Diagonal().Maximum();
//
//         bool found = gradient.InfinityNorm() < epsilon1;
//
//         int iterations = 0;
//         for (; iterations < maxIterations && !found; iterations++)
//         {
//             hessian.SetDiagonal(hessian.Diagonal() + mu); // (J'WJ + mu diag(J'WJ))
//
//             Vector<double> hStep = hessian.Solve(-gradient);
//
//             if (hStep.L2Norm() <= epsilon2 * (p.L2Norm() + epsilon2))
//                 found = true;
//             else
//             {
//                 Vector<double> pNew = p + hStep;
//                 double chiSqu = model.Value;
//                 double predictedGain = 0.5 * hStep.DotProduct(mu * hStep - gradient);
//                 
//                 model.EvaluateAt(pNew);
//                 double chiSquNew = model.Value;
//                 
//                 double roh = predictedGain != 0? (chiSqu - chiSquNew) / predictedGain : 0;
//
//                 if (roh > 0)
//                 {
//                     p = pNew;
//                     hessian = model.Hessian;
//                     gradient = model.Gradient;
//                     
//                     found = gradient.InfinityNorm() < epsilon1;
//
//                     mu = mu * Math.Max(1.0 / 3.0, 1 - Math.Pow(2 * roh - 1, 3));
//                     nu = 2;
//                 }
//                 else
//                 {
//                     mu = mu * nu;
//                     nu = 2 * nu;
//                 }
//             }
//         }
//         
//         Console.WriteLine($"Found Minimum in: {iterations} iterations");
//         
//         return p;
//     }
// }