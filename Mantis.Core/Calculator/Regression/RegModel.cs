using Mantis.Core.LogCommands;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;

namespace Mantis.Core.Calculator;

public class RegModel<T> where T : FuncCore,new()
{
    public readonly ParaFunc<T> ParaFunction;

    public readonly DataSet Data;

    public Matrix<double> Weights;

    public int DegreesOfFreedom => Data.Count - ParaFunction.ParameterCount;

    public ErDouble[] ErParameters => ParaFunction.ErParameters;

    public RegModel(ParaFunc<T> paraFunction, DataSet data)
    {
        ParaFunction = paraFunction;
        Data = data;
        
        Matrix<double> yErrorMatrix = Matrix<double>.Build.DiagonalOfDiagonalVector(Data.YErrors).Power(2);
         Weights = yErrorMatrix.Determinant() != 0
             ? yErrorMatrix.Inverse()
             : Matrix<double>.Build.DiagonalIdentity(Data.Count);
    }

    public RegModel<T> Fork()
    {
        return new RegModel<T>(ParaFunction.Fork(), Data);
    }
    

     public double CalculateReducedResidual()
     {
         Vector<double> distance =
                 ParaFunction.CalculateResultPointWise(Data.XValues) - Data.YValues;
             var residual = distance.DotProduct( distance); // Weights should also be multiplied?
             return residual / DegreesOfFreedom;

     }

     public double CalculateFitProbability() =>
         CalculateReducedResidualProbability(CalculateReducedResidual(), DegreesOfFreedom);

     private double CalculateReducedResidualProbability(double reducedResidual, int degreeOfFreedom)
     {
         var chiDistribution = new ChiSquared(degreeOfFreedom);
         double probability = 1- chiDistribution.CumulativeDistribution(reducedResidual );
         return probability;
     }

     public override string ToString()
     {
         string res = "***Regression Model***\n";
         res += $"DataCount: {Data.Count}\n";
         res += ParaFunction.ToString();
         res += "\n******";
         return res;
     }

     public List<ILogCommand> GetGoodnessOfFitLog()
     {
         List<ILogCommand> commands = new List<ILogCommand>();
         commands.Add("Degrees of Freedom",DegreesOfFreedom);
         double reducedResidual = CalculateReducedResidual();
         commands.Add("Reduced Residual",reducedResidual);
         commands.Add("Probability",CalculateReducedResidualProbability(reducedResidual,DegreesOfFreedom));
         return commands;
     }
}

public static class FastRegModelConstructor
{
    public static RegModel<T> CreateRegModel<T,U>(this IEnumerable<U> data, Func<U, (ErDouble, ErDouble)> selector, ParaFunc<T> paraFunction)
        where T : FuncCore,new()
    {
        return new RegModel<T>(
            paraFunction:paraFunction,
            data:FastDataSetConstructor.CreateDataSet(data,selector)
            );
    }
}