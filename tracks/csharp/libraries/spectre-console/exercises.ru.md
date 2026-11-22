---
title: "Упражнения: Spectre.Console"
lang: ru
track: csharp
kind: library
doc: exercises
slug: spectre-console
part: 2
tags: [spectre-console, tables, prompts, spectre-console-cli, testconsole]
status: draft
updated: 2026-11-22
---

# Упражнения: Spectre.Console

🌐 **Русский** · [English](exercises.en.md)

> Решайте сами. Решения проверены запуском на `TestConsole` (Spectre.Console 0.57.2, Spectre.Console.Testing 0.57.2, Spectre.Console.Cli 0.57.2). Запускаемых версий в `code/` нет (уровня 5 нет).

## 1. 🟢 Разметка и экранирование

**Задача:** выведите `Hello world!` с цветами, буквальные скобки `[not a tag]` и пользовательский ввод `[red]x[/]` без интерпретации.

**Ожидаемый результат:** `Hello world!`, `Escape brackets: [not a tag]`, `User input: [red]x[/]`.

<details><summary>Подсказка</summary>

`[[`/`]]` и `Markup.Escape(...)`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 1: markup, escaping brackets and user input.
c.MarkupLine("[bold red]Hello[/] [green]world[/]!");
c.MarkupLine("Escape brackets: [[not a tag]]");
c.MarkupLine("User input: " + Markup.Escape("[red]x[/]"));
```

Проверено: выведено `Hello world!`, `Escape brackets: [not a tag]`, `User input: [red]x[/]`.

</details>

## 2. 🟡 Таблица

**Задача:** таблица `Name | Qty` (число выровнено вправо) на двух строках: `apple 3`, `kiwi 12`; рамка ASCII.

**Ожидаемый результат:**

```text
+-------------+
| Name  | Qty |
|-------+-----|
| apple |   3 |
| kiwi  |  12 |
+-------------+
```

<details><summary>Подсказка</summary>

`new Table().Border(TableBorder.Ascii)`, `TableColumn("Qty").RightAligned()`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 2: a table with a right-aligned column.
var table = new Table().Border(TableBorder.Ascii).AddColumn("Name").AddColumn(new TableColumn("Qty").RightAligned());
table.AddRow("apple", "3");
table.AddRow("kiwi", "12");
c.Write(table);
```

Проверено: вывод совпал с ожидаемым.

</details>

## 3. 🟡 Prompts

**Задача:** прочитайте имя и возраст; при вводе `abc` для возраста выведите ошибку и повторите запрос; затем выбор из `one/two/three` стрелкой вниз.

**Ожидаемый результат:** `Ann, 42`; после `abc` и `7` — `7`, ошибка показана; выбор `two`.

<details><summary>Подсказка</summary>

`TestConsole` с `Capabilities.Interactive = true`, `Input.PushTextWithEnter(...)`, `Input.PushKey(ConsoleKey.DownArrow)`; `TextPrompt<int>(...).ValidationErrorMessage(...)`.

</details>

<details><summary>Решение</summary>

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

Проверено: `prompt results: Ann, 42`; `invalid then valid: 7 | error shown: True`; `selection: two`.

</details>

## 4. 🔴 CLI-команда с валидацией

**Задача:** команда `greet <name> [--times N] [--shout]`. Проверьте: успешный вызов, нечисловой `--times`, отсутствие аргумента, `--times 0`.

**Ожидаемый результат:** код `0` и `HELLO ANN` дважды; `Failed to convert 'x' to Int32.`; `Command 'greet' is missing required argument 'name'.`; `times must be >= 1`.

<details><summary>Подсказка</summary>

`CommandSettings` с `[CommandArgument]`/`[CommandOption]`, `Validate()` и `c.PropagateExceptions()` для теста.

</details>

<details><summary>Решение</summary>

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

Проверено: `exit(ok): 0`; `bad option: CommandRuntimeException - Failed to convert 'x' to Int32.`; `missing arg: … missing required argument 'name'.`; `validation: … times must be >= 1`. Команда в проверке писала в статический `AnsiConsole` — для вывода в тестовую консоль внедряйте `IAnsiConsole`.

</details>
