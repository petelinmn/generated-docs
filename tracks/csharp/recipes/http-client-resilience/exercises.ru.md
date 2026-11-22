---
title: "Упражнения: Устойчивый HttpClient"
lang: ru
track: csharp
kind: recipe
doc: exercises
slug: http-client-resilience
part: 2
tags: [httpclient, retry, circuit-breaker, timeout, resilience]
status: draft
updated: 2026-11-22
---

# Упражнения: Устойчивый HttpClient

🌐 **Русский** · [English](exercises.en.md)

> Решайте сами, потом открывайте подсказки. Решения проверены запуском: локальный Kestrel-сервер с нужными ответами + пакет `Microsoft.Extensions.Http.Resilience` 11.0.1. Запускаемых версий в `code/` нет (уровня 5 нет).

## 1. 🟢 Ретраи на нестабильном сервисе

**Задача:** сервер отвечает `503` дважды, затем `200`. Добейтесь, чтобы клиент вернул `200`.

**Ожидаемый результат:** обычный клиент — `503` за 1 вызов, с обработчиком — `200` за 3 вызова.

<details><summary>Подсказка</summary>

`AddHttpClient(...).AddStandardResilienceHandler()`. Для скорости уменьшите `o.Retry.Delay` и отключите джиттер.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 1: standard handler retries transient 503.
services.AddHttpClient("c", c => c.BaseAddress = new Uri(baseUrl))
    .AddStandardResilienceHandler(o =>
    {
        o.Retry.Delay = TimeSpan.FromMilliseconds(50);
        o.Retry.UseJitter = false;
    });
```

Проверено: `standard flaky: 200 after 3 server calls`.

</details>

## 2. 🟡 Таймаут попытки

**Задача:** сервис отвечает за 3 с. Ограничьте попытку 300 мс, один ретрай. Что получит код?

**Ожидаемый результат:** `TimeoutRejectedException` примерно через 630 мс после 2 вызовов.

<details><summary>Подсказка</summary>

`o.AttemptTimeout.Timeout`, `o.TotalRequestTimeout.Timeout`, `o.Retry.MaxRetryAttempts = 1`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 2: per-attempt timeout.
.AddStandardResilienceHandler(o =>
{
    o.AttemptTimeout.Timeout = TimeSpan.FromMilliseconds(300);
    o.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(5);
    o.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(1000);
    o.Retry.MaxRetryAttempts = 1;
    o.Retry.Delay = TimeSpan.FromMilliseconds(10);
});
```

Проверено: `TimeoutRejectedException after 2 server calls, 633ms`. `SamplingDuration` увеличен, потому что валидатор требует, чтобы окно не было короче суммы таймаутов попыток.

> ⚠️ Проверить: точное правило валидации `SamplingDuration` — значение 1000 с взято из рабочего запуска, правило не проверялось по документации.

</details>

## 3. 🟡 Circuit breaker

**Задача:** сервер отвечает `500`; настройте breaker (`MinimumThroughput = 2`, `FailureRatio = 0.5`, `BreakDuration = 1 с`). Сделайте 5 вызовов, затем «вылечите» сервер и повторите после паузы.

**Ожидаемый результат:** вызов 1 — `500`, 2–5 — `BrokenCircuitException` без обращения к серверу; после паузы — `200`.

<details><summary>Подсказка</summary>

Ретрай ≥ 1 обязателен (0 запрещён). Первый вызов делает две попытки — этого достаточно, чтобы пройти порог. Ловите `BrokenCircuitException` из `Polly.CircuitBreaker`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 3: a short-circuiting breaker.
services.AddHttpClient<GitClient>(c => c.BaseAddress = new Uri(baseUrl))
    .AddStandardResilienceHandler(o =>
    {
        o.Retry.MaxRetryAttempts = 1; o.Retry.Delay = TimeSpan.FromMilliseconds(10); o.Retry.UseJitter = false;
        o.CircuitBreaker.MinimumThroughput = 2;
        o.CircuitBreaker.FailureRatio = 0.5;
        o.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(10);
        o.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(1);
        o.AttemptTimeout.Timeout = TimeSpan.FromSeconds(2);
        o.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(5);
    });
```

Проверено: `1: status 500, hit server=True`, `2..5: BrokenCircuitException, hit server=False`, `after break: 200`.

</details>

## 4. 🔴 Свой конвейер без стандартного обработчика

**Задача:** клиент с таймаутом 1 с и двумя ретраями с постоянной паузой 20 мс. Сколько вызовов получит сервер на постоянном `500`?

**Ожидаемый результат:** итоговый `500` после 3 вызовов.

<details><summary>Подсказка</summary>

`AddResilienceHandler("my", p => { p.AddTimeout(…); p.AddRetry(new HttpRetryStrategyOptions { … }); })`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 4: custom resilience pipeline.
.AddResilienceHandler("my", p =>
{
    p.AddTimeout(TimeSpan.FromSeconds(1));
    p.AddRetry(new HttpRetryStrategyOptions
    {
        MaxRetryAttempts = 2, Delay = TimeSpan.FromMilliseconds(20),
        BackoffType = DelayBackoffType.Constant, UseJitter = false
    });
});
```

Проверено: `custom pipeline: 500 after 3 calls`.

</details>
