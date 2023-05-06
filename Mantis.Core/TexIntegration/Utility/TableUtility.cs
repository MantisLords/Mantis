namespace Mantis.Core.TexIntegration.Utility;

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
                    res[i, j] = e[j].ToString();
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
}