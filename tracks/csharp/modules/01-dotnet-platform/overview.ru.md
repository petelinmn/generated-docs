---
title: "Платформа .NET и dotnet CLI"
lang: ru
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

# Платформа .NET и dotnet CLI

🌐 **Русский** · [English](overview.en.md)

## Коротко

.NET — платформа, на которой выполняется код на C#. Компилятор превращает исходники в IL (промежуточный язык), а среда выполнения CLR при запуске компилирует IL в машинный код и управляет памятью. Все действия разработчика — создать проект, собрать, запустить, протестировать, опубликовать — выполняются одной утилитой `dotnet`.

## Ключевые идеи

- **SDK ≠ runtime.** SDK (компилятор, MSBuild, CLI) нужен для сборки, runtime (CLR и базовая библиотека) — для запуска. У них разные номера версий: SDK `11.0.100`, runtime `11.0.0`.
- **Проект — файл `.csproj`**, решение — файл `.slnx`. Начиная с .NET 10 `dotnet new sln` создаёт `.slnx` по умолчанию.
- **File-based apps** (.NET 10): один `.cs` файл запускается командой `dotnet run app.cs`, без проекта. Пакеты и настройки задаются директивами `#:package`, `#:property`.
- **JIT** компилирует метод при первом вызове. Tiered compilation сначала компилирует быстро, а горячие методы потом оптимизирует.
- **Native AOT** собирает нативный исполняемый файл без JIT: быстрый старт, но нельзя генерировать код во время выполнения.
- **Версии:** новая версия .NET выходит каждый ноябрь. Чётные — LTS (3 года поддержки), нечётные — STS (2 года). .NET 11 — STS с поддержкой до ноября 2028 года, язык по умолчанию — C# 15; .NET 10 — LTS.
- **NuGet** — менеджер пакетов: `dotnet add package <имя>`.

## Минимальный пример

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

## Когда применять, а когда нет

- ✅ `dotnet new` + `.csproj` — для любого приложения или библиотеки, которые будут жить дольше одного вечера.
- ✅ File-based app — для экспериментов, утилит, изучения языка.
- ✅ Native AOT — для CLI-утилит, serverless и контейнеров, где важны старт и размер.
- ❌ Native AOT — если код опирается на `Reflection.Emit`, динамическую загрузку сборок или библиотеки, несовместимые с trimming.
- ❌ File-based app — для приложения из нескольких проектов: переведи его в проект командой `dotnet project convert`.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | SDK и runtime, dotnet CLI, `.csproj`, `.slnx`, file-based apps, NuGet |
| [2. Продвинутая теория](advanced.ru.md) | путь от исходника до машинного кода, JIT, tiered compilation, ReadyToRun и Native AOT, публикация, версии, `global.json` |
| [3. Экспертная теория](expert.ru.md) | запуск приложения (hostfxr, hostpolicy, coreclr), roll-forward, IL изнутри, ловушка `PublishAot`; справочник по теме, подвохи, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) с решениями |
| [5. Запускаемые примеры](code/) | `code/Examples/` и `code/Solutions/` — решение `Code.slnx` |
| [6. Тесты](code/) | `code/Tests/` — `dotnet test` в папке `code/` |


## Связанные материалы

- Дальше: [02 · Типы и переменные](../02-types-and-variables/overview.ru.md)
- То же в других треках: [Python](../../../python/overview.ru.md) — интерпретатор и uv вместо CLR и dotnet CLI
- Рецепты и библиотеки: [Spectre.Console](../../libraries/spectre-console/overview.ru.md)
