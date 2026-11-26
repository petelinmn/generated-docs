// Example: Try-Confirm/Cancel: reserve first, then confirm or release; reservations expire.
// Module: system-design/10-distributed-transactions (expert)
// Tests: dotnet test --filter "FullyQualifiedName~Tcc"
namespace Transactions.Examples;

public sealed class Stock(int available, TimeProvider clock)
{
    private readonly Dictionary<string, (int Quantity, DateTimeOffset ExpiresAt)> _holds = [];
    private readonly HashSet<string> _confirmed = [];

    public int Available
    {
        get { Expire(); return available - _holds.Values.Sum(h => h.Quantity); }
    }

    public bool Try(string id, int quantity, TimeSpan holdFor)
    {
        if (_holds.ContainsKey(id) || _confirmed.Contains(id)) return true; // a repeated Try
        if (Available < quantity) return false;
        _holds[id] = (quantity, clock.GetUtcNow() + holdFor);
        return true;
    }

    // false: the hold has expired or never existed, so the caller must not assume success.
    public bool Confirm(string id)
    {
        Expire();
        if (_confirmed.Contains(id)) return true;
        if (!_holds.Remove(id, out var hold)) return false;

        available -= hold.Quantity;
        _confirmed.Add(id);
        return true;
    }

    // Safe to repeat, and safe when Try never arrived.
    public void Cancel(string id) => _holds.Remove(id);

    private void Expire()
    {
        DateTimeOffset now = clock.GetUtcNow();
        foreach (string id in _holds.Where(h => h.Value.ExpiresAt <= now).Select(h => h.Key).ToList()) _holds.Remove(id);
    }
}
