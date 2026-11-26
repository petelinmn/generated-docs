// Example: short codes for a URL shortener: base62 and a time-based id generator.
// Module: system-design/17-design-case-studies (basics, advanced)
// Tests: dotnet test --filter "FullyQualifiedName~UrlShortener"
namespace CaseStudies.Examples;

public static class Base62
{
    private const string Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

    public static string Encode(long value)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value);
        Span<char> buffer = stackalloc char[11]; // enough for long.MaxValue
        int i = buffer.Length;
        do
        {
            buffer[--i] = Alphabet[(int)(value % 62)];
            value /= 62;
        }
        while (value > 0);
        return new string(buffer[i..]);
    }
}

// 41 bits of milliseconds, 10 bits of node, 12 bits of sequence.
public sealed class IdGenerator(int node, TimeProvider clock)
{
    private static readonly long Epoch = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero).ToUnixTimeMilliseconds();
    private readonly Lock _gate = new();
    private long _lastMs = -1;
    private int _sequence;

    private readonly int _node = node is >= 0 and < 1024 ? node : throw new ArgumentOutOfRangeException(nameof(node));

    public long Next()
    {
        lock (_gate)
        {
            long ms = clock.GetUtcNow().ToUnixTimeMilliseconds() - Epoch;
            // Refusing is safer than issuing an id that may already exist.
            if (ms < _lastMs) throw new InvalidOperationException("The clock moved backwards.");

            _sequence = ms == _lastMs ? _sequence + 1 : 0;
            if (_sequence > 4095) throw new InvalidOperationException("Sequence exhausted for this millisecond.");
            _lastMs = ms;

            return (ms << 22) | ((long)_node << 12) | (uint)_sequence;
        }
    }
}
