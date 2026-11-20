---
title: "Квиз: Платформа .NET и dotnet CLI"
lang: ru
track: csharp
kind: module
doc: quiz
slug: 01-dotnet-platform
part: 1
tags: [dotnet, dotnet-cli, sdk, runtime, jit, native-aot]
status: verified
updated: 2026-11-20
---

# Квиз: Платформа .NET и dotnet CLI

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает, из какого уровня вопрос: 🟢 [базовая теория](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Что входит в SDK, но не входит в runtime?

- A) Сборщик мусора (GC)
- B) Компилятор C# (Roslyn) и MSBuild
- C) Базовая библиотека классов (BCL)
- D) JIT-компилятор

<details><summary>Ответ</summary>

**B.** SDK = runtime + инструменты разработки: компилятор, MSBuild, dotnet CLI, шаблоны. GC, JIT и BCL — части runtime: они нужны любому запущенному приложению, даже на сервере без SDK.
→ [Базовая теория](basics.ru.md)

</details>

## 2. 🟢 Какая команда покажет установленные версии runtime?

- A) `dotnet --version`
- B) `dotnet --list-sdks`
- C) `dotnet --list-runtimes`
- D) `dotnet new list`

<details><summary>Ответ</summary>

**C.** `dotnet --version` показывает версию SDK (например, `11.0.100`), а не runtime (`11.0.0`). `--list-sdks` — список SDK, `dotnet new list` — список шаблонов.
→ [Базовая теория](basics.ru.md)

</details>

## 3. 🟢 В какой сборке находится тип `System.String`?

- A) `System.Runtime`
- B) `System.Private.CoreLib`
- C) `mscorlib`
- D) `System.String.dll`

<details><summary>Ответ</summary>

**B.** Базовые типы во время выполнения живут в `System.Private.CoreLib`, это проверено тестом в примере [Ex02_AssemblyInfo](code/Examples/Ex02_AssemblyInfo.cs). `System.Runtime` — сборка, на которую ссылается ваш код при компиляции, она перенаправляет типы в CoreLib. `mscorlib` — сборка старого .NET Framework.
→ [Базовая теория](basics.ru.md)

</details>

## 4. 🟢 Как в .NET 10 и новее запустить программу из одного файла `hello.cs`, не создавая проект?

- A) Сначала создать проект: `dotnet new console`
- B) `dotnet run hello.cs`
- C) `dotnet hello.exe`
- D) Установить C# Interactive и выполнить `csi hello.cs`

<details><summary>Ответ</summary>

**B.** Это file-based app: SDK сам создаёт виртуальный проект. Настройки задаются директивами `#:package`, `#:property` и другими в начале файла.
→ [Базовая теория](basics.ru.md)

</details>

## 5. 🟢 В каком формате `dotnet new sln` создаёт решение начиная с .NET 10?

- A) `.sln`
- B) `.slnx`
- C) `.csproj`
- D) `solution.json`

<details><summary>Ответ</summary>

**B.** Начиная с .NET 10 по умолчанию создаётся `.slnx` — компактный XML-формат. Старый `.sln` по-прежнему поддерживается.
→ [Базовая теория](basics.ru.md)

</details>

## 6. 🟡 Чему будут равны `firstCall` и `secondCall`?

```csharp
Action call = () => JitObserver.Double(21);

long firstCall = JitObserver.CountJitCompilations(call);
long secondCall = JitObserver.CountJitCompilations(call);
```

`CountJitCompilations` считает методы, скомпилированные JIT-ом на текущем потоке во время вызова.

- A) `1` и `1`
- B) `2` и `0`
- C) `0` и `0`
- D) `1` и `0`

<details><summary>Ответ</summary>

**B.** Реальный результат — `2` и `0`. При первом вызове JIT компилирует две вещи: тело лямбды и метод `Double`. При втором вызове оба уже скомпилированы. Если бы во второй раз передавалась новая лямбда, её пришлось бы компилировать отдельно.
→ [Продвинутая теория](advanced.ru.md)

</details>

## 7. 🟡 Выбери все верные утверждения о Native AOT

- [ ] A) Во время выполнения нет JIT-компилятора
- [ ] B) `System.Reflection.Emit` работает так же, как с JIT
- [ ] C) Требуется trimming — удаление неиспользуемого кода
- [ ] D) В Windows для сборки нужен Visual Studio с workload Desktop development with C++

<details><summary>Ответ</summary>

**A, C, D.** Native AOT компилирует всё заранее, поэтому генерировать код во время выполнения (`Reflection.Emit`) и динамически загружать сборки нельзя. Trimming обязателен. В Windows для нативной линковки нужны C++-инструменты Visual Studio.
→ [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Какая версия C# используется по умолчанию в проекте с `<TargetFramework>net11.0</TargetFramework>`?

- A) C# 13
- B) C# 14
- C) C# 15
- D) Последняя preview-версия

<details><summary>Ответ</summary>

**C.** Версия языка по умолчанию привязана к TFM. Для `net11.0` это C# 15, для `net10.0` — C# 14: так сообщает компилятор SDK 11.0.100 на директиву `#error version`.
→ [Продвинутая теория](advanced.ru.md)

</details>

## 9. 🟡 Найди ошибку

Тестовый проект на xUnit v3 (`xunit.v3` 4.0.3) в .NET 10 SDK и новее не запускается. `dotnet test` пишет: `Testing with VSTest target is no longer supported by Microsoft.Testing.Platform on .NET 10 SDK and later`.

```xml
<PropertyGroup>
  <TargetFramework>net11.0</TargetFramework>
  <OutputType>Exe</OutputType>
</PropertyGroup>
<ItemGroup>
  <PackageReference Include="xunit.v3" Version="4.0.3" />
</ItemGroup>
```

<details><summary>Ответ</summary>

Сам проект верный. Не хватает `global.json` рядом с решением, который переключает `dotnet test` в режим Microsoft.Testing.Platform:

```json
{
  "test": {
    "runner": "Microsoft.Testing.Platform"
  }
}
```

xUnit v3 работает на MTP, а старый режим VSTest для таких проектов в .NET 10 SDK и новее не поддерживается.
→ [Продвинутая теория](advanced.ru.md)

</details>

## 10. 🔴 В `runtimeconfig.json` указан `Microsoft.NETCore.App` версии `10.0.0`. Установлены runtime 10.0.8, 10.0.10 и 11.0.0. На каком запустится приложение (`RollForward` не задан)?

- A) 10.0.0 — ровно та версия, что указана
- B) 10.0.8 — ближайшая к указанной
- C) 10.0.10 — последний патч той же версии
- D) 11.0.0 — самая новая из установленных

<details><summary>Ответ</summary>

**C.** Переход на последний установленный патч происходит всегда. Политика по умолчанию `Minor` переходит на более новую minor-версию, только если нужной нет, и никогда не переходит на новую мажорную версию. Так тесты этого модуля, пока он был нацелен на .NET 10, запрашивали `"version": "10.0.0"` и выполнялись на 10.0.10.
→ [Экспертная теория](expert.ru.md)

</details>

## 11. 🔴 Почему file-based app при обычном `dotnet run` выводит `IsDynamicCodeSupported=False`, хотя код выполняется JIT-ом?

- A) File-based apps всегда компилируются Native AOT
- B) В file-based app неявно включён `PublishAot=true`, и SDK записывает в `runtimeconfig.json` переключатель, запрещающий динамический код
- C) `dotnet run` для `.cs` файлов использует интерпретатор
- D) Это ошибка runtime 11.0.0

<details><summary>Ответ</summary>

**B.** Это видно в результате `dotnet project convert`: в нём есть `<PublishAot>true</PublishAot>`. Переключатель в `runtimeconfig.json` заставляет runtime сообщать об ограничениях Native AOT уже при разработке. При этом `JitInfo` показывает, что JIT работает.
→ [Экспертная теория](expert.ru.md)

</details>

## 12. 🔴 Открытый вопрос

Опиши, что происходит между командой `dotnet app.dll` и вызовом `Main`.

<details><summary>Ключевые пункты ответа</summary>

- `dotnet` (muxer) или apphost запускает hostfxr.
- hostfxr читает `app.runtimeconfig.json` и выбирает версию shared framework по правилам roll-forward.
- hostpolicy по `app.deps.json` формирует список сборок приложения и зависимостей.
- coreclr инициализирует runtime (GC, JIT), загружает сборки и вызывает `Main`.
- Методы компилируются JIT-ом при первом вызове (или берутся из ReadyToRun).

</details>
