// Example: in a file-based app PublishAot=true is on by default, and it affects even `dotnet run`.
// Module: csharp/01-dotnet-platform (expert)
// Run: dotnet run aot-check.cs
using System.Runtime;
using System.Runtime.CompilerServices;

Console.WriteLine($"IsDynamicCodeSupported={RuntimeFeature.IsDynamicCodeSupported}");
Console.WriteLine($"IsDynamicCodeCompiled={RuntimeFeature.IsDynamicCodeCompiled}");

// Yet the JIT is clearly working: calling a new method compiles it.
long before = JitInfo.GetCompiledMethodCount(currentThread: true);
Helper.Touch();
long after = JitInfo.GetCompiledMethodCount(currentThread: true);
Console.WriteLine($"JIT compiled during first call: {after - before}");

static class Helper
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int Touch() => 42;
}
