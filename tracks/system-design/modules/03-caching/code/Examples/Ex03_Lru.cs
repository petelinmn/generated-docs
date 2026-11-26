// Example: LRU eviction in O(1).
// Module: system-design/03-caching (advanced)
// Tests: dotnet test --filter "FullyQualifiedName~Lru"
namespace CachingDesign.Examples;

// Not thread-safe: even a read mutates the recency list.
public sealed class LruCache<TKey, TValue>(int capacity) where TKey : notnull
{
    private readonly Dictionary<TKey, LinkedListNode<(TKey Key, TValue Value)>> _map = [];
    private readonly LinkedList<(TKey Key, TValue Value)> _order = new(); // head = most recent

    public bool TryGet(TKey key, out TValue value)
    {
        if (!_map.TryGetValue(key, out var node))
        {
            value = default!;
            return false;
        }
        _order.Remove(node);
        _order.AddFirst(node);
        value = node.Value.Value;
        return true;
    }

    public void Set(TKey key, TValue value)
    {
        if (_map.Remove(key, out var existing))
        {
            _order.Remove(existing);
        }
        else if (_map.Count == capacity)
        {
            var oldest = _order.Last!;
            _order.RemoveLast();
            _map.Remove(oldest.Value.Key);
        }
        _map[key] = _order.AddFirst((key, value));
    }
}
