---
title: "Exercises: Spectre.Console"
lang: en
track: csharp
kind: library
doc: exercises
slug: spectre-console
part: 2
tags: [spectre-console, tables, prompts, spectre-console-cli, testconsole]
status: draft
updated: 2026-11-22
---

# Exercises: Spectre.Console

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself. The solutions were verified by running on `TestConsole` (Spectre.Console 0.57.2, Spectre.Console.Testing 0.57.2, Spectre.Console.Cli 0.57.2). There are no runnable versions in `code/` (level 5 does not exist).

## 1. 🟢 Markup and escaping

**Task:** print `Hello world!` with colors, literal brackets `[not a tag]` and user input `[red]x[/]` without interpretation.

**Expected result:** `Hello world!`, `Escape brackets: [not a tag]`, `User input: [red]x[/]`.

<details><summary>Hint</summary>

`[[`/`]]` and `Markup.Escape(...)`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 1: markup, escaping brackets and user input.
c.MarkupLine("[bold red]Hello[/] [green]world[/]!");
c.MarkupLine("Escape brackets: [[not a tag]]");
c.MarkupLine("User input: " + Markup.Escape("[red]x[/]"));
```

Verified: `Hello world!`, `Escape brackets: [not a tag]`, `User input: [red]x[/]` were printed.

</details>

## 2. 🟡 A table

**Task:** a table `Name | Qty` (the number right-aligned) with two rows: `apple 3`, `kiwi 12`; an ASCII border.

**Expected result:**

```text
+-------------+
| Name  | Qty |
|-------+-----|
| apple |   3 |
| kiwi  |  12 |
+-------------+
```

<details><summary>Hint</summary>

`new Table().Border(TableBorder.Ascii)`, `TableColumn("Qty").RightAligned()`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 2: a table with a right-aligned column.
var table = new Table().Border(TableBorder.Ascii).AddColumn("Name").AddColumn(new TableColumn("Qty").RightAligned());
table.AddRow("apple", "3");
table.AddRow("kiwi", "12");
c.Write(table);
```

Verified: the output matched the expected one.

</details>

## 3. 🟡 Prompts

**Task:** read a name and an age; on the input `abc` for the age print an error and repeat the prompt; then a choice from `one/two/three` with the down arrow.

**Expected result:** `Ann, 42`; after `abc` and `7` — `7`, the error shown; the choice `two`.

<details><summary>Hint</summary>

`TestConsole` with `Capabilities.Interactive = true`, `Input.PushTextWithEnter(...)`, `Input.PushKey(ConsoleKey.DownArrow)`; `TextPrompt<int>(...).ValidationErrorMessage(...)`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 3: prompts with scripted input.
var pc = new TestConsole();
pc.Profile.Capabilities.Interactive = true;
pc.Input.PushTextWithEnter("abc");
pc.Input.PushTextWithEnter("7");
var age = pc.Prompt(new TextPrompt<int>("Age?").ValidationErrorMessage("[red]not a number[/]"));

pc3.Input.PushKey(ConsoleKey.DownArrow);
pc3.Input.PushKey(ConsoleKey.Enter);
var pick = pc3.Prompt(new SelectionPrompt<string>().Title("Pick").AddChoices("one", "two", "three"));
```

Verified: `prompt results: Ann, 42`; `invalid then valid: 7 | error shown: True`; `selection: two`.

</details>

## 4. 🔴 A CLI command with validation

**Task:** a command `greet <name> [--times N] [--shout]`. Check: a successful call, a non-numeric `--times`, a missing argument, `--times 0`.

**Expected result:** code `0` and `HELLO ANN` twice; `Failed to convert 'x' to Int32.`; `Command 'greet' is missing required argument 'name'.`; `times must be >= 1`.

<details><summary>Hint</summary>

`CommandSettings` with `[CommandArgument]`/`[CommandOption]`, `Validate()` and `c.PropagateExceptions()` for the test.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 4: a Spectre.Console.Cli command with validation.
class GreetSettings : CommandSettings
{
    [CommandArgument(0, "<name>")] public string Name { get; set; } = "";
    [CommandOption("-t|--times")] [DefaultValue(1)] public int Times { get; set; }
    [CommandOption("--shout")] public bool Shout { get; set; }
    public override ValidationResult Validate() => Times < 1 ? ValidationResult.Error("times must be >= 1") : ValidationResult.Success();
}
```

Verified: `exit(ok): 0`; `bad option: CommandRuntimeException - Failed to convert 'x' to Int32.`; `missing arg: … missing required argument 'name'.`; `validation: … times must be >= 1`. In the check the command wrote to the static `AnsiConsole` — to print to a test console inject `IAnsiConsole`.

</details>
