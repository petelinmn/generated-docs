// Example: in-process retries, then Fault<T> when they are exhausted.
// Module: csharp/libraries/masstransit (advanced)
// Tests: dotnet test --filter "FullyQualifiedName~RetryAndFault"
using System.Collections.Concurrent;
using MassTransit;

namespace Shop;

// Records what the consumers saw, so a test can assert on it.
public class AttemptLog
{
    public ConcurrentQueue<int> RetryAttempts { get; } = new();
}

public class FlakyConsumer(AttemptLog log) : IConsumer<OrderSubmitted>
{
    public Task Consume(ConsumeContext<OrderSubmitted> context)
    {
        log.RetryAttempts.Enqueue(context.GetRetryAttempt());
        if (log.RetryAttempts.Count < 3)
            throw new InvalidOperationException("transient failure");
        return Task.CompletedTask;
    }
}

public class AlwaysFailsConsumer(AttemptLog log) : IConsumer<OrderSubmitted>
{
    public Task Consume(ConsumeContext<OrderSubmitted> context)
    {
        log.RetryAttempts.Enqueue(context.GetRetryAttempt());
        throw new InvalidOperationException("boom");
    }
}

public class FaultObserver : IConsumer<Fault<OrderSubmitted>>
{
    public Task Consume(ConsumeContext<Fault<OrderSubmitted>> context)
    {
        var error = context.Message.Exceptions[0];
        Console.WriteLine($"Fault<OrderSubmitted>: {error.ExceptionType} : {error.Message}");
        return Task.CompletedTask;
    }
}
