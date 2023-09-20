using System.Globalization;
using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;

namespace Mantis.Workspace.C1_Trials.Utility;

public record struct OsziData(ErDouble Time, ErDouble Voltage);

public class OsziRowWiseCsvReader : RowWiseCsvReaderBase
{
    public List<OsziData> Data = new List<OsziData>();
    public int TimeColumn = 3;

    public int VoltageColumn = 4;
    
    public OsziRowWiseCsvReader(string path) : base(path)
    {
    }

    protected override void ProcessContentRow(string[] row, int index)
    {
        if (string.IsNullOrEmpty(row[TimeColumn]) || string.IsNullOrEmpty(row[VoltageColumn])) return;

    ErDouble time = new ErDouble(double.Parse(row[TimeColumn],CultureInfo.InvariantCulture),0);
        ErDouble voltage = new ErDouble(double.Parse(row[VoltageColumn], CultureInfo.InvariantCulture), 0);
        
        
        Data.Add(new OsziData(time,voltage));

    }
}