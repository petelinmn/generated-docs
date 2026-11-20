---
title: "Упражнения: Платформа .NET и dotnet CLI"
lang: ru
track: csharp
kind: module
doc: exercises
slug: 01-dotnet-platform
part: 1
tags: [dotnet, reflection, runtimeconfig]
status: verified
updated: 2026-11-20
---

# Упражнения: Платформа .NET и dotnet CLI

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки. Решения — под спойлерами, запускаемые версии — в [code/Solutions/](code/Solutions/), тесты к ним — в [code/Tests/SolutionsTests.cs](code/Tests/SolutionsTests.cs).

## 1. 🟢 Отчёт о платформе

**Задача:** напиши метод `PlatformReport.Create()`, который возвращает строку с версией runtime, ОС и архитектурой процессора в одну строку.

**Ожидаемый результат:**

```text
Runtime: .NET 11.0.0 | OS: Microsoft Windows 10.0.26200 | Arch: X64
```

<details><summary>Подсказка</summary>

Всё нужное есть в классе `System.Runtime.InteropServices.RuntimeInformation`.

</details>

<details><summary>Решение</summary>

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

[S01_PlatformReport.cs](code/Solutions/S01_PlatformReport.cs) — три свойства `RuntimeInformation`: `FrameworkDescription`, `OSDescription`, `ProcessArchitecture`. Тест проверяет формат регулярным выражением, потому что версии и ОС у всех разные.

</details>

## 2. 🟡 Зависимости сборки

**Задача:** напиши `AssemblyInspector.GetReferencedAssemblyNames(Assembly assembly)` — отсортированный по ординалу список имён сборок, на которые ссылается `assembly`, без повторов.

**Ожидаемый результат:** для сборки `Solutions` список содержит `System.Runtime`.

<details><summary>Подсказка</summary>

Ссылки хранятся в метаданных сборки и доступны через `Assembly.GetReferencedAssemblies()`. Для сортировки подойдёт `Order(StringComparer.Ordinal)`.

</details>

<details><summary>Решение</summary>

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

[S02_AssemblyInspector.cs](code/Solutions/S02_AssemblyInspector.cs). Обрати внимание: в ссылках указан `System.Runtime`, а не `System.Private.CoreLib`. Код компилируется против reference-сборок, а реальные типы runtime находит при запуске.

</details>

## 3. 🔴 Какой runtime просит приложение

**Задача:** напиши `RuntimeConfigReader.ReadFrameworks(string json)`. Метод разбирает содержимое `runtimeconfig.json` и возвращает список пар `(Name, Version)` shared frameworks. Учти оба варианта: одиночный `"framework"` (консольное приложение) и массив `"frameworks"` (например, ASP.NET Core).

**Ожидаемый результат:**

```text
{"runtimeOptions": {"framework": {"name": "Microsoft.NETCore.App", "version": "11.0.0"}}}
→ [("Microsoft.NETCore.App", "11.0.0")]
```

Для реального `Tests.runtimeconfig.json` (лежит рядом с `Tests.dll`) результат — один фреймворк `Microsoft.NETCore.App` версии `11.0.*`.

<details><summary>Подсказка</summary>

`System.Text.Json.JsonDocument.Parse`, затем `GetProperty("runtimeOptions")` и `TryGetProperty` для обоих вариантов.

</details>

<details><summary>Решение</summary>

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

[S03_RuntimeConfigReader.cs](code/Solutions/S03_RuntimeConfigReader.cs). Подумай, на каком runtime запустится приложение, если файл просит `11.0.0`, а установлены `11.0.0` и `11.0.2`: ответ — в разделе про roll-forward в [Экспертная теория](expert.ru.md).

</details>
