// See https://aka.ms/new-console-template for more information

using Mantis.Core.FileManagement;
using Mantis.Workspace.BasicTests;
using Mantis.Workspace.C1_Trials.V42_Microwaves_Measurement;
using Mantis.Workspace.Fr2.Sheet3_Correlation;
using Mantis.Workspace.Fr2.Sheet4_Regression1;
using Mantis.Workspace.Fr2.Sheet5_Regression2;
using Mantis.Workspace.Fr2.Sheet6_Regression3;
using Mantis.Workspace.Fr2.Sheet7_NonLinearRegression;
using ScottPlot;

Console.WriteLine("Hello, World!");

FileManager.GlobalWorkspace = "C1_Trials/V42_MicrowaveMeasurement/Data_Smailagic_Karb";
V42_MicrowaveMeasurement_Main.Process();




