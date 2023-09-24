// See https://aka.ms/new-console-template for more information

using Mantis.Core.Calculator;
using Mantis.Core.FileManagement;
using Mantis.Core.TexIntegration;
using Mantis.Workspace.BasicTests;
using Mantis.Workspace.C1_Trials.V42_MicrowaveMeasurement;
using Mantis.Workspace.C1_Trials.V41_EMWaweSpeed;

using Mantis.Workspace.C1_Trials.V31_RealGasStateVariables;

Console.WriteLine("Hello, World!");

FileManager.GlobalWorkspace = "C1_Trials/V31_RealGasStateVariables/Data_Smailagic_Karb/Data";
V31_Main.Process();



