using System.Text;

namespace Mantis.Core.TexIntegration;

public interface ITexWritable
{
    public void AppendToTex(StringBuilder builder);
}