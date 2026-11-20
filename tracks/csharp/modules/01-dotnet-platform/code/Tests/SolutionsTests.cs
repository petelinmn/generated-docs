using System.Runtime.InteropServices;
using Solutions;

namespace Tests;

public class S01_PlatformReportTests
{
    [Fact]
    public void Report_HasExpectedFormat()
    {
        string report = PlatformReport.Create();

        Assert.Matches(@"^Runtime: \.NET \d+\.\d+\.\d+ \| OS: .+ \| Arch: \w+$", report);
        Assert.EndsWith($"Arch: {RuntimeInformation.ProcessArchitecture}", report);
    }
}

public class S02_AssemblyInspectorTests
{
    [Fact]
    public void References_AreDistinctSortedAndIncludeSystemRuntime()
    {
        IReadOnlyList<string> names = AssemblyInspector.GetReferencedAssemblyNames(typeof(AssemblyInspector).Assembly);

        Assert.Contains("System.Runtime", names);
        Assert.Equal(names.Distinct().Count(), names.Count);
        Assert.Equal(names.Order(StringComparer.Ordinal), names);
    }
}

public class S03_RuntimeConfigReaderTests
{
    [Fact]
    public void ConsoleApp_HasSingleFramework()
    {
        const string json = """
            { "runtimeOptions": { "tfm": "net11.0",
              "framework": { "name": "Microsoft.NETCore.App", "version": "11.0.0" } } }
            """;

        var frameworks = RuntimeConfigReader.ReadFrameworks(json);

        Assert.Equal([("Microsoft.NETCore.App", "11.0.0")], frameworks);
    }

    [Fact]
    public void WebApp_CanHaveSeveralFrameworks()
    {
        const string json = """
            { "runtimeOptions": { "tfm": "net11.0", "frameworks": [
              { "name": "Microsoft.NETCore.App", "version": "11.0.0" },
              { "name": "Microsoft.AspNetCore.App", "version": "11.0.0" } ] } }
            """;

        var frameworks = RuntimeConfigReader.ReadFrameworks(json);

        Assert.Equal(["Microsoft.NETCore.App", "Microsoft.AspNetCore.App"], frameworks.Select(f => f.Name));
    }

    [Fact]
    public void TestApp_RuntimeConfig_AsksForNetCoreApp11()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Tests.runtimeconfig.json");

        var frameworks = RuntimeConfigReader.ReadFrameworks(File.ReadAllText(path));

        var netCore = Assert.Single(frameworks);
        Assert.Equal("Microsoft.NETCore.App", netCore.Name);
        Assert.StartsWith("11.0.", netCore.Version);
    }
}
