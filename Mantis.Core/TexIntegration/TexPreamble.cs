using System.Globalization;
using System.Numerics;
using System.Text;
using Mantis.Core.Utility;

namespace Mantis.Core.TexIntegration;

public static class TexPreamble
{
    public static TexWriter PreambleWriter = new TexWriter();

    public static List<string> Packages = new List<string>()
    {
    };

    public static void AddCommand(this string content,string label,int arguments = 0,bool makeLabelValid = true)
    {
        if (!IsValidCommandLabel(label, out char invalidChar))
        {
            if(!makeLabelValid)
                throw new ArgumentException(
                $"The tex command label '{label}' is invalid. The character '{invalidChar}' is invalid");
            else
            {
                label = MakeLabelValid(label);
            }
        }
        
        
        string argString = arguments is > 0 and <= 9 ? "[" + arguments + ']' : "";
        PreambleWriter.Builder.AppendCommand($"newcommand{{\\{label}}}{argString}{{{content}}}");
    }
    
    public static void AddCommandAndLog(this string content,string label)
    {
        content.AddCommand(label);
        Console.WriteLine($"{label} = {content}");
    }

    public static void AddCommand<T>(this INumber<T> number, string label, string unit = "",bool makeLabelValid = true) where T : INumber<T>
    {
        string res = number.ToString("G4",CultureInfo.InvariantCulture);
        if (!string.IsNullOrEmpty(unit))
            res += "\\, \\mathrm{" + unit + "}";
        AddCommand(res,label,makeLabelValid:makeLabelValid);
    }
    
    public static void AddCommandAndLog<T>(this INumber<T> number, string label, string unit = "",bool makeLabelValid = true) where T : INumber<T>
    {
        number.AddCommand(label,unit,makeLabelValid);
        string res = label + " = " + number.ToString("g4",CultureInfo.InvariantCulture);
        if (!string.IsNullOrEmpty(unit))
            res += " " + unit;
        Console.WriteLine(res);
    }

    public static void GeneratePreamble()
    {
        StringBuilder packageBuilder = new StringBuilder();
        packageBuilder.AppendLine();
        
        packageBuilder.AppendCommand($"newcommand{{\\degree}}{{^\\circ}}");

        foreach (string package in Packages)
        {
            packageBuilder.AppendCommand($"usepackage{{{package}}}");
        }
        packageBuilder.AppendLine();

        PreambleWriter.Builder.Insert(0, packageBuilder.ToString());
        
        PreambleWriter.Save("Preamble");

        PreambleWriter.Clear();
    }

    private static void ValidateCommandLabel(string label)
    {
        if (!IsValidCommandLabel(label, out char invalidChar))
        {
            throw new ArgumentException(
                $"The tex command label '{label}' is invalid. The character '{invalidChar}' is invalid");
        }
    }

    private static string[] NumbersAsWords = new[]
        {"Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine"};
    
    private static string MakeLabelValid(string label)
    {
        StringBuilder builder = new StringBuilder();
        foreach (char c in label)
        {
            if (char.IsLetter(c))
                builder.Append(c);
            else if (char.IsDigit(c))
            {
                int i = (int) c - 48;
                if(i > 0 && i < NumbersAsWords.Length)
                    builder.Append(NumbersAsWords[i]);
            }
        }

        return builder.ToString();
    }

    private static bool IsValidCommandLabel(string label,out char invalidChar)
    {
        foreach (char c in label)
        {
            if (!(char.IsLetter(c)))
            {
                invalidChar = c;
                return false;
            }
        }

        invalidChar = (char)0;
        return true;
    }
}