// Example: the N+1 problem of nested resolvers and the batching that fixes it.
// Module: system-design/05-api-design (expert)
// Tests: dotnet test --filter "FullyQualifiedName~Batching"
namespace ApiDesign.Examples;

// A minimal data loader: collect keys first, load them together. One instance per request.
// Not thread-safe; real implementations dispatch the batch automatically.
public sealed class BatchLoader<TKey, TValue>(Func<IReadOnlyCollection<TKey>, Task<IReadOnlyDictionary<TKey, TValue>>> loadMany)
    where TKey : notnull
{
    private readonly Dictionary<TKey, TaskCompletionSource<TValue>> _requested = [];
    private List<TKey> _pending = [];

    public Task<TValue> LoadAsync(TKey key)
    {
        if (!_requested.TryGetValue(key, out var source))
        {
            _requested[key] = source = new(); // the same key is asked for once
            _pending.Add(key);
        }
        return source.Task;
    }

    public async Task DispatchAsync()
    {
        List<TKey> batch = _pending;
        _pending = [];
        if (batch.Count == 0) return;

        IReadOnlyDictionary<TKey, TValue> found = await loadMany(batch);
        foreach (TKey key in batch) _requested[key].SetResult(found[key]);
    }
}
