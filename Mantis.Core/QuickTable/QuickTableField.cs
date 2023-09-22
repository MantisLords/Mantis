using System;

namespace Mantis.Core.QuickTable
{
    public class QuickTableField : Attribute
    {
        public QuickTableField(string name, string unit = "",string symbol = "",bool doesImport = true,double lastDigitError = 0)
        {
            Name = name;
            Unit = unit;
            Symbol = symbol;
            DoesImport = doesImport;
            LastDigitError = lastDigitError;
        }

        public readonly string Name;
        public readonly string Unit;
        public readonly string Symbol = "";
        public readonly bool DoesImport = true;
        public readonly double LastDigitError = 0;

        public string? GetImportHeaderStartPattern()
        {
            return !DoesImport ? null : Name;
        }

        public string GetExportHeader()
        {
            string res = "";
            if (!string.IsNullOrEmpty(Name))
                res += Name + " ";
            if (!string.IsNullOrEmpty(Symbol))
                res += "$" + Symbol + "$ ";
            if (!string.IsNullOrEmpty(Unit))
                res += "in $" + Unit + "$";
            
            return res;
        }


    }
}