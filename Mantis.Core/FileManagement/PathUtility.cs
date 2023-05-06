using System.IO;

namespace Mantis.Core.FileManagement
{
    public static class PathUtility
    {
        public static string RelativeProjectPath = "../../../";  
        
        public static string GetFormattedDirectory(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            if (!Path.IsPathRooted(path))
            {
                path = RelativeProjectPath + path;
                
            }

            return path = Path.GetFullPath(path + '/');;
        }

        public static string TryCombineAndAddExtension(string directory, string filepath, string extension)
        {
            if (string.IsNullOrEmpty(filepath))
                throw new ArgumentException("The path may not be empty or null");
            
            if (!Path.IsPathRooted(filepath))
                filepath = Path.Combine(directory, filepath);

            if (!Path.HasExtension(filepath))
                filepath += '.' + extension;

            return filepath;
        }
    }
}