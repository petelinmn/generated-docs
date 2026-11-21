using Examples;

namespace Tests;

// Concurrency tests assert invariants (totals, upper bounds, exceptions), never timing or a lucky interleaving.
public class Ex01_CountersTests
{
    private const int Threads = 4;
    private const int Iterations = 200_000;

    [Fact]
    public void SynchronizedCounters_AreExact()
    {
        Assert.Equal(Threads * Iterations, CounterRunner.Run(new MonitorCounter(), Threads, Iterations));
        Assert.Equal(Threads * Iterations, CounterRunner.Run(new LockCounter(), Threads, Iterations));
        Assert.Equal(Threads * Iterations, CounterRunner.Run(new InterlockedCounter(), Threads, Iterations));
    }

    [Fact]
    public void UnsafeCounter_NeverExceedsTheExpectedValue()
    {
        // Lost updates can only make the result smaller; how much smaller depends on the run.
        int result = CounterRunner.Run(new UnsafeCounter(), Threads, Iterations);
        Assert.InRange(result, 1, Threads * Iterations);
    }
}

public class Ex02_ThreadKindsTests
{
    [Fact]
    public void ThreadKinds_DifferAsDocumented()
    {
        Assert.Equal(new ThreadKind(IsThreadPoolThread: false, IsBackground: false), ThreadKinds.DedicatedThread());
        Assert.Equal(new ThreadKind(IsThreadPoolThread: true, IsBackground: true), ThreadKinds.PoolTask());
        Assert.False(ThreadKinds.LongRunningTask().IsThreadPoolThread);
    }
}

public class Ex03_ConcurrentCollectionsTests
{
    [Fact]
    public void AddOrUpdate_LosesNoUpdates()
    {
        string[] words = Enumerable.Repeat(new[] { "a", "b", "a" }, 10_000).SelectMany(w => w).ToArray();

        var counts = ConcurrentCollections.CountWords(words, threads: 8);

        Assert.Equal(20_000, counts["a"]);
        Assert.Equal(10_000, counts["b"]);
    }

    [Fact]
    public void GetOrAdd_CanRunTheFactoryForEveryCaller_LazyRunsItOnce()
    {
        Assert.Equal(8, ConcurrentCollections.FactoryCallsWithGetOrAdd(callers: 8));
        Assert.Equal(1, ConcurrentCollections.FactoryCallsWithLazy(callers: 8));
    }
}

public class Ex04_ThrottlingTests
{
    [Fact]
    public async Task Semaphore_LimitsConcurrency()
    {
        int peak = await Throttling.RunAsync(jobs: 12, maxConcurrency: 3, TimeSpan.FromMilliseconds(20));

        Assert.InRange(peak, 1, 3);
    }
}

public class Ex05_ReaderWriterCacheTests
{
    [Fact]
    public void Cache_GetOrAdd_CreatesOnce()
    {
        using var cache = new ReaderWriterCache<string, int>();
        int created = 0;

        Parallel.For(0, 100, _ => cache.GetOrAdd("key", _ => Interlocked.Increment(ref created)));

        Assert.Equal(1, created);
        Assert.True(cache.TryGet("key", out int value));
        Assert.Equal(1, value);
    }

    [Fact]
    public void WriteLockInsideReadLock_Throws()
    {
        using var cache = new ReaderWriterCache<string, int>();

        Assert.Throws<LockRecursionException>(() => cache.WriteWhileReadingWrong("key", 1));
    }
}

public class Ex06_SignalingTests
{
    [Fact]
    public void Workers_WaitForTheGate_ThenAllFinish()
    {
        using var gate = new StartGate(workers: 5);

        // Nobody can pass a closed gate, whatever the timing.
        Assert.Equal(0, gate.Passed);
        Assert.Equal(5, gate.Remaining);

        gate.Open();

        Assert.True(gate.WaitAll(TimeSpan.FromSeconds(10)));
        Assert.Equal(5, gate.Passed);
    }
}

public class Ex07_ProducerConsumerTests
{
    [Fact]
    public async Task BoundedChannel_DeliversEveryItemOnce()
    {
        long sum = await ProducerConsumer.SumAsync(items: 10_000, capacity: 16, consumers: 4);

        Assert.Equal(10_000L * 10_001 / 2, sum);
    }

    [Fact]
    public void FullChannel_RejectsTryWrite() =>
        Assert.Equal([true, true, false], ProducerConsumer.TryWriteIntoFullChannel());
}

public class Ex08_DeadlockTests
{
    [Fact]
    public void OppositeLockOrder_Deadlocks_TryEnterTimesOut() =>
        Assert.Equal((false, false), DeadlockDemo.OppositeOrder(TimeSpan.FromMilliseconds(200)));

    [Fact]
    public void ConsistentLockOrder_TransfersSafely()
    {
        var a = new Account(1, 1000m);
        var b = new Account(2, 1000m);

        Parallel.For(0, 10_000, i =>
        {
            if (i % 2 == 0) Account.Transfer(a, b, 1m);
            else Account.Transfer(b, a, 1m);
        });

        Assert.Equal(2000m, a.Balance + b.Balance);
        Assert.Equal(1000m, a.Balance);
    }
}

public class Ex09_BlockingBufferTests
{
    [Fact]
    public void ProducersAndConsumers_ExchangeAllItems()
    {
        var buffer = new BlockingBuffer<int>(capacity: 4);
        long sum = 0;

        var producer = new Thread(() => { for (int i = 1; i <= 1000; i++) buffer.Add(i); });
        var consumer = new Thread(() => { for (int i = 0; i < 1000; i++) sum += buffer.Take(); });
        producer.Start(); consumer.Start();
        producer.Join(); consumer.Join();

        Assert.Equal(1000L * 1001 / 2, sum);
    }
}

public class Ex10_LockFreeStackTests
{
    [Fact]
    public void ConcurrentPushes_AreAllKept()
    {
        var stack = new LockFreeStack<int>();

        Parallel.For(0, 10_000, stack.Push);

        int count = 0;
        while (stack.TryPop(out _))
        {
            count++;
        }
        Assert.Equal(10_000, count);
    }
}

public class Ex11_SpinLockPitfallTests
{
    [Fact]
    public void SpinLockInReadonlyField_ExitsACopyThatWasNeverEntered() =>
        Assert.Throws<SynchronizationLockException>(() => new SpinLockInReadonlyField().EnterAndExit());

    [Fact]
    public void SpinLockInMutableField_Works()
    {
        var counter = new SpinLockInMutableField();

        Parallel.For(0, 100_000, _ => counter.Increment());

        Assert.Equal(100_000, counter.Value);
    }
}

public class Ex12_ContextFlowTests
{
    [Fact]
    public void AsyncLocalFlowsToANewThread_ThreadLocalDoesNot() =>
        Assert.Equal(("request-42", null), ContextFlow.ReadFromNewThread());
}
