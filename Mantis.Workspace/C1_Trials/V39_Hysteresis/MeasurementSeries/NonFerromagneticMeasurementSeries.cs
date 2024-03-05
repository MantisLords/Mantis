using Mantis.Core.Calculator;
using Mantis.Core.ScottPlotUtility;
using MathNet.Numerics;
using ScottPlot;

namespace Mantis.Workspace.C1_Trials.V39_Hysteresis;

public class NonFerromagneticMeasurementSeries : HysteresisMeasurementSeries
{
    public ErDouble? MagneticPermeability = null;
    private RegModel? LinearFit = null;

    public bool DrawBestFit = true;
    
    internal NonFerromagneticMeasurementSeries(string name, List<PascoData> rawData, MeasurementSeriesInfo seriesInfo, RingCore ringCore, double errorVoltage, bool removeDrift = true, bool centerData = true) : base(name, rawData, seriesInfo, ringCore, errorVoltage, removeDrift, centerData)
    {
        CalculateMagneticPermeability();
    }

    public void CalculateMagneticPermeability()
    {
        LinearFit = DataList.CreateRegModel(e => (e.H, e.B), new ParaFunc(2,new LineFunc()));
        LinearFit.DoLinearRegression(false);
        var slope = LinearFit.ErParameters[1];
        MagneticPermeability = slope / Constants.MagneticPermeability;
    }

    public override void SaveAndLogCalculatedData()
    {
        //MagneticPermeability?.AddCommandAndLog("MagneticPermeability"+Label,"Tm/A");
    }

    public override Plot PlotData(Plot plt)
    {
        base.PlotData(plt);

        if (DrawBestFit)
        {
            
            plt.AddDynFunction(LinearFit.ParaFunction, label: "Fit zu Bestimmung\nder Permeabilität"); //"Best fit for calculating\nmagnetic permeability");
        }
        
        return plt;
    }
}