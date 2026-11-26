// Example: Lamport clocks order cause before effect without synchronized time.
// Module: system-design/12-consensus-and-coordination (expert)
// Tests: dotnet test --filter "FullyQualifiedName~Lamport"
namespace Consensus.Examples;

public sealed class LamportClock
{
    public long Time { get; private set; }

    public long Tick() => ++Time; // a local event or a send

    public long Receive(long remote) => Time = Math.Max(Time, remote) + 1;
}
