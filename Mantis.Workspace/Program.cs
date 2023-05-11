// See https://aka.ms/new-console-template for more information

using Mantis.Core.FileManagement;
using Mantis.Workspace.BasicTests;
using Mantis.Workspace.Fr2.Sheet3_Correlation;

Console.WriteLine("Hello, World!");

FileManager.GlobalWorkspace = "Fr2/Sheet3_Correlation/Data_Thomas_Karb";
Sheet3_Correlation_Main.Process();