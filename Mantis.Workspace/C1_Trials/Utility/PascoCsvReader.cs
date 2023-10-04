using System.Globalization;
using Mantis.Core.FileImporting;

namespace Mantis.Workspace.C1_Trials.V39_Hysteresis;

public record struct PascoData(double Time, double VoltageA, double CurrentA, double VoltageB, double CurrentB);

public class PascoCsvReader : RowWiseCsvReaderBase
{
    public int DataColumnCount = 3;

    public Dictionary<string, List<PascoData>> MeasurementSeries = new Dictionary<string, List<PascoData>>();

    private List<PascoData>[] _measurementSeries;

    public int SeriesCount = 0;

    public string FirstHeaderElementStart = "Zeit (s) ";

    public readonly CultureInfo CultureInfo;
    
    public PascoCsvReader(string path) : this(path,
        System.Globalization.CultureInfo.GetCultureInfo("de-De")){}
    
    public PascoCsvReader(string path,CultureInfo cultureInfo) : base(path)
    {
        CultureInfo = cultureInfo;
        if (CultureInfo == CultureInfo.GetCultureInfo("de-DE"))
        {
            ColumnSeparator = ';';
        }
    }

    protected override void ProcessContentRow(string[] row, int index)
    {
        
        if (index == 0)
        {
            SeriesCount = row.Length / DataColumnCount;

            _measurementSeries = new List<PascoData>[SeriesCount]; 
            for (int i = 0; i < SeriesCount; i++)
            {
                int columnIndex = i * DataColumnCount;
                _measurementSeries[i] = new List<PascoData>();

                string seriesName = row[columnIndex].Substring(FirstHeaderElementStart.Length);
                MeasurementSeries.Add(seriesName,_measurementSeries[i]);
            }
        }
        else
        {
            for (int i = 0; i < SeriesCount; i++)
            {
                int columnIndex = i * DataColumnCount;
                if (!string.IsNullOrEmpty(row[columnIndex]))
                {
                    var element = new PascoData(
                        double.Parse(row[columnIndex + 0], CultureInfo),
                        double.Parse(row[columnIndex + 1], CultureInfo),
                        0,//double.Parse(row[columnIndex + 3], CultureInfo),
                        double.Parse(row[columnIndex + 2], CultureInfo),
                        0//double.Parse(row[columnIndex + 5], CultureInfo)
                    );
                    _measurementSeries[i].Add(element);
                }
            }
        }
    }

    public override string ToString()
    {
        string res = "";

        foreach (var series in MeasurementSeries)
        {
            res += $" {series.Key}({series.Value.Count})\t";
        }

        return res;
    }
}