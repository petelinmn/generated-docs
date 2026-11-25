// Example: ConcurrentMessageLimit bounds how many messages an endpoint processes at once.
// Module: csharp/libraries/masstransit (advanced)
// Tests: dotnet test --filter "FullyQualifiedName~Concurrency"
using MassTransit;

namespace Shop;

// Counts consumers that are inside Consume; Release lets them finish.
public class ConcurrencyProbe
{
    private readonly TaskCompletionSource _gate = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private int _current;
    private int _max;

    public int Max => Volatile.Read(ref _max);

    public void Release() => _gate.TrySetResult();

    public async Task EnterAsync()
    {
        int now = Interlocked.Increment(ref _current);
        int seen;
        while (now > (seen = Volatile.Read(ref _max)) && Interlocked.CompareExchange(ref _max, now, seen) != seen)
        {
        }

        await _gate.Task;
        Interlocked.Decrement(ref _current);
    }
}

public class SlowConsumer(ConcurrencyProbe probe) : IConsumer<OrderSubmitted>
{
    public Task Consume(ConsumeContext<OrderSubmitted> context) => probe.EnterAsync();
}
