---
title: "Наблюдаемость и SLO: базовая теория"
lang: ru
track: system-design
kind: module
doc: basics
slug: 14-observability-and-slo
part: 3
tags: [system-design, observability, distributed-tracing, metrics, slo, histogram, sampling]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Наблюдаемость и SLO: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** веб-приложения на .NET · **Время:** ~2 ч

## Зачем это нужно

«Всё тормозит» — не диагноз. Нужно знать, какая доля запросов страдает, с какого момента и на каком участке.

## Три сигнала

| Сигнал | Отвечает на | Цена растёт с |
|---|---|---|
| Метрики | сколько и как часто | числом комбинаций тегов |
| Трассировки | где прошло время | числом сохранённых спанов |
| Логи | что именно случилось | объёмом событий |

## Метрики в .NET

| Инструмент | Для чего |
|---|---|
| `Counter` | только растёт: запросы, ошибки |
| `UpDownCounter` | растёт и убывает: элементы в очереди |
| `Gauge`, `ObservableGauge` | текущее значение |
| `Histogram` | распределение: длительности |

```csharp
var outcome = new KeyValuePair<string, object?>("outcome", succeed ? "ok" : "error");
_orders.Add(1, outcome);
_duration.Record(seconds, outcome);
```

Имена пишут строчными буквами через точки, единицы указывают отдельно.

## Спаны

В .NET спан — это `Activity`.

```csharp
// Null when nobody listens, hence the "?." below.
using Activity? activity = Source.StartActivity("PlaceOrder");
```

Без слушателя `StartActivity` возвращает `null`: это экономит ресурсы и это нужно учитывать. Файл: [Ex01_Instrumentation.cs](code/Examples/Ex01_Instrumentation.cs). Тесты слушают метрики и спаны через `MeterListener` и `ActivityListener`; в приложении этим занимается экспортёр, например OpenTelemetry.

## Ошибки новичка

- ❌ Писать всё в логи и считать их метриками → ✅ метрики отдельно.
- ❌ `activity.SetTag` без проверки на `null` → ✅ `activity?.`.
- ❌ Следить только за CPU → ✅ ошибки и задержка с точки зрения пользователя.
- ❌ Секреты в тегах и логах → ✅ скрывать.

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [Наблюдаемость в .NET](../../../csharp/modules/22-observability/overview.ru.md)

## Источники

- [.NET: инструментирование метрик](https://learn.microsoft.com/dotnet/core/diagnostics/metrics-instrumentation)
- [.NET: инструментирование трассировки](https://learn.microsoft.com/dotnet/core/diagnostics/distributed-tracing-instrumentation-walkthroughs)
