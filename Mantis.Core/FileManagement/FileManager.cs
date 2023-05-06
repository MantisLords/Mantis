using System;
using System.IO;
using System.Reflection;

namespace Mantis.Core.FileManagement
{
    public static class FileManager
    {
        public static string GlobalWorkspace
        {
            get => _globalWorkspace ?? throw new Exception("You need to set a global workspace path");
            set => _globalWorkspace = PathUtility.GetFormattedDirectory(value) ?? throw new Exception("You need to set a valid global workspace path");
        }

        public static string CurrentWorkspace
        {
            get => _currentWorkspace??GlobalWorkspace;
            set => _currentWorkspace = PathUtility.GetFormattedDirectory(value);
        }
        public static void ResetCurrentWorkspace() => _currentWorkspace = null;

        public static string CurrentOutputDir
        {
            get => _currentOutputDir ?? CurrentWorkspace + "Generated"+Path.DirectorySeparatorChar;
            set => _currentOutputDir = PathUtility.GetFormattedDirectory(value);
        }

        public static void ResetCurrentOutputDir()
        {
            _currentOutputDir = null;
        }

        private static string _currentInputDirOverride;
        private static string _currentOutputDir;
        private static string _currentWorkspace;
        private static string _globalWorkspace;

        public static string CurrentInputDir
        {
            get => _currentInputDirOverride ?? CurrentWorkspace;
            set => _currentInputDirOverride = (PathUtility.GetFormattedDirectory(value));
        }

        public static void ResetCurrentInputDir()
        {
            _currentInputDirOverride = null;
        }

        
        
        
    }
}