using Mantis.Core.Calculator;
using Mantis.Core.QuickTable;

namespace Mantis.Workspace.Fr2.Sheet6_Regression3;

[QuickTable("Angle dependence of scattering antiprotons at hydrogen","tab:HScatteringData")]
public record struct HScatteringData
{
    [QuickTableField("detector angle", "\\degree","\\phi")]
    public double Angle;

    [QuickTableField("scattering count at full target", "", "N_{full}")]
    public ErDouble ScatteringCount;

    [QuickTableField("scattering count at empty target", "", "N_{empty}")]
    public ErDouble BackgroundScatteringCount;

    [QuickTableField("cleansed scattering count", "", "N", false)]
    public ErDouble CleansedScatteringCount;

    public double CosOfAngle;
    
    public HScatteringData(){}

}

public static class Sheet6_Regression3_Main
{
    /*public static void Process()
    {
        List<HScatteringData> data =
            new SimpleTableProtocolReader("HScatteringData.csv").ExtractTable<HScatteringData>();
        
        data.ForEachRef(AddErrorAndCleanseData);
        
        data.CreateTexTable().SaveLabeled();

        ErDouble[] parameters = data.LinearRegressionLegendrePolynomial(e => (e.CosOfAngle, e.CleansedScatteringCount), 3,
            RegressionCommand.UseYErrors);

        int freedom = data.Count - parameters.Length;
        double reducedChiSquared = CalculateReducedChiSquaredHacked(data, parameters);
        var chiDistribution = new ChiSquared(freedom);
        double probability = 1- chiDistribution.CumulativeDistribution(reducedChiSquared * freedom);
        Console.WriteLine($"Reduced Chi Squared: {reducedChiSquared} \n propability : {probability} \n freedom: {freedom}");
        
        
        freedom.AddCommand("DegreesOfFreedom");
        reducedChiSquared.AddCommand("ReducedChiSquared");
        probability.AddCommand("ProbabilityChiSquared");

        for (int i = 0; i < parameters.Length; i++)
        {
            Console.WriteLine($"Parameters A{i} = {parameters[i].ToString()}");
            parameters[i].AddCommand($"regressionParameter{(char)(65+i)}");
        }

        Sketchbook sketchbook = new Sketchbook(
            axis: new AxisLayout("Cos of detector angle $\\cos(\\phi)$", "Scattering Count $N$"),
            label: "fig:HScattering",
            caption: "Scattering of antiprotons at hydrogen atoms",
            title:"Scattering graph with legendre polynomials Thomas Karb 1.6.23");
        
        sketchbook.Add(new DataMarkSketch()
        {
            Data = data.Select(e => ((ErDouble)e.CosOfAngle,e.CleansedScatteringCount)),
            Legend = "Cleansed scattering count"
        });
        
        sketchbook.Add(new FunctionPlot()
        {
            FunctionString = $"({parameters[0].Value}) + x * ({parameters[1].Value}) + 0.5 * (3*x^2-1) * ({parameters[2].Value})",
            Legend = "Best fit legendre polynomial"
        });
        
        sketchbook.SaveLabeled();
        
        TexPreamble.AddCommand("^{\\circ}","\\degree");
            
        TexPreamble.GeneratePreamble();
    }

    private static void AddErrorAndCleanseData(ref HScatteringData e)
    {
        e.ScatteringCount.Error = Math.Sqrt(e.ScatteringCount.Value);
        e.BackgroundScatteringCount.Error = Math.Sqrt(e.BackgroundScatteringCount.Value);

        e.CleansedScatteringCount = e.ScatteringCount - 2 * e.BackgroundScatteringCount;

        e.CosOfAngle = Math.Cos(e.Angle * Math.PI / 180.0);
    }

    private static double CalculateReducedChiSquaredHacked(List<HScatteringData> data, ErDouble[] parameters)
    {
        double chiSquared = 0;
        
        int freedom = data.Count - parameters.Length;
        
        foreach (var d in data)
        {
           var xss = PolynomialRegression.LegendrePolynomialEvaluation(d.CosOfAngle, 3).ToArray();
            double y = xss[0] * parameters[0].Value + xss[1] * parameters[1].Value + xss[2] * parameters[2].Value;
            chiSquared += (d.CleansedScatteringCount.Value - y) * (d.CleansedScatteringCount.Value - y) /
                          d.CleansedScatteringCount.Error / d.CleansedScatteringCount.Error;
        }

        double reduced = chiSquared / freedom;
        
{        return reduced ; 
    }*/
}