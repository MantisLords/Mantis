using System.Text;

namespace Mantis.Core.TexIntegration;

public interface ITexWritable
{
    public void AppendToTex(StringBuilder builder);
}

public static class TexWritableExtension
{
    public static void Save(this ITexWritable writable,string filepath)
    {
        TexWriter writer = new TexWriter();
        writer.Write(writable);
        writer.Save(filepath);
    }
}