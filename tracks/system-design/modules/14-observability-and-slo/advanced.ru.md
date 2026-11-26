---
title: "Наблюдаемость и SLO: продвинутая теория"
lang: ru
track: system-design
kind: module
doc: advanced
slug: 14-observability-and-slo
part: 3
tags: [system-design, observability, distributed-tracing, metrics, slo, histogram, sampling]
versions: ".NET 11, C# 15; Microsoft.Extensions.Telemetry 11.0.1"
status: draft
updated: 2026-11-26
---

# Наблюдаемость и SLO: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Распространение контекста

W3C Trace Context: `traceparent: version-trace-id-parent-id-trace-flags`, шестнадцатеричные символы в нижнем регистре.

| Поле | Длина | Недопустимо |
|---|---|---|
| version | 2 | `ff` |
| trace-id | 32 | все нули |
| parent-id | 16 | все нули |
| trace-flags | 2 | — |

Младший бит флагов — sampled. Это рекомендация, а не гарантия записи.

## Гистограммы

```csharp
public void Merge(Histogram other)
{
    for (int i = 0; i < _counts.Length; i++) _counts[i] += other._counts[i];
}
```

Корзины разных экземпляров складываются, а перцентили — нет. Цена: результат известен с точностью до границы корзины. Файл: [Ex02_Histogram.cs](code/Examples/Ex02_Histogram.cs).

## Кардинальность

Каждая комбинация значений тегов — отдельный временной ряд. Документация .NET называет безопасным порядок менее 1000 комбинаций на инструмент, а для гистограмм — в 10–100 раз меньше. Идентификатор клиента тегом — плохая идея; ему место в спане или логе.

## Алерты по SLO

Алерт «бюджет горит быстро» требует превышения burn rate в длинном и коротком окне; пороги разобраны в [модуле 01](../01-design-process/advanced.ru.md).

## Лучшие практики

- Для каждого сервиса: частота, ошибки, длительность.
- Добавляйте `trace_id` в логи.
- Границы корзин ставьте вокруг порога SLO.
- У алерта должна быть инструкция, что делать.

## Частые ошибки

- ❌ Алерты на причины (CPU) вместо симптомов → ✅ на то, что чувствует пользователь.
- ❌ Каждый сервис решает о сэмплировании случайно → дырявые трассы. ✅ Согласованное решение.
- ❌ Создавать `Meter` на каждый запрос → ✅ один на компонент.
- ❌ URL с идентификаторами тегом → ✅ шаблон маршрута.

## Сравнение с другими треками

- Health checks и OpenTelemetry — [C#](../../../csharp/modules/22-observability/overview.ru.md).
- Сбор в кластере — [Kubernetes](../../../kubernetes/modules/16-observability/overview.ru.md).

## Связанные материалы

- [Базовая теория](basics.ru.md)
- [Экспертная теория](expert.ru.md)

## Источники

- [W3C Trace Context](https://www.w3.org/TR/trace-context/)
- [.NET: инструментирование метрик](https://learn.microsoft.com/dotnet/core/diagnostics/metrics-instrumentation)
