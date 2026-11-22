---
title: "Spectre.Console"
lang: ru
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

🌐 **Русский** · [English](overview.en.md)

## Коротко

Spectre.Console — библиотека для красивых консольных приложений: разметка (цвета и стили), таблицы, деревья, панели, графики, прогресс, интерактивные запросы и разбор команд (`Spectre.Console.Cli`). Всё строится из `IRenderable` и адаптируется к возможностям терминала. Для тестов есть `TestConsole`. Вид прогресса в живом терминале и `--help` в этом материале не запускались.

## Ключевые идеи

- Разметка `[bold red]…[/]`; `[[ ]]` для скобок; `Markup.Escape` для ввода пользователя.
- Виджеты: `Table`, `Tree`, `Panel`, `Rule`, `BarChart` — все `IRenderable`.
- Prompts: `TextPrompt<T>`, `SelectionPrompt`, `MultiSelectionPrompt`; в неинтерактивной консоли — исключение.
- `Progress`, `Status`, `Live` — «живые» компоненты, по одному за раз.
- `Spectre.Console.Cli`: настройки, команды, `Validate()`.
- `IAnsiConsole` как зависимость + `TestConsole` в тестах.

## Минимальный пример

```csharp
AnsiConsole.MarkupLine("[bold green]Done![/]");
var table = new Table().AddColumn("Name").AddColumn("Qty");
table.AddRow("apple", "3");
AnsiConsole.Write(table);
```

## Когда применять, а когда нет

- ✅ CLI-утилиты, инструменты разработчика, отчёты в консоли.
- ✅ Интерактивные мастера (prompts).
- ✅ Тестируемый консольный вывод.
- ❌ Библиотеки (вывод — ответственность приложения).
- ❌ Логирование для машин (используйте структурные логи).
- ❌ Полноценный TUI на весь экран (нужны другие инструменты).

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Основы](basics.ru.md) | разметка, таблицы, деревья, панели, prompts |
| [2. Продвинутый](advanced.ru.md) | `TestConsole`, прогресс, `Spectre.Console.Cli` |
| [3. Эксперт](expert.ru.md) | `IRenderable`, возможности терминала, подвохи, 11 вопросов с собеседований |
| [4. Самопроверка](quiz.ru.md) | 10 вопросов и [4 упражнения](exercises.ru.md) с решениями |
| 5. Запускаемые примеры | не сгенерирован — `/learn csharp spectre-console level 5` |
| 6. Тесты | не сгенерирован — `/learn csharp spectre-console level 6` |

## Связанные материалы

- Пререквизиты: [01 · Платформа .NET](../../modules/01-dotnet-platform/overview.ru.md), [03 · Управление потоком и методы](../../modules/03-control-flow-and-methods/overview.ru.md)
- Рядом: [16 · Юнит-тестирование](../../modules/16-testing-unit/overview.ru.md), [Serilog](../serilog/overview.ru.md)
- В других треках: [Python](../../../python/overview.ru.md) — `rich`; [Linux](../../../linux/overview.ru.md) — терминал и ANSI
