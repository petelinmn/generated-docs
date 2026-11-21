// Example: a bounded blocking buffer built on Monitor.Wait / Monitor.PulseAll — what Channel<T> does for you.
// Module: csharp/18-concurrency (expert)
// Tests: dotnet test --filter "FullyQualifiedName~BlockingBuffer"
namespace Examples;

public sealed class BlockingBuffer<T>(int capacity)
{
    private readonly object _gate = new();
    private readonly Queue<T> _items = new();

    public void Add(T item)
    {
        lock (_gate)
        {
            // Always wait in a loop: after waking up, the condition may already be false again.
            while (_items.Count == capacity)
            {
                Monitor.Wait(_gate); // releases the lock while waiting, re-acquires it before returning
            }
            _items.Enqueue(item);
            Monitor.PulseAll(_gate);
        }
    }

    public T Take()
    {
        lock (_gate)
        {
            while (_items.Count == 0)
            {
                Monitor.Wait(_gate);
            }
            T item = _items.Dequeue();
            Monitor.PulseAll(_gate);
            return item;
        }
    }
}
