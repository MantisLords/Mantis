using System.Text;
using Mantis.Core.FileManagement;

namespace Mantis.Core.TexIntegration;

public class TexWriter
{
    public StringBuilder Builder { get; private set; } = new StringBuilder();
    
    public TexWriter() { }

    public void Write(string text)
    {
        Builder.Append(text);
    }

    public void Write(ITexWritable writable)
    {
        writable.AppendToTex(Builder);
    }

    public void Save(string filePath)
    {
        string path = PathUtility.TryCombineAndAddExtension(FileManager.CurrentOutputDir, filePath,"tex");

        Directory.CreateDirectory(Path.GetDirectoryName(path));
        
        File.WriteAllText(path,Builder.ToString());
    }

}