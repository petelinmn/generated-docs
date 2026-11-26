// Example: two-phase commit and its blocking window.
// Module: system-design/10-distributed-transactions (basics, advanced)
// Tests: dotnet test --filter "FullyQualifiedName~TwoPhase"
namespace Transactions.Examples;

public enum State { Active, Prepared, Committed, Aborted }

public sealed class Participant(bool canCommit = true)
{
    public State State { get; private set; } = State.Active;

    // Voting yes is a promise: from now on the participant may not decide alone.
    public bool Prepare()
    {
        State = canCommit ? State.Prepared : State.Aborted;
        return canCommit;
    }

    public void Commit() => State = State.Committed;
    public void Abort() => State = State.Aborted;
}

public static class TwoPhaseCommit
{
    // crashAfterPrepare models a coordinator that dies before announcing the decision.
    public static void Run(IReadOnlyList<Participant> participants, bool crashAfterPrepare = false)
    {
        bool all = true;
        foreach (Participant p in participants) all &= p.Prepare();
        if (crashAfterPrepare) return;

        foreach (Participant p in participants)
            if (all) p.Commit();
            else p.Abort();
    }
}
