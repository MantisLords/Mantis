// See https://aka.ms/new-console-template for more information

using Mantis.Core.FileManagement;
using Mantis.Workspace.BasicTests;
using Mantis.Workspace.Fr2.Sheet3_Correlation;
using Mantis.Workspace.Fr2.Sheet4_Regression1;
using Mantis.Workspace.Fr2.Sheet5_Regression2;
using Mantis.Workspace.Fr2.Sheet6_Regression3;

Console.WriteLine("Hello, World!");

FileManager.GlobalWorkspace = "Fr2/Sheet6_Regression3/Data_Thomas_Karb";
Sheet6_Regression3_Main.Process();