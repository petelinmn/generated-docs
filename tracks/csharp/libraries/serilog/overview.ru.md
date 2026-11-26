---
title: "Serilog"
lang: ru
track: csharp
kind: library
doc: overview
slug: serilog
part: 2
levels: [1, 2, 3, 4]
tags: [serilog, logging, structured-logging, sinks, enrichers]
versions: ".NET 11, C# 15; Serilog 4.4.0"
status: draft
updated: 2026-11-22
---

# Serilog

🌐 **Русский** · [English](overview.en.md)

## Коротко

Serilog — библиотека структурного логирования. Сообщение — шаблон с именованными свойствами; значения хранятся как данные и доступны для поиска. Вывод настраивается sinks (консоль, файл, JSON, сеть), контекст добавляют обогатители и `LogContext`, шумные источники приглушают `MinimumLevel.Override`. Через `Serilog.Extensions.Logging` библиотека принимает сообщения стандартного `ILogger`.

## Ключевые идеи

- Шаблон с параметрами вместо интерполяции (проверено: интерполяция теряет свойства).
- `{@obj}` — деструктуризация, `{$obj}` — строка.
- Уровни `Verbose…Fatal`, `MinimumLevel` и `Override` по префиксу источника.
- `Enrich.WithProperty`, `Enrich.FromLogContext` + `LogContext.PushProperty`.
- Sinks: консоль, файл с ротацией, JSON (`CompactJsonFormatter`), собственные (`ILogEventSink`).
- `AddSerilog` — мост из `ILogger`.
- `Log.CloseAndFlush()` при выходе.

## Минимальный пример

```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();
Log.Information("User {User} has {Count} items", "ann", 3);
Log.CloseAndFlush();
```

## Когда применять, а когда нет

- ✅ Приложения, чьи логи анализируют в Seq/Elastic/Loki.
- ✅ Нужны богатые sinks, обогащение, деструктуризация.
- ✅ Хочется единый формат логов в нескольких сервисах.
- ❌ Достаточно встроенного `ILogger` с консолью.
- ❌ Горячие пути без защиты уровня/`LoggerMessage`.
- ❌ Логирование секретов и персональных данных.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Основы](basics.ru.md) | конфигурация, шаблоны, `@`/`$`, уровни, файл |
| [2. Продвинутый](advanced.ru.md) | обогащение, `LogContext`, `Override`, JSON, мост `ILogger` |
| [3. Эксперт](expert.ru.md) | устройство, свой sink, подвохи, 11 вопросов с собеседований |
| [4. Самопроверка](quiz.ru.md) | 10 вопросов и [4 упражнения](exercises.ru.md) с решениями |
| 5. Запускаемые примеры | не сгенерирован — `/learn csharp serilog level 5` |
| 6. Тесты | не сгенерирован — `/learn csharp serilog level 6` |

## Связанные материалы

- Пререквизиты: [13 · DI, конфигурация, логирование](../../modules/13-di-config-logging/overview.ru.md)
- Рядом: [22 · Наблюдаемость](../../modules/22-observability/overview.ru.md), [Фоновые задачи](../../recipes/background-jobs/overview.ru.md)
- В других треках: [Python](../../../python/overview.ru.md) — `logging`, structlog
- Рецепты и библиотеки: [Spectre.Console](../spectre-console/overview.ru.md)
- [System design: наблюдаемость и SLO](../../../system-design/modules/14-observability-and-slo/overview.ru.md)
