using Mantis.Core.FileImporting;

namespace Mantis.Workspace.BasicTests;

public static class CSVImporterTest
{
    public static void ImportDummyFile()
    {
        BasicCSVImporter importer = new BasicCSVImporter();
        importer.ReadFile("SimpleCSVFile");


    }
}