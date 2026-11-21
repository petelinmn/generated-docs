// Example: a dedicated Thread, a thread-pool task and a LongRunning task.
// Module: csharp/18-concurrency (basics)
// Tests: dotnet test --filter "FullyQualifiedName~ThreadKinds"
namespace Examples;

public readonly record struct ThreadKind(bool IsThreadPoolThread, bool IsBackground);

public static class ThreadKinds
{
    private static ThreadKind Current() =>
        new(Thread.CurrentThread.IsThreadPoolThread, Thread.CurrentThread.IsBackground);

    // A new Thread is a foreground thread by default: the process waits for it before exiting.
    public static ThreadKind DedicatedThread()
    {
        ThreadKind result = default;
        var thread = new Thread(() => result = Current());
        thread.Start();
        thread.Join();
        return result;
    }

    // Task.Run queues work to the thread pool; pool threads are background threads.
    public static ThreadKind PoolTask() => Task.Run(Current).GetAwaiter().GetResult();

    // LongRunning tells the default scheduler not to occupy a pool thread with a long blocking job.
    public static ThreadKind LongRunningTask() =>
        Task.Factory.StartNew(Current, TaskCreationOptions.LongRunning).GetAwaiter().GetResult();
}
