using Mantis.Core.FileImporting;

namespace Mantis.Workspace.BasicTests;
using Mantis.Core.Utility;

public static class CSVImporterTest
{
    public static void ImportDummyFile()
    {
        SimpleTableProtocolReader reader = new SimpleTableProtocolReader();
        reader.ReadFile("SimpleCSVFile");

        var dummyTable = reader.ExtractTable<DummyType>();
        Console.WriteLine($"DummyTable\n{TableUtility.ToStringForeach(dummyTable)}");

        var simpleValue = reader.ExtractSingleValue<double>("SimpleValue");
        Console.WriteLine(simpleValue);
        var simpleErValue = reader.ExtractSingleValue<ErDouble>("SimpleErDouble");
        Console.WriteLine(simpleErValue);
        
        Console.WriteLine();
        var dummyTable2 = reader.ExtractTable<DummyType>("DummyTable2");
        Console.WriteLine($"DummyTable2\n{TableUtility.ToStringForeach(dummyTable2)}");



    }
    
    
}