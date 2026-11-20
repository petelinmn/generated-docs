---
title: "Quiz: The .NET platform and dotnet CLI"
lang: en
track: csharp
kind: module
doc: quiz
slug: 01-dotnet-platform
part: 1
tags: [dotnet, dotnet-cli, sdk, runtime, jit, native-aot]
status: verified
updated: 2026-11-20
---

# Quiz: The .NET platform and dotnet CLI

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What is included in the SDK but not in the runtime?

- A) The garbage collector (GC)
- B) The C# compiler (Roslyn) and MSBuild
- C) The Base Class Library (BCL)
- D) The JIT compiler

<details><summary>Answer</summary>

**B.** SDK = runtime + development tools: the compiler, MSBuild, the dotnet CLI, templates. The GC, JIT and BCL are parts of the runtime: every running app needs them, even on a server without the SDK.
→ [Basics](basics.en.md)

</details>

## 2. 🟢 Which command shows the installed runtime versions?

- A) `dotnet --version`
- B) `dotnet --list-sdks`
- C) `dotnet --list-runtimes`
- D) `dotnet new list`

<details><summary>Answer</summary>

**C.** `dotnet --version` shows the SDK version (e.g. `11.0.100`), not the runtime (`11.0.0`). `--list-sdks` lists SDKs, `dotnet new list` lists templates.
→ [Basics](basics.en.md)

</details>

## 3. 🟢 Which assembly contains the `System.String` type?

- A) `System.Runtime`
- B) `System.Private.CoreLib`
- C) `mscorlib`
- D) `System.String.dll`

<details><summary>Answer</summary>

**B.** At run time, base types live in `System.Private.CoreLib`; a test checks this in the [Ex02_AssemblyInfo](code/Examples/Ex02_AssemblyInfo.cs) example. `System.Runtime` is the assembly your code references at compile time; it forwards the types to CoreLib. `mscorlib` is the assembly of the old .NET Framework.
→ [Basics](basics.en.md)

</details>

## 4. 🟢 How do you run a single-file program `hello.cs` in .NET 10 and later without creating a project?

- A) Create a project first: `dotnet new console`
- B) `dotnet run hello.cs`
- C) `dotnet hello.exe`
- D) Install C# Interactive and run `csi hello.cs`

<details><summary>Answer</summary>

**B.** This is a file-based app: the SDK creates a virtual project itself. Settings are set with `#:package`, `#:property` and other directives at the top of the file.
→ [Basics](basics.en.md)

</details>

## 5. 🟢 In which format does `dotnet new sln` create a solution starting with .NET 10?

- A) `.sln`
- B) `.slnx`
- C) `.csproj`
- D) `solution.json`

<details><summary>Answer</summary>

**B.** Starting with .NET 10, `.slnx` — a compact XML format — is created by default. The old `.sln` is still supported.
→ [Basics](basics.en.md)

</details>

## 6. 🟡 What will `firstCall` and `secondCall` be?

```csharp
Action call = () => JitObserver.Double(21);

long firstCall = JitObserver.CountJitCompilations(call);
long secondCall = JitObserver.CountJitCompilations(call);
```

`CountJitCompilations` counts the methods JIT-compiled on the current thread during the call.

- A) `1` and `1`
- B) `2` and `0`
- C) `0` and `0`
- D) `1` and `0`

<details><summary>Answer</summary>

**B.** The real result is `2` and `0`. On the first call the JIT compiles two things: the lambda body and the `Double` method. On the second call both are already compiled. If a new lambda were passed the second time, it would have to be compiled separately.
→ [Advanced](advanced.en.md)

</details>

## 7. 🟡 Select all correct statements about Native AOT

- [ ] A) There is no JIT compiler at run time
- [ ] B) `System.Reflection.Emit` works the same as with the JIT
- [ ] C) Trimming — removing unused code — is required
- [ ] D) On Windows, building requires Visual Studio with the Desktop development with C++ workload

<details><summary>Answer</summary>

**A, C, D.** Native AOT compiles everything ahead of time, so generating code at run time (`Reflection.Emit`) and loading assemblies dynamically are not possible. Trimming is required. On Windows, native linking needs the Visual Studio C++ tools.
→ [Advanced](advanced.en.md)

</details>

## 8. 🟡 Which C# version is used by default in a project with `<TargetFramework>net11.0</TargetFramework>`?

- A) C# 13
- B) C# 14
- C) C# 15
- D) The latest preview version

<details><summary>Answer</summary>

**C.** The default language version is tied to the TFM. For `net11.0` it is C# 15, for `net10.0` it is C# 14: that is what the SDK 11.0.100 compiler reports for the `#error version` directive.
→ [Advanced](advanced.en.md)

</details>

## 9. 🟡 Find the bug

An xUnit v3 test project (`xunit.v3` 4.0.3) does not run on the .NET 10 SDK or later. `dotnet test` prints: `Testing with VSTest target is no longer supported by Microsoft.Testing.Platform on .NET 10 SDK and later`.

```xml
<PropertyGroup>
  <TargetFramework>net11.0</TargetFramework>
  <OutputType>Exe</OutputType>
</PropertyGroup>
<ItemGroup>
  <PackageReference Include="xunit.v3" Version="4.0.3" />
</ItemGroup>
```

<details><summary>Answer</summary>

The project itself is correct. What is missing is a `global.json` next to the solution that switches `dotnet test` to the Microsoft.Testing.Platform mode:

```json
{
  "test": {
    "runner": "Microsoft.Testing.Platform"
  }
}
```

xUnit v3 runs on MTP, and the .NET 10 SDK and later no longer support the old VSTest mode for such projects.
→ [Advanced](advanced.en.md)

</details>

## 10. 🔴 `runtimeconfig.json` asks for `Microsoft.NETCore.App` version `10.0.0`. Runtimes 10.0.8, 10.0.10 and 11.0.0 are installed. Which one will the app run on (`RollForward` is not set)?

- A) 10.0.0 — exactly the version specified
- B) 10.0.8 — the closest to the specified one
- C) 10.0.10 — the latest patch of the same version
- D) 11.0.0 — the newest installed one

<details><summary>Answer</summary>

**C.** Moving to the latest installed patch always happens. The default `Minor` policy moves to a newer minor version only if the requested one is missing, and never moves to a new major version. That is why this module's tests, while it targeted .NET 10, asked for `"version": "10.0.0"` and ran on 10.0.10.
→ [Expert](expert.en.md)

</details>

## 11. 🔴 Why does a file-based app print `IsDynamicCodeSupported=False` on a plain `dotnet run`, although the code is executed by the JIT?

- A) File-based apps are always compiled with Native AOT
- B) A file-based app implicitly sets `PublishAot=true`, and the SDK writes a switch that forbids dynamic code into `runtimeconfig.json`
- C) `dotnet run` uses an interpreter for `.cs` files
- D) It is a bug in runtime 11.0.0

<details><summary>Answer</summary>

**B.** You can see it in the output of `dotnet project convert`: it contains `<PublishAot>true</PublishAot>`. The switch in `runtimeconfig.json` makes the runtime report Native AOT limitations already during development. Meanwhile `JitInfo` shows that the JIT is working.
→ [Expert](expert.en.md)

</details>

## 12. 🔴 Open question

Describe what happens between the `dotnet app.dll` command and the call to `Main`.

<details><summary>Key points of a good answer</summary>

- `dotnet` (the muxer) or the apphost starts hostfxr.
- hostfxr reads `app.runtimeconfig.json` and selects the shared framework version according to the roll-forward rules.
- hostpolicy uses `app.deps.json` to build the list of the app's assemblies and dependencies.
- coreclr initializes the runtime (GC, JIT), loads assemblies and calls `Main`.
- Methods are compiled by the JIT on their first call (or taken from ReadyToRun).

</details>
