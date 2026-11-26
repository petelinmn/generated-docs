// Example: atomic writes with offsets, and what a read-committed consumer sees (a model of the idea).
// Module: system-design/08-messaging-and-events (expert)
// Tests: dotnet test --filter "FullyQualifiedName~Transactions"
namespace Messaging.Examples;

public sealed class TransactionalLog
{
    private readonly List<(long Transaction, string Value)> _records = [];
    private readonly Dictionary<long, bool?> _outcome = []; // null while the transaction is open
    private readonly Dictionary<long, int> _pendingOffsets = [];
    private long _last;

    public int ConsumedOffset { get; private set; }

    public long Begin()
    {
        _outcome[++_last] = null;
        return _last;
    }

    // Records are in the log at once; whether they count is decided later.
    public void Send(long transaction, string value) => _records.Add((transaction, value));

    public void SendOffset(long transaction, int offset) => _pendingOffsets[transaction] = offset;

    public void Commit(long transaction)
    {
        _outcome[transaction] = true;
        if (_pendingOffsets.TryGetValue(transaction, out int offset)) ConsumedOffset = offset;
    }

    public void Abort(long transaction) => _outcome[transaction] = false;

    public IEnumerable<string> ReadUncommitted() => _records.Select(r => r.Value);

    // Stops at the first record of an open transaction, so order is preserved.
    public IEnumerable<string> ReadCommitted() =>
        _records.TakeWhile(r => _outcome[r.Transaction] is not null)
            .Where(r => _outcome[r.Transaction] == true)
            .Select(r => r.Value);
}
