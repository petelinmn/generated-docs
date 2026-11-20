// Solution: exercise 1 — a one-line platform report.
// Module: csharp/01-dotnet-platform (exercises)
// Tests: dotnet test --filter "FullyQualifiedName~PlatformReport"
using System.Runtime.InteropServices;

namespace Solutions;

public static class PlatformReport
{
    public static string Create() =>
        $"Runtime: {RuntimeInformation.FrameworkDescription} | " +
        $"OS: {RuntimeInformation.OSDescription} | " +
        $"Arch: {RuntimeInformation.ProcessArchitecture}";
}
