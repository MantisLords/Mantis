using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Mantis.Core.Calculator;

public class ParaFunc<T> where T : FuncCore,new()
{

    public string[]? Labels
    {
        get => ParaSet.Labels;
        set => ParaSet.Labels = value;
    }

    public string[]? Units
    {
        get => ParaSet.Units;
        set => ParaSet.Units = value;
    }

    public readonly int ParameterCount;
    
    public readonly T FuncCore;

    public readonly ParaSet ParaSet;

    public ErDouble[] ErParameters
    {
        get
        {
            if (ParaSet.ErParameters != null)
                return ParaSet.ErParameters;
            throw new ArgumentException("There were no parameters initialized of the 'ParaFunc'");
        }
    }

    public ParaFunc(int parameterCount)
    {
        FuncCore = new T();

        if (FuncCore is IFixedParameterCount fixedParameterCount)
            ParameterCount = fixedParameterCount.ParameterCount;
        else
            ParameterCount = parameterCount;

        ParaSet = new ParaSet(ParameterCount);
    }
    
    public ParaFunc(ErDouble[] parameters)
    {
        FuncCore = new T();

        if (FuncCore is IFixedParameterCount fixedParameterCount)
            if (fixedParameterCount.ParameterCount != parameters.Length)
                throw new ArgumentException("Parameter Array has the wrong length");

        ParameterCount = parameters.Length;

        ParaSet = new ParaSet(parameters);
    }

    private ParaFunc(ParaSet paraSet)
    {
        FuncCore = new T();
        ParameterCount = paraSet.Count;
        ParaSet = paraSet;
    }


    #region Evaluate

    public double EvaluateAtDouble(double x)
    {
        return CalculateResult(x);
    }

    public ErDouble EvaluateAt(double x)
    {
        double yRes = CalculateResult(x);
        Vector<double> gradient = CalculateGradientVector(x);

        return new ErDouble(yRes, Math.Sqrt(gradient.DotProduct(ParaSet.CovarianceMatrix * gradient)));
    }

    public ErDouble EvaluateAt(ErDouble x)
    {
        ErDouble yWithoutX = EvaluateAt(x.Value);
        double xDerivative = FuncCore.CalculateXDerivative(ParaSet.Parameters, x.Value);
        double newError = Math.Sqrt(Math.Pow(yWithoutX.Error, 2) + Math.Pow(xDerivative * x.Error, 2));
        return new ErDouble(yWithoutX.Value, newError);
    }

    #endregion
    
    public double CalculateResult(double x)
    {
        if (ParaSet.Parameters != null)
            return FuncCore.CalculateResult(ParaSet.Parameters, x);
        throw new ArgumentException("There were no parameters initialized of the 'ParaFunc'");
    }
    
    public Vector<double> CalculateResultPointWise(Vector<double> x)
    {
        if (ParaSet.Parameters != null)
            return FuncCore.CalculateResultPointWise(ParaSet.Parameters, x);
        throw new ArgumentException("There were no parameters initialized of the 'ParaFunc'");
    }
    
    public Matrix<double> CalculateGradientPointWise(Vector<double> x)
    {
        if (ParaSet.Parameters != null)
            return FuncCore.CalculateGradientPointWise(ParaSet.Parameters, x);
        throw new ArgumentException("There were no parameters initialized of the 'ParaFunc'");
    }

    public Vector<double> CalculateGradientVector(double x)
    {
        if (ParaSet.Parameters != null)
            return FuncCore.CalculateGradientVector(ParaSet.Parameters, x);
        throw new ArgumentException("There were no parameters initialized of the 'ParaFunc'");
    }


    public Vector<double> CalculateXDerivativePointWise(Vector<double> x)
    {
        if (ParaSet.Parameters != null)
            return FuncCore.CalculateXDerivativePointWise(ParaSet.Parameters, x);
        throw new ArgumentException("There were no parameters initialized of the 'ParaFunc'");
    }
    
    public double CalculateXDerivative(double x)
    {
        if (ParaSet.Parameters != null)
            return FuncCore.CalculateXDerivative(ParaSet.Parameters, x);
        throw new ArgumentException("There were no parameters initialized of the 'ParaFunc'");
    }
    
    



    public ParaFunc<T> Fork()
    {
        return new ParaFunc<T>(ParaSet.Fork());
    }

    public override string ToString()
    {
        string res = $"---{typeof(T).Name}---\n";
        res += ParaSet.ToString();
        return res;
    }

    // Vector<double> NumericalGradient(double observedX,Vector<double> parameters, int accuracyOrder = 2)
    //     {
    //         const double sqrtEpsilon = 1.4901161193847656250E-8; // sqrt(machineEpsilon)
    //
    //         Vector<double> derivertives = Vector<double>.Build.Dense( ParameterCount);
    //
    //         var d = 0.000003 * parameters.PointwiseAbs().PointwiseMaximum(sqrtEpsilon);
    //
    //         var h = Vector<double>.Build.Dense(ParameterCount);
    //         for (int j = 0; j < ParameterCount; j++)
    //         {
    //             h[j] = d[j];
    //
    //             if (accuracyOrder >= 6)
    //             {
    //                 // f'(x) = {- f(x - 3h) + 9f(x - 2h) - 45f(x - h) + 45f(x + h) - 9f(x + 2h) + f(x + 3h)} / 60h + O(h^6)
    //                 var f1 = CalculateResult(observedX,parameters - 3 * h);
    //                 var f2 = CalculateResult(observedX,parameters - 2 * h);
    //                 var f3 = CalculateResult(observedX,parameters - h);
    //                 var f4 = CalculateResult(observedX,parameters + h);
    //                 var f5 = CalculateResult(observedX,parameters + 2 * h);
    //                 var f6 = CalculateResult(observedX,parameters + 3 * h);
    //
    //                 var prime = (-f1 + 9 * f2 - 45 * f3 + 45 * f4 - 9 * f5 + f6) / (60 * h[j]);
    //                 derivertives[j] = prime;
    //             }
    //             else if (accuracyOrder == 5)
    //             {
    //                 // f'(x) = {-137f(x) + 300f(x + h) - 300f(x + 2h) + 200f(x + 3h) - 75f(x + 4h) + 12f(x + 5h)} / 60h + O(h^5)
    //                 var f1 = CalculateResult(observedX,parameters);
    //                 var f2 = CalculateResult(observedX,parameters + h);
    //                 var f3 = CalculateResult(observedX,parameters + 2 * h);
    //                 var f4 = CalculateResult(observedX,parameters + 3 * h);
    //                 var f5 = CalculateResult(observedX,parameters + 4 * h);
    //                 var f6 = CalculateResult(observedX,parameters + 5 * h);
    //
    //                 var prime = (-137 * f1 + 300 * f2 - 300 * f3 + 200 * f4 - 75 * f5 + 12 * f6) / (60 * h[j]);
    //                 derivertives[j] = prime;
    //             }
    //             else if (accuracyOrder == 4)
    //             {
    //                 // f'(x) = {f(x - 2h) - 8f(x - h) + 8f(x + h) - f(x + 2h)} / 12h + O(h^4)
    //                 var f1 = CalculateResult(observedX,parameters - 2 * h);
    //                 var f2 = CalculateResult(observedX,parameters - h);
    //                 var f3 = CalculateResult(observedX,parameters + h);
    //                 var f4 = CalculateResult(observedX,parameters + 2 * h);
    //
    //                 var prime = (f1 - 8 * f2 + 8 * f3 - f4) / (12 * h[j]);
    //                 derivertives[j] = prime;                }
    //             else if (accuracyOrder == 3)
    //             {
    //                 // f'(x) = {-11f(x) + 18f(x + h) - 9f(x + 2h) + 2f(x + 3h)} / 6h + O(h^3)
    //                 var f1 =  CalculateResult(observedX,parameters);;
    //                 var f2 = CalculateResult(observedX, parameters + h);
    //                 var f3 = CalculateResult(observedX, parameters + 2 * h);
    //                 var f4 = CalculateResult(observedX, parameters + 3 * h);
    //
    //                 var prime = (-11 * f1 + 18 * f2 - 9 * f3 + 2 * f4) / (6 * h[j]);
    //                 derivertives[j] = prime;
    //             }
    //             else if (accuracyOrder == 2)
    //             {
    //                 // f'(x) = {f(x + h) - f(x - h)} / 2h + O(h^2)
    //                 var f1 = CalculateResult(observedX,parameters + h);
    //                 var f2 = CalculateResult(observedX,parameters - h);
    //
    //                 var prime = (f1 - f2) / (2 * h[j]);
    //                 derivertives[j] = prime;                   }
    //             else
    //             {
    //                 // f'(x) = {- f(x) + f(x + h)} / h + O(h)
    //                 var f1 =  CalculateResult(observedX,parameters);;
    //                 var f2 = CalculateResult(observedX,parameters + h);
    //
    //                 var prime = (-f1 + f2) / h[j];
    //                 derivertives[j] = prime;                   }
    //
    //             h[j] = 0;
    //         }
    //
    //         return derivertives;
    //     }
}
