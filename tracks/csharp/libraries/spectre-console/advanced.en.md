---
title: "Spectre.Console: advanced"
lang: en
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

# Spectre.Console: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [basics](basics.en.md). **Time:** ~45 min.

## Testing output: `TestConsole`

The `Spectre.Console.Testing` package provides `TestConsole`: the output is collected into a string, input is scripted:

```csharp
var c = new TestConsole().Width(60).Colors(ColorSystem.NoColors);
c.Profile.Capabilities.Ansi = false;
c.Write(table);
string text = c.Output;
```

All the examples of this material were verified exactly this way: deterministic output without ANSI codes. For prompts:

```csharp
var pc = new TestConsole();
pc.Profile.Capabilities.Interactive = true;
pc.Input.PushTextWithEnter("Ann");
pc.Input.PushKey(ConsoleKey.DownArrow);
```

Verified ([basics](basics.en.md#prompting-the-user)). To make code testable, accept an `IAnsiConsole` as a dependency rather than using the static `AnsiConsole` directly.

## Progress and live components

```csharp
console.Progress().Start(ctx =>
{
    var t = ctx.AddTask("work", maxValue: 10);
    while (!t.IsFinished) t.Increment(5);
});
```

Verified on a test console: a task of 10 units with a step of 5 finished in 2 iterations. `Status()` (a spinner) and `Live()` (a re-rendered display) are similar. These are "live" components: while one is active, other output to the same console conflicts — do not mix them.

> ⚠️ Verify: how progress and the spinner look in a real terminal, and `Live`/`Status`, were not run in this material.

## `Spectre.Console.Cli`: commands and arguments

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

Verified (`PropagateExceptions()` enabled for the test):

| Arguments | Result |
|---|---|
| `greet Ann --times 2 --shout` | return code `0`, `HELLO ANN` twice |
| `greet Ann --times x` | `CommandRuntimeException: Failed to convert 'x' to Int32.` |
| `greet` | `Command 'greet' is missing required argument 'name'.` |
| `greet Ann --times 0` | `times must be >= 1` (from `Validate()`) |

The settings are an ordinary class with `[CommandArgument]`/`[CommandOption]` attributes; `Validate()` checks values before execution. By default (without `PropagateExceptions`) the application prints the error and returns a non-zero code.

> **A gotcha:** in the check the command printed through the static `AnsiConsole`, and `console.Output` (the test console) stayed empty although `ConfigureConsole(console)` was set. Inject `IAnsiConsole` into the command so the output goes where it is configured.

> ⚠️ Verify: generating help (`--help`), command branches and dependency injection (`ITypeRegistrar`) were not run in this material.

## Best practices

| Rule | Why |
|---|---|
| `Markup.Escape` for user data | otherwise broken markup/an exception |
| `IAnsiConsole` as a dependency | testability |
| `TestConsole` in tests | deterministic output |
| Check interactivity | prompts do not work without a TTY |
| Do not mix "live" components | output conflicts |
| Validate command settings in `Validate()` | one place and clear errors |

## Related materials

- Back: [basics](basics.en.md). Next: [expert](expert.en.md).
- [16 · Unit testing](../../modules/16-testing-unit/overview.en.md)
- [13 · DI, configuration, logging](../../modules/13-di-config-logging/overview.en.md) — injecting the console.
- [Serilog](../serilog/overview.en.md) — console logs (a separate sink).

## Sources

- [Spectre.Console: Progress](https://spectreconsole.net/console/how-to/progress)
- [Spectre.Console: Testing](https://spectreconsole.net/console/how-to/testing)
- [Spectre.Console.Cli](https://spectreconsole.net/cli)
