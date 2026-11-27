// Example: take tokens from the shared bucket in batches to save round trips.
// Recipe: system-design/recipes/distributed-rate-limiter (expert)
// Tests: dotnet test --filter "FullyQualifiedName~Leased"
namespace RateLimiting.Examples;

public sealed class LeasedLimiter(ILimiter shared, int batch) : ILimiter
{
    private readonly Dictionary<string, int> _leases = [];
    private readonly SemaphoreSlim _gate = new(1, 1);

    public int SharedCalls { get; private set; }

    public async ValueTask<Decision> AcquireAsync(string key, int cost = 1, CancellationToken cancellationToken = default)
    {
        // One gate for all keys keeps the example short; a service would use one per key.
        await _gate.WaitAsync(cancellationToken);
        try
        {
            int held = _leases.GetValueOrDefault(key);
            if (held < cost)
            {
                SharedCalls++;
                var lease = await shared.AcquireAsync(key, Math.Max(batch, cost), cancellationToken);
                if (!lease.Allowed)
                {
                    // Less than a batch may remain: ask for exactly what is needed.
                    SharedCalls++;
                    var exact = await shared.AcquireAsync(key, cost, cancellationToken);
                    return exact;
                }

                held += Math.Max(batch, cost);
            }

            // Tokens held here are invisible to other instances until used.
            _leases[key] = held - cost;
            return new Decision(true, held - cost, TimeSpan.Zero);
        }
        finally
        {
            _gate.Release();
        }
    }
}
