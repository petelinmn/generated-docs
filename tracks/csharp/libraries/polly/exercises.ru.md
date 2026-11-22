---
title: "Упражнения: Polly"
lang: ru
track: csharp
kind: library
doc: exercises
slug: polly
part: 2
tags: [polly, retry, timeout, circuit-breaker, fallback, hedging]
status: draft
updated: 2026-11-22
---

# Упражнения: Polly

🌐 **Русский** · [English](exercises.en.md)

> Решайте сами. Решения проверены запуском (Polly 8.9.1, Polly.Extensions, Polly.RateLimiting). Запускаемых версий в `code/` нет (уровня 5 нет).

## 1. 🟢 Retry

**Задача:** операция падает дважды (`InvalidOperationException`), на третий раз успешна. Настройте 3 ретрая с экспоненциальной задержкой от 20 мс.

**Ожидаемый результат:** `ok after 3`; задержки 20 и 40 мс.

<details><summary>Подсказка</summary>

`AddRetry(new RetryStrategyOptions { MaxRetryAttempts = 3, Delay = …, BackoffType = DelayBackoffType.Exponential, UseJitter = false, ShouldHandle = new PredicateBuilder().Handle<InvalidOperationException>() })`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 1: retry with exponential backoff.
var retry = new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions
{
    MaxRetryAttempts = 3, Delay = TimeSpan.FromMilliseconds(20),
    BackoffType = DelayBackoffType.Exponential, UseJitter = false,
    ShouldHandle = new PredicateBuilder().Handle<InvalidOperationException>()
}).Build();
var r = await retry.ExecuteAsync(async ct => { attempts++; if (attempts < 3) throw new InvalidOperationException(); return "ok after " + attempts; });
```

Проверено: `retry #1 after 20ms`, `retry #2 after 40ms`, `ok after 3`; при постоянном сбое — исключение после 4 попыток.

</details>

## 2. 🟡 Timeout

**Задача:** прервите операцию на 2 с через 100 мс. Затем проверьте операцию `Thread.Sleep(300)`.

**Ожидаемый результат:** `TimeoutRejectedException` за ~100 мс; вторая операция завершается штатно за ~300 мс.

<details><summary>Подсказка</summary>

`AddTimeout(TimeSpan.FromMilliseconds(100))`; таймаут отменяет токен — игнорирующий токен код не прервётся.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 2: a cooperative timeout.
var to = new ResiliencePipelineBuilder().AddTimeout(TimeSpan.FromMilliseconds(100)).Build();
await to.ExecuteAsync(async ct => await Task.Delay(2000, ct));                   // TimeoutRejectedException
await to.ExecuteAsync(async ct => { Thread.Sleep(300); await Task.CompletedTask; });   // not interrupted
```

Проверено: `timeout: TimeoutRejectedException in ~100ms`, `non-cooperative: finished normally in 300ms (token ignored)`.

</details>

## 3. 🟡 Circuit breaker

**Задача:** настройте breaker (порог 2 вызова, доля 0.5, пауза 500 мс). Сделайте 4 вызова всегда падающей операции, подождите и сделайте успешный вызов.

**Ожидаемый результат:** вызовы 1–2 — исключение, 3–4 — `BrokenCircuitException` без выполнения; после паузы — успех.

<details><summary>Подсказка</summary>

`AddCircuitBreaker(new CircuitBreakerStrategyOptions { FailureRatio = 0.5, MinimumThroughput = 2, SamplingDuration = …, BreakDuration = … })`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 3: circuit breaker opens after two failures.
var cb = new ResiliencePipelineBuilder().AddCircuitBreaker(new CircuitBreakerStrategyOptions
{
    FailureRatio = 0.5, MinimumThroughput = 2,
    SamplingDuration = TimeSpan.FromSeconds(10), BreakDuration = TimeSpan.FromMilliseconds(500),
    ShouldHandle = new PredicateBuilder().Handle<InvalidOperationException>()
}).Build();
```

Проверено: `call 1/2: failure passed through (calls=1/2)`, `OPENED`, `call 3/4: BrokenCircuitException (calls=2)`, затем `HALF-OPEN`, `CLOSED`, `recovered`.

</details>

## 4. 🔴 Fallback и hedging

**Задача:** (а) верните `default value` вместо исключения; (б) запустите дублирующий вызов через 50 мс, если первый (500 мс) медленный.

**Ожидаемый результат:** `fallback: default value`; `hedging: answer from call 2`.

<details><summary>Подсказка</summary>

`ResiliencePipelineBuilder<string>().AddFallback(…)` и `.AddHedging(new HedgingStrategyOptions<string> { MaxHedgedAttempts = 1, Delay = …, ActionGenerator = a => () => a.Callback(a.ActionContext) })`; вызывайте `ExecuteAsync<string>(…)`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 4: fallback value and hedged call.
var fb = new ResiliencePipelineBuilder<string>().AddFallback(new FallbackStrategyOptions<string>
{
    ShouldHandle = new PredicateBuilder<string>().Handle<InvalidOperationException>(),
    FallbackAction = a => Outcome.FromResultAsValueTask("default value")
}).Build();

var hedge = new ResiliencePipelineBuilder<string>().AddHedging(new HedgingStrategyOptions<string>
{
    MaxHedgedAttempts = 1, Delay = TimeSpan.FromMilliseconds(50),
    ShouldHandle = new PredicateBuilder<string>().Handle<Exception>(),
    ActionGenerator = a => () => a.Callback(a.ActionContext)
}).Build();
```

Проверено: `fallback: default value`, `hedging: answer from call 2 (total calls started: 2)`.

</details>
