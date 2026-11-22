---
title: "Polly: продвинутый уровень"
lang: ru
track: csharp
kind: library
doc: advanced
slug: polly
part: 2
tags: [polly, resilience-pipeline, dependency-injection, hedging, rate-limiter, outcome]
versions: ".NET 11, C# 15; Polly 8.9.1, Polly.Extensions, Polly.RateLimiting"
status: draft
updated: 2026-11-22
---

# Polly: продвинутый уровень

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [основы](basics.ru.md). **Время:** ~45 мин.

## Композиция стратегий

Стратегии добавляются в конвейер по порядку; **первая — внешняя**, она оборачивает остальные:

```csharp
var composed = new ResiliencePipelineBuilder()
    .AddRetry(new RetryStrategyOptions { MaxRetryAttempts = 2, Delay = TimeSpan.Zero,
        ShouldHandle = new PredicateBuilder().Handle<TimeoutRejectedException>() })
    .AddTimeout(TimeSpan.FromMilliseconds(50))
    .Build();
```

Проверено: `retry` снаружи + `timeout` внутри: тело операции запускалось **3 раза** (каждая попытка получила свой таймаут 50 мс). Если поменять порядок (`timeout` снаружи), один таймаут охватит все попытки сразу. Типичный порядок: общий таймаут → retry → circuit breaker → таймаут попытки.

## Dependency injection

Пакет `Polly.Extensions` регистрирует именованные конвейеры:

```csharp
services.AddResiliencePipeline("my-pipeline", b => b.AddRetry(new RetryStrategyOptions { MaxRetryAttempts = 2, Delay = TimeSpan.FromMilliseconds(10) }));

var provider = sp.GetRequiredService<ResiliencePipelineProvider<string>>();
var pipeline = provider.GetPipeline("my-pipeline");
```

Проверено: конвейер получен из провайдера и повторил операцию (результат после 3 попыток). Конвейеры переиспользуются — не создавайте новый на каждый вызов (состояние circuit breaker живёт в конвейере).

## Rate limiter

Пакет `Polly.RateLimiting` оборачивает `System.Threading.RateLimiting`:

```csharp
var rl = new ResiliencePipelineBuilder().AddRateLimiter(new SlidingWindowRateLimiter(
    new SlidingWindowRateLimiterOptions { PermitLimit = 2, Window = TimeSpan.FromSeconds(5), SegmentsPerWindow = 1 })).Build();
```

Проверено: три вызова — `ok, ok, rejected` (`RateLimiterRejectedException` на третьем). Это ограничение **исходящих** вызовов; серверную сторону см. [Rate limiting](../../recipes/rate-limiting/overview.ru.md).

## Hedging

Если первый вызов медленный, через `Delay` запускается второй, берётся первый завершившийся:

```csharp
new ResiliencePipelineBuilder<string>().AddHedging(new HedgingStrategyOptions<string>
{
    MaxHedgedAttempts = 1, Delay = TimeSpan.FromMilliseconds(50),
    ShouldHandle = new PredicateBuilder<string>().Handle<Exception>(),
    ActionGenerator = a => () => a.Callback(a.ActionContext)
}).Build();
```

Проверено: первый вызов спал 500 мс, второй (через 50 мс) вернулся за 10 мс — результат `answer from call 2`, запущено 2 вызова. Hedging умножает нагрузку и годится только для идемпотентных операций.

## Результат без исключений

`ExecuteOutcomeAsync` возвращает `Outcome<T>` вместо броска исключения:

```csharp
var outcome = await p.ExecuteOutcomeAsync<int, string>(async (ctx, s) => { … }, ResilienceContextPool.Shared.Get(), "state");
// outcome.Exception, outcome.Result
```

Проверено: `outcome.Exception` содержит `InvalidOperationException` без броска наружу.

## Наблюдаемость

В `OnRetry`, `OnOpened`, `OnClosed`, `OnHalfOpened` логируйте события; Polly также публикует телеметрию (метрики через `System.Diagnostics.Metrics`) при подключении `AddTelemetry`.

> ⚠️ Проверить: подключение телеметрии Polly (`ConfigureTelemetry`/метрики) в этом материале не запускалось.

## Best practices

| Правило | Почему |
|---|---|
| Конвейер — singleton/из DI | состояние breaker общее |
| Порядок: timeout → retry → breaker → timeout попытки | предсказуемое поведение |
| `ShouldHandle` для транзиентных сбоев | не ретраить ошибки логики |
| Джиттер | нет синхронных всплесков |
| Идемпотентность для retry/hedging | нет побочных эффектов |
| Логировать срабатывания | видно «лечение» сбоев |

## Связанные материалы

- Назад: [основы](basics.ru.md). Дальше: [эксперт](expert.ru.md).
- [Устойчивый HttpClient](../../recipes/http-client-resilience/overview.ru.md) — стандартный обработчик на Polly.
- [Rate limiting](../../recipes/rate-limiting/overview.ru.md) — серверный лимит.
- [13 · DI, конфигурация, логирование](../../modules/13-di-config-logging/overview.ru.md) — регистрация сервисов.

## Источники

- [Polly: Resilience pipelines](https://www.pollydocs.org/pipelines/index.html)
- [Polly: Dependency injection](https://www.pollydocs.org/advanced/dependency-injection.html)
- [Polly: Hedging](https://www.pollydocs.org/strategies/hedging.html)
- [Polly: Rate limiter](https://www.pollydocs.org/strategies/rate-limiter.html)
