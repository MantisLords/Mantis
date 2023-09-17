using System.Globalization;
using Mantis.Core.FileImporting;

namespace Mantis.Workspace.C1_Trials.Utility;

public record struct OsziData(double Time, double Voltage);

public class OsziRowWiseCsvReader : RowWiseCsvReaderBase
{
    public List<OsziData> Data = new List<OsziData>();

    public int TimeColumn = 4;

    public int VoltageColumn = 5;
    
    public OsziRowWiseCsvReader(string path) : base(path)
    {
    }

    protected override void ProcessContentRow(string[] row,int index)
    {
        double time = double.Parse(row[TimeColumn],CultureInfo.InvariantCulture);
        double voltage = double.Parse(row[VoltageColumn],CultureInfo.InvariantCulture);
        
        
        Data.Add(new OsziData(time,voltage));

    }
}