using Examples;

namespace Tests;

public class Ex01_RuntimeInfoTests
{
    [Fact]
    public void Framework_IsDotNet11()
    {
        Assert.StartsWith(".NET 11.", RuntimeInfo.Framework);
        Assert.Equal(11, RuntimeInfo.RuntimeVersion.Major);
    }

    [Fact]
    public void Describe_ContainsFrameworkOsAndArchitecture()
    {
        string description = RuntimeInfo.Describe();

        Assert.Contains(RuntimeInfo.Framework, description);
        Assert.Contains(RuntimeInfo.OperatingSystem, description);
        Assert.Contains(RuntimeInfo.ProcessArchitecture.ToString(), description);
    }
}

public class Ex02_AssemblyInfoTests
{
    [Fact]
    public void TargetFramework_ComesFromCsproj()
    {
        string? framework = AssemblyInfo.TargetFrameworkOf(typeof(AssemblyInfo).Assembly);

        Assert.Equal(".NETCoreApp,Version=v11.0", framework);
    }

    [Fact]
    public void BaseTypes_LiveInCoreLib()
    {
        Assert.Equal("System.Private.CoreLib", AssemblyInfo.AssemblyNameOf(typeof(int)));
        Assert.Equal("System.Private.CoreLib", AssemblyInfo.AssemblyNameOf(typeof(string)));
        Assert.Equal("Examples", AssemblyInfo.AssemblyNameOf(typeof(AssemblyInfo)));
    }
}

public class Ex03_JitObserverTests
{
    [Fact]
    public void Method_IsCompiledOnFirstCallOnly()
    {
        // The same delegate is used twice, so the second call needs no new code.
        Action call = () => JitObserver.Double(21);

        long firstCall = JitObserver.CountJitCompilations(call);
        long secondCall = JitObserver.CountJitCompilations(call);

        Assert.True(firstCall >= 1, $"Expected JIT work on the first call, got {firstCall}");
        Assert.Equal(0, secondCall);
    }
}

public class Ex04_ExecutionModelTests
{
    [Fact]
    public void RegularProject_SupportsDynamicCode()
    {
        // The test project is a regular JIT-based app without PublishAot.
        Assert.True(ExecutionModel.CanGenerateCodeAtRuntime);
        Assert.Equal("Dynamic code is supported", ExecutionModel.Describe());
    }
}

public class Ex05_IlInspectorTests
{
    [Fact]
    public void Add_CompilesToFourIlInstructions()
    {
        // ldarg.0 (02), ldarg.1 (03), add (58), ret (2A)
        Assert.Equal("0203582A", IlInspector.AddIlAsHex());
    }
}
