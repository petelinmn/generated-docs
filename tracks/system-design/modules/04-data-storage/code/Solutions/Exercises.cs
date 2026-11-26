// Solutions to the exercises of system-design/04-data-storage.
// Tests: dotnet test --filter "FullyQualifiedName~Solutions"
using System.Globalization;

namespace DataStorage.Solutions;

public static class Exercises
{
    // Exercise 1: a key whose ordinal order is "by chat, newest first".
    public static string MessageKey(long chatId, long sentAtUnixMs) =>
        string.Create(CultureInfo.InvariantCulture, $"{chatId:D19}#{long.MaxValue - sentAtUnixMs:D19}");

    // Exercise 2: merge two segments; the newer one wins, tombstones (null) are kept on request.
    public static SortedDictionary<string, string?> Merge(
        IReadOnlyDictionary<string, string?> older,
        IReadOnlyDictionary<string, string?> newer,
        bool dropTombstones)
    {
        var result = new SortedDictionary<string, string?>(StringComparer.Ordinal);
        foreach (var (key, value) in older) result[key] = value;
        foreach (var (key, value) in newer) result[key] = value;

        if (dropTombstones)
            foreach (string key in result.Where(e => e.Value is null).Select(e => e.Key).ToList())
                result.Remove(key);

        return result;
    }

    // Exercise 3: bytes written to disk per byte of user data.
    public static double WriteAmplification(long userBytes, params long[] diskWrites) =>
        (double)diskWrites.Sum() / userBytes;
}
