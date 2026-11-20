---
title: "The .NET platform and dotnet CLI: basics"
lang: en
track: csharp
kind: module
doc: basics
slug: 01-dotnet-platform
part: 1
tags: [dotnet, dotnet-cli, sdk, runtime, clr, jit, native-aot, msbuild, nuget, file-based-apps]
versions: ".NET 11 (SDK 11.0.100, runtime 11.0.0), C# 15"
status: verified
updated: 2026-11-20
---

# The .NET platform and dotnet CLI: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md) · [exercises](exercises.en.md) · [5–6. Code and tests](code/)

**Prerequisites:** none — this is the first module of the track.

## What .NET consists of

.NET is a platform, not just a language. It needs two things: an environment in which code **runs**, and tools with which code is **built**.

| Component | What it does | Who needs it |
|---|---|---|
| **Runtime** | The CLR (Common Language Runtime) loads assemblies, compiles them with the JIT compiler, manages memory (GC), exceptions and threads. The BCL (Base Class Library) provides base types, collections, files, networking | everyone who runs an app |
| **SDK** | runtime + the C# compiler (Roslyn), the MSBuild build system, the `dotnet` CLI, project templates, the NuGet client | developers |

The runtime ships as **shared frameworks** — sets of assemblies shared by all apps on the machine:

- `Microsoft.NETCore.App` — the base one, required by every app;
- `Microsoft.AspNetCore.App` — web (ASP.NET Core);
- `Microsoft.WindowsDesktop.App` — WinForms and WPF, Windows only.

## SDK and runtime have different versions

The most common beginner confusion is taking the SDK version for the platform version. Here is real output from the machine this module was written on:

```text
> dotnet --version
11.0.100

> dotnet --list-runtimes   (excerpt)
Microsoft.AspNetCore.App 11.0.0 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App]
Microsoft.NETCore.App 11.0.0 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]
```

SDK `11.0.100` and runtime `11.0.0` are different products with their own numbers. In the SDK, the third part of the number consists of the feature band (hundreds) and the patch: in `11.0.100` it is band `1xx` (the first band of a new version) and patch `00`, and in `10.0.302` it is band `3xx` and patch `02`. The app is executed by the runtime, and its version is available from code:

```csharp
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
```

```text
.NET 11.0.0 on Microsoft Windows 10.0.26200 (X64)
```

Code: [Ex01_RuntimeInfo.cs](code/Examples/Ex01_RuntimeInfo.cs), test: [ExamplesTests.cs](code/Tests/ExamplesTests.cs).

## The first program and the dotnet CLI

```bash
dotnet new console -o HelloApp   # create a project from a template
cd HelloApp
dotnet run                       # restore + build + run
```

The template creates `HelloApp.csproj` and a `Program.cs` with a single line, `Console.WriteLine("Hello, World!");`. There is no `Program` class and no `Main` method in the file: these are **top-level statements**, and the compiler generates the rest.

Main commands:

| Command | What it does |
|---|---|
| `dotnet new <template>` | create a project or file from a template (`console`, `classlib`, `xunit`, `sln`, …) |
| `dotnet build` | build (restore runs first) |
| `dotnet run` | build and run |
| `dotnet test` | build and run the tests |
| `dotnet add package <name>` | add a NuGet package to the project |
| `dotnet restore` | download dependencies |
| `dotnet publish` | prepare the app for deployment |
| `dotnet sln add <project>` | add a project to a solution |
| `dotnet --info` | SDK and runtime versions and environment details |

## The project: a .csproj file

A project is an MSBuild file with the `.csproj` extension. This is the examples project of this module:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net11.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

</Project>
```

- `Sdk="Microsoft.NET.Sdk"` brings in the standard build rules. Web projects use `Microsoft.NET.Sdk.Web`.
- `TargetFramework` (TFM) — which platform to build for: `net11.0`.
- `ImplicitUsings` adds global `using` directives for common namespaces (`System`, `System.IO`, `System.Linq`, `System.Collections.Generic`, …).
- `Nullable` turns on null-reference analysis — see module [02 · Types and variables](../02-types-and-variables/advanced.en.md#nullable-reference-types).
- `OutputType` is not set here, so the result is a library (`.dll`). An application sets `<OutputType>Exe</OutputType>`.
- There is no need to list `.cs` files: an SDK-style project includes every file in its folder and subfolders.

Project settings end up in the compiled assembly. For example, the TFM becomes an attribute:

```csharp
public static class AssemblyInfo
{
    // <TargetFramework>net11.0</TargetFramework> becomes an attribute of the compiled assembly.
    public static string? TargetFrameworkOf(Assembly assembly) =>
        assembly.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName;

    // Base types such as int and string live in System.Private.CoreLib, the core of the BCL.
    public static string? AssemblyNameOf(Type type) => type.Assembly.GetName().Name;
}
```

```text
.NETCoreApp,Version=v11.0
System.Private.CoreLib
```

Code: [Ex02_AssemblyInfo.cs](code/Examples/Ex02_AssemblyInfo.cs).

## The solution: a .slnx file

A solution groups several projects so that they can be built and tested with one command. Starting with .NET 10, `dotnet new sln` creates a solution in the **`.slnx`** format by default — short XML instead of the old `.sln`. The solution of this module:

```xml
<Solution>
  <Project Path="Examples/Examples.csproj" />
  <Project Path="Solutions/Solutions.csproj" />
  <Project Path="Tests/Tests.csproj" />
</Solution>
```

`dotnet test` in the solution folder builds all three projects and runs the tests.

## File-based apps: a program without a project

Starting with .NET 10, a single-file program can run without a `.csproj`:

```csharp
// Example: a file-based app: a single .cs file without a .csproj (.NET 10 SDK or newer).
// Module: csharp/01-dotnet-platform (basics)
// Run: dotnet run hello.cs -- Alice
#:property AssemblyName=HelloFileApp

// Top-level statements: the compiler generates the Program class and Main for us.
var name = args.Length > 0 ? args[0] : "world";
var assemblyName = typeof(Program).Assembly.GetName().Name;
Console.WriteLine($"Hello, {name}! Running {assemblyName} on .NET {Environment.Version.Major}");
```

```text
> dotnet run hello.cs -- Alice
Hello, Alice! Running HelloFileApp on .NET 11
```

Everything you would normally write in a `.csproj` is set with `#:` directives at the top of the file:

| Directive | Example | Purpose |
|---|---|---|
| `#:package` | `#:package Humanizer@2.14.1` | a NuGet package |
| `#:property` | `#:property AssemblyName=HelloFileApp` | an MSBuild property |
| `#:sdk` | `#:sdk Microsoft.NET.Sdk.Web` | a different SDK, e.g. for web |
| `#:project` | `#:project ../Examples/Examples.csproj` | a project reference |
| `#:include` | | include other files (.NET 11 SDK; in SDK 10 starting with 10.0.300) |

A file-based app targets the platform of the SDK that runs it: SDK 11 builds the file for `net11.0` (C# 15), while SDK 10 builds it for `net10.0` (C# 14), and then the output ends with `on .NET 10`. To pin the platform, use the `#:property TargetFramework=net10.0` directive.

When a script grows, it is turned into a regular project with `dotnet project convert hello.cs`. File-based apps are handy for experiments, utilities and learning the language.

Code: [hello.cs](code/FileBasedApp/hello.cs), test: [FileBasedAppTests.cs](code/Tests/FileBasedAppTests.cs).

## NuGet: packages

NuGet is the .NET package manager. `dotnet add package Serilog` adds a `<PackageReference Include="Serilog" Version="…" />` line to the `.csproj`. On restore, packages are downloaded to the global folder `%USERPROFILE%\.nuget\packages` (`~/.nuget/packages` on Linux and macOS) and reused by all projects.

The `bin/` (build output) and `obj/` (intermediate files) folders are created automatically. They are not committed: they are already in this repository's `.gitignore`.

## Related materials

- Next: [02 · Types and variables](../02-types-and-variables/overview.en.md), [03 · Control flow and methods](../03-control-flow-and-methods/overview.en.md)
- The same idea in other tracks: [Python](../../../python/overview.en.md) — the CPython interpreter, uv and `pyproject.toml` instead of the dotnet CLI and `.csproj`; [Kubernetes](../../../kubernetes/overview.en.md) — containers for .NET apps

## Sources

- [.NET overview](https://learn.microsoft.com/dotnet/core/introduction)
- [dotnet CLI](https://learn.microsoft.com/dotnet/core/tools/)
- [File-based apps](https://learn.microsoft.com/dotnet/core/sdk/file-based-apps)
- [Runtime compilation settings: tiered compilation, PGO](https://learn.microsoft.com/dotnet/core/runtime-config/compilation)
- [Tiered compilation: design (.NET Core 2.1)](https://github.com/dotnet/runtime/blob/main/docs/design/features/tiered-compilation.md)
- [Native AOT](https://learn.microsoft.com/dotnet/core/deploying/native-aot/)
- [.NET version selection and roll-forward](https://learn.microsoft.com/dotnet/core/versions/selection)
- [.NET releases and support](https://learn.microsoft.com/dotnet/core/releases-and-support)
- [dotnet test and Microsoft.Testing.Platform](https://learn.microsoft.com/dotnet/core/testing/unit-testing-with-dotnet-test)
- [C# language version](https://learn.microsoft.com/dotnet/csharp/language-reference/configure-language-version)
