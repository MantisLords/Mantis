using Mantis.Core.QuickTable;
using Mantis.Core.TexIntegration;

namespace Mantis.Workspace.BasicTests;

[QTable("This is a dummy Table", "DummyTable")]
public record struct DummyType
{
    [QTableField("A")] public double a;

    [QTableField("B")] public double b;
}

public static class TableTest
{
    public static List<DummyType> DummyList = new List<DummyType>()
    {
        new DummyType(){a = 1,b = 1.2},
        new DummyType(){a = 2,b = 2.2},
        new DummyType(){a = 3,b = 3.2},
    };
    
    public static void CreateSimpleTestTable()
    {
        
        DummyList.CreateTexTable().SaveLabeled();

    }
}