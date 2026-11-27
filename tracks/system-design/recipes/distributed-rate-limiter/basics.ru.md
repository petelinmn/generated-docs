---
title: "Распределённый rate limiter: базовая теория"
lang: ru
track: system-design
kind: recipe
doc: basics
slug: distributed-rate-limiter
part: 2
tags: [rate-limiting, redis, token-bucket, lua, distributed-systems]
versions: ".NET 11, C# 15; StackExchange.Redis 3.4.35; Redis 8"
status: draft
updated: 2026-11-27
---

# Распределённый rate limiter: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [09 · Паттерны надёжности](../../modules/09-reliability-patterns/basics.ru.md) · **Время:** ~1 ч

## Token bucket

В корзине до `Capacity` токенов, и она пополняется с постоянной скоростью. Запрос забирает токены; если их мало, он отклоняется.

```csharp
// Capacity bounds a burst; the refill rate bounds the long-term average.
public sealed record Limit(int Capacity, double RefillPerSecond);

public sealed record Decision(bool Allowed, int Remaining, TimeSpan RetryAfter);
```

```csharp
public static (Decision Decision, Bucket Next) Take(Bucket? current, Limit limit, DateTimeOffset now, int cost)
{
    // A negative cost would add tokens.
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(cost);
    var bucket = current ?? new Bucket(limit.Capacity, now);
    // Time that goes backwards must not remove or add tokens.
    double elapsed = Math.Max(0, (now - bucket.UpdatedAt).TotalSeconds);
    double tokens = Math.Min(limit.Capacity, bucket.Tokens + elapsed * limit.RefillPerSecond);

    bool allowed = tokens >= cost;
    if (allowed) tokens -= cost;

    TimeSpan retryAfter = allowed
        ? TimeSpan.Zero
        : TimeSpan.FromSeconds((cost - tokens) / limit.RefillPerSecond);
    return (new Decision(allowed, (int)tokens, retryAfter), new Bucket(tokens, now));
}
```

Время передаётся в функцию параметром, поэтому тестам не нужно ждать. При ёмкости 5 и 1 токене в секунду пять запросов проходят сразу, шестой отклоняется с `RetryAfter` в одну секунду.

## В одном процессе

```csharp
lock (_gate) // read and write must be one step
{
    Bucket? current = _buckets.TryGetValue(key, out var found) ? found : null;
    var (decision, next) = TokenBucket.Take(current, limit, clock.GetUtcNow(), cost);
    _buckets[key] = next;
    return ValueTask.FromResult(decision);
}
```

Блокировка важна: без неё два запроса увидели бы один и тот же последний токен. Файл: [Ex01_TokenBucket.cs](code/Examples/Ex01_TokenBucket.cs).

## Почему этого мало

Три экземпляра с таким ограничителем за балансировщиком пропустят 15 запросов вместо 5 (это показывает тест). Состояние должно быть общим: см. [продвинутую теорию](advanced.ru.md).

## Алгоритмы

| Алгоритм | Всплеск | Память на ключ | Примечание |
|---|---|---|---|
| Фиксированное окно | до двух лимитов у границы | один счётчик | самый простой |
| Журнал скользящего окна | точно | метка времени на запрос | дорого |
| Счётчик скользящего окна | приближённо | два счётчика | см. упражнения |
| Token bucket | до ёмкости | два числа | используется здесь |

## Запуск

```bash
cd code
dotnet test --filter "FullyQualifiedName~TokenBucket"
```

## Ошибки новичка

- ❌ Читать и писать без синхронизации → ✅ один атомарный шаг.
- ❌ Брать настоящие часы внутри алгоритма → ✅ передавать время параметром.

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)

## Источники

- [Redis: scripting with Lua](https://redis.io/docs/latest/develop/programmability/eval-intro/)
- [Redis: Lua API](https://redis.io/docs/latest/develop/programmability/lua-api/)
- [Redis: TIME](https://redis.io/docs/latest/commands/time/)
- [RFC 6585, status 429](https://www.rfc-editor.org/rfc/rfc6585#section-4)
- [ASP.NET Core rate limiting](https://learn.microsoft.com/aspnet/core/performance/rate-limit)
- [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis)
