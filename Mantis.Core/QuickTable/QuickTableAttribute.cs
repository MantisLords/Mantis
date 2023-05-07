using System;

namespace Mantis.Core.QuickTable
{
    public class QuickTableAttribute : Attribute
    {
        public readonly string Caption;
        public readonly string Label;

        public QuickTableAttribute(string caption, string label)
        {
            Caption = caption;
            Label = label;
        }
    }
}