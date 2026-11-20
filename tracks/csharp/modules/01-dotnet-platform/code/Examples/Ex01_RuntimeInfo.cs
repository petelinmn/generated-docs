// Example: which runtime, OS and CPU architecture the code is running on.
// Module: csharp/01-dotnet-platform (basics)
// Tests: dotnet test --filter "FullyQualifiedName~RuntimeInfo"
using System.Runtime.InteropServices;

namespace Examples;

public static class RuntimeInfo
{
    // The runtime that actually executes the code (e.g. ".NET 11.0.0"), not the SDK version.
    public static string Framework => RuntimeInformation.FrameworkDescription;

    public static Version RuntimeVersion => Environment.Version;

    public static string OperatingSystem => RuntimeInformation.OSDescription;

    public static Architecture ProcessArchitecture => RuntimeInformation.ProcessArchitecture;

    public static string Describe() =>
        $"{Framework} on {OperatingSystem} ({ProcessArchitecture})";
}
