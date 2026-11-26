// Example: the voting rule of Raft leader election (not a full Raft).
// Module: system-design/12-consensus-and-coordination (basics, advanced)
// Tests: dotnet test --filter "FullyQualifiedName~Election"
namespace Consensus.Examples;

public sealed class Node(int id, long lastLogTerm = 0, long lastLogIndex = 0)
{
    private int? _votedFor;

    public int Id { get; } = id;
    public long Term { get; private set; }
    public long LastLogTerm { get; } = lastLogTerm;
    public long LastLogIndex { get; } = lastLogIndex;

    public bool RequestVote(long term, int candidate, long candidateLogTerm, long candidateLogIndex)
    {
        if (term < Term) return false;
        if (term > Term) (Term, _votedFor) = (term, null);

        // The log with the later last term wins; with equal terms the longer one.
        bool upToDate = candidateLogTerm > LastLogTerm
            || (candidateLogTerm == LastLogTerm && candidateLogIndex >= LastLogIndex);
        if (!upToDate || (_votedFor is not null && _votedFor != candidate)) return false;

        _votedFor = candidate; // one vote per term
        return true;
    }

    // reachable includes the other nodes only; a majority is counted over the whole cluster.
    public bool StartElection(IEnumerable<Node> reachable, int clusterSize)
    {
        (Term, _votedFor) = (Term + 1, Id);
        int votes = 1 + reachable.Count(n => n.RequestVote(Term, Id, LastLogTerm, LastLogIndex));
        return votes > clusterSize / 2;
    }
}
