// Example: ConcurrentDictionary — atomic updates, and why GetOrAdd may run the factory several times.
// Module: csharp/18-concurrency (basics and advanced)
// Tests: dotnet test --filter "FullyQualifiedName~ConcurrentCollections"
using System.Collections.Concurrent;

namespace Examples;

public static class ConcurrentCollections
{
    // AddOrUpdate retries the update delegate until it wins, so no increment is lost.
    public static IReadOnlyDictionary<string, int> CountWords(IEnumerable<string> words, int threads)
    {
        var counts = new ConcurrentDictionary<string, int>();
        Parallel.ForEach(words, new ParallelOptions { MaxDegreeOfParallelism = threads }, word =>
            counts.AddOrUpdate(word, 1, (_, current) => current + 1));
        return counts;
    }

    // The value factory runs outside the dictionary's lock: concurrent callers can all run it.
    // The barrier forces `callers` threads to be inside the factory at the same time.
    public static int FactoryCallsWithGetOrAdd(int callers)
    {
        var cache = new ConcurrentDictionary<string, int>();
        int calls = 0;
        using var barrier = new Barrier(callers);
        RunConcurrently(callers, () => cache.GetOrAdd("key", _ =>
        {
            Interlocked.Increment(ref calls);
            barrier.SignalAndWait();
            return 42;
        }));
        return calls;
    }

    // Wrapping the value in Lazy<T> makes only one factory run: every caller gets the same Lazy instance.
    public static int FactoryCallsWithLazy(int callers)
    {
        var cache = new ConcurrentDictionary<string, Lazy<int>>();
        int calls = 0;
        RunConcurrently(callers, () => _ = cache.GetOrAdd("key", _ => new Lazy<int>(() =>
        {
            Interlocked.Increment(ref calls);
            return 42;
        })).Value);
        return calls;
    }

    private static void RunConcurrently(int count, Action body)
    {
        var threads = Enumerable.Range(0, count).Select(_ => new Thread(() => body())).ToList();
        threads.ForEach(t => t.Start());
        threads.ForEach(t => t.Join());
    }
}
