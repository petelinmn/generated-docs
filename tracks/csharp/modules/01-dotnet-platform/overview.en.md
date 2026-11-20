---
title: "The .NET platform and dotnet CLI"
lang: en
track: csharp
kind: module
doc: overview
slug: 01-dotnet-platform
part: 1
levels: [1, 2, 3, 4, 5, 6]
tags: [dotnet, dotnet-cli, sdk, runtime, clr, jit, native-aot, msbuild, nuget, file-based-apps]
versions: ".NET 11 (SDK 11.0.100, runtime 11.0.0), C# 15"
status: verified
updated: 2026-11-20
---

# The .NET platform and dotnet CLI

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

.NET is the platform that runs C# code. The compiler turns source code into IL (Intermediate Language), and the CLR runtime compiles IL into machine code at run time and manages memory. Everything a developer does — create a project, build, run, test, publish — is done with a single tool, `dotnet`.

## Key ideas

- **SDK ≠ runtime.** The SDK (compiler, MSBuild, CLI) is needed to build, the runtime (the CLR and the base library) is needed to run. They have different version numbers: SDK `11.0.100`, runtime `11.0.0`.
- **A project is a `.csproj` file**, a solution is a `.slnx` file. Starting with .NET 10, `dotnet new sln` creates `.slnx` by default.
- **File-based apps** (.NET 10): a single `.cs` file runs with `dotnet run app.cs`, without a project. Packages and settings are set with `#:package` and `#:property` directives.
- **The JIT** compiles a method on its first call. Tiered compilation compiles quickly first and optimizes hot methods later.
- **Native AOT** builds a native executable without a JIT: fast startup, but no code generation at run time.
- **Versions:** a new .NET version ships every November. Even-numbered ones are LTS (3 years of support), odd-numbered ones are STS (2 years). .NET 11 is STS, supported until November 2028, and its default language is C# 15; .NET 10 is LTS.
- **NuGet** is the package manager: `dotnet add package <name>`.

## Minimal example

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

## When to use and when not

- ✅ `dotnet new` + `.csproj` — for any app or library that will live longer than one evening.
- ✅ A file-based app — for experiments, utilities, learning the language.
- ✅ Native AOT — for CLI tools, serverless and containers, where startup and size matter.
- ❌ Native AOT — if the code relies on `Reflection.Emit`, dynamic assembly loading or libraries that are not trimming-compatible.
- ❌ A file-based app — for an app made of several projects: convert it to a project with `dotnet project convert`.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | SDK and runtime, the dotnet CLI, `.csproj`, `.slnx`, file-based apps, NuGet |
| [2. Advanced](advanced.en.md) | from source code to machine code, JIT, tiered compilation, ReadyToRun and Native AOT, publishing, versions, `global.json` |
| [3. Expert](expert.en.md) | app startup (hostfxr, hostpolicy, coreclr), roll-forward, IL from the inside, the `PublishAot` trap; a reference for the topic, gotchas, interview questions |
| [4. Self-check](quiz.en.md) | a quiz and [exercises](exercises.en.md) with solutions |
| [5. Runnable examples](code/) | `code/Examples/` and `code/Solutions/` — the `Code.slnx` solution |
| [6. Tests](code/) | `code/Tests/` — `dotnet test` inside `code/` |


## Related materials

- Next: [02 · Types and variables](../02-types-and-variables/overview.en.md)
- The same idea in other tracks: [Python](../../../python/overview.en.md) — an interpreter and uv instead of the CLR and the dotnet CLI
