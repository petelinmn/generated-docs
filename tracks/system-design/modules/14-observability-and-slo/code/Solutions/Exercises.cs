// Solutions to the exercises of system-design/14-observability-and-slo.
// Tests: dotnet test --filter "FullyQualifiedName~Solutions"
using System.Buffers;
using System.Globalization;

namespace Observability.Solutions;

public static class Exercises
{
    private static readonly SearchValues<char> Hex = SearchValues.Create("0123456789abcdef");

    // Exercise 1: parse a version 00 traceparent header.
    public static (string TraceId, string ParentId, bool Sampled)? ParseTraceParent(string header)
    {
        string[] p = header.Split('-');
        if (p.Length != 4 || p[0] != "00") return null;
        if (!Valid(p[1], 32) || !Valid(p[2], 16) || p[3].Length != 2 || p[3].AsSpan().ContainsAnyExcept(Hex)) return null;
        if (p[1].All(c => c == '0') || p[2].All(c => c == '0')) return null;

        return (p[1], p[2], (byte.Parse(p[3], NumberStyles.HexNumber) & 1) == 1);

        static bool Valid(string s, int length) => s.Length == length && !s.AsSpan().ContainsAnyExcept(Hex);
    }

    // Exercise 2: page only when both windows burn too fast.
    public static bool ShouldPage(double longWindowBurn, double shortWindowBurn, double threshold = 14.4) =>
        longWindowBurn > threshold && shortWindowBurn > threshold;

    // Exercise 3: every service makes the same sampling decision for one trace.
    public static bool Sample(string traceId, double ratio)
    {
        ulong value = ulong.Parse(traceId.AsSpan(traceId.Length - 16), NumberStyles.HexNumber);
        return value / (double)ulong.MaxValue < ratio;
    }
}
