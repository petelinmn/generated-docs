// Example: producer/consumer with a bounded Channel<T> — the producer waits when the buffer is full.
// Module: csharp/18-concurrency (advanced)
// Tests: dotnet test --filter "FullyQualifiedName~ProducerConsumer"
using System.Threading.Channels;

namespace Examples;

public static class ProducerConsumer
{
    public static async Task<long> SumAsync(int items, int capacity, int consumers)
    {
        Channel<int> channel = Channel.CreateBounded<int>(capacity);

        Task producer = Task.Run(async () =>
        {
            for (int i = 1; i <= items; i++)
            {
                await channel.Writer.WriteAsync(i); // waits while the channel is full
            }
            channel.Writer.Complete(); // lets ReadAllAsync finish
        });

        Task<long>[] readers = Enumerable.Range(0, consumers).Select(_ => Task.Run(async () =>
        {
            long sum = 0;
            await foreach (int item in channel.Reader.ReadAllAsync())
            {
                sum += item;
            }
            return sum;
        })).ToArray();

        await producer;
        long[] partial = await Task.WhenAll(readers);
        return partial.Sum();
    }

    // The default full mode is Wait: TryWrite fails instead of dropping items.
    public static bool[] TryWriteIntoFullChannel()
    {
        Channel<int> channel = Channel.CreateBounded<int>(2);
        return [channel.Writer.TryWrite(1), channel.Writer.TryWrite(2), channel.Writer.TryWrite(3)];
    }
}
