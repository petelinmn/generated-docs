// Solutions to the exercises of system-design/08-messaging-and-events.
// Tests: dotnet test --filter "FullyQualifiedName~Solutions"
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace Messaging.Solutions;

public static class Exercises
{
    // Exercise 1: messages with the same key always go to the same partition.
    public static int PartitionFor(string key, int partitions)
    {
        uint hash = BinaryPrimitives.ReadUInt32BigEndian(SHA256.HashData(Encoding.UTF8.GetBytes(key)));
        return (int)(hash % (uint)partitions);
    }
}

// Exercise 2: remember only the last `window` ids.
public sealed class Deduplicator(int window)
{
    private readonly HashSet<string> _seen = [];
    private readonly Queue<string> _order = new();

    public bool IsNew(string id)
    {
        if (!_seen.Add(id)) return false;

        _order.Enqueue(id);
        if (_order.Count > window) _seen.Remove(_order.Dequeue());
        return true;
    }
}

// Exercise 3: release messages in sequence order, whatever order they arrive in.
public sealed class Resequencer<T>
{
    private readonly SortedDictionary<long, T> _pending = [];
    private long _next = 1;

    public IReadOnlyList<T> Add(long sequence, T item)
    {
        var ready = new List<T>();
        if (sequence >= _next) _pending[sequence] = item; // older ones are duplicates

        while (_pending.Remove(_next, out T? value))
        {
            ready.Add(value);
            _next++;
        }
        return ready;
    }
}
