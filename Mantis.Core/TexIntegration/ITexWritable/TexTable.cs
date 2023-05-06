using System.Text;

namespace Mantis.Core.TexIntegration;

public class TexTable : ITexWritable,ILabel,ICaption
{

    public string[] Header;
    public string[,] Content;
    
    public enum TableOrientation{Vertical,Horizontal}

    public TexTable(string[] header, string[,] content, string label, string caption)
    {
        Header = header;
        Content = content;
        Label = label;
        Caption = caption;
    }

    public TexTable(string[] header, IEnumerable<object[]> content, string label, string caption) :
        this(header,TableUtility.ListToMatrix(content,header.Length),label,caption){}

    public TableOrientation Orientation { get; set; } 
    
    public string Label { get; set; }
    
    public string Caption { get; set; }

    private int GetLength(int dim)
    {
        if (dim == 0)
            return Header == null ? Content.GetLength(0) : Content.GetLength(0) + 1;
        else
            return Content.GetLength(1);
    }

    private int GetLengthOriented(int dim)
    {
        if (Orientation == TableOrientation.Vertical)
            return GetLength(dim);
        
        if (dim == 0) return GetLength(1); 
        return GetLength(0);
    }

    private string GetElementAt(int row, int column)
    {
        if (Header != null)
        {
            if (row == 0)
                return Header[column];
            row -= 1;
        }

        return Content[row, column];
    }

    private string GetElementAtOriented(int row, int column)
    {
        if (Orientation == TableOrientation.Vertical)
            return GetElementAt(row, column);
        else
        {
            return GetElementAt(column, row);
        }
    }

    public void AppendToTex(StringBuilder builder)
    {
        builder.AppendCommand("begin{table}[h!]");
        builder.AppendCommand("centering");
        
        AppendTableContents(builder);
                
        builder.AppendLabel(this);
        builder.AppendCaption(this);
        
        builder.AppendEnd("table");
        
    }

    private void AppendTableContents(StringBuilder builder)
    {
        builder.AppendBegin("tabular");
        
        GetRowFormat(builder);
        
        builder.AppendCommand("hline");
        for (int i = 0; i < GetLengthOriented(0); i++)
        {
            GetRow(i,builder);
            builder.AppendCommand("hline");
        }
        
        builder.AppendEnd("tabular");
    }

    private void GetRowFormat(StringBuilder builder)
    {
        builder.Append('{');
        builder.Append('|');
        for (int i = 0; i < GetLengthOriented(1); i++)
        {
            builder.Append(" c |");
        }

        builder.Append("}\n");
    }

    private void GetRow(int row, StringBuilder builder)
    {
        for (int i = 0; i < GetLengthOriented(1); i++)
        {
            if (i > 0)
                builder.Append('&');

            builder.Append(' ');
            builder.Append(GetElementAtOriented(row, i));
            builder.Append(' ');
        }

        builder.Append("\\\\ \n");
    }
}