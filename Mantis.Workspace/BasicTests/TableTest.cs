using Mantis.Core.TexIntegration;
using Mantis.Core.TexIntegration.Utility;

namespace Mantis.Workspace.BasicTests;

public record struct DummyType(double a, double b);

public static class TableTest
{
    public static void CreateSimpleTestTable()
    {
        List<DummyType> dummyList = new List<DummyType>()
        {
            new DummyType(1, 1.2),
            new DummyType(2, 2.2),
            new DummyType(3, 3.2)
        };

        new TexTable(
            header: new string[] { "X", "Y" },
            content: dummyList.Select(d => new object[]{d.a,d.b}),
            label: "SimpleTable",
            caption: "this is is a simple table")
        {
            Orientation = TexTable.TableOrientation.Horizontal
        }.SaveLabeled();

    }
}