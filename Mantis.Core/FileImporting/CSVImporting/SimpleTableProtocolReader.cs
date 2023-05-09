using System.Numerics;
using System.Reflection;
using Mantis.Core.Math.BasicTypes;
using Mantis.Core.QuickTable;

namespace Mantis.Core.FileImporting;

public delegate void FieldParsingHandle<T>(ref T instance, string fieldValue);

public class SimpleTableProtocolReader : BasicCSVReader
{
    public static readonly string TABLE_MARKER = "# ";
    public static readonly string SINGLE_VALUE_MARKER = "* ";
    
    public List<T> ExtractTable<T>(string? label = null)
    {
        QuickTablePropertyAccess<T> access = QuickTablePropertyAccess<T>.Instance;
        if (string.IsNullOrEmpty(label))
            label = access.TableData.Label;

        if (!TryGetRowWhichStartsWith(TABLE_MARKER + label, 0, out int rowHeadingIndex))
        {
            throw new ArgumentException(
                $"There was no field found in the first column which starts with {TABLE_MARKER + label}");
        }

        int maxEndOfTable = GetRowIndexOfNextMarkedField(rowHeadingIndex+1);
        if (maxEndOfTable - rowHeadingIndex <= 2)
            throw new ArgumentException("Your table needs a header and at least one row");

        ParseHeadersAndGetHandel(rowHeadingIndex+1,access,
            out int columnCount,out FieldParsingHandle<T>[] dataFieldParsingHandle);

        List<T> result = ParseListContent(rowHeadingIndex + 2, access, maxEndOfTable, columnCount, dataFieldParsingHandle);

        return result;
    }

    public T ExtractSingleValue<T>(string name) where T : INumber<T>
    {
        if (!TryGetRowWhichStartsWith(SINGLE_VALUE_MARKER + name, 0, out int row))
            throw new Exception(
                $"There was no field found int the first column which starts with {SINGLE_VALUE_MARKER + name}");

        if (ColumnCount < 2)
            throw new Exception($"Your table needs at least two columns");
        if (string.IsNullOrEmpty(DataMatrix[row, 1]))
            throw new Exception($"You need to insert the value right next to the name. Here at [{row},{1}]");
        T number = T.Parse(DataMatrix[row, 1], null);

        if (number is ErDouble erNumber && ColumnCount >= 3 && !string.IsNullOrEmpty(DataMatrix[row,2]))
        {
            erNumber.Error = double.Parse(DataMatrix[row, 2]);
        }

        return number;
    }

    private void ParseHeadersAndGetHandel<T>(int headerRowIndex, QuickTablePropertyAccess<T> access,
        out int columnCount,
        out FieldParsingHandle<T>[] dataFieldParsingHandle)
    {
        string[] propertyHeaders = access.GetHeader();
        columnCount = GetColumnOrEnd(string.IsNullOrEmpty, headerRowIndex);
        dataFieldParsingHandle = new FieldParsingHandle<T>[columnCount];
        
        for (int column = 0; column < columnCount; column++)
        {
            string headerField = DataMatrix[headerRowIndex, column];

            for (int i = 0; i < propertyHeaders.Length; i++)
            {
                if (headerField.StartsWith(propertyHeaders[i]))
                {
                    var i1 = i;
                    dataFieldParsingHandle[column] = (ref T instance,string fieldValue) => access.Fields[i1].ParseValueT<T>(ref instance,fieldValue);
                }
            }
        }
    }

    private List<T> ParseListContent<T>(int firstRowIndex, QuickTablePropertyAccess<T> access,
        int maxRowIndex,
        int columnCount,
        FieldParsingHandle<T>[] dataFieldParsingHandle)
    {
        List<T> result = new List<T>();
        for (int row = firstRowIndex; row < maxRowIndex; row++)
        {
            if (IsRowEmpty(row, columnCount))
                break;

            T currentEntry = access.GetNewInstance();

            for (int column = 0; column < columnCount; column++)
            {
                
                dataFieldParsingHandle[column].Invoke(ref currentEntry,DataMatrix[row,column]);
            }
            
            result.Add(currentEntry);
        }

        return result;
    }

    private bool IsRowEmpty(int rowIndex, int columnCount)
    {
        for (int column = 0; column < columnCount; column++)
        {
            if (!string.IsNullOrEmpty(DataMatrix[rowIndex, column]))
            {
                return false;
            }
        }

        return true;
    }
    

    private bool TryGetRowWhichStartsWith(string start, int column,out int foundRow)
    {
        return TryGetRowWhich(s => s.StartsWith(start), column, out foundRow);
    }
    
    private bool TryGetRowWhich(Func<string,bool> selectRow, int column,out int foundRow,int startingRow = 0)
    {
        for (int row = startingRow; row < RowCount; row++)
        {
            if (selectRow(DataMatrix[row, column]))
            {
                foundRow = row;
                return true;
            }
        }

        foundRow = 0;
        return false;
    }

    private int GetColumnOrEnd(Func<string, bool> selectColumn, int row, int startingColumn = 0)
    {
        if (TryGetColumnWhich(selectColumn, row, out int foundColumn, startingColumn))
            return foundColumn;
        else return ColumnCount;
    }
    
    private bool TryGetColumnWhich(Func<string,bool> selectColumn, int row,out int foundColumn,int startingColumn = 0)
    {
        for (int column = startingColumn; column < ColumnCount; column++)
        {
            if (selectColumn(DataMatrix[row, column]))
            {
                foundColumn = column;
                return true;
            }
        }

        foundColumn = 0;
        return false;
    }

    private int GetRowIndexOfNextMarkedField(int startingRowIndex)
    {
        int rowIndexOfNextMarkedField = 0;
        if (!TryGetRowWhich(s => s.StartsWith(TABLE_MARKER) || s.StartsWith(SINGLE_VALUE_MARKER), 0,
                out rowIndexOfNextMarkedField,startingRowIndex))
            rowIndexOfNextMarkedField = RowCount;
        return rowIndexOfNextMarkedField;
    }
    
    
}