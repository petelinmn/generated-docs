---
title: "Spectre.Console"
lang: en
track: csharp
kind: library
doc: overview
slug: spectre-console
part: 2
levels: [1, 2, 3, 4]
tags: [spectre-console, console, cli, tables, markup, prompts]
versions: ".NET 11, C# 15; Spectre.Console 0.57.2"
status: draft
updated: 2026-11-22
---

# Spectre.Console

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

Spectre.Console is a library for beautiful console applications: markup (colors and styles), tables, trees, panels, charts, progress, interactive prompts and command parsing (`Spectre.Console.Cli`). Everything is built from `IRenderable` and adapts to the terminal's capabilities. For tests there is `TestConsole`. How progress looks in a live terminal and `--help` were not run in this material.

## Key ideas

- Markup `[bold red]…[/]`; `[[ ]]` for brackets; `Markup.Escape` for user input.
- Widgets: `Table`, `Tree`, `Panel`, `Rule`, `BarChart` — all `IRenderable`.
- Prompts: `TextPrompt<T>`, `SelectionPrompt`, `MultiSelectionPrompt`; in a non-interactive console — an exception.
- `Progress`, `Status`, `Live` — "live" components, one at a time.
- `Spectre.Console.Cli`: settings, commands, `Validate()`.
- `IAnsiConsole` as a dependency + `TestConsole` in tests.

## Minimal example

```csharp
AnsiConsole.MarkupLine("[bold green]Done![/]");
var table = new Table().AddColumn("Name").AddColumn("Qty");
table.AddRow("apple", "3");
AnsiConsole.Write(table);
```

## When to use and when not

- ✅ CLI utilities, developer tools, console reports.
- ✅ Interactive wizards (prompts).
- ✅ Testable console output.
- ❌ Libraries (output is the application's responsibility).
- ❌ Logging for machines (use structured logs).
- ❌ A full-screen TUI (you need other tools).

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | markup, tables, trees, panels, prompts |
| [2. Advanced](advanced.en.md) | `TestConsole`, progress, `Spectre.Console.Cli` |
| [3. Expert](expert.en.md) | `IRenderable`, terminal capabilities, gotchas, 11 interview questions |
| [4. Self-check](quiz.en.md) | 10 questions and [4 exercises](exercises.en.md) with solutions |
| 5. Runnable examples | not generated yet — `/learn csharp spectre-console level 5` |
| 6. Tests | not generated yet — `/learn csharp spectre-console level 6` |

## Related materials

- Prerequisites: [01 · The .NET platform](../../modules/01-dotnet-platform/overview.en.md), [03 · Control flow and methods](../../modules/03-control-flow-and-methods/overview.en.md)
- Nearby: [16 · Unit testing](../../modules/16-testing-unit/overview.en.md), [Serilog](../serilog/overview.en.md)
- In other tracks: [Python](../../../python/overview.en.md) — `rich`; [Linux](../../../linux/overview.en.md) — the terminal and ANSI
