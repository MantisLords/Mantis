using System.Text;
using Mantis.Core.FileManagement;

namespace Mantis.Core.FileImporting;

public class BasicCSVImporter
{
    public string[,]? DataMatrix { get; private set; }

    public char ColumnSeparator  = ',';

    public bool UseQuotesForEntry = true;

    public void ReadFile(string path)
    {
        path = PathUtility.TryCombineAndAddExtension(FileManager.CurrentInputDir, path, "csv");

        using StreamReader reader = File.OpenText(path);
        var (rawData, rowCount, columnCount) = ParseInputStream(reader);
        
        PopulateMatrixWithRawData(rawData,rowCount,columnCount);
    }

    private (List<List<string>>,int,int) ParseInputStream(StreamReader reader)
    {
        List<List<string>> rawData = new List<List<string>>();
        rawData.Add(new List<string>());
        int maxColumnIndex = 0;
        int columnIndex = 0;
        int rowIndex = 0;
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
                
                columnIndex++;
                rawData[rowIndex].Add(builder.ToString());
                builder.Clear();
                   
                maxColumnIndex = Math.Max(maxColumnIndex, columnIndex);
                    
                rawData.Add(new List<string>());
                rowIndex++;
                columnIndex = 0;
                isLineBreakAtEnd = true;
            }else if (c == ColumnSeparator)
            {
                columnIndex++;
                rawData[rowIndex].Add(builder.ToString());
                builder.Clear();
            }
            else
            {
                builder.Append(c);
            }
        }

        if (!isLineBreakAtEnd)
        {
            columnIndex++;
            rawData[rowIndex].Add(builder.ToString());
            builder.Clear();
                   
            maxColumnIndex = Math.Max(maxColumnIndex, columnIndex);
                    
            rawData.Add(new List<string>());
            rowIndex++;
        }

        return (rawData, rowIndex, maxColumnIndex);
    }

    private void PopulateMatrixWithRawData(List<List<string>> rawData, int rowCount, int columnCount)
    {
        DataMatrix = new string[rowCount, columnCount];
        for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
        {
            for (int columnIndex = 0; columnIndex < columnCount ; columnIndex++)
            {
                DataMatrix[rowIndex, columnIndex] = columnIndex < rawData[rowIndex].Count ? rawData[rowIndex][columnIndex] : "";
            }
        }
    }


}