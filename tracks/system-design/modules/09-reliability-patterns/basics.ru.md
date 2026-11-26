---
title: "Паттерны надёжности: базовая теория"
lang: ru
track: system-design
kind: module
doc: basics
slug: 09-reliability-patterns
part: 2
tags: [system-design, resilience, retry, circuit-breaker, bulkhead, rate-limiting, load-shedding]
versions: ".NET 11, C# 15; Polly.Core 8.9.1; System.Threading.RateLimiting 11.0.0"
status: draft
updated: 2026-11-26
---

# Паттерны надёжности: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** async/await, `CancellationToken` · **Время:** ~2 ч

## Зачем это нужно

Одна зависимость замедлилась, вызывающие ждут и копят потоки, соединения и память, и вскоре не отвечает уже вся система. Это каскадный отказ.

## Таймауты

Без таймаута ожидание может длиться бесконечно долго. Таймаут в Polly отменяет `CancellationToken`, поэтому код должен его принимать и уважать.

```csharp
new ResiliencePipelineBuilder { TimeProvider = clock }
    .AddTimeout(new TimeoutStrategyOptions { Timeout = timeout })
    .Build();
```

При истечении бросается `TimeoutRejectedException`.

## Повторы

```csharp
.AddRetry(new RetryStrategyOptions
{
    ShouldHandle = new PredicateBuilder().Handle<TransientException>(),
    MaxRetryAttempts = maxRetries, // retries, not counting the first call
    Delay = delay,
    BackoffType = DelayBackoffType.Exponential,
})
```

`MaxRetryAttempts = 3` даёт четыре вызова. Исключение, не перечисленное в `ShouldHandle`, не повторяется. Файл: [Ex01_Pipelines.cs](code/Examples/Ex01_Pipelines.cs).

## Что повторять

| Повторять | Не повторять |
|---|---|
| обрыв соединения, таймаут | ошибка проверки данных |
| 408, 429, 5xx | большинство прочих 4xx |
| идемпотентные операции | POST без idempotency key |

## Backoff и jitter

Задержка растёт экспоненциально, чтобы дать зависимости восстановиться. Случайная составляющая (jitter) не даёт тысячам клиентов повторить в один и тот же момент.

## Ошибки новичка

- ❌ Нет таймаута → ✅ задавать всегда.
- ❌ Повторять всё подряд → ✅ только временные ошибки.
- ❌ Мгновенные повторы → ✅ backoff с jitter.
- ❌ Игнорировать `CancellationToken` → ✅ передавать дальше.

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [Polly](../../../csharp/libraries/polly/overview.ru.md)

## Источники

- [Polly: retry](https://www.pollydocs.org/strategies/retry.html)
- [Polly: timeout](https://www.pollydocs.org/strategies/timeout.html)
