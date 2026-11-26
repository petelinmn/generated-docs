// Example: single-decree Paxos as described in "Paxos Made Simple".
// Module: system-design/12-consensus-and-coordination (expert)
// Tests: dotnet test --filter "FullyQualifiedName~Paxos"
namespace Consensus.Examples;

public sealed class Acceptor
{
    private long _promised;
    private (long Number, string Value)? _accepted;

    // Phase 1b: promise not to accept lower numbers, report what was accepted before.
    public (bool Ok, (long Number, string Value)? Accepted) Prepare(long n)
    {
        if (n <= _promised) return (false, null);
        _promised = n;
        return (true, _accepted);
    }

    // Phase 2b: accept unless a higher prepare has been answered since.
    public bool Accept(long n, string value)
    {
        if (n < _promised) return false;
        _promised = n;
        _accepted = (n, value);
        return true;
    }
}

public static class Proposer
{
    // reachable: the acceptors that answer. Returns the chosen value, or null if there was no majority.
    public static string? Propose(long n, string value, IReadOnlyList<Acceptor> reachable, int clusterSize)
    {
        var promises = reachable.Select(a => (a, Reply: a.Prepare(n))).Where(p => p.Reply.Ok).ToList();
        if (promises.Count <= clusterSize / 2) return null;

        // Phase 2a: a value that may already be chosen must be proposed again.
        var earlier = promises.Where(p => p.Reply.Accepted is not null)
            .Select(p => p.Reply.Accepted!.Value)
            .OrderByDescending(a => a.Number)
            .ToList();
        string proposed = earlier.Count > 0 ? earlier[0].Value : value;

        int accepted = promises.Count(p => p.a.Accept(n, proposed));
        return accepted > clusterSize / 2 ? proposed : null;
    }
}
