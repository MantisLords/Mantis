using Mantis.Core.FileManagement;

namespace Mantis.Workspace.BasicTests;

public static class BasicTest_Main
{
    public static void Run()
    {
        FileManager.GlobalWorkspace = "BasicTests";
        
        TableTest.CreateSimpleTestTable();
        
        SimplePlotTest.CreateSimplePlot();
    }

   
    
    
}