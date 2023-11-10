// See https://aka.ms/new-console-template for more information

using Mantis.Core.Calculator;
using Mantis.Core.FileManagement;
using Mantis.Workspace.C1_Trials.V39_Hysteresis;
using Mantis.Workspace.C1_Trials.V42_MicrowaveMeasurement;
using Mantis.Workspace.C1_Trials.V41_EMWaweSpeed;
using Mantis.Workspace.CondensedMatter1.Exercise2_DebyeModel;


Console.WriteLine("Hello, World!");

// FileManager.GlobalWorkspace = "C1_Trials/V41_EMWaveSpeed/Data_Smailagic_Karb";
// V41_Main.Process();

FileManager.GlobalWorkspace = "C1_Trials/V42_MicrowaveMeasurement/Data_Smailagic_Karb";
V42_MicrowaveMeasurement_Main.Process();

// FileManager.GlobalWorkspace = "C1_Trials/V39_Hysteresis/Data_Smailagic_Karb";
// V39_Hysteresis_Main.Process();

// FileManager.GlobalWorkspace = "CondensedMatter1/Exercise2_DebyeModel";
// CM1_Ex2_DebyeModel_Main.Process();