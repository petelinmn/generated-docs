---
title: "Наблюдаемость и SLO"
lang: ru
track: system-design
kind: module
doc: overview
slug: 14-observability-and-slo
part: 3
levels: [1, 2, 3, 4, 5, 6]
tags: [system-design, observability, distributed-tracing, metrics, slo, histogram, sampling]
versions: ".NET 11, C# 15; Microsoft.Extensions.Telemetry 11.0.1"
status: draft
updated: 2026-11-26
---

# Наблюдаемость и SLO

🌐 **Русский** · [English](overview.en.md)

## Коротко

Наблюдаемость позволяет по сигналам снаружи понять, что происходит внутри. Основные сигналы — метрики, трассировки и логи; SLO говорят, какие из отклонений важны.

## Ключевые идеи

- Метрики показывают, что что-то не так; трассировки — где.
- Теги с большим числом значений дорого стоят.
- Контекст трассировки передаётся между сервисами заголовком `traceparent`.
- Перцентили нельзя усреднять, а корзины гистограмм можно складывать.
- Алерты строят по SLO и burn rate.
- Сэмплирование должно быть согласованным по всей трассе.
- В .NET инструменты уже есть в `System.Diagnostics`.

## Минимальный пример

```csharp
using Activity? activity = Source.StartActivity("PlaceOrder");
_orders.Add(1, outcome);
```

## Когда применять, а когда нет

- ✅ Любой сервис в эксплуатации.
- ✅ Распределённый запрос, который иначе не проследить.
- ❌ Идентификатор пользователя тегом метрики.
- ❌ Алерт на всё, что можно измерить.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | сигналы, инструменты метрик, спаны |
| [2. Продвинутая теория](advanced.ru.md) | контекст, гистограммы, кардинальность, алерты |
| [3. Экспертная теория](expert.ru.md) | справочник, подвохи, вопросы |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| [5. Запускаемые примеры](code/Examples/) | `Meter`, `ActivitySource`, гистограмма |
| [6. Тесты](code/Tests/) | `dotnet test` в папке `code/` |

## Связанные материалы

- Пререквизиты: [процесс проектирования и SLO](../01-design-process/overview.ru.md), [паттерны надёжности](../09-reliability-patterns/overview.ru.md)
- В других треках: [наблюдаемость в .NET](../../../csharp/modules/22-observability/overview.ru.md), [наблюдаемость в Kubernetes](../../../kubernetes/modules/16-observability/overview.ru.md), [Serilog](../../../csharp/libraries/serilog/overview.ru.md)
- [Производительность запросов в PostgreSQL](../../../postgresql/modules/09-performance-explain/overview.ru.md)
