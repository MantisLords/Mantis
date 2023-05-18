using System.Globalization;
using System.Text;
using Mantis.Core.QuickTable;

namespace Mantis.Core.Utility;

public delegate void RefAction<T>(ref T e);

public static class TableUtility
{
    public static string[,] ListToMatrix(IEnumerable<object[]> list, int columns = -1)
    {
        int count = list.Count();
        if (count == 0)
            return new string[0,0];

        if (columns <= 0)
        {
            columns = list.First().Length;
        }

        string[,] res = new string[count, columns];

        int i = 0;
        foreach (var e in list)
        {
            for (int j = 0; j < columns; j++)
            {
                if (j < e.Length && e[j] != null)
                {
                    if (e[j] is IFormattable formattable)
                        res[i, j] = formattable.ToString("G4",CultureInfo.CurrentCulture);
                    else
                        res[i, j] = e[j].ToString();
                }
                else
                {
                    res[i, j] = "";
                }
            }

            i++;
            if (i >= count)
                break;
        }

        return res;
    }

    public static string[,] QListToMatrix<T>(IEnumerable<T> list)
    {
        var access = QuickTablePropertyAccess<T>.Instance;
        IEnumerable<object[]> arrayList = list.Select(
            e =>
                access.Fields.Select(field => field.GetValue(e)).ToArray()
            );
        return ListToMatrix(arrayList);
    }

    public static string ToStringForeach<T>(IEnumerable<T> list)
    {
        StringBuilder builder = new StringBuilder();
        foreach (var e in list)
        {
            builder.Append(e.ToString());
            builder.Append('\n');
        }

        return builder.ToString();
    }

    public static void ForEachRef<T>(this List<T> list,RefAction<T> action)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T e = list[i];
            action(ref e);
            list[i] = e;
        }
    }
}