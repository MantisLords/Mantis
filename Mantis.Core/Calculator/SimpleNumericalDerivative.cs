using MathNet.Numerics.LinearAlgebra;

namespace Mantis.Core.Calculator;

public static class SimpleNumericalDerivative
{
    public static double NumericalDerivative(Func<double,double> userFunction,double x, int accuracyOrder = 2)
        {
            const double sqrtEpsilon = 1.4901161193847656250E-8; // sqrt(machineEpsilon)

            var currentValues = userFunction(x);
            
            double derivative = 0;

            var d = 0.000003 * Math.Max(Math.Abs(x), sqrtEpsilon);

            var h = d;
            

                if (accuracyOrder >= 6)
                {
                    // f'(x) = {- f(x - 3h) + 9f(x - 2h) - 45f(x - h) + 45f(x + h) - 9f(x + 2h) + f(x + 3h)} / 60h + O(h^6)
                    var f1 = userFunction(x - 3 * h);
                    var f2 = userFunction(x - 2 * h);
                    var f3 = userFunction(x - h);
                    var f4 = userFunction(x + h);
                    var f5 = userFunction(x + 2 * h );
                    var f6 = userFunction(x + 3 * h );

                    derivative = (-f1 + 9 * f2 - 45 * f3 + 45 * f4 - 9 * f5 + f6) / (60 * d);
                    
                }
                else if (accuracyOrder == 5)
                {
                    // f'(x) = {-137f(x) + 300f(x + h) - 300f(x + 2h) + 200f(x + 3h) - 75f(x + 4h) + 12f(x + 5h)} / 60h + O(h^5)
                    var f1 = currentValues;
                    var f2 = userFunction(x + h );
                    var f3 = userFunction(x + 2 * h );
                    var f4 = userFunction(x + 3 * h );
                    var f5 = userFunction(x + 4 * h );
                    var f6 = userFunction(x + 5 * h );

                    derivative = (-137 * f1 + 300 * f2 - 300 * f3 + 200 * f4 - 75 * f5 + 12 * f6) / (60 * d);
                     
                }
                else if (accuracyOrder == 4)
                {
                    // f'(x) = {f(x - 2h) - 8f(x - h) + 8f(x + h) - f(x + 2h)} / 12h + O(h^4)
                    var f1 = userFunction(x - 2 * h );
                    var f2 = userFunction(x - h );
                    var f3 = userFunction(x + h );
                    var f4 = userFunction(x + 2 * h );

                    derivative = (f1 - 8 * f2 + 8 * f3 - f4) / (12 * d);
                     
                }
                else if (accuracyOrder == 3)
                {
                    // f'(x) = {-11f(x) + 18f(x + h) - 9f(x + 2h) + 2f(x + 3h)} / 6h + O(h^3)
                    var f1 = currentValues;
                    var f2 = userFunction(x + h );
                    var f3 = userFunction(x + 2 * h );
                    var f4 = userFunction(x + 3 * h );

                    derivative = (-11 * f1 + 18 * f2 - 9 * f3 + 2 * f4) / (6 * d);
                     
                }
                else if (accuracyOrder == 2)
                {
                    // f'(x) = {f(x + h) - f(x - h)} / 2h + O(h^2)
                    var f1 = userFunction(x + h );
                    var f2 = userFunction(x - h );

                    derivative = (f1 - f2) / (2 * d);
                     
                }
                else
                {
                    // f'(x) = {- f(x) + f(x + h)} / h + O(h)
                    var f1 = currentValues;
                    var f2 = userFunction(x + h );

                    derivative = (-f1 + f2) / d;
                     
                }
            

            return derivative;
        }
    
    public static Vector<double> NumericalGradient(Func<Vector<double>,double> userFunction,Vector<double> parameters, int accuracyOrder = 2)
        {
            const double sqrtEpsilon = 1.4901161193847656250E-8; // sqrt(machineEpsilon)

            var numberOfParameters = parameters.Count;
            var currentValues = userFunction(parameters);
            
            Vector<double> derivertives = Vector<double>.Build.Dense( numberOfParameters);

            var d = 0.000003 * parameters.PointwiseAbs().PointwiseMaximum(sqrtEpsilon);

            var h = Vector<double>.Build.Dense(numberOfParameters);
            
            for (int j = 0; j < numberOfParameters; j++)
            {
                h[j] = d[j];

                if (accuracyOrder >= 6)
                {
                    // f'(x) = {- f(x - 3h) + 9f(x - 2h) - 45f(x - h) + 45f(x + h) - 9f(x + 2h) + f(x + 3h)} / 60h + O(h^6)
                    var f1 = userFunction(parameters - 3 * h);
                    var f2 = userFunction(parameters - 2 * h);
                    var f3 = userFunction(parameters - h);
                    var f4 = userFunction(parameters + h);
                    var f5 = userFunction(parameters + 2 * h );
                    var f6 = userFunction(parameters + 3 * h );

                    var prime = (-f1 + 9 * f2 - 45 * f3 + 45 * f4 - 9 * f5 + f6) / (60 * h[j]);
                    derivertives[j] = prime;
                }
                else if (accuracyOrder == 5)
                {
                    // f'(x) = {-137f(x) + 300f(x + h) - 300f(x + 2h) + 200f(x + 3h) - 75f(x + 4h) + 12f(x + 5h)} / 60h + O(h^5)
                    var f1 = currentValues;
                    var f2 = userFunction(parameters + h );
                    var f3 = userFunction(parameters + 2 * h );
                    var f4 = userFunction(parameters + 3 * h );
                    var f5 = userFunction(parameters + 4 * h );
                    var f6 = userFunction(parameters + 5 * h );

                    var prime = (-137 * f1 + 300 * f2 - 300 * f3 + 200 * f4 - 75 * f5 + 12 * f6) / (60 * h[j]);
                     derivertives[j] = prime;
                }
                else if (accuracyOrder == 4)
                {
                    // f'(x) = {f(x - 2h) - 8f(x - h) + 8f(x + h) - f(x + 2h)} / 12h + O(h^4)
                    var f1 = userFunction(parameters - 2 * h );
                    var f2 = userFunction(parameters - h );
                    var f3 = userFunction(parameters + h );
                    var f4 = userFunction(parameters + 2 * h );

                    var prime = (f1 - 8 * f2 + 8 * f3 - f4) / (12 * h[j]);
                     derivertives[j] = prime;
                }
                else if (accuracyOrder == 3)
                {
                    // f'(x) = {-11f(x) + 18f(x + h) - 9f(x + 2h) + 2f(x + 3h)} / 6h + O(h^3)
                    var f1 = currentValues;
                    var f2 = userFunction(parameters + h );
                    var f3 = userFunction(parameters + 2 * h );
                    var f4 = userFunction(parameters + 3 * h );

                    var prime = (-11 * f1 + 18 * f2 - 9 * f3 + 2 * f4) / (6 * h[j]);
                     derivertives[j] = prime;
                }
                else if (accuracyOrder == 2)
                {
                    // f'(x) = {f(x + h) - f(x - h)} / 2h + O(h^2)
                    var f1 = userFunction(parameters + h );
                    var f2 = userFunction(parameters - h );

                    var prime = (f1 - f2) / (2 * h[j]);
                     derivertives[j] = prime;
                }
                else
                {
                    // f'(x) = {- f(x) + f(x + h)} / h + O(h)
                    var f1 = currentValues;
                    var f2 = userFunction(parameters + h );

                    var prime = (-f1 + f2) / h[j];
                     derivertives[j] = prime;
                }

                h[j] = 0;
            }

            return derivertives;
        }
    
    public static double NumericalGradientIndex(Func<Vector<double>,double> userFunction,Vector<double> parameters,int index, int accuracyOrder = 2)
        {
            const double sqrtEpsilon = 1.4901161193847656250E-8; // sqrt(machineEpsilon)

            var numberOfParameters = parameters.Count;
            var currentValues = userFunction(parameters);
            
            Vector<double> derivertives = Vector<double>.Build.Dense( numberOfParameters);

            var d = 0.000003 * parameters.PointwiseAbs().PointwiseMaximum(sqrtEpsilon);

            var h = Vector<double>.Build.Dense(numberOfParameters);
            h[index] = d[index]; 
            

                if (accuracyOrder >= 6)
                {
                    // f'(x) = {- f(x - 3h) + 9f(x - 2h) - 45f(x - h) + 45f(x + h) - 9f(x + 2h) + f(x + 3h)} / 60h + O(h^6)
                    var f1 = userFunction(parameters - 3 * h);
                    var f2 = userFunction(parameters - 2 * h);
                    var f3 = userFunction(parameters - h);
                    var f4 = userFunction(parameters + h);
                    var f5 = userFunction(parameters + 2 * h );
                    var f6 = userFunction(parameters + 3 * h );

                    return (-f1 + 9 * f2 - 45 * f3 + 45 * f4 - 9 * f5 + f6) / (60 * h[index]);
                    
                }
                else if (accuracyOrder == 5)
                {
                    // f'(x) = {-137f(x) + 300f(x + h) - 300f(x + 2h) + 200f(x + 3h) - 75f(x + 4h) + 12f(x + 5h)} / 60h + O(h^5)
                    var f1 = currentValues;
                    var f2 = userFunction(parameters + h );
                    var f3 = userFunction(parameters + 2 * h );
                    var f4 = userFunction(parameters + 3 * h );
                    var f5 = userFunction(parameters + 4 * h );
                    var f6 = userFunction(parameters + 5 * h );

                    return (-137 * f1 + 300 * f2 - 300 * f3 + 200 * f4 - 75 * f5 + 12 * f6) / (60 * h[index]);
                     
                }
                else if (accuracyOrder == 4)
                {
                    // f'(x) = {f(x - 2h) - 8f(x - h) + 8f(x + h) - f(x + 2h)} / 12h + O(h^4)
                    var f1 = userFunction(parameters - 2 * h );
                    var f2 = userFunction(parameters - h );
                    var f3 = userFunction(parameters + h );
                    var f4 = userFunction(parameters + 2 * h );

                    return (f1 - 8 * f2 + 8 * f3 - f4) / (12 * h[index]);
                     
                }
                else if (accuracyOrder == 3)
                {
                    // f'(x) = {-11f(x) + 18f(x + h) - 9f(x + 2h) + 2f(x + 3h)} / 6h + O(h^3)
                    var f1 = currentValues;
                    var f2 = userFunction(parameters + h );
                    var f3 = userFunction(parameters + 2 * h );
                    var f4 = userFunction(parameters + 3 * h );

                    return (-11 * f1 + 18 * f2 - 9 * f3 + 2 * f4) / (6 * h[index]);
                     
                }
                else if (accuracyOrder == 2)
                {
                    // f'(x) = {f(x + h) - f(x - h)} / 2h + O(h^2)
                    var f1 = userFunction(parameters + h );
                    var f2 = userFunction(parameters - h );

                    return (f1 - f2) / (2 * h[index]);
                     
                }
                else
                {
                    // f'(x) = {- f(x) + f(x + h)} / h + O(h)
                    var f1 = currentValues;
                    var f2 = userFunction(parameters + h );

                    return (-f1 + f2) / h[index];
                     
                }
        }
    
    public static Matrix<double> NumericalJacobian(Func<Vector<double>,Vector<double>> userFunction,Vector<double> parameters, int accuracyOrder = 2)
        {
            const double sqrtEpsilon = 1.4901161193847656250E-8; // sqrt(machineEpsilon)

            var numberOfParameters = parameters.Count;
            var currentValues = userFunction(parameters);
            var numberOfObservations = currentValues.Count;
            
            Matrix<double> derivertives = Matrix<double>.Build.Dense(numberOfObservations, numberOfParameters);

            var d = 0.000003 * parameters.PointwiseAbs().PointwiseMaximum(sqrtEpsilon);

            var h = Vector<double>.Build.Dense(numberOfParameters);
            
            for (int j = 0; j < numberOfParameters; j++)
            {
                h[j] = d[j];

                if (accuracyOrder >= 6)
                {
                    // f'(x) = {- f(x - 3h) + 9f(x - 2h) - 45f(x - h) + 45f(x + h) - 9f(x + 2h) + f(x + 3h)} / 60h + O(h^6)
                    var f1 = userFunction(parameters - 3 * h);
                    var f2 = userFunction(parameters - 2 * h);
                    var f3 = userFunction(parameters - h);
                    var f4 = userFunction(parameters + h);
                    var f5 = userFunction(parameters + 2 * h );
                    var f6 = userFunction(parameters + 3 * h );

                    var prime = (-f1 + 9 * f2 - 45 * f3 + 45 * f4 - 9 * f5 + f6) / (60 * h[j]);
                    derivertives.SetColumn(j, prime);
                }
                else if (accuracyOrder == 5)
                {
                    // f'(x) = {-137f(x) + 300f(x + h) - 300f(x + 2h) + 200f(x + 3h) - 75f(x + 4h) + 12f(x + 5h)} / 60h + O(h^5)
                    var f1 = currentValues;
                    var f2 = userFunction(parameters + h );
                    var f3 = userFunction(parameters + 2 * h );
                    var f4 = userFunction(parameters + 3 * h );
                    var f5 = userFunction(parameters + 4 * h );
                    var f6 = userFunction(parameters + 5 * h );

                    var prime = (-137 * f1 + 300 * f2 - 300 * f3 + 200 * f4 - 75 * f5 + 12 * f6) / (60 * h[j]);
                    derivertives.SetColumn(j, prime);
                }
                else if (accuracyOrder == 4)
                {
                    // f'(x) = {f(x - 2h) - 8f(x - h) + 8f(x + h) - f(x + 2h)} / 12h + O(h^4)
                    var f1 = userFunction(parameters - 2 * h );
                    var f2 = userFunction(parameters - h );
                    var f3 = userFunction(parameters + h );
                    var f4 = userFunction(parameters + 2 * h );

                    var prime = (f1 - 8 * f2 + 8 * f3 - f4) / (12 * h[j]);
                    derivertives.SetColumn(j, prime);
                }
                else if (accuracyOrder == 3)
                {
                    // f'(x) = {-11f(x) + 18f(x + h) - 9f(x + 2h) + 2f(x + 3h)} / 6h + O(h^3)
                    var f1 = currentValues;
                    var f2 = userFunction(parameters + h );
                    var f3 = userFunction(parameters + 2 * h );
                    var f4 = userFunction(parameters + 3 * h );

                    var prime = (-11 * f1 + 18 * f2 - 9 * f3 + 2 * f4) / (6 * h[j]);
                    derivertives.SetColumn(j, prime);
                }
                else if (accuracyOrder == 2)
                {
                    // f'(x) = {f(x + h) - f(x - h)} / 2h + O(h^2)
                    var f1 = userFunction(parameters + h );
                    var f2 = userFunction(parameters - h );

                    var prime = (f1 - f2) / (2 * h[j]);
                    derivertives.SetColumn(j, prime);
                }
                else
                {
                    // f'(x) = {- f(x) + f(x + h)} / h + O(h)
                    var f1 = currentValues;
                    var f2 = userFunction(parameters + h );

                    var prime = (-f1 + f2) / h[j];
                    derivertives.SetColumn(j, prime);
                }

                h[j] = 0;
            }

            return derivertives;
        }
}