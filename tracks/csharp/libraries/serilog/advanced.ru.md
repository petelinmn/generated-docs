---
title: "Serilog: продвинутый уровень"
lang: ru
track: csharp
kind: library
doc: advanced
slug: serilog
part: 2
tags: [serilog, enrichers, log-context, minimum-level-override, compact-json, ilogger]
versions: ".NET 11, C# 15; Serilog 4.4.0, Serilog.Extensions.Logging 11.0.0, Serilog.Formatting.Compact 3.0.0"
status: draft
updated: 2026-11-22
---

# Serilog: продвинутый уровень

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [основы](basics.ru.md). **Время:** ~45 мин.

## Обогащение и контекст

Обогатители добавляют свойства ко всем событиям. `LogContext` — свойства «на время области»:

```csharp
Log.Logger = new LoggerConfiguration()
    .Enrich.WithProperty("App", "demo")
    .Enrich.FromLogContext()          // required for LogContext properties
    .WriteTo.Console(outputTemplate: "[{Level:u3}] {Message:lj} {Properties:j}{NewLine}")
    .CreateLogger();

using (LogContext.PushProperty("RequestId", "r-1"))
    Log.Warning("inside context");
Log.Warning("outside context");
```

Проверено: внутри области `{"App": "demo", "RequestId": "r-1"}`, снаружи — только `{"App": "demo"}`. Без `.Enrich.FromLogContext()` свойство из `LogContext` не появится.

`Log.ForContext("SourceContext", "Noisy.Thing")` или `Log.ForContext<T>()` создаёт логгер с «источником».

## Переопределение уровней по источнику

```csharp
.MinimumLevel.Debug()
.MinimumLevel.Override("Noisy", LogEventLevel.Warning)
```

Проверено на четырёх сообщениях: из `Noisy.Thing` пропущено только `Warning` (info отброшен), из `Noisy` — `Warning`, из `Other` и `Foo` — `Information` (прошли, так как минимум `Debug`). Переопределение работает по префиксу `SourceContext`. Так приглушают шумные пространства имён (в ASP.NET Core — `Microsoft.AspNetCore`).

## Структурированный JSON

```csharp
.WriteTo.File(new CompactJsonFormatter(), "log.json")      // Serilog.Formatting.Compact
```

Проверено: одна строка на событие, `{"@t":"…","@mt":"User {User} has {Count} items","User":"ann","Count":3,"App":"demo"}` — `@t` время, `@mt` шаблон, `@l` уровень (опущен для `Information`). Числа сохраняются как числа (`"Count":3`), поэтому по ним работают числовые запросы.

## Интеграция с `Microsoft.Extensions.Logging`

```csharp
using var lf = LoggerFactory.Create(b => b.AddSerilog(Log.Logger, dispose: false));   // Serilog.Extensions.Logging
var logger = lf.CreateLogger("MyApp.Service");
using (logger.BeginScope("scope-{Id}", 42))
    logger.LogInformation("Order {OrderId} placed by {User}", 7, "ann");
```

Проверено: сообщение пришло в Serilog со свойствами `OrderId=7`, `User="ann"`, `SourceContext="MyApp.Service"` и свойствами области (`Id=42`, `Scope=["scope-42"]`). `ILogger<T>` в коде приложения остаётся стандартным, а вывод обеспечивает Serilog. В ASP.NET Core обычно используют пакет `Serilog.AspNetCore` (`builder.Host.UseSerilog(...)`).

> ⚠️ Проверить: `Serilog.AspNetCore` (`UseSerilog`, `UseSerilogRequestLogging`) в этом материале не запускался.

## Конфигурация из файла

`Serilog.Settings.Configuration` читает настройки (`MinimumLevel`, `WriteTo`) из `appsettings.json`: уровни можно менять без пересборки.

> ⚠️ Проверить: формат раздела `Serilog` в `appsettings.json` и перезагрузка конфигурации в этом материале не запускались.

## Best practices

| Правило | Почему |
|---|---|
| Шаблоны с параметрами, не интерполяция | структурность |
| `Override` для шумных источников | меньше мусора |
| Согласованные имена свойств (`UserId`, `OrderId`) | поиск по полю |
| Общие свойства — обогатителями/`LogContext` | не повторять в каждом сообщении |
| JSON для машинной обработки | запросы в системах логов |
| `CloseAndFlush` при выходе | не потерять хвост |
| Не логировать секреты | безопасность |

## Связанные материалы

- Назад: [основы](basics.ru.md). Дальше: [эксперт](expert.ru.md).
- [13 · DI, конфигурация, логирование](../../modules/13-di-config-logging/overview.ru.md) — `ILogger`.
- [22 · Наблюдаемость](../../modules/22-observability/overview.ru.md) — корреляция логов и трассировок.
- [Фоновые задачи](../../recipes/background-jobs/overview.ru.md) — логирование в воркерах.

## Источники

- [Serilog: Enrichment](https://github.com/serilog/serilog/wiki/Enrichment)
- [Serilog: Configuration basics (MinimumLevel.Override)](https://github.com/serilog/serilog/wiki/Configuration-Basics)
- [serilog-extensions-logging](https://github.com/serilog/serilog-extensions-logging)
- [serilog-formatting-compact](https://github.com/serilog/serilog-formatting-compact)
