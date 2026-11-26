// Example: a hybrid logical clock (Kulkarni et al.).
// Module: system-design/12-consensus-and-coordination (expert)
// Tests: dotnet test --filter "FullyQualifiedName~Hybrid"
namespace Consensus.Examples;

// L: the largest physical time heard of. C: orders events that share L.
public readonly record struct HlcStamp(long L, int C) : IComparable<HlcStamp>
{
    public int CompareTo(HlcStamp other) => (L, C).CompareTo((other.L, other.C));
}

public sealed class HybridClock(Func<long> physicalTime)
{
    private long _l;
    private int _c;

    // A local event or a send.
    public HlcStamp Tick()
    {
        long previous = _l;
        _l = Math.Max(previous, physicalTime());
        _c = _l == previous ? _c + 1 : 0;
        return new(_l, _c);
    }

    public HlcStamp Receive(HlcStamp message)
    {
        long previous = _l;
        _l = Math.Max(Math.Max(previous, message.L), physicalTime());

        _c = (_l == previous, _l == message.L) switch
        {
            (true, true) => Math.Max(_c, message.C) + 1,
            (true, false) => _c + 1,
            (false, true) => message.C + 1,
            _ => 0,
        };
        return new(_l, _c);
    }
}
