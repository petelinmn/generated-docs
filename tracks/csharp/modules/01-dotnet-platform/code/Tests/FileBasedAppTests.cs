using System.Diagnostics;

namespace Tests;

// These tests start a real `dotnet run`, so they are slower than unit tests and need the .NET 10 SDK or newer.
public class FileBasedAppTests
{
    [Fact]
    [Trait("Category", "Integration")]
    public async Task HelloCs_RunsWithoutProjectFile()
    {
        string output = await DotnetRun.ScriptAsync("hello.cs", "--", "Alice");

        // A file-based app targets the TFM of the SDK that runs it: SDK 10 -> .NET 10, SDK 11 -> .NET 11.
        Assert.Matches(@"Hello, Alice! Running HelloFileApp on \.NET \d+", output);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task AotCheck_ReportsNoDynamicCode_ButJitStillWorks()
    {
        string output = await DotnetRun.ScriptAsync("aot-check.cs");

        Assert.Contains("IsDynamicCodeSupported=False", output);
        Assert.Contains("IsDynamicCodeCompiled=False", output);
        Assert.Matches(@"JIT compiled during first call: [1-9]", output);
    }
}

internal static class DotnetRun
{
    public static async Task<string> ScriptAsync(string scriptName, params string[] arguments)
    {
        CancellationToken cancellation = TestContext.Current.CancellationToken;
        string script = TestFiles.Find("FileBasedApp", scriptName);
        var startInfo = new ProcessStartInfo("dotnet", ["run", script, .. arguments])
        {
            WorkingDirectory = Path.GetDirectoryName(script)!,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        using Process process = Process.Start(startInfo)!;
        // Read both streams concurrently so that a full buffer cannot block the child process.
        Task<string> output = process.StandardOutput.ReadToEndAsync(cancellation);
        Task<string> errors = process.StandardError.ReadToEndAsync(cancellation);
        await process.WaitForExitAsync(cancellation);

        Assert.True(process.ExitCode == 0, await errors);
        return await output;
    }
}

internal static class TestFiles
{
    // Walks up from the test output folder (Tests/bin/Debug/net11.0) to the module's code/ folder.
    public static string Find(params string[] relativePath)
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            string candidate = Path.Combine([dir.FullName, .. relativePath]);
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }
        throw new FileNotFoundException($"Not found: {Path.Combine(relativePath)}");
    }
}
