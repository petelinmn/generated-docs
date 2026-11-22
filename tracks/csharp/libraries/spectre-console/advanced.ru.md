---
title: "Spectre.Console: продвинутый уровень"
lang: ru
track: csharp
kind: library
doc: advanced
slug: spectre-console
part: 2
tags: [spectre-console, progress, testconsole, spectre-console-cli, commands]
versions: ".NET 11, C# 15; Spectre.Console 0.57.2, Spectre.Console.Cli 0.57.2, Spectre.Console.Testing 0.57.2"
status: draft
updated: 2026-11-22
---

# Spectre.Console: продвинутый уровень

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [основы](basics.ru.md). **Время:** ~45 мин.

## Тестирование вывода: `TestConsole`

Пакет `Spectre.Console.Testing` даёт `TestConsole`: вывод собирается в строку, ввод программируется:

```csharp
var c = new TestConsole().Width(60).Colors(ColorSystem.NoColors);
c.Profile.Capabilities.Ansi = false;
c.Write(table);
string text = c.Output;
```

Все примеры этого материала проверялись именно так: детерминированный вывод без ANSI-кодов. Для prompts:

```csharp
var pc = new TestConsole();
pc.Profile.Capabilities.Interactive = true;
pc.Input.PushTextWithEnter("Ann");
pc.Input.PushKey(ConsoleKey.DownArrow);
```

Проверено ([основы](basics.ru.md#запросы-пользователю)). Чтобы код тестировался, принимайте `IAnsiConsole` как зависимость, а не обращайтесь к статическому `AnsiConsole` напрямую.

## Прогресс и live-компоненты

```csharp
console.Progress().Start(ctx =>
{
    var t = ctx.AddTask("work", maxValue: 10);
    while (!t.IsFinished) t.Increment(5);
});
```

Проверено на тестовой консоли: задача на 10 единиц с шагом 5 завершилась за 2 итерации. Аналогично `Status()` (спиннер), `Live()` (обновляемый рендер). Это «живые» компоненты: пока один активен, другой вывод в ту же консоль конфликтует — не смешивайте.

> ⚠️ Проверить: внешний вид прогресса и спиннера в реальном терминале, а также `Live`/`Status`, в этом материале не запускали.

## `Spectre.Console.Cli`: команды и аргументы

```csharp
var app = new CommandApp();
app.Configure(c => c.AddCommand<GreetCommand>("greet").WithDescription("Greets someone"));
return app.Run(args);

class GreetSettings : CommandSettings
{
    [CommandArgument(0, "<name>")] [Description("Who to greet")] public string Name { get; set; } = "";
    [CommandOption("-t|--times")] [DefaultValue(1)] public int Times { get; set; }
    [CommandOption("--shout")] public bool Shout { get; set; }
    public override ValidationResult Validate() => Times < 1 ? ValidationResult.Error("times must be >= 1") : ValidationResult.Success();
}
class GreetCommand : Command<GreetSettings>
{
    public override int Execute(CommandContext context, GreetSettings s, CancellationToken ct) { /* … */ return 0; }
}
```

Проверено (`PropagateExceptions()` включено для теста):

| Аргументы | Результат |
|---|---|
| `greet Ann --times 2 --shout` | код возврата `0`, `HELLO ANN` дважды |
| `greet Ann --times x` | `CommandRuntimeException: Failed to convert 'x' to Int32.` |
| `greet` | `Command 'greet' is missing required argument 'name'.` |
| `greet Ann --times 0` | `times must be >= 1` (из `Validate()`) |

Настройки — обычный класс с атрибутами `[CommandArgument]`/`[CommandOption]`; `Validate()` проверяет значения до выполнения. По умолчанию (без `PropagateExceptions`) приложение печатает ошибку и возвращает ненулевой код.

> **Подвох:** в проверке команда выводила через статический `AnsiConsole`, и `console.Output` (тестовая консоль) остался пустым, хотя `ConfigureConsole(console)` был задан. Внедряйте `IAnsiConsole` в команду, чтобы вывод шёл туда, куда настроено.

> ⚠️ Проверить: генерацию справки (`--help`), ветки команд и внедрение зависимостей (`ITypeRegistrar`) в этом материале не запускали.

## Best practices

| Правило | Почему |
|---|---|
| `Markup.Escape` для данных пользователя | иначе поломка разметки/исключение |
| `IAnsiConsole` как зависимость | тестируемость |
| `TestConsole` в тестах | детерминированный вывод |
| Проверка интерактивности | prompts не работают без TTY |
| Не смешивать «живые» компоненты | конфликт вывода |
| Валидация настроек команды в `Validate()` | единое место и понятные ошибки |

## Связанные материалы

- Назад: [основы](basics.ru.md). Дальше: [эксперт](expert.ru.md).
- [16 · Юнит-тестирование](../../modules/16-testing-unit/overview.ru.md)
- [13 · DI, конфигурация, логирование](../../modules/13-di-config-logging/overview.ru.md) — внедрение консоли.
- [Serilog](../serilog/overview.ru.md) — логи для консоли (отдельный sink).

## Источники

- [Spectre.Console: Progress](https://spectreconsole.net/console/how-to/progress)
- [Spectre.Console: Testing](https://spectreconsole.net/console/how-to/testing)
- [Spectre.Console.Cli](https://spectreconsole.net/cli)
