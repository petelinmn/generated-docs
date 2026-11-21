// Solutions to the exercises of module csharp/12-memory-and-gc.
// Tests: dotnet test --filter "FullyQualifiedName~Tests.S0"
using System.Reflection;

namespace Solutions;

// Exercise 1: a temporary file that deletes itself on Dispose.
public sealed class TempFile : IDisposable
{
    private bool _disposed;

    public TempFile(string content)
    {
        Path = System.IO.Path.GetTempFileName();
        File.WriteAllText(Path, content);
    }

    public string Path { get; }

    public string Read()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return File.ReadAllText(Path);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;
        File.Delete(Path); // does not throw if the file is already gone
    }
}

// Exercise 2: a container that disposes everything it holds — in reverse order, even if some Dispose throws.
public sealed class CompositeDisposable : IDisposable
{
    private readonly List<IDisposable> _items = [];
    private bool _disposed;

    public void Add(IDisposable item)
    {
        if (_disposed)
        {
            item.Dispose(); // the owner is gone: release the newcomer right away
            return;
        }
        _items.Add(item);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;

        List<Exception>? errors = null;
        for (int i = _items.Count - 1; i >= 0; i--)
        {
            try
            {
                _items[i].Dispose();
            }
            catch (Exception e)
            {
                (errors ??= []).Add(e);
            }
        }
        _items.Clear();
        if (errors is not null)
        {
            throw new AggregateException(errors);
        }
    }
}

// Exercise 3: subscribing returns a token; disposing the token unsubscribes.
public sealed class MessageHub
{
    private readonly List<Action<string>> _handlers = [];

    public int Count => _handlers.Count;

    public IDisposable Subscribe(Action<string> handler)
    {
        _handlers.Add(handler);
        return new Subscription(this, handler);
    }

    public void Publish(string message)
    {
        foreach (Action<string> handler in _handlers.ToArray())
        {
            handler(message);
        }
    }

    private sealed class Subscription(MessageHub hub, Action<string> handler) : IDisposable
    {
        private MessageHub? _hub = hub;

        public void Dispose()
        {
            _hub?._handlers.Remove(handler);
            _hub = null; // idempotent, and the token no longer keeps the hub alive
        }
    }
}

// Exercise 4: an event that does not keep its subscribers alive.
public sealed class WeakEvent<TArgs>
{
    private readonly List<(WeakReference<object>? Target, MethodInfo Method, Action<TArgs>? Static)> _handlers = [];

    public int Count => _handlers.Count;

    public void Subscribe(Action<TArgs> handler)
    {
        if (handler.Target is null)
        {
            _handlers.Add((null, handler.Method, handler)); // static methods have no target to lose
        }
        else
        {
            // Keep only a weak reference to the target, never the delegate itself:
            // the delegate references the target strongly.
            _handlers.Add((new WeakReference<object>(handler.Target), handler.Method, null));
        }
    }

    public void Raise(TArgs args)
    {
        foreach (var (target, method, staticHandler) in _handlers.ToArray())
        {
            if (staticHandler is not null)
            {
                staticHandler(args);
            }
            else if (target!.TryGetTarget(out object? instance))
            {
                method.Invoke(instance, [args]);
            }
        }
        // Subscribers that were collected: clean up their entries.
        _handlers.RemoveAll(h => h.Target is not null && !h.Target.TryGetTarget(out _));
    }
}
