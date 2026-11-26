---
title: "Polly"
lang: ru
track: csharp
kind: library
doc: overview
slug: polly
part: 2
levels: [1, 2, 3, 4]
tags: [polly, resilience, retry, circuit-breaker, timeout, fallback, hedging]
versions: ".NET 11, C# 15; Polly 8.9.1"
status: draft
updated: 2026-11-22
---

# Polly

🌐 **Русский** · [English](overview.en.md)

## Коротко

Polly — библиотека отказоустойчивости. Стратегии (retry, timeout, circuit breaker, fallback, hedging, rate limiter) собираются в неизменяемый потокобезопасный `ResiliencePipeline`. Он лежит под `Microsoft.Extensions.Http.Resilience`, но применим к любому коду: БД, очередям, внешним API. Поведение всех основных стратегий проверено запуском.

## Ключевые идеи

- Конвейер = стратегии по порядку, первая — внешняя.
- Retry: `ShouldHandle`, экспонента, джиттер; после исчерпания — исключение.
- Timeout кооперативный (отменяет токен).
- Circuit breaker: Closed → Open → Half-Open; `BrokenCircuitException` без выполнения тела.
- Fallback, hedging, rate limiter (`Polly.RateLimiting`).
- DI: `AddResiliencePipeline`, `ResiliencePipelineProvider`.
- Конвейер — singleton: состояние breaker живёт в нём.

## Минимальный пример

```csharp
var pipeline = new ResiliencePipelineBuilder()
    .AddRetry(new RetryStrategyOptions { MaxRetryAttempts = 3, Delay = TimeSpan.FromMilliseconds(20) })
    .AddTimeout(TimeSpan.FromSeconds(1))
    .Build();
var result = await pipeline.ExecuteAsync(async ct => await CallServiceAsync(ct));
```

## Когда применять, а когда нет

- ✅ Вызовы внешних сервисов, БД, очередей с транзиентными сбоями.
- ✅ Нужны таймауты, breaker и деградация.
- ✅ Единая политика устойчивости в приложении.
- ❌ Ретраи неидемпотентных операций без защиты.
- ❌ Маскировка постоянных ошибок конфигурации.
- ❌ Только HTTP и устраивают значения по умолчанию — достаточно готового стандартного обработчика.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Основы](basics.ru.md) | retry, timeout, circuit breaker, fallback |
| [2. Продвинутый](advanced.ru.md) | композиция, DI, rate limiter, hedging, `Outcome` |
| [3. Эксперт](expert.ru.md) | устройство, подвохи, альтернативы, 11 вопросов с собеседований |
| [4. Самопроверка](quiz.ru.md) | 10 вопросов и [4 упражнения](exercises.ru.md) с решениями |
| 5. Запускаемые примеры | не сгенерирован — `/learn csharp polly level 5` |
| 6. Тесты | не сгенерирован — `/learn csharp polly level 6` |

## Связанные материалы

- Пререквизиты: [10 · async/await](../../modules/10-async-await/overview.ru.md), [08 · Исключения](../../modules/08-exceptions/overview.ru.md)
- Рядом: [Устойчивый HttpClient](../../recipes/http-client-resilience/overview.ru.md), [Rate limiting](../../recipes/rate-limiting/overview.ru.md), [Refit](../refit/overview.ru.md), [MassTransit](../masstransit/overview.ru.md) — повторы на уровне конечной точки брокера
- В других треках: [Python](../../../python/overview.ru.md) — tenacity
- [System design: паттерны надёжности](../../../system-design/modules/09-reliability-patterns/overview.ru.md)
