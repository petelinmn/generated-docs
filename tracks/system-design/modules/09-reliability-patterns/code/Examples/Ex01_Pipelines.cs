// Example: Polly pipelines: retry, timeout and circuit breaker.
// Module: system-design/09-reliability-patterns (basics, advanced)
// Tests: dotnet test --filter "FullyQualifiedName~Pipeline"
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

namespace Reliability.Examples;

// A failure that is worth repeating. Everything else is a bug or a permanent error.
public sealed class TransientException : Exception;

public static class Pipelines
{
    public static ResiliencePipeline Retry(int maxRetries, TimeSpan delay, TimeProvider clock) =>
        new ResiliencePipelineBuilder { TimeProvider = clock }
            .AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<TransientException>(),
                MaxRetryAttempts = maxRetries, // retries, not counting the first call
                Delay = delay,
                BackoffType = DelayBackoffType.Exponential,
            })
            .Build();

    public static ResiliencePipeline Timeout(TimeSpan timeout, TimeProvider clock) =>
        new ResiliencePipelineBuilder { TimeProvider = clock }
            .AddTimeout(new TimeoutStrategyOptions { Timeout = timeout })
            .Build();

    public static ResiliencePipeline Breaker(TimeProvider clock) =>
        new ResiliencePipelineBuilder { TimeProvider = clock }
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<TransientException>(),
                FailureRatio = 0.5,
                MinimumThroughput = 4, // do not judge by one or two calls
                SamplingDuration = TimeSpan.FromSeconds(10),
                BreakDuration = TimeSpan.FromSeconds(5),
            })
            .Build();
}
