using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.QuickTable;
using Mantis.Core.ScottPlotUtility;
using Mantis.Core.TexIntegration;
using Mantis.Core.Utility;
using MathNet.Numerics;
using ScottPlot;

namespace Mantis.Workspace.C1_Trials.V42_MicrowaveMeasurement;

[QuickTable("","")]
public record struct AngleVoltageData
{
    [QuickTableField("angle", "\\degree")] public ErDouble Angle = 0;

    [QuickTableField("diodeVoltage", "V",lastDigitError:1)] public ErDouble Voltage = 0;
    
    public AngleVoltageData(){}
}

public static class Part1_AngleDispersion
{
    
    // In this part we want to measure the angle dependence of the diode.
    // So you mount the emitting diode on a rotary table. And then you measure the received microwaves depending on the angle
    // For the evaluation: First we want to import the measured angle-intensity data from the csv-file
    // Next we make a gauss fit so we get an estimation of the FWDH
    // At last we plot the data with the fit.
    // This is all we have to do in this part
    public static void Process()
    {
        // Step 1: Importing:
        // We have to initialize the csv-reader. We need to give it the file-path
        // Note that the file path you enter here is relative to the FileManage.GlobalPath which you specified in the
        // Program.cs file.
        var csvReader = new SimpleTableProtocolReader("Part1_AngleDispersion.csv");
        
        // Then we want to extract single values in the csv-file
        // Single Values are marked with the '* ' token in the csv-file.
        // You then enter the name of the SingleValue
        var voltmeterRange = csvReader.ExtractSingleValue<double>("voltmeterRange");
        var errorAngle = csvReader.ExtractSingleValue<double>("error_angle");
        
        // Analog (kind of) you extract a table
        // Tables in the csv-file are marked with '# '
        // The next row are then the column names. And after that the data follows
        // To extract a table you need to declare a struct type with a [QuickTable(...)]-attribute
        // This struct type then has fields with the [QuickTableField(columnName,...)]-attribute
        // The importer will compare the the columnNames of the csv-file with the columnNames of the QuickTableField-attribute
        // If they match it will assign the value from the csv-file to the QuickTableField 
        List<AngleVoltageData> dataList = csvReader.ExtractTable<AngleVoltageData>("tab:angleDispersion");
        
        // Step 2: Doing some calculations
        // Here we want to initialize the errors of the 'AngleDispersionData'
        // So we perform the method 'CalculateErrors(...)' for each element in the list
        // We need to use the method 'ForEachRef' since AngleDispersionData is a struct
        dataList.ForEachRef(((ref AngleVoltageData e) => CalculateErrors(ref e,voltmeterRange,errorAngle)));
        // The following code would be analogous
        // for (int i = 0; i < dataList.Count; i++)
        // {
        //     var e = dataList[i];
        //     CalculateErrors(ref e,voltmeterRange,errorAngle);
        //     dataList[i] = e;
        // }

        ErDouble voltageOffset = csvReader.ExtractSingleValue<double>("voltageOffset");
        
        dataList.ForEachRef((ref AngleVoltageData data) => 
            data.Voltage -= voltageOffset);
        
        // Step 3: Regression
        // First we need to create a RegModel
        // This needs to be a RegModel<GaussFunc> since we want to fit a Gaussian function
        //
        // In the class GaussFunc derives from the abstract class 'AutoDerivativeFunc'
        // It overrides the method 'CalculateResult(parameters,x)' and returns the gauss function value
        // If you want to fit a custom function you need to create also a type which derives from the 'AutoDerivativeFunc'
        // And then override the method 'CalculateResult(...)' 
        //
        // To initialize the RegModel I use the utility-method 'CreateRegModel'
        // This is a (extension-)method of the dataList
        // The first method parameter is a selector which selects from each element of the List the x and y points
        // selector: e => (x,y)
        // These are the data points.
        // The second method parameter is a Parameter Function
        // It also needs to be a ParaFunc<GaussFunc> since we want to fit a gauss curve
        // In the constructor we say the ParaFunc<GaussFunc> has a parameter Count of 4 because the gauss curve
        // has 4 free-parameters
        //
        // Lastly for convenience I already specify the Units of the free-parameters. Since they will be added
        // when we log the parameters
        RegModel model = dataList.CreateRegModel(e => (e.Angle, VoltageDiode: e.Voltage),
            new ParaFunc(4,new GaussFunc())
            {
                Units = new[] {"V", "V", "\\degree", "\\degree"}
            });
        
        // Now I do a Levenberg Marquradt regression
        // The first method parameter is the initial Guess of the free-parameters. Make sure that they are
        // not completely wrong. Because then the regression won't work 
        model.DoRegressionLevenbergMarquardt(new double[] {0, 1, 0, 1},false);
        
        // Now the free parameters were calculated in the regression and saved to the RegModel
        // We can now log them and also add them to the TexPreamble so we can use them in the LaTex-file
        model.AddParametersToPreambleAndLog("AngleDispersionGaussFit");
        
        
        
        // In this trail-part we want to calculate the FWHM. For a normal distribution the FWHM
        // is calculated by fwhm = constants * variance
        // The variance of the normal distribution is the fourth free-parameter
        // You can access the free parameters with model.ErParamaters
        var fwhm = 2 * Math.Sqrt(2 * Constants.Ln2) * model.ErParameters[3];
        // We log the fwhm and add it to the TexPreamble
        fwhm.AddCommandAndLog("AngleDispersionGaussFWHM","\\degree");
        
        // Step 4: Plotting
        // For Plotting I use the ScottPlot library. So please feel free to look at its documentation:
        // https://scottplot.net/cookbook/4.1/
        // But i have written some Extension-Methods for convenience
        
        // The first creates a pre-configured Plot with all the style settings I like
        // You can change those settings via the 'plot' object
        DynPlot plot = new DynPlot("Winkel in °","Spannung in V");//"Angle in °", "Voltage in V");
        
        // Next I add the RegModel to the Plot
        // It will automatically draw the DataPoints and the Function Curve
        // You need to specify the legend - labels
        plot.AddRegModel(model, "Signal des Empfängers", "Gauss-Anpassung"); //"Reciever output", "Gauss Fit",errorBars:false);
        
        
        double xn = model.ErParameters[2].Value - fwhm.Value / 2;
        double xp = model.ErParameters[2].Value + fwhm.Value / 2;
        double y = model.ParaFunction.EvaluateAtDouble(xp);
        //var bracket = plot.Add.Bracket(xn, y, xp, y, "fwhm");
        //bracket.Color = plot.Palette.GetColor(2);
        
        
        // Change the Legend alignment
        plot.Legend.Location = Alignment.UpperRight;
        
        
        // Last Save the plot and also add a reference to the TexPreamble file. So you can conveniently use it in your
        // Tex-file with the \figAngleDispersion command
        plot.SaveAndAddCommand("fig:AngleDispersion");

        string mathematicaList = "";
        foreach (var data in dataList)
        {
            mathematicaList += $"{data.Angle.Value}\t{data.Voltage.Value}\n";
        }
    }

    private static void CalculateErrors(ref AngleVoltageData data, double voltmeterRange, double errorAngle)
    {
        data.Angle.Error = errorAngle;
        
        // I use this Utility method to get the voltage error of the Aglient34405-Device. Here these wierd tables from
        // the manual are saved.
        // data.VoltageDiode = DeviceErrorsUtil.CalculateDeviceError(Devices.Aglient34405, DataTypes.VoltageDC,
        //     data.VoltageDiode.Value, voltmeterRange);
    }
}