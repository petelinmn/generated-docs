// Example: a bounded channel: wait (backpressure) or drop when the consumer is slow.
// Module: system-design/08-messaging-and-events (basics, advanced)
// Tests: dotnet test --filter "FullyQualifiedName~Backpressure"
using System.Threading.Channels;

namespace Messaging.Examples;

public static class Backpressure
{
    public static Channel<int> Create(int capacity, BoundedChannelFullMode mode) =>
        Channel.CreateBounded<int>(new BoundedChannelOptions(capacity) { FullMode = mode });

    // Writes 1..count without a consumer and reports what was kept.
    public static (int Accepted, int[] Buffered) Overflow(BoundedChannelFullMode mode, int capacity, int count)
    {
        Channel<int> channel = Create(capacity, mode);
        int accepted = Enumerable.Range(1, count).Count(i => channel.Writer.TryWrite(i));

        var buffered = new List<int>();
        while (channel.Reader.TryRead(out int item)) buffered.Add(item);
        return (accepted, [.. buffered]);
    }
}
