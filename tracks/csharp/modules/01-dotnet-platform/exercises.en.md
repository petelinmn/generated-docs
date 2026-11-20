---
title: "Exercises: The .NET platform and dotnet CLI"
lang: en
track: csharp
kind: module
doc: exercises
slug: 01-dotnet-platform
part: 1
tags: [dotnet, reflection, runtimeconfig]
status: verified
updated: 2026-11-20
---

# Exercises: The .NET platform and dotnet CLI

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. Solutions are under spoilers, runnable versions are in [code/Solutions/](code/Solutions/), their tests are in [code/Tests/SolutionsTests.cs](code/Tests/SolutionsTests.cs).

## 1. 🟢 Platform report

**Task:** write a `PlatformReport.Create()` method that returns a single line with the runtime version, the OS and the processor architecture.

**Expected result:**

```text
Runtime: .NET 11.0.0 | OS: Microsoft Windows 10.0.26200 | Arch: X64
```

<details><summary>Hint</summary>

Everything you need is in the `System.Runtime.InteropServices.RuntimeInformation` class.

</details>

<details><summary>Solution</summary>

```csharp
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
```

[S01_PlatformReport.cs](code/Solutions/S01_PlatformReport.cs) — three `RuntimeInformation` properties: `FrameworkDescription`, `OSDescription`, `ProcessArchitecture`. The test checks the format with a regular expression, because versions and operating systems differ from machine to machine.

</details>

## 2. 🟡 Assembly dependencies

**Task:** write `AssemblyInspector.GetReferencedAssemblyNames(Assembly assembly)` — an ordinally sorted list of the names of assemblies that `assembly` references, without duplicates.

**Expected result:** for the `Solutions` assembly, the list contains `System.Runtime`.

<details><summary>Hint</summary>

References are stored in the assembly metadata and are available through `Assembly.GetReferencedAssemblies()`. `Order(StringComparer.Ordinal)` works for sorting.

</details>

<details><summary>Solution</summary>

```csharp
// Solution: exercise 2 — which assemblies an assembly depends on.
// Module: csharp/01-dotnet-platform (exercises)
// Tests: dotnet test --filter "FullyQualifiedName~AssemblyInspector"
using System.Reflection;

namespace Solutions;

public static class AssemblyInspector
{
    // References are recorded in the assembly metadata at compile time.
    public static IReadOnlyList<string> GetReferencedAssemblyNames(Assembly assembly) =>
        assembly.GetReferencedAssemblies()
            .Select(reference => reference.Name)
            .OfType<string>()
            .Distinct()
            .Order(StringComparer.Ordinal)
            .ToList();
}
```

[S02_AssemblyInspector.cs](code/Solutions/S02_AssemblyInspector.cs). Note that the references list `System.Runtime`, not `System.Private.CoreLib`. Code is compiled against reference assemblies, and the runtime finds the real types at startup.

</details>

## 3. 🔴 Which runtime does the app ask for

**Task:** write `RuntimeConfigReader.ReadFrameworks(string json)`. The method parses the contents of a `runtimeconfig.json` and returns a list of `(Name, Version)` pairs of shared frameworks. Handle both variants: a single `"framework"` (a console app) and a `"frameworks"` array (e.g. ASP.NET Core).

**Expected result:**

```text
{"runtimeOptions": {"framework": {"name": "Microsoft.NETCore.App", "version": "11.0.0"}}}
→ [("Microsoft.NETCore.App", "11.0.0")]
```

For the real `Tests.runtimeconfig.json` (next to `Tests.dll`), the result is a single framework, `Microsoft.NETCore.App`, version `11.0.*`.

<details><summary>Hint</summary>

`System.Text.Json.JsonDocument.Parse`, then `GetProperty("runtimeOptions")` and `TryGetProperty` for both variants.

</details>

<details><summary>Solution</summary>

```csharp
// Solution: exercise 3 — which shared frameworks an app asks the host for.
// Module: csharp/01-dotnet-platform (exercises)
// Tests: dotnet test --filter "FullyQualifiedName~RuntimeConfigReader"
using System.Text.Json;

namespace Solutions;

public static class RuntimeConfigReader
{
    // A console app has a single "framework"; an ASP.NET Core app may list several in "frameworks".
    public static IReadOnlyList<(string Name, string Version)> ReadFrameworks(string json)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        JsonElement options = document.RootElement.GetProperty("runtimeOptions");

        var result = new List<(string Name, string Version)>();
        if (options.TryGetProperty("framework", out JsonElement single))
        {
            result.Add(ReadOne(single));
        }
        if (options.TryGetProperty("frameworks", out JsonElement many))
        {
            foreach (JsonElement framework in many.EnumerateArray())
            {
                result.Add(ReadOne(framework));
            }
        }
        return result;
    }

    private static (string Name, string Version) ReadOne(JsonElement framework) =>
        (framework.GetProperty("name").GetString() ?? "",
         framework.GetProperty("version").GetString() ?? "");
}
```

[S03_RuntimeConfigReader.cs](code/Solutions/S03_RuntimeConfigReader.cs). Think about which runtime the app will run on if the file asks for `11.0.0` and `11.0.0` and `11.0.2` are installed: the answer is in the roll-forward section of [Expert](expert.en.md).

</details>
