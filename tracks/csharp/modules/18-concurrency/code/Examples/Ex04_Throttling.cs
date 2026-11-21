// Example: SemaphoreSlim limits how many operations run at the same time, and works with await.
// Module: csharp/18-concurrency (advanced)
// Tests: dotnet test --filter "FullyQualifiedName~Throttling"
namespace Examples;

public static class Throttling
{
    // Runs all jobs, at most `maxConcurrency` at a time; returns the peak concurrency actually observed.
    public static async Task<int> RunAsync(int jobs, int maxConcurrency, TimeSpan jobDuration)
    {
        using var semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
        int running = 0;
        int peak = 0;

        async Task RunJobAsync()
        {
            await semaphore.WaitAsync(); // unlike lock, waiting does not block a thread
            try
            {
                int now = Interlocked.Increment(ref running);
                InterlockedMax(ref peak, now);
                await Task.Delay(jobDuration);
            }
            finally
            {
                Interlocked.Decrement(ref running);
                semaphore.Release();
            }
        }

        await Task.WhenAll(Enumerable.Range(0, jobs).Select(_ => RunJobAsync()));
        return peak;
    }

    private static void InterlockedMax(ref int location, int value)
    {
        int current = Volatile.Read(ref location);
        while (value > current)
        {
            int seen = Interlocked.CompareExchange(ref location, value, current);
            if (seen == current)
            {
                return;
            }
            current = seen;
        }
    }
}
