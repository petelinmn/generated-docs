// Example: asking the runtime whether it can generate code at run time (JIT) or not (Native AOT).
// Module: csharp/01-dotnet-platform (advanced)
// Tests: dotnet test --filter "FullyQualifiedName~ExecutionModel"
using System.Runtime.CompilerServices;

namespace Examples;

public static class ExecutionModel
{
    // false under Native AOT, and also when the project sets PublishAot=true (see expert).
    public static bool CanGenerateCodeAtRuntime => RuntimeFeature.IsDynamicCodeSupported;

    public static string Describe() => CanGenerateCodeAtRuntime
        ? "Dynamic code is supported"
        : "Dynamic code is not supported";
}
