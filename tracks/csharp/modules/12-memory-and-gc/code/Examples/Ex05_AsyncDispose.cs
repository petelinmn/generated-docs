// Example: IAsyncDisposable and await using.
// Module: csharp/12-memory-and-gc (basics and advanced)
// Tests: dotnet test --filter "FullyQualifiedName~AsyncDispose"
namespace Examples;

// Implements both interfaces: await using calls DisposeAsync, a plain using calls Dispose.
public sealed class BufferedSink(List<string> log) : IAsyncDisposable, IDisposable
{
    private readonly List<string> _buffer = [];

    public void Write(string item) => _buffer.Add(item);

    // Flushing may need I/O, so the asynchronous path is the main one.
    public async ValueTask DisposeAsync()
    {
        await Task.Yield(); // stands in for an asynchronous flush
        log.Add($"flushed {_buffer.Count} async");
        _buffer.Clear();
    }

    public void Dispose()
    {
        log.Add($"flushed {_buffer.Count} sync");
        _buffer.Clear();
    }
}

public static class AsyncDisposeExamples
{
    public static async Task<List<string>> AwaitUsing()
    {
        var log = new List<string>();
        await using (var sink = new BufferedSink(log))
        {
            sink.Write("a");
            sink.Write("b");
            log.Add("body");
        }
        return log;
    }

    public static List<string> PlainUsing()
    {
        var log = new List<string>();
        using (var sink = new BufferedSink(log))
        {
            sink.Write("a");
        }
        return log;
    }
}
