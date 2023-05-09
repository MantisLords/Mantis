using System.Numerics;

namespace Mantis.Core.Math.BasicTypes;

public class InfoV<T> where T : INumber<T>
{
    public string Name;

    public T Value;
}