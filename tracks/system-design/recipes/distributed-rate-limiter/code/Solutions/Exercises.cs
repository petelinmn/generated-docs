// Solutions for the exercises of the recipe system-design/recipes/distributed-rate-limiter.
// Tests: dotnet test --filter "FullyQualifiedName~Solutions"
namespace RateLimiting.Solutions;

// Exercise 1: a sliding window counter estimates the last window from two fixed ones.
public sealed class SlidingWindow(int limit, TimeSpan window, TimeProvider clock)
{
    private long _index = long.MinValue;
    private int _current;
    private int _previous;

    public bool TryAcquire()
    {
        long ticks = clock.GetUtcNow().UtcTicks;
        long index = ticks / window.Ticks;
        if (index != _index)
        {
            _previous = index == _index + 1 ? _current : 0;
            _current = 0;
            _index = index;
        }

        double passed = (double)(ticks % window.Ticks) / window.Ticks;
        if (_previous * (1 - passed) + _current + 1 > limit) return false;
        _current++;
        return true;
    }
}

// Exercise 2: several limits at once, and a refusal consumes from none of them.
public sealed class AllOrNothing(params int[] budgets)
{
    private readonly int[] _left = [.. budgets];
    private readonly Lock _gate = new();

    public bool TryAcquire(int cost)
    {
        lock (_gate)
        {
            if (_left.Any(left => left < cost)) return false;
            for (int i = 0; i < _left.Length; i++) _left[i] -= cost;
            return true;
        }
    }

    public IReadOnlyList<int> Left => _left;
}

// Exercise 3: split a hot limit into shards; the parts add up to exactly the whole.
public static class Shards
{
    public static int[] Split(int limit, int shards) =>
        [.. Enumerable.Range(0, shards).Select(i => limit / shards + (i < limit % shards ? 1 : 0))];
}
