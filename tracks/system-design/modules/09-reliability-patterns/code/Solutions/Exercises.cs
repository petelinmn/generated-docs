// Solutions to the exercises of system-design/09-reliability-patterns.
// Tests: dotnet test --filter "FullyQualifiedName~Solutions"
namespace Reliability.Solutions;

public static class Exercises
{
    // Exercise 1: exponential backoff with full jitter: a random delay up to the exponential cap.
    public static TimeSpan Backoff(int retry, TimeSpan baseDelay, TimeSpan maxDelay, Random random)
    {
        double cap = Math.Min(maxDelay.TotalMilliseconds, baseDelay.TotalMilliseconds * Math.Pow(2, retry));
        return TimeSpan.FromMilliseconds(random.NextDouble() * cap);
    }

    // Exercise 2: the time left for the next call, or null when it is not worth starting.
    public static TimeSpan? Remaining(DateTimeOffset deadline, DateTimeOffset now, TimeSpan minimumUseful)
    {
        TimeSpan left = deadline - now;
        return left >= minimumUseful ? left : null;
    }
}

// Exercise 3: retries are allowed only as a share of ordinary requests.
public sealed class RetryBudget(double ratio, double maxTokens)
{
    private double _tokens;
    private readonly Lock _gate = new();

    public void OnRequest()
    {
        lock (_gate) _tokens = Math.Min(maxTokens, _tokens + ratio);
    }

    public bool TryRetry()
    {
        lock (_gate)
        {
            if (_tokens < 1 - 1e-9) return false; // tolerance: 0.1 added ten times is below 1
            _tokens -= 1;
            return true;
        }
    }
}
