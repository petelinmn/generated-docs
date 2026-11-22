---
title: "Упражнения: Serilog"
lang: ru
track: csharp
kind: library
doc: exercises
slug: serilog
part: 2
tags: [serilog, enrichers, log-context, sinks, minimum-level-override]
status: draft
updated: 2026-11-22
---

# Упражнения: Serilog

🌐 **Русский** · [English](exercises.en.md)

> Решайте сами. Решения проверены запуском (Serilog 4.4.0, Serilog.Sinks.Console 6.1.1, Serilog.Sinks.File 7.0.0, Serilog.Formatting.Compact 3.0.0, Serilog.Extensions.Logging 11.0.0). Запускаемых версий в `code/` нет (уровня 5 нет).

## 1. 🟢 Структурное сообщение

**Задача:** выведите в консоль `User ann has 3 items` с добавленным свойством `App = demo`.

**Ожидаемый результат:** `[INF] User ann has 3 items {"App": "demo"}`.

<details><summary>Подсказка</summary>

`Enrich.WithProperty`, `WriteTo.Console(outputTemplate: "[{Level:u3}] {Message:lj} {Properties:j}{NewLine}")`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 1: structured message with an enricher property.
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .Enrich.WithProperty("App", "demo")
    .WriteTo.Console(outputTemplate: "[{Level:u3}] {Message:lj} {Properties:j}{NewLine}")
    .CreateLogger();
Log.Information("User {User} has {Count} items", "ann", 3);
Log.CloseAndFlush();
```

Проверено: `[INF] User ann has 3 items {"App": "demo"}`.

</details>

## 2. 🟡 Деструктуризация

**Задача:** залогируйте `record Order(int Id, string Customer, decimal Total)` тремя способами — `{Order}`, `{@Order}`, `{$Order}`. Чем отличаются?

**Ожидаемый результат:** первый и третий — строка `Order { … }`, второй — структура JSON.

<details><summary>Подсказка</summary>

Сравните вывод и тип значения в `LogEvent.Properties`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 2: scalar vs destructured vs stringified.
var order = new Order(7, "Ann", 9.5m);
Log.Information("Plain: {Order}", order);
Log.Information("Destructured: {@Order}", order);
Log.Information("Stringified: {$Order}", order);
```

Проверено: `Plain: Order { Id = 7, Customer = Ann, Total = 9,5 }`, `Destructured: {"Id": 7, "Customer": "Ann", "Total": 9.5, "$type": "Order"}`, `Stringified: Order { … }`.

</details>

## 3. 🟡 Контекст и переопределение уровня

**Задача:** (а) добавьте `RequestId` только к сообщениям внутри области; (б) приглушите источник `Noisy` до `Warning`.

**Ожидаемый результат:** внутри области есть `RequestId`, снаружи нет; из `Noisy.Thing` проходит только `Warning`.

<details><summary>Подсказка</summary>

`Enrich.FromLogContext()` + `LogContext.PushProperty`; `MinimumLevel.Override("Noisy", LogEventLevel.Warning)` и `Log.ForContext("SourceContext", "Noisy.Thing")`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 3: scoped property and per-source level override.
.MinimumLevel.Override("Noisy", LogEventLevel.Warning)
.Enrich.FromLogContext()

using (LogContext.PushProperty("RequestId", "r-1")) Log.Warning("inside context");
Log.Warning("outside context");

var noisy = Log.ForContext("SourceContext", "Noisy.Thing");
noisy.Information("dropped");
noisy.Warning("kept");
```

Проверено: `inside context {"App": "demo", "RequestId": "r-1"}`, `outside context {"App": "demo"}`; на четырёх тестовых сообщениях из `Noisy`, `Noisy.Thing`, `Other`, `Foo` отброшен только info из `Noisy.Thing`.

</details>

## 4. 🔴 JSON-файл и интеграция с `ILogger`

**Задача:** пишите события в `log.json` через `CompactJsonFormatter`, а сообщения `ILogger` с областью `scope-{Id}` направьте в Serilog. Какие свойства получит событие?

**Ожидаемый результат:** `OrderId=7`, `User="ann"`, `SourceContext="MyApp.Service"`, `Id=42`, `Scope=["scope-42"]`.

<details><summary>Подсказка</summary>

`.WriteTo.File(new CompactJsonFormatter(), "log.json")`; `LoggerFactory.Create(b => b.AddSerilog(Log.Logger, dispose: false))`; `logger.BeginScope("scope-{Id}", 42)`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 4: JSON output and ILogger bridge.
Log.Logger = new LoggerConfiguration().MinimumLevel.Information()
    .WriteTo.File(new CompactJsonFormatter(), "log.json")
    .CreateLogger();
using var lf = LoggerFactory.Create(b => b.AddSerilog(Log.Logger, dispose: false));
var logger = lf.CreateLogger("MyApp.Service");
using (logger.BeginScope("scope-{Id}", 42))
    logger.LogInformation("Order {OrderId} placed by {User}", 7, "ann");
Log.CloseAndFlush();
```

Проверено: свойства события `OrderId=7,User="ann",SourceContext="MyApp.Service",Id=42,Scope=["scope-42"]`; строка JSON: `{"@t":"…","@mt":"User {User} has {Count} items","User":"ann","Count":3,"App":"demo"}`.

</details>
