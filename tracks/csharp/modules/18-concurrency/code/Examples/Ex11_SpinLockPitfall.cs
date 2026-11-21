// Example: SpinLock is a struct — in a readonly field every call works on a copy.
// Module: csharp/18-concurrency (expert)
// Tests: dotnet test --filter "FullyQualifiedName~SpinLockPitfall"
namespace Examples;

public sealed class SpinLockInReadonlyField
{
    // Wrong: readonly makes the compiler pass a defensive copy to Enter and to Exit.
#pragma warning disable IDE0044
    private readonly SpinLock _spin = new(enableThreadOwnerTracking: true);
#pragma warning restore IDE0044

    public void EnterAndExit()
    {
        bool taken = false;
        _spin.Enter(ref taken); // locks a temporary copy
        _spin.Exit();           // unlocks another copy, which was never locked
    }
}

public sealed class SpinLockInMutableField
{
    private SpinLock _spin = new(enableThreadOwnerTracking: true);
    private int _value;

    public void Increment()
    {
        bool taken = false;
        try
        {
            _spin.Enter(ref taken);
            _value++;
        }
        finally
        {
            if (taken)
            {
                _spin.Exit();
            }
        }
    }

    public int Value => Volatile.Read(ref _value);
}
