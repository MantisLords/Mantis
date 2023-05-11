using System.Numerics;

namespace Mantis.Core.Calculator;

public class InfoV<T> where T : INumber<T>
{
    public string Name;

    public T Value;
}