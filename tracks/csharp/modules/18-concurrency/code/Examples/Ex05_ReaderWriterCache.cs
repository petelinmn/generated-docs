// Example: ReaderWriterLockSlim — many readers at once, one writer, upgradeable read for get-or-add.
// Module: csharp/18-concurrency (advanced)
// Tests: dotnet test --filter "FullyQualifiedName~ReaderWriterCache"
namespace Examples;

public sealed class ReaderWriterCache<TKey, TValue> : IDisposable
    where TKey : notnull
{
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly Dictionary<TKey, TValue> _items = [];

    public bool TryGet(TKey key, out TValue? value)
    {
        _lock.EnterReadLock();
        try
        {
            return _items.TryGetValue(key, out value);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public void Set(TKey key, TValue value)
    {
        _lock.EnterWriteLock();
        try
        {
            _items[key] = value;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    // Upgradeable read: only one thread at a time may hold it, so check-then-add is safe.
    public TValue GetOrAdd(TKey key, Func<TKey, TValue> factory)
    {
        _lock.EnterUpgradeableReadLock();
        try
        {
            if (_items.TryGetValue(key, out TValue? existing))
            {
                return existing;
            }
            _lock.EnterWriteLock();
            try
            {
                TValue created = factory(key);
                _items[key] = created;
                return created;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
        finally
        {
            _lock.ExitUpgradeableReadLock();
        }
    }

    // A read lock cannot be turned into a write lock: by default this throws LockRecursionException.
    public void WriteWhileReadingWrong(TKey key, TValue value)
    {
        _lock.EnterReadLock();
        try
        {
            Set(key, value);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public void Dispose() => _lock.Dispose();
}
