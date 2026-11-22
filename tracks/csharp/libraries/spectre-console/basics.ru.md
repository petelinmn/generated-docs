---
title: "Spectre.Console: основы"
lang: ru
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

# Spectre.Console: основы

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [01 · Платформа .NET](../../modules/01-dotnet-platform/overview.ru.md), [03 · Управление потоком и методы](../../modules/03-control-flow-and-methods/overview.ru.md). **Время:** ~30 мин.

## Что это и зачем

Spectre.Console — библиотека для красивых консольных приложений: цветной текст через разметку, таблицы, деревья, панели, графики, индикаторы прогресса, интерактивные запросы (prompts) и (в пакете `Spectre.Console.Cli`) разбор командной строки. Библиотека определяет возможности терминала (цвета, Unicode, интерактивность) и подстраивает вывод.

```bash
dotnet add package Spectre.Console
dotnet add package Spectre.Console.Testing   # test console for tests
```

## Разметка (markup)

```csharp
AnsiConsole.MarkupLine("[bold red]Hello[/] [green]world[/]!");
AnsiConsole.MarkupLine("Escape brackets: [[not a tag]]");
AnsiConsole.MarkupLine("User input: " + Markup.Escape(userInput));
```

Вывод (проверено через тестовую консоль без цветов): `Hello world!`, `Escape brackets: [not a tag]`, `User input: [red]x[/]`.

- Теги — `[стиль]текст[/]` (цвет, `bold`, `underline`…).
- Квадратные скобки в тексте удваивают: `[[` и `]]`.
- **Пользовательский ввод экранируйте** `Markup.Escape(...)`: иначе `[red]x[/]` будет воспринят как разметка (проверено: после `Escape` вывод — буквально `[red]x[/]`).
- Неизвестный стиль — исключение: проверено `InvalidOperationException: Could not find color or style 'unknown'.`

## Таблицы, деревья, панели

```csharp
var table = new Table().Border(TableBorder.Ascii).AddColumn("Name").AddColumn(new TableColumn("Qty").RightAligned());
table.AddRow("apple", "3");
table.AddRow("kiwi", "12");
AnsiConsole.Write(table);
```

Результат (проверено):

```text
+-------------+
| Name  | Qty |
|-------+-----|
| apple |   3 |
| kiwi  |  12 |
+-------------+
```

Дерево (`Tree`) выводит иерархию (`root → child → leaf`) символами `└──`; `Panel("inside").Header("Title")` — рамку с заголовком; `Rule("section")` — горизонтальную линию с подписью; `BarChart` — горизонтальные столбцы. Всё это — объекты `IRenderable`: их можно вкладывать друг в друга.

## Запросы пользователю

```csharp
var name = AnsiConsole.Prompt(new TextPrompt<string>("Name?"));
var age  = AnsiConsole.Prompt(new TextPrompt<int>("Age?").ValidationErrorMessage("not a number"));
var pick = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Pick").AddChoices("one", "two", "three"));
```

Проверено на тестовой консоли с запрограммированным вводом: текст `Ann` и число `42` считаны; для `TextPrompt<int>` ввод `abc` вызвал сообщение об ошибке и повторный запрос, затем `7` принят; `SelectionPrompt` после `↓` и `Enter` вернул `two`; `MultiSelectionPrompt` (пробел, `↓↓`, пробел, `Enter`) вернул `a,c`. В **неинтерактивной** консоли (вывод перенаправлен, нет TTY) запрос бросает `InvalidOperationException` (проверено).

## Частые ошибки новичка

- ❌ Вставлять пользовательский текст в разметку без `Markup.Escape` → ✅ экранировать.
- ❌ Забыть удвоить `[` и `]` в обычном тексте → ✅ `[[` и `]]`.
- ❌ Использовать prompts в неинтерактивной среде (CI, перенаправление) → ✅ проверять `AnsiConsole.Profile.Capabilities.Interactive` или принимать значения аргументами.
- ❌ Прогресс/`Status` одновременно с другим выводом в консоль → ✅ один «живой» компонент за раз ([продвинутый](advanced.ru.md#прогресс-и-live-компоненты)).
- ❌ Ожидать цвета везде → ✅ терминал может не поддерживать ANSI.

## Связанные материалы

- Дальше: [продвинутый](advanced.ru.md) — прогресс, тестирование консоли, `Spectre.Console.Cli`.
- [01 · Платформа .NET](../../modules/01-dotnet-platform/overview.ru.md) — консольные приложения.
- [16 · Юнит-тестирование](../../modules/16-testing-unit/overview.ru.md) — тесты вывода.
- [Linux](../../../linux/overview.ru.md) — терминал, ANSI и TTY.
- [Python](../../../python/overview.ru.md) — библиотека `rich`.

## Источники

- [Spectre.Console: документация](https://spectreconsole.net/)
- [Spectre.Console: Markup](https://spectreconsole.net/console/how-to/markup)
- [Spectre.Console на GitHub](https://github.com/spectreconsole/spectre.console)
