using System.Text;
using Mantis.Core.FileManagement;

namespace Mantis.Core.FileImporting;

public abstract class RowWiseCsvReaderBase
{
    public char ColumnSeparator  = ',';

    public bool UseQuotesForEntry = true;

    public string AbsoluteFilePath = "";
    
    public RowWiseCsvReaderBase(string path)
    {
        AbsoluteFilePath = PathUtility.TryCombineAndAddExtension(FileManager.CurrentInputDir, path, "csv");
    }

    public void ReadFile()
    {
        using StreamReader reader = File.OpenText(AbsoluteFilePath);
        ParseInputStream(reader);
    }

    protected abstract void ProcessContentRow(string[] row,int index);

    private void ParseInputStream(StreamReader reader)
    {
        int rowIndex = 0;
        string[] firstRow = ParseFirstRow(reader);
        int columnCount = firstRow.Length;
        
        ProcessContentRow(firstRow,rowIndex);

        rowIndex++;

        StringBuilder builder = new StringBuilder();
        string[] rowContainer = new string[columnCount];
        
        while (!reader.EndOfStream)
        {
            ParseRow(reader, rowContainer, builder);
            
            ProcessContentRow(rowContainer,rowIndex);
        }
    }

    private string[] ParseFirstRow(StreamReader reader)
    {
        List<string> columns = new List<string>();
        
        StringBuilder builder = new StringBuilder();

        bool isInQuotes = false;

        bool isLineBreakAtEnd = false;
        
        while (!reader.EndOfStream)
        {
            char c = (char) reader.Read();

            isLineBreakAtEnd = false;
            if (UseQuotesForEntry && c == '"')
            {
                isInQuotes = !isInQuotes;
            }else if (isInQuotes)
            {
                builder.Append(c);
            }
            else if (c == '\n' || c == '\r')
            {
                if (c == '\r' && !reader.EndOfStream && (char)reader.Peek() == '\n') // Special case Windows \r\n
                    reader.Read();
                
                columns.Add(builder.ToString());
                builder.Clear();
                
                
                isLineBreakAtEnd = true;
                break;
            }else if (c == ColumnSeparator)
            {
                columns.Add(builder.ToString());
                builder.Clear();
            }
            else
            {
                builder.Append(c);
            }
        }

        if (!isLineBreakAtEnd)
        {
            columns.Add(builder.ToString());
            builder.Clear();
        }

        return columns.ToArray();
    }
    
    private void ParseRow(StreamReader reader,string[] columnContainer,StringBuilder builder)
    {
        builder.Clear();

        bool isInQuotes = false;

        bool isLineBreakAtEnd = false;

        int columnIndex = 0;
        
        while (!reader.EndOfStream)
        {
            char c = (char) reader.Read();

            isLineBreakAtEnd = false;
            if (UseQuotesForEntry && c == '"')
            {
                isInQuotes = !isInQuotes;
            }else if (isInQuotes)
            {
                builder.Append(c);
            }
            else if (c == '\n' || c == '\r')
            {
                if (c == '\r' && !reader.EndOfStream && (char)reader.Peek() == '\n') // Special case Windows \r\n
                    reader.Read();
                
                if (columnIndex < columnContainer.Length)
                    columnContainer[columnIndex] = builder.ToString();
                builder.Clear();
                
                
                isLineBreakAtEnd = true;
                break;
            }else if (c == ColumnSeparator)
            {
                if (columnIndex < columnContainer.Length)
                    columnContainer[columnIndex] = builder.ToString();
                columnIndex++;
                builder.Clear();
            }
            else
            {
                builder.Append(c);
            }
        }

        if (!isLineBreakAtEnd)
        {
            if (columnIndex < columnContainer.Length)
                columnContainer[columnIndex] = builder.ToString();
            builder.Clear();
        }
    }
}