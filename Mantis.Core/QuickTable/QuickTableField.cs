using System;

namespace Mantis.Core.QuickTable
{
    public class QuickTableField : Attribute
    {
        public QuickTableField(string name, string unit = "")
        {
            Name = name;
            Unit = unit;
        }

        public readonly string Name;
        public readonly string Unit;
        public readonly string Symbol = "";


    }
}