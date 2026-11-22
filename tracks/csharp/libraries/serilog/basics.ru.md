---
title: "Serilog: основы"
lang: ru
track: csharp
kind: library
doc: basics
slug: serilog
part: 2
tags: [serilog, logging, structured-logging, sinks, message-templates]
versions: ".NET 11, C# 15; Serilog 4.4.0, Serilog.Sinks.Console 6.1.1, Serilog.Sinks.File 7.0.0"
status: draft
updated: 2026-11-22
---

# Serilog: основы

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [13 · DI, конфигурация, логирование](../../modules/13-di-config-logging/overview.ru.md). **Время:** ~30 мин.

## Что это и зачем

Serilog — библиотека **структурного** логирования. Сообщение — это шаблон с именованными свойствами (`"User {User} has {Count} items"`), а не просто строка: значения сохраняются как данные и их можно искать и фильтровать в системах логов (Seq, Elastic, Loki). Вывод настраивается через **sinks** (консоль, файл, сеть).

```bash
dotnet add package Serilog
dotnet add package Serilog.Sinks.Console
dotnet add package Serilog.Sinks.File
```

## Первая конфигурация

```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .Enrich.WithProperty("App", "demo")
    .WriteTo.Console(outputTemplate: "[{Level:u3}] {Message:lj} {Properties:j}{NewLine}")
    .CreateLogger();

var user = "ann"; var count = 3;
Log.Information("User {User} has {Count} items", user, count);
Log.CloseAndFlush();   // flush buffered sinks on exit
```

Вывод (проверено запуском):

```text
[INF] User ann has 3 items {"App": "demo"}
```

`{Properties:j}` печатает дополнительные свойства (здесь — добавленное обогатителем `App`) в JSON. Свойства `User` и `Count` уже вошли в текст сообщения, поэтому в `{Properties}` их нет.

## Уровни

`Verbose < Debug < Information < Warning < Error < Fatal`. `MinimumLevel.Debug()` пропускает всё от `Debug` и выше; сообщение `Log.Debug("debug visible")` в примере прошло, а при `MinimumLevel.Information()` было бы отфильтровано.

## Шаблоны сообщений и свойства

| Запись | Что делает |
|---|---|
| `{User}` | значение как скаляр (строка в кавычках в JSON) |
| `{@Order}` | **деструктуризация**: объект превращается в структуру со свойствами |
| `{$Order}` | принудительно строка (`ToString()`) |

Проверено на `record Order(int Id, string Customer, decimal Total)`: `{Order}` и `{$Order}` дали `Order { Id = 7, Customer = Ann, Total = 9,5 }` (строка), а `{@Order}` — структуру `{"Id": 7, "Customer": "Ann", "Total": 9.5, "$type": "Order"}`. Обратите внимание: в строковом виде десятичная дробь отформатирована текущей культурой (`9,5`), а в структуре — числом `9.5`.

Исключение передают первым аргументом: `Log.Error(e, "failed for {User}", user)` — текст и стек сохраняются отдельно от шаблона.

## Файл

```csharp
.WriteTo.File("logs/app-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 3)
```

Проверено: имя файла получает дату (`app-20261122.log`), строка — `2026-11-22 17:19:18.016 +04:00 [INF] Order 7 placed by ann`.

## Частые ошибки новичка

- ❌ Интерполяция строк: `Log.Information($"user {user}")` → ✅ шаблон с параметрами. Проверено: при интерполяции шаблон — готовая строка `interpolated ann`, а свойств нет; искать по `User` нельзя.
- ❌ Не вызвать `Log.CloseAndFlush()` → ✅ последние сообщения могут не записаться.
- ❌ Логировать целые объекты без `@` → ✅ получите `ToString()`, а не структуру.
- ❌ Логировать секреты и персональные данные → ✅ исключайте/маскируйте.
- ❌ `Debug` в production «на всякий случай» → ✅ уровень по умолчанию `Information`.

## Связанные материалы

- Дальше: [продвинутый](advanced.ru.md) — обогащение, контекст, переопределения, JSON, интеграция с `ILogger`.
- [13 · DI, конфигурация, логирование](../../modules/13-di-config-logging/overview.ru.md) — `ILogger<T>`.
- [22 · Наблюдаемость](../../modules/22-observability/overview.ru.md) — логи, метрики, трассировка.
- [Python](../../../python/overview.ru.md) — `logging` и structlog.

## Источники

- [Serilog: Getting started](https://github.com/serilog/serilog/wiki/Getting-Started)
- [Serilog: Writing log events](https://github.com/serilog/serilog/wiki/Writing-Log-Events)
- [Serilog: Structured data](https://github.com/serilog/serilog/wiki/Structured-Data)
