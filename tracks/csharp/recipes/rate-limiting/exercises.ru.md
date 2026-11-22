---
title: "Упражнения: Rate limiting"
lang: ru
track: csharp
kind: recipe
doc: exercises
slug: rate-limiting
part: 2
tags: [rate-limiting, fixed-window, token-bucket, partitioned-limiter, concurrency-limiter]
status: draft
updated: 2026-11-22
---

# Упражнения: Rate limiting

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки. Решения проверены запуском (Kestrel + `HttpClient`; встроенный middleware, пакеты не нужны). Запускаемых версий в `code/` нет (уровень 5 не создан).

## 1. 🟢 Фиксированное окно

**Задача:** ограничь `/fixed` тремя запросами за 2 секунды. Отправь 5 одновременных запросов и один после окна.

**Ожидаемый результат:** среди 5 — три `200` и два `429`; после окна — `200`.

<details><summary>Подсказка</summary>

`AddFixedWindowLimiter("fixed", f => { f.PermitLimit = 3; f.Window = TimeSpan.FromSeconds(2); f.QueueLimit = 0; })` и `RequireRateLimiting("fixed")`; `app.UseRateLimiter()`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 1: a fixed window policy attached to an endpoint.
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.AddFixedWindowLimiter("fixed", f => { f.PermitLimit = 3; f.Window = TimeSpan.FromSeconds(2); f.QueueLimit = 0; });
});
app.UseRateLimiter();
app.MapGet("/fixed", () => "ok").RequireRateLimiting("fixed");
```

Проверено: `429 200 200 429 200` (порядок параллельных ответов не определён), после окна — `200`.

</details>

## 2. 🟡 Лимит по пользователю с Retry-After

**Задача:** политика `per-user`: 2 запроса за 5 секунд на пользователя (заголовок `X-User`). Отказы должны содержать `Retry-After`.

**Ожидаемый результат:** `ann`: `200 200 429(Retry-After=…)`, `bob`: `200`.

<details><summary>Подсказка</summary>

`o.AddPolicy("per-user", ctx => RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions { … }))` и `OnRejected`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 2: a partitioned limiter and the Retry-After header.
o.AddPolicy("per-user", ctx => RateLimitPartition.GetFixedWindowLimiter(
    ctx.Request.Headers["X-User"].FirstOrDefault() ?? "anonymous",
    _ => new FixedWindowRateLimiterOptions { PermitLimit = 2, Window = TimeSpan.FromSeconds(5), QueueLimit = 0 }));
o.OnRejected = (ctx, ct) =>
{
    if (ctx.Lease.TryGetMetadata(MetadataName.RetryAfter, out var ra))
        ctx.HttpContext.Response.Headers.RetryAfter = ((int)ra.TotalSeconds).ToString();
    return ValueTask.CompletedTask;
};
```

Проверено: `ann: 200 200 429(Retry-After=4)`, `bob: 200`.

</details>

## 3. 🟡 Token bucket

**Задача:** ёмкость 2 токена, пополнение +1 в секунду. Отправь три запроса подряд, подожди 1,1 с и отправь ещё один.

**Ожидаемый результат:** `200 200 429 200`.

<details><summary>Подсказка</summary>

`AddTokenBucketLimiter("bucket", t => { t.TokenLimit = 2; t.TokensPerPeriod = 1; t.ReplenishmentPeriod = TimeSpan.FromSeconds(1); t.QueueLimit = 0; t.AutoReplenishment = true; })`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 3: a burst of 2 is allowed, then one request per second on average.
o.AddTokenBucketLimiter("bucket", t =>
{
    t.TokenLimit = 2; t.TokensPerPeriod = 1; t.ReplenishmentPeriod = TimeSpan.FromSeconds(1);
    t.QueueLimit = 0; t.AutoReplenishment = true;
});
```

Проверено: `200 200 429(Retry-After=1) 200`.

</details>

## 4. 🔴 Защита тяжёлой операции

**Задача:** точка `/slow` (300 мс работы) должна обслуживать не больше одного запроса одновременно, остальные получают 429. Проверь тремя одновременными запросами.

**Ожидаемый результат:** один `200`, два `429`.

<details><summary>Подсказка</summary>

`AddConcurrencyLimiter("concurrent", c => { c.PermitLimit = 1; c.QueueLimit = 0; })`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 4: a concurrency limiter protects an expensive endpoint.
o.AddConcurrencyLimiter("concurrent", c => { c.PermitLimit = 1; c.QueueLimit = 0; });
app.MapGet("/slow", async () => { await Task.Delay(300); return "done"; }).RequireRateLimiting("concurrent");
```

Проверено: `200 429 429`. С `QueueLimit > 0` лишние запросы ждали бы, а не получали отказ.

</details>
