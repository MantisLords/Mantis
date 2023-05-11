using System.Numerics;

namespace Mantis.Core.Calculator;

public struct Rect2
{
    public Vector2 Min
    {
        readonly get => _min;
        set
        {
            _min = value;
            //CheckFlip();
        }
    }

    public Vector2 Max
    {
        readonly get => _max;
        set
        {
            _max = value;
            //CheckFlip();
        }
    }

    private Vector2 _min;
    private Vector2 _max;

    private void CheckFlip()
    {
        if (Min.X > Max.X)
        {
            (_min.X, _max.X) = (_max.X, _min.X);
        }

        if (Min.Y > Max.Y)
            (_min.Y, _max.Y) = (_max.Y, _min.Y);
    }
}