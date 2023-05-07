using System;

namespace Mantis.Core.QuickTable
{
    public class QTableFieldAttribute : Attribute
    {
        public QTableFieldAttribute(string name, string unit = "")
        {
            Name = name;
            Unit = unit;
        }

        public readonly string Name;
        public readonly string Unit;
        public readonly string Symbol;


    }
}