﻿using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.QuickTable;
using Mantis.Core.TexIntegration;
using Mantis.Core.Utility;
using ScottPlot.Finance;

namespace Mantis.Workspace.C1_Trials.V46_Radioactivity;


public class V46_LocalDosis
{
    public static void Process()
    {
        var csvReader = new SimpleTableProtocolReader("LocalDosisData.csv");
        List<double> dataListNear = csvReader.ExtractSingleValue("val:LocalDosisData").Select(double.Parse).ToList();
        List<double> dataListFar = csvReader.ExtractSingleValue("val:1mDosisData").Select(double.Parse).ToList();
        List<double> ambientList = csvReader.ExtractSingleValue("val:AmbientDosis").Select(double.Parse).ToList();

        
        
        ErDouble ambientRadiation = ambientList.WeightedMean();
        ambientRadiation.AddCommandAndLog("AmbientRadiation","ySv/h");
        //(ambientRadiation * 2.5).AddCommandAndLog("AmbientDosis2andHalfHours","ySv");
        (ambientRadiation*24*365).AddCommandAndLog("AmbientDosisPerAnnum","ySv");
        ErDouble meanNear = dataListNear.WeightedMean();
        meanNear.AddCommandAndLog("RadiationNear" ,"ySv/h");
        ErDouble meanFar = dataListFar.WeightedMean();
        meanFar.AddCommandAndLog("RadiationFar","ySv/h");
        Calculate4HourDose(meanNear,meanFar);
        CalculateMaximalWorkingTime(meanFar);
        CalculateYearlyPercentage(meanFar * 4).AddCommandAndLog("YearlyPercentageCsRadiation","");
    }

    public static void Calculate4HourDose(ErDouble perHourDoseNear, ErDouble perHourDoseFar)
    {
        (perHourDoseNear*4).AddCommandAndLog("4HourNear","ySv");
        (perHourDoseFar*4).AddCommandAndLog("4HourFar","ySv");
    }

    public static void CalculateMaximalWorkingTime(ErDouble perHourFar)
    {
        ((5 * Math.Pow(10, -3) )/ (perHourFar * Math.Pow(10, -6))).AddCommandAndLog("SafeTime","h");
    }

    public static ErDouble CalculateYearlyPercentage(ErDouble perHourDose)
    {
        return 100*(perHourDose*Math.Pow(10,-6))/(5*Math.Pow(10,-3));
    }
}