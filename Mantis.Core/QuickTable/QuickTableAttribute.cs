using System;

namespace Mantis.Core.QuickTable
{
    public class QTableAttribute : Attribute
    {
        public readonly string Caption;
        public readonly string Label;

        public QTableAttribute(string caption, string label)
        {
            Caption = caption;
            Label = label;
        }
    }
}