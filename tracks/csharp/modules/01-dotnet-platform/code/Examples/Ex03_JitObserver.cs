// Example: the JIT compiles a method on its first call, and only once.
// Module: csharp/01-dotnet-platform (advanced)
// Tests: dotnet test --filter "FullyQualifiedName~JitObserver"
using System.Runtime;
using System.Runtime.CompilerServices;

namespace Examples;

public static class JitObserver
{
    // Counts methods JIT-compiled on the current thread while the action runs.
    // currentThread: true ignores background compilation (e.g. tier-1 recompilation).
    public static long CountJitCompilations(Action action)
    {
        long before = JitInfo.GetCompiledMethodCount(currentThread: true);
        action();
        return JitInfo.GetCompiledMethodCount(currentThread: true) - before;
    }

    // NoInlining keeps the method a separate unit of JIT compilation.
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int Double(int x) => x * 2;
}
