using Mantis.Core.LogCommands;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Mantis.Core.Calculator;

public class ParaSet
{
    public readonly int Count;

    public Vector<double>? Parameters;

    public Matrix<double>? CovarianceMatrix;

    public ErDouble[]? ErParameters;

    public string[]? Labels;
    public string[]? Units;

    public ParaSet(int count)
    {
        Count = count;
    }

    public ParaSet(ErDouble[] erParameters)
    {
        Count = erParameters.Length;
        Parameters = Vector<double>.Build.DenseOfEnumerable(erParameters.Select(e => e.Value));
        var errors = Vector<double>.Build.DenseOfEnumerable(erParameters.Select(e => e.Error));
        CovarianceMatrix = Matrix<double>.Build.DiagonalOfDiagonalVector(errors.PointwisePower(2));
        ErParameters = erParameters;
    }

    public ParaSet(int count, Vector<double> parameters, Matrix<double> covarianceMatrix, ErDouble[] erParameters)
    {
        Count = count;
        Parameters = parameters;
        CovarianceMatrix = covarianceMatrix;
        ErParameters = erParameters;
    }

    public void SetParametersAndErrorsWithApprox(Vector<double> parameters, Matrix<double> covariance)
    {
        Parameters = parameters;
        SetCovarianceAndErrorsWithApprox(covariance);
    }

    public void SetCovarianceAndErrorsWithApprox(Matrix<double> covariance)
    {
        CovarianceMatrix = covariance;
        Vector<double> standardErrors = covariance.Diagonal().PointwiseSqrt();
        ErParameters = new ErDouble[Count];
        for (int i = 0; i < Count; i++)
        {
            ErParameters[i] = new ErDouble(Parameters[i], standardErrors[i]);
        }
    }

    public ParaSet Fork()
    {
        return new ParaSet(Count, Parameters, CovarianceMatrix, ErParameters);
    }

    public override string ToString()
    {
        string res = "";
        if (ErParameters == null || Parameters == null)
            res += $"ParaSet #Count: {Count}";
        else
            res += GetParametersLog().ToLogString();
        return res;
    }

    public List<ILogCommand> GetParametersLog()
    {
        List<ILogCommand> commands = new List<ILogCommand>(Count);
        if (ErParameters != null || Parameters != null)
        {
            
            for (int i = 0; i < Count; i++)
            {
                string label = Labels != null ? Labels[i] :((char)(i + 64+1)).ToString();
                string unit = Units != null ? Units[i] : "";
                if(ErParameters != null)
                    commands.Add(new NumberLogCom<ErDouble>(label,ErParameters[i],unit));
                else if(Parameters != null)
                    commands.Add(new NumberLogCom<double>(label,Parameters[i],unit));
            }
        }
        return commands;
    }
}