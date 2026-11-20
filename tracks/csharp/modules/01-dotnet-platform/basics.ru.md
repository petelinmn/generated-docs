---
title: "Платформа .NET и dotnet CLI: базовая теория"
lang: ru
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

# Платформа .NET и dotnet CLI: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md) · [упражнения](exercises.ru.md) · [5–6. Код и тесты](code/)

**Пререквизиты:** нет — это первый модуль трека.

## Из чего состоит .NET

.NET — это платформа, а не только язык. Ей нужны две вещи: среда, в которой код **выполняется**, и инструменты, которыми код **собирается**.

| Компонент | Что делает | Кому нужен |
|---|---|---|
| **Runtime** | CLR (Common Language Runtime) загружает сборки, компилирует их JIT-компилятором, управляет памятью (GC), исключениями, потоками. BCL (Base Class Library) — базовые типы, коллекции, файлы, сеть | всем, кто запускает приложение |
| **SDK** | runtime + компилятор C# (Roslyn), система сборки MSBuild, `dotnet` CLI, шаблоны проектов, клиент NuGet | разработчику |

Runtime поставляется в виде **shared frameworks** — наборов сборок, общих для всех приложений на машине:

- `Microsoft.NETCore.App` — базовый, нужен любому приложению;
- `Microsoft.AspNetCore.App` — веб (ASP.NET Core);
- `Microsoft.WindowsDesktop.App` — WinForms и WPF, только Windows.

## SDK и runtime — разные версии

Самая частая путаница новичка — считать версию SDK версией платформы. Вот реальный вывод на машине, где писался этот модуль:

```text
> dotnet --version
11.0.100

> dotnet --list-runtimes   (фрагмент)
Microsoft.AspNetCore.App 11.0.0 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App]
Microsoft.NETCore.App 11.0.0 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]
```

SDK `11.0.100` и runtime `11.0.0` — разные продукты со своими номерами. У SDK третья часть номера состоит из «полосы возможностей» (feature band, сотни) и патча: в `11.0.100` это полоса `1xx` (первая полоса новой версии) и патч `00`, а в `10.0.302` — полоса `3xx` и патч `02`. Приложение выполняет runtime, и узнать его версию можно из кода:

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

Код: [Ex01_RuntimeInfo.cs](code/Examples/Ex01_RuntimeInfo.cs), тест: [ExamplesTests.cs](code/Tests/ExamplesTests.cs).

## Первая программа и dotnet CLI

```bash
dotnet new console -o HelloApp   # create a project from a template
cd HelloApp
dotnet run                       # restore + build + run
```

Шаблон создаёт `HelloApp.csproj` и `Program.cs` с одной строкой `Console.WriteLine("Hello, World!");`. Класса `Program` и метода `Main` в файле нет: это **top-level statements**, компилятор генерирует их сам.

Основные команды:

| Команда | Что делает |
|---|---|
| `dotnet new <шаблон>` | создать проект или файл из шаблона (`console`, `classlib`, `xunit`, `sln`, …) |
| `dotnet build` | собрать (перед этим выполняется restore) |
| `dotnet run` | собрать и запустить |
| `dotnet test` | собрать и запустить тесты |
| `dotnet add package <имя>` | добавить NuGet-пакет в проект |
| `dotnet restore` | скачать зависимости |
| `dotnet publish` | подготовить приложение к развёртыванию |
| `dotnet sln add <проект>` | добавить проект в решение |
| `dotnet --info` | версии SDK, runtime и сведения об окружении |

## Проект: файл .csproj

Проект — это файл MSBuild с расширением `.csproj`. Так выглядит проект с примерами этого модуля:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net11.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

</Project>
```

- `Sdk="Microsoft.NET.Sdk"` подключает стандартные правила сборки. В веб-проектах используется `Microsoft.NET.Sdk.Web`.
- `TargetFramework` (TFM) — под какую платформу собирать: `net11.0`.
- `ImplicitUsings` добавляет глобальные `using` для частых пространств имён (`System`, `System.IO`, `System.Linq`, `System.Collections.Generic`, …).
- `Nullable` включает анализ null-ссылок — подробно в модуле [02 · Типы и переменные](../02-types-and-variables/advanced.ru.md#nullable-reference-types).
- `OutputType` здесь не указан, поэтому получается библиотека (`.dll`). Для приложения пишут `<OutputType>Exe</OutputType>`.
- Перечислять `.cs`-файлы не нужно: SDK-проект включает все файлы из своей папки и подпапок.

Настройки проекта попадают в скомпилированную сборку. Например, TFM превращается в атрибут:

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

Код: [Ex02_AssemblyInfo.cs](code/Examples/Ex02_AssemblyInfo.cs).

## Решение: файл .slnx

Решение (solution) объединяет несколько проектов, чтобы собирать и тестировать их одной командой. Начиная с .NET 10 `dotnet new sln` по умолчанию создаёт решение в формате **`.slnx`** — короткий XML вместо старого `.sln`. Решение этого модуля:

```xml
<Solution>
  <Project Path="Examples/Examples.csproj" />
  <Project Path="Solutions/Solutions.csproj" />
  <Project Path="Tests/Tests.csproj" />
</Solution>
```

`dotnet test` в папке с решением собирает все три проекта и запускает тесты.

## File-based apps: программа без проекта

Начиная с .NET 10 программу из одного файла можно запустить без `.csproj`:

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

Всё, что обычно пишут в `.csproj`, задаётся директивами `#:` в начале файла:

| Директива | Пример | Назначение |
|---|---|---|
| `#:package` | `#:package Humanizer@2.14.1` | NuGet-пакет |
| `#:property` | `#:property AssemblyName=HelloFileApp` | свойство MSBuild |
| `#:sdk` | `#:sdk Microsoft.NET.Sdk.Web` | другой SDK, например для веба |
| `#:project` | `#:project ../Examples/Examples.csproj` | ссылка на проект |
| `#:include` | | включить другие файлы (SDK .NET 11; в SDK 10 — начиная с 10.0.300) |

Целевая платформа file-based app — та, что у SDK, которым его запускают: SDK 11 собирает файл под `net11.0` (C# 15), а SDK 10 — под `net10.0` (C# 14), и тогда вывод заканчивается на `on .NET 10`. Зафиксировать платформу можно директивой `#:property TargetFramework=net10.0`.

Когда скрипт вырастает, его превращают в обычный проект командой `dotnet project convert hello.cs`. File-based apps удобны для экспериментов, утилит и изучения языка.

Код: [hello.cs](code/FileBasedApp/hello.cs), тест: [FileBasedAppTests.cs](code/Tests/FileBasedAppTests.cs).

## NuGet: пакеты

NuGet — менеджер пакетов .NET. Команда `dotnet add package Serilog` добавляет в `.csproj` строку `<PackageReference Include="Serilog" Version="…" />`. При restore пакеты скачиваются в глобальную папку `%USERPROFILE%\.nuget\packages` (в Linux и macOS — `~/.nuget/packages`) и переиспользуются всеми проектами.

Папки `bin/` (результат сборки) и `obj/` (промежуточные файлы) создаются автоматически. Их не коммитят: они уже в `.gitignore` этого репозитория.

## Связанные материалы

- Дальше: [02 · Типы и переменные](../02-types-and-variables/overview.ru.md), [03 · Управляющие конструкции и методы](../03-control-flow-and-methods/overview.ru.md)
- То же в других треках: [Python](../../../python/overview.ru.md) — интерпретатор CPython, uv и `pyproject.toml` вместо dotnet CLI и `.csproj`; [Kubernetes](../../../kubernetes/overview.ru.md) — контейнеры для .NET-приложений

## Источники

- [Обзор .NET](https://learn.microsoft.com/dotnet/core/introduction)
- [dotnet CLI](https://learn.microsoft.com/dotnet/core/tools/)
- [File-based apps](https://learn.microsoft.com/dotnet/core/sdk/file-based-apps)
- [Настройки компиляции runtime: tiered compilation, PGO](https://learn.microsoft.com/dotnet/core/runtime-config/compilation)
- [Tiered compilation: дизайн (.NET Core 2.1)](https://github.com/dotnet/runtime/blob/main/docs/design/features/tiered-compilation.md)
- [Native AOT](https://learn.microsoft.com/dotnet/core/deploying/native-aot/)
- [Выбор версии .NET и roll-forward](https://learn.microsoft.com/dotnet/core/versions/selection)
- [Релизы и поддержка .NET](https://learn.microsoft.com/dotnet/core/releases-and-support)
- [dotnet test и Microsoft.Testing.Platform](https://learn.microsoft.com/dotnet/core/testing/unit-testing-with-dotnet-test)
- [Версия языка C#](https://learn.microsoft.com/dotnet/csharp/language-reference/configure-language-version)
