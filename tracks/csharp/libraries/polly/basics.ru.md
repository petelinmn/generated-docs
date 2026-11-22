---
title: "Polly: основы"
lang: ru
track: csharp
kind: library
doc: basics
slug: polly
part: 2
tags: [polly, resilience, retry, timeout, circuit-breaker, fallback]
versions: ".NET 11, C# 15; Polly 8.9.1"
status: draft
updated: 2026-11-22
---

# Polly: основы

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [10 · async/await](../../modules/10-async-await/overview.ru.md), [08 · Исключения](../../modules/08-exceptions/overview.ru.md). **Время:** ~30 мин.

## Что это и зачем

Polly — библиотека отказоустойчивости: оборачивает операцию (вызов сервиса, запрос к БД) стратегиями, которые справляются с временными сбоями. В версии 8 стратегии собираются в **конвейер** (`ResiliencePipeline`): retry, timeout, circuit breaker, fallback, hedging, rate limiter. Polly лежит в основе `Microsoft.Extensions.Http.Resilience` ([рецепт про HttpClient](../../recipes/http-client-resilience/overview.ru.md)).

```bash
dotnet add package Polly
```

## Retry

```csharp
var retry = new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions
{
    MaxRetryAttempts = 3, Delay = TimeSpan.FromMilliseconds(20),
    BackoffType = DelayBackoffType.Exponential, UseJitter = false,
    ShouldHandle = new PredicateBuilder().Handle<InvalidOperationException>()
}).Build();

var r = await retry.ExecuteAsync(async ct => { attempts++; if (attempts < 3) throw new InvalidOperationException(); return attempts; });
```

Проверено: операция падала дважды, на третьей попытке вернула результат (`ok after 3`); задержки между попытками `20 мс`, `40 мс` (экспоненциально).

- Если все попытки исчерпаны, исключение пробрасывается: при постоянном сбое проверено `InvalidOperationException` после **4** попыток (1 + 3 ретрая).
- `ShouldHandle` задаёт, какие сбои повторять: исключение другого типа (`ArgumentException`) не ретраилось — **1 попытка**.

## Timeout

```csharp
var to = new ResiliencePipelineBuilder().AddTimeout(TimeSpan.FromMilliseconds(100)).Build();
await to.ExecuteAsync(async ct => await Task.Delay(2000, ct));    // TimeoutRejectedException
```

Проверено: операция на 2 с прервана через ~100 мс с `TimeoutRejectedException`. **Важно:** таймаут «кооперативный» — он отменяет токен `ct`. Операция, которая токен игнорирует (`Thread.Sleep(300)`), не прерывается: проверено — завершилась штатно за ~300 мс.

## Circuit breaker

```csharp
var cb = new ResiliencePipelineBuilder().AddCircuitBreaker(new CircuitBreakerStrategyOptions
{
    FailureRatio = 0.5, MinimumThroughput = 2, SamplingDuration = TimeSpan.FromSeconds(10), BreakDuration = TimeSpan.FromMilliseconds(500),
    ShouldHandle = new PredicateBuilder().Handle<InvalidOperationException>()
}).Build();
```

Проверено на всегда падающей операции: вызовы 1 и 2 — исключение проходит (операция выполнялась), цепь **разомкнулась** (`OPENED`); вызовы 3 и 4 — `BrokenCircuitException`, **тело не выполнялось** (счётчик вызовов остался 2). Через `BreakDuration` цепь стала `HALF-OPEN`, пробный успешный вызов замкнул её (`CLOSED`).

## Fallback

```csharp
var fb = new ResiliencePipelineBuilder<string>().AddFallback(new FallbackStrategyOptions<string>
{
    ShouldHandle = new PredicateBuilder<string>().Handle<InvalidOperationException>(),
    FallbackAction = a => Outcome.FromResultAsValueTask("default value")
}).Build();
```

Проверено: вместо исключения — `default value`. Для конвейера с результатом нужен `ResiliencePipelineBuilder<T>`, а при вызове — явный тип: `fb.ExecuteAsync<string>(…)` (без него — ошибка компиляции CS0411).

## Частые ошибки новичка

- ❌ Ретраить всё подряд → ✅ `ShouldHandle` только для транзиентных сбоев.
- ❌ Ретраи без задержки и джиттера → ✅ экспонента + джиттер.
- ❌ Ожидать, что timeout остановит код, не смотрящий на токен → ✅ передавайте `ct` во все ожидания.
- ❌ Ретраить неидемпотентные операции → ✅ идемпотентность ([HttpClient: идемпотентность](../../recipes/http-client-resilience/advanced.ru.md#идемпотентность-и-ретраи)).
- ❌ Не обработать `BrokenCircuitException` и `TimeoutRejectedException` → ✅ предусмотреть деградацию.

## Связанные материалы

- Дальше: [продвинутый](advanced.ru.md) — композиция, DI, hedging, rate limiter, результат без исключений.
- [Устойчивый HttpClient](../../recipes/http-client-resilience/overview.ru.md) — Polly поверх `HttpClient`.
- [10 · async/await](../../modules/10-async-await/overview.ru.md) — отмена.
- [Python](../../../python/overview.ru.md) — tenacity и аналоги.

## Источники

- [Polly: documentation](https://www.pollydocs.org/)
- [Polly: Retry strategy](https://www.pollydocs.org/strategies/retry.html)
- [Polly: Circuit breaker](https://www.pollydocs.org/strategies/circuit-breaker.html)
- [Polly: Timeout](https://www.pollydocs.org/strategies/timeout.html)
