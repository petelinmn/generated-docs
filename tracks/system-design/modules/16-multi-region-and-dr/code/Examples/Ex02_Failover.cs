// Example: failover with thresholds, so one bad probe does not move the traffic.
// Module: system-design/16-multi-region-and-dr (advanced)
// Tests: dotnet test --filter "FullyQualifiedName~Failover"
namespace MultiRegion.Examples;

// Not thread-safe: one health checker feeds it.
public sealed class FailoverController(string primary, string secondary, int failAfter, int recoverAfter)
{
    private int _failures, _successes;

    private readonly string _primary = primary;

    public string Active { get; private set; } = primary;

    public void Probe(bool primaryHealthy)
    {
        (_successes, _failures) = primaryHealthy ? (_successes + 1, 0) : (0, _failures + 1);

        if (Active == _primary && _failures >= failAfter) Active = secondary;
        // Returning needs a longer streak: flapping is worse than staying on the secondary.
        else if (Active == secondary && _successes >= recoverAfter) Active = _primary;
    }
}
