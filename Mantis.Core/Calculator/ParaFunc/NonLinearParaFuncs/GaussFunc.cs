using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;

namespace Mantis.Core.Calculator;


/// <summary>
/// Gauss curve f(x) = A + B * 1/D * Exp(-0.5 ((x-C) / D) ^ 2 ) * (2Pi)^-0.5
/// </summary>
public class GaussFunc : AutoDerivativeFunc,IFixedParameterCount
{

    public int ParameterCount => 4;
    public override double CalculateResult(Vector<double> ps, double x)
    {
        _ratioInExponent = ((x - ps[2]) / ps[3]);

        return ps[0] + ps[1] * Constants.InvSqrt2Pi /ps[3]  * Math.Exp(-0.5 * _ratioInExponent * _ratioInExponent) ;
    }

    private double _ratioInExponent = 0;
    private double _functionValue = 0;

    // public override double CalculateGradient(Vector<double> ps, double x, int index, double previousValue)
    // {
    //     switch (index)
    //     {
    //         case 0:
    //             return 1;
    //         case 1:
    //             _functionValue = CalculateResult(ps, x) - ps[0];
    //             return _functionValue / ps[1];
    //         case 2:
    //             return _functionValue * _ratioInExponent / ps[3];
    //         case 3:
    //             return _functionValue / ps[3] * (-1+ _ratioInExponent * _ratioInExponent);
    //         default:
    //             throw new IndexOutOfRangeException();
    //     }
    // }
    //
    // public override double CalculateXDerivative(Vector<double> ps, double x)
    // {
    //     return (CalculateResult(ps, x) - ps[0]) * _ratioInExponent / ps[3];
    // }
}