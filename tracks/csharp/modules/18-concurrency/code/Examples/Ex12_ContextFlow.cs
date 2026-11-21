// Example: AsyncLocal<T> flows with the execution context to other threads; ThreadLocal<T> stays on its thread.
// Module: csharp/18-concurrency (expert)
// Tests: dotnet test --filter "FullyQualifiedName~ContextFlow"
namespace Examples;

public static class ContextFlow
{
    public static (string? AsyncLocalValue, string? ThreadLocalValue) ReadFromNewThread()
    {
        var asyncLocal = new AsyncLocal<string>();
        using var threadLocal = new ThreadLocal<string>();
        asyncLocal.Value = "request-42";
        threadLocal.Value = "request-42";

        (string?, string?) seen = default;
        // Thread.Start captures the current ExecutionContext, and AsyncLocal values live in it.
        var thread = new Thread(() => seen = (asyncLocal.Value, threadLocal.Value));
        thread.Start();
        thread.Join();
        return seen;
    }
}
