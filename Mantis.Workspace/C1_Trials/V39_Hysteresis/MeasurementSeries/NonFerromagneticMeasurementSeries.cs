using Mantis.Core.Calculator;
using Mantis.Core.ScottPlotUtility;
using MathNet.Numerics;
using ScottPlot;

namespace Mantis.Workspace.C1_Trials.V39_Hysteresis;

public class NonFerromagneticMeasurementSeries : HysteresisMeasurementSeries
{
    public ErDouble? MagneticPermeability = null;
    private RegModel<LineFunc>? LinearFit = null;
    
    internal NonFerromagneticMeasurementSeries(string name, List<PascoData> rawData, MeasurementSeriesInfo seriesInfo, RingCore ringCore, double errorVoltage, bool removeDrift = true, bool centerData = true) : base(name, rawData, seriesInfo, ringCore, errorVoltage, removeDrift, centerData)
    {
        CalculateMagneticPermeability();
    }

    public void CalculateMagneticPermeability()
    {
        LinearFit = DataList.CreateRegModel(e => (e.H, e.B), new ParaFunc<LineFunc>(2));
        LinearFit.DoLinearRegression(false);
        var slope = LinearFit.ErParameters[1];
        MagneticPermeability = slope / Constants.MagneticPermeability;
    }

    public void PlotData(Plot plt,bool drawBestFit = true)
    {
        base.PlotData(plt);

        if (drawBestFit)
        {
            plt.AddFunction(LinearFit.ParaFunction, label: "Best fit for calculating\nmagnetic permeability");
        }
        
        Console.WriteLine($"Magnetic Permeability {MagneticPermeability.ToString()}");
    }
}