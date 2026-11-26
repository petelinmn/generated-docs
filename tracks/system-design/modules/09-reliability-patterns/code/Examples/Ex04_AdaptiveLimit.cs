// Example: a concurrency limit that adapts: additive increase, multiplicative decrease.
// Module: system-design/09-reliability-patterns (expert)
// Tests: dotnet test --filter "FullyQualifiedName~Adaptive"
namespace Reliability.Examples;

// Not thread-safe: it shows the control rule only.
public sealed class AimdLimit(int initial, int min, int max, TimeSpan latencyTarget)
{
    private double _limit = initial;

    public int Current => (int)_limit;

    public void Record(TimeSpan latency, bool failed)
    {
        // Trouble shrinks the limit quickly; good results grow it by about one per full window.
        _limit = failed || latency > latencyTarget
            ? Math.Max(min, _limit * 0.5)
            : Math.Min(max, _limit + 1 / _limit);
    }
}
