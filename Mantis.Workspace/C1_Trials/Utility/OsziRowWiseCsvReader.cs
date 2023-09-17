using System.Globalization;
using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;

namespace Mantis.Workspace.C1_Trials.Utility;

public record struct OsziData(ErDouble Time, ErDouble Voltage);

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
        ErDouble time = new ErDouble(double.Parse(row[TimeColumn],CultureInfo.InvariantCulture),double.Parse(row[TimeColumn],CultureInfo.InvariantCulture)* 0.05);
        ErDouble voltage = new ErDouble(double.Parse(row[VoltageColumn], CultureInfo.InvariantCulture), double.Parse(row[VoltageColumn], CultureInfo.InvariantCulture)*0.05);
        
        
        Data.Add(new OsziData(time,voltage));

    }
}