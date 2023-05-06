using System.IO;

namespace Mantis.Core.FileManagement
{
    public static class PathUtility
    {
        public static string RelativeProjectPath = "../../";  
        
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
    }
}