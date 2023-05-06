using System.Reflection.Emit;
using Mantis.Core.FileManagement;
using Mantis.Core.TexIntegration;
using Mantis.Core.TexIntegration.Utility;

namespace Mantis.Workspace.BasicTests;

public static class BasicTest_Main
{
    public static void Run()
    {
        FileManager.GlobalWorkspace = "BasicTests";
        
        TableTest.CreateSimpleTestTable();
    }

   
    
    
}