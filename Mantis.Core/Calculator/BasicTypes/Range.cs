namespace Mantis.Core.Calculator;

public struct Range<T> where T : IComparable<T>
{
    
    public T Min
    {
        readonly get => _min;
        set
        {
            _min = value;
            CheckFlip();
        }
    }

    public T Max
    {
        readonly get => _max;
        set
        {
            _max = value;
            CheckFlip();
        }
    }

    private T _min;
    private T _max;

    private void CheckFlip()
    {
        if (Max.CompareTo(Min) > 0)
        {
            (_min, _max) = (_max, _min);
        }
    }
}