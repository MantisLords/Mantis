using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices.JavaScript;

namespace Mantis.Core.Calculator;
/// <summary>
/// A datatype representing a value with error. It supports basic arithmetic +-*/Pow . It !only! calculates the correct
/// error if in your formula the variable occurs once!
/// </summary>
public struct ErDouble : INumber<ErDouble>
{
    public double Value;
    public double Error;

    public ErDouble(double value, double error=0)
    {
        Value = value;
        Error = error;
    }

    public static implicit operator ErDouble(double value) => new ErDouble(value);
    public static explicit operator double(ErDouble erDouble) => erDouble.Value;

    public double Max => Value + Error;
    public double Min => Value - Error;

    /// <summary>
    /// Realative Error
    /// </summary>
    public double RelEr
    {
        get => Error / Value;
        set => Error = value * Value;
    }

    /// <summary>
    /// Relative Error Squared
    /// </summary>
    public double RelErSq => RelEr * RelEr;

    public static ErDouble operator +(ErDouble a, ErDouble b)
        => new ErDouble(a.Value + b.Value, Math.Sqrt(a.Error * a.Error + b.Error * b.Error));

    public static ErDouble operator -(ErDouble a)
        => new ErDouble(-a.Value, a.Error);

    public static ErDouble operator -(ErDouble a, ErDouble b) => -b + a;

    public static ErDouble operator *(ErDouble a, ErDouble b)
        => new ErDouble(a.Value * b.Value,  Math.Sqrt(a.Error * b.Value * a.Error * b.Value + b.Error * a.Value * b.Error * a.Value));

    public static ErDouble operator /(ErDouble a, ErDouble b)
        => new ErDouble(a.Value / b.Value, a.Value / b.Value * Math.Sqrt(a.RelErSq + b.RelErSq));

    /// <summary>
    /// Raises the ErDouble to the power of "power"
    /// </summary>
    public ErDouble Pow(double power)
        => new ErDouble(Math.Pow(Value, power), Math.Pow(Value, power) * RelEr * power);

    /// <summary>
    /// Multiplies the ErDouble with 10^power
    /// </summary>
    public ErDouble Mul10E(int power)
        => this * Math.Pow(10, power);


    /// <summary>
    /// Calculates e^exponent
    /// </summary>
    public static ErDouble Exp(ErDouble exponent)
    {
        ErDouble res = Math.Exp(exponent.Value);
        res.Error = res.Value * exponent.Error;
        return res;
    }

    /// <summary>
    /// Returns the natural log of the "argument"
    /// </summary>
    public static ErDouble Log(ErDouble argument)
    {
        ErDouble res = Math.Log(argument.Value);
        res.Error = argument.RelEr;
        return res;
    }

    public static ErDouble Cos(ErDouble phi)
    {
        ErDouble res = Math.Cos(phi.Value);
        res.Error = phi.Error * Math.Sin(phi.Value);
        return res;
    }
    
    public static ErDouble Sin(ErDouble phi)
    {
        ErDouble res = Math.Sin(phi.Value);
        res.Error = phi.Error * Math.Cos(phi.Value);
        return res;
    }
    

    public static ErDouble operator %(ErDouble left, ErDouble right)
    {
        throw new NotImplementedException();
    }

    public static ErDouble operator +(ErDouble value)
    {
        return value;
    }
    
    public static ErDouble Abs(ErDouble value)
    {
        return new ErDouble(Math.Abs(value.Value), value.Error);
    }

    #region To String Stuff

    private string ToStringFormatted(bool isLatex)
    {
        int power = GetPowerFormatted();

        double formattedValue = GetValueFormatted(power);

        double formattedError = GetErrorFormatted(power);

        int digits = GetDigits(power);
        char formatFix = Error > 0 ? 'F' : 'G';
        string format = $"{formatFix}{digits}";


        if (isLatex)
        {
            string appendix = "";
            if (power < 0)
                appendix = $" \\cdot 10^{{{power}}}";
            else if (power > 0)
                appendix = $" \\cdot 10^{{{power}}}";

            string sign = Value < 0 ? "-" : "";


            if (formattedError > 0)
                return $"({sign}{formattedValue.ToString(format)} \\pm {formattedError.ToString(format)})" + appendix;
            else
            {
                return $"{sign}{formattedValue.ToString(format)} " + appendix;
            }

        }else
        {
            string appendix = "";
            if (power < 0)
                appendix = $" E{power}";
            else if (power > 0)
                appendix = $" E+{power}";

            string sign = Value < 0 ? "-" : "";


            if (formattedError > 0)
                return $"({sign}{formattedValue.ToString(format)} {(char)0x00B1} {formattedError.ToString(format)})" + appendix;
            else
            {
                return $"{sign}{formattedValue.ToString(format)} " + appendix;
            }
        }
    }

    public override string ToString()
    {
        return ToStringFormatted(false);
    }

    private double GetErrorFormatted(int power)
    {
        return Error * Math.Pow(10, -power);
    }

    private double GetValueFormatted(int power)
    {
        return Math.Abs(Value) * Math.Pow(10, -power);
    }

    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        bool isLatex = !string.IsNullOrEmpty(format) && (format.StartsWith('G'));
        
        return ToStringFormatted(isLatex);
    }

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        charsWritten = 0;
        return false;
    }

    private int GetDigits(int power)
    {
        double formattedValue = Math.Abs(Value) * Math.Pow(10, -power);

        int digits = 4;
        if (Error > 0)
        {
            int powerFormattedError = (int)Math.Floor(Math.Log10(GetErrorFormatted(power)));
            digits = -powerFormattedError + 1;
        }

        return digits;
    }


    private int GetPower()
    {
        double absValue = Math.Abs(Value);
        if (absValue == 0)
            return 0;
        return (int) Math.Floor(Math.Log10(absValue));
    }

    private int GetPowerFormatted()
    {
        int power = GetPower();
        if (power is <= 2 and >= -2)
            power = 0;

        return power;
    }

    #endregion




    #region Cmpare operaters

    public int CompareTo(ErDouble other)
    {
        int v = Value.CompareTo(other.Value);
        if (v == 0)
            v = Error.CompareTo(other.Error);
        return v;
    }

    public bool Equals(ErDouble other)
    {
        return other.Value == Value && other.Error == Error;
    }
    
    public int CompareTo(object? obj)
    {
        throw new NotImplementedException();
    }
    
    public static bool operator ==(ErDouble left, ErDouble right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ErDouble left, ErDouble right)
    {
        return !(left == right);
    }

    public static bool operator >(ErDouble left, ErDouble right)
    {
        throw new NotImplementedException();
    }

    public static bool operator >=(ErDouble left, ErDouble right)
    {
        throw new NotImplementedException();
    }

    public static bool operator <(ErDouble left, ErDouble right)
    {
        throw new NotImplementedException();
    }

    public static bool operator <=(ErDouble left, ErDouble right)
    {
        throw new NotImplementedException();
    }

    public static ErDouble operator --(ErDouble value)
    {
        throw new NotImplementedException();
    }

    public static ErDouble operator ++(ErDouble value)
    {
        throw new NotImplementedException();
    }

    #endregion
    

    #region Integer Infinity Stuff

    public static bool IsCanonical(ErDouble value)
    {
        return true;
    }

    public static bool IsComplexNumber(ErDouble value)
    {
        return false;
    }

    public static bool IsEvenInteger(ErDouble value)
    {
        throw new NotImplementedException();
    }

    public static bool IsFinite(ErDouble value)
    {
        return Double.IsFinite(value.Value);
    }

    public static bool IsImaginaryNumber(ErDouble value)
    {
        return false;
    }

    public static bool IsInfinity(ErDouble value)
    {
        return double.IsInfinity(value.Value);
    }

    public static bool IsInteger(ErDouble value)
    {
        return false;
    }

    public static bool IsNaN(ErDouble value)
    {
        return double.IsNaN(value.Value);
    }

    public static bool IsNegative(ErDouble value)
    {
        return value.Value < 0;
    }

    public static bool IsNegativeInfinity(ErDouble value)
    {
        return double.IsNegativeInfinity(value.Value);
    }

    public static bool IsNormal(ErDouble value)
    {
        throw new NotImplementedException();
    }

    public static bool IsOddInteger(ErDouble value)
    {
        throw new NotImplementedException();
    }

    public static bool IsPositive(ErDouble value)
    {
        throw new NotImplementedException();
    }

    public static bool IsPositiveInfinity(ErDouble value)
    {
        throw new NotImplementedException();
    }

    public static bool IsRealNumber(ErDouble value)
    {
        throw new NotImplementedException();
    }

    public static bool IsSubnormal(ErDouble value)
    {
        throw new NotImplementedException();
    }

    public static bool IsZero(ErDouble value)
    {
        throw new NotImplementedException();
    }

    public static ErDouble MaxMagnitude(ErDouble x, ErDouble y)
    {
        throw new NotImplementedException();
    }

    public static ErDouble MaxMagnitudeNumber(ErDouble x, ErDouble y)
    {
        throw new NotImplementedException();
    }

    public static ErDouble MinMagnitude(ErDouble x, ErDouble y)
    {
        throw new NotImplementedException();
    }

    public static ErDouble MinMagnitudeNumber(ErDouble x, ErDouble y)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Parse

    public static ErDouble Parse(string s, IFormatProvider? provider)
    {
        return double.Parse(s, provider);
    }

    public static bool TryParse(string? s, IFormatProvider? provider, out ErDouble result)
    {
        bool success = double.TryParse(s, provider, out double resD);
        result = resD;
        return success;
    }

    public static ErDouble Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        return double.Parse(s, provider);
    }

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out ErDouble result)
    {
        bool success = double.TryParse(s, provider, out double resD);
        result = resD;
        return success;
    }
    
    public static ErDouble Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider)
    {
        return double.Parse(s, style, provider);
    }

    public static ErDouble Parse(string s, NumberStyles style, IFormatProvider? provider)
    {
        return double.Parse(s, style, provider);
    }
    
    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out ErDouble result)
    {
        bool success = double.TryParse(s,style, provider, out double resD);
        result = resD;
        return success;
    }

    public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out ErDouble result)
    {
        bool success = double.TryParse(s,style, provider, out double resD);
        result = resD;
        return success;
    }

    #endregion

    #region TryConvert

    public static bool TryConvertFromChecked<TOther>(TOther value, out ErDouble result) where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }

    public static bool TryConvertFromSaturating<TOther>(TOther value, out ErDouble result) where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }

    public static bool TryConvertFromTruncating<TOther>(TOther value, out ErDouble result) where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }

    public static bool TryConvertToChecked<TOther>(ErDouble value, out TOther result) where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }

    public static bool TryConvertToSaturating<TOther>(ErDouble value, out TOther result) where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }

    public static bool TryConvertToTruncating<TOther>(ErDouble value, out TOther result) where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }

    #endregion

    public static ErDouble AdditiveIdentity { get => Zero; }

    public static ErDouble MultiplicativeIdentity => One;
    
    public static ErDouble One => new ErDouble(1, 0);
    public static int Radix => 10;
    public static ErDouble Zero => new ErDouble(0, 0);
}