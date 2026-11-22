---
title: "Spectre.Console: basics"
lang: en
track: csharp
kind: library
doc: basics
slug: spectre-console
part: 2
tags: [spectre-console, console, cli, tables, markup]
versions: ".NET 11, C# 15; Spectre.Console 0.57.2, Spectre.Console.Testing 0.57.2"
status: draft
updated: 2026-11-22
---

# Spectre.Console: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [01 · The .NET platform](../../modules/01-dotnet-platform/overview.en.md), [03 · Control flow and methods](../../modules/03-control-flow-and-methods/overview.en.md). **Time:** ~30 min.

## What it is and why

Spectre.Console is a library for beautiful console applications: colored text through markup, tables, trees, panels, charts, progress indicators, interactive prompts and (in the `Spectre.Console.Cli` package) command-line parsing. The library detects the terminal's capabilities (colors, Unicode, interactivity) and adapts the output.

```bash
dotnet add package Spectre.Console
dotnet add package Spectre.Console.Testing   # test console for tests
```

## Markup

```csharp
AnsiConsole.MarkupLine("[bold red]Hello[/] [green]world[/]!");
AnsiConsole.MarkupLine("Escape brackets: [[not a tag]]");
AnsiConsole.MarkupLine("User input: " + Markup.Escape(userInput));
```

Output (verified through a test console without colors): `Hello world!`, `Escape brackets: [not a tag]`, `User input: [red]x[/]`.

- Tags are `[style]text[/]` (a color, `bold`, `underline`…).
- Square brackets in text are doubled: `[[` and `]]`.
- **Escape user input** with `Markup.Escape(...)`: otherwise `[red]x[/]` is taken for markup (verified: after `Escape` the output is literally `[red]x[/]`).
- An unknown style is an exception: verified `InvalidOperationException: Could not find color or style 'unknown'.`

## Tables, trees, panels

```csharp
var table = new Table().Border(TableBorder.Ascii).AddColumn("Name").AddColumn(new TableColumn("Qty").RightAligned());
table.AddRow("apple", "3");
table.AddRow("kiwi", "12");
AnsiConsole.Write(table);
```

The result (verified):

```text
+-------------+
| Name  | Qty |
|-------+-----|
| apple |   3 |
| kiwi  |  12 |
+-------------+
```

A tree (`Tree`) prints a hierarchy (`root → child → leaf`) with `└──`; `Panel("inside").Header("Title")` — a frame with a title; `Rule("section")` — a horizontal line with a label; `BarChart` — horizontal bars. All of these are `IRenderable` objects: they can be nested in each other.

## Prompting the user

```csharp
var name = AnsiConsole.Prompt(new TextPrompt<string>("Name?"));
var age  = AnsiConsole.Prompt(new TextPrompt<int>("Age?").ValidationErrorMessage("not a number"));
var pick = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Pick").AddChoices("one", "two", "three"));
```

Verified on a test console with scripted input: the text `Ann` and the number `42` were read; for `TextPrompt<int>` the input `abc` produced an error message and a repeated prompt, then `7` was accepted; `SelectionPrompt` after `↓` and `Enter` returned `two`; `MultiSelectionPrompt` (space, `↓↓`, space, `Enter`) returned `a,c`. In a **non-interactive** console (output redirected, no TTY) a prompt throws `InvalidOperationException` (verified).

## Beginner mistakes

- ❌ Inserting user text into markup without `Markup.Escape` → ✅ escape it.
- ❌ Forgetting to double `[` and `]` in plain text → ✅ `[[` and `]]`.
- ❌ Using prompts in a non-interactive environment (CI, redirection) → ✅ check `AnsiConsole.Profile.Capabilities.Interactive` or take values as arguments.
- ❌ Progress/`Status` together with other console output → ✅ one "live" component at a time ([advanced](advanced.en.md#progress-and-live-components)).
- ❌ Expecting colors everywhere → ✅ a terminal may not support ANSI.

## Related materials

- Next: [advanced](advanced.en.md) — progress, console testing, `Spectre.Console.Cli`.
- [01 · The .NET platform](../../modules/01-dotnet-platform/overview.en.md) — console applications.
- [16 · Unit testing](../../modules/16-testing-unit/overview.en.md) — output tests.
- [Linux](../../../linux/overview.en.md) — the terminal, ANSI and TTY.
- [Python](../../../python/overview.en.md) — the `rich` library.

## Sources

- [Spectre.Console: documentation](https://spectreconsole.net/)
- [Spectre.Console: Markup](https://spectreconsole.net/console/how-to/markup)
- [Spectre.Console on GitHub](https://github.com/spectreconsole/spectre.console)
