using Mantis.Core.LogCommands;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Statistics;

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

     public double CalculateRSquared() => CalculateRSquared(CalculateReducedResidual());
     public double CalculateRSquared(double reducedResidual)
     {
         var residual = reducedResidual * DegreesOfFreedom;
         var yMean = Data.YValues.Mean();
         var ssTotDistance = Data.YValues - yMean;
         var ssTot = ssTotDistance.DotProduct(ssTotDistance);

         var rSquared = 1 - residual / ssTot;
         return rSquared;
     }

     public double CalculateAdjustedRSquared() => CalculateAdjustedRSquared(CalculateRSquared());

     private double CalculateAdjustedRSquared(double rSquared)
     {
         return 1 - (1 - rSquared) * (Data.Count - 1) / (DegreesOfFreedom - 1);
     }

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

         double rSquared = CalculateRSquared(reducedResidual);
         commands.Add("R Squared",rSquared);
         commands.Add("Adjusted R Squared",CalculateAdjustedRSquared(rSquared));
         
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