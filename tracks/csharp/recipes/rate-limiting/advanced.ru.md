---
title: "Rate limiting: production"
lang: ru
track: csharp
kind: recipe
doc: advanced
slug: rate-limiting
part: 2
tags: [rate-limiting, partitioned-limiter, retry-after, token-bucket, concurrency-limiter]
versions: ".NET 11, C# 15; ASP.NET Core 11.0"
status: draft
updated: 2026-11-22
---

# Rate limiting: production

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Минимум](basics.ru.md) · **2. Production** · [3. Масштаб и грани](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [минимальное решение](basics.ru.md). **Время:** ~45 мин.

## Ограничение по пользователю

Общий лимит на всех бессмыслен: один клиент съест лимит остальных. Нужен **ключ раздела** (partition): пользователь, API-ключ или IP.

```csharp
o.AddPolicy("per-user", ctx => RateLimitPartition.GetFixedWindowLimiter(
    ctx.Request.Headers["X-User"].FirstOrDefault() ?? "anonymous",
    _ => new FixedWindowRateLimiterOptions { PermitLimit = 2, Window = TimeSpan.FromSeconds(5), QueueLimit = 0 }));
```

Проверено: пользователь `ann` — `200 200 429`, а `bob` в то же время — `200`: у каждого ключа свой счётчик. В реальном приложении ключ берут из аутентифицированного пользователя (`ctx.User.Identity?.Name`), для анонимов — из IP (`ctx.Connection.RemoteIpAddress`); за прокси нужен корректный `X-Forwarded-For` (`UseForwardedHeaders`), иначе IP у всех один — адрес прокси.

## Ответ клиенту: Retry-After

Клиенту полезно знать, когда повторить:

```csharp
o.OnRejected = (ctx, ct) =>
{
    if (ctx.Lease.TryGetMetadata(MetadataName.RetryAfter, out var ra))
        ctx.HttpContext.Response.Headers.RetryAfter = ((int)ra.TotalSeconds).ToString();
    return ValueTask.CompletedTask;
};
```

Проверено: отказ содержит `Retry-After` (для fixed window — оставшиеся секунды окна, например `1` или `4`). `OnRejected` — место для логирования отказов и тела ответа (`ProblemDetails`, [14](../../modules/14-aspnet-core-minimal-api/advanced.ru.md#валидация-и-problemdetails)).

## Очереди и выбор алгоритма

`QueueLimit > 0` ставит лишние запросы в очередь вместо немедленного отказа (`QueueProcessingOrder.OldestFirst`/`NewestFirst`): запрос ждёт освобождения лимита. Очередь сглаживает короткие всплески, но удерживает соединения и память; для большинства API `QueueLimit = 0` и быстрый отказ лучше.

| Задача | Алгоритм |
|---|---|
| защита входа от перебора | fixed/sliding window по IP+логину |
| публичный API с квотой | fixed window по ключу API |
| допускать всплески при ограниченной средней скорости | token bucket (проверено: 2 токена, +1/с) |
| тяжёлая операция (отчёт, экспорт) | concurrency (проверено: 1 из 3 одновременных) |

## Глобальный и вложенный лимиты

`GlobalLimiter` применяется ко всем запросам; `RequireRateLimiting` добавляет ограничение точки поверх. Для исключений есть `DisableRateLimiting()` (например, health checks, [22 · Наблюдаемость](../../modules/22-observability/overview.ru.md)). Цепочка лимитеров комбинируется через `PartitionedRateLimiter.CreateChained`.

## Лучшие практики

| Правило | Зачем |
|---|---|
| Лимит по ключу, а не общий | справедливость |
| `Retry-After` и понятное тело 429 | клиенты не долбят повторно |
| Строже на `/login`, мягче на чтении | разные риски |
| Логируйте отказы и считайте метрикой | видеть атаки и неверные лимиты |
| Исключайте health checks | оркестратор не должен получать 429 |
| Тестируйте границы окна | ошибки на стыке окон |

## Связанные материалы

- Назад: [минимум](basics.ru.md). Дальше: [масштаб и грани](expert.ru.md).
- [JWT-аутентификация](../jwt-auth/overview.ru.md) — ключ по пользователю.
- [22 · Наблюдаемость](../../modules/22-observability/overview.ru.md) — метрики отказов.
- [Кэширование](../caching/overview.ru.md) — снижение нагрузки без отказов.

## Источники

- [Ограничение частоты в ASP.NET Core](https://learn.microsoft.com/aspnet/core/performance/rate-limit)
- [Forwarded headers](https://learn.microsoft.com/aspnet/core/host-and-deploy/proxy-load-balancer)
