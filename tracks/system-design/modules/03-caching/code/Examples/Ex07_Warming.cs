// Example: warming a cache with bounded concurrency, so the warm-up does not overload the source.
// Module: system-design/03-caching (expert)
// Tests: dotnet test --filter "FullyQualifiedName~Warming"
using System.Collections.Concurrent;

namespace CachingDesign.Examples;

public static class Warming
{
    // Returns the keys that failed: a warm-up must not stop the application from starting.
    public static async Task<IReadOnlyCollection<string>> WarmAsync(
        IEnumerable<string> keys, Func<string, CancellationToken, ValueTask> load,
        int maxConcurrency, CancellationToken ct = default)
    {
        var failed = new ConcurrentBag<string>();
        await Parallel.ForEachAsync(
            keys,
            new ParallelOptions { MaxDegreeOfParallelism = maxConcurrency, CancellationToken = ct },
            async (key, token) =>
            {
                try { await load(key, token); }
                catch (Exception e) when (e is not OperationCanceledException) { failed.Add(key); }
            });
        return failed;
    }
}
