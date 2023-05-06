using System.Text;

namespace Mantis.Core.TexIntegration;

public interface ITexWritableEnclosed
{
    public void AppendBegin(StringBuilder builder);

    public void AppendEnd(StringBuilder builder);
}