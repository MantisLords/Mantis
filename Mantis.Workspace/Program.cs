// See https://aka.ms/new-console-template for more information

using Mantis.Core.FileManagement;
using Mantis.Core.TexIntegration;
using Mantis.Workspace.BasicTests;
using Mantis.Workspace.C1_Trials.V42_MicrowaveMeasurement;
using Mantis.Workspace.C1_Trials.V41_EMWaweSpeed;


Console.WriteLine("Hello, World!");

FileManager.GlobalWorkspace = "C1_Trials\\V41_EMWaveSpeed";
V41_Main.Process();

