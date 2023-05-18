// See https://aka.ms/new-console-template for more information

using Mantis.Core.FileManagement;
using Mantis.Workspace.BasicTests;
using Mantis.Workspace.Fr2.Sheet3_Correlation;
using Mantis.Workspace.Fr2.Sheet4_Regression1;
using Mantis.Workspace.Fr2.Sheet5_Regression2;

Console.WriteLine("Hello, World!");

FileManager.GlobalWorkspace = "Fr2/Sheet5_Regression2/Data_Thomas_Karb";
Sheet5_Regression2_Main.Process();