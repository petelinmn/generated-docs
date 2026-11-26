// Example: row layout versus column layout, and run-length encoding of a column.
// Module: system-design/04-data-storage (expert)
// Tests: dotnet test --filter "FullyQualifiedName~Columnar"
namespace DataStorage.Examples;

public static class Columnar
{
    // Values that must be read to sum one column.
    public static long ValuesRead(int rows, int columns, bool columnLayout) =>
        columnLayout ? rows : (long)rows * columns;

    // Works well on a sorted or low-cardinality column.
    public static List<(string Value, int Count)> RunLengthEncode(IEnumerable<string> column)
    {
        var runs = new List<(string Value, int Count)>();
        foreach (string value in column)
            if (runs.Count > 0 && runs[^1].Value == value) runs[^1] = (value, runs[^1].Count + 1);
            else runs.Add((value, 1));
        return runs;
    }
}
