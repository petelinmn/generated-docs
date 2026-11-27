---
title: "Распределённый rate limiter: продвинутая теория"
lang: ru
track: system-design
kind: recipe
doc: advanced
slug: distributed-rate-limiter
part: 2
tags: [rate-limiting, redis, token-bucket, lua, distributed-systems]
versions: ".NET 11, C# 15; StackExchange.Redis 3.4.35; Redis 8"
status: draft
updated: 2026-11-27
---

# Распределённый rate limiter: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Скрипт

```csharp
local capacity = tonumber(ARGV[1])
local rate = tonumber(ARGV[2])
local cost = tonumber(ARGV[3])

-- the clock of Redis, so that instances with different clocks agree
local t = redis.call('TIME')
local now = tonumber(t[1]) + tonumber(t[2]) / 1000000

local state = redis.call('HMGET', KEYS[1], 'tokens', 'at')
local tokens = tonumber(state[1]) or capacity
local at = tonumber(state[2]) or now
tokens = math.min(capacity, tokens + math.max(0, now - at) * rate)

local allowed = 0
local retry_ms = 0
if tokens >= cost then
  allowed = 1
  tokens = tokens - cost
else
  retry_ms = math.ceil((cost - tokens) / rate * 1000)
end

redis.call('HSET', KEYS[1], 'tokens', tostring(tokens), 'at', string.format('%.6f', now))
-- a bucket idle long enough to be full again can be forgotten
redis.call('PEXPIRE', KEYS[1], math.ceil(capacity / rate * 1000) + 1000)

return { allowed, math.floor(tokens), retry_ms }
```

Скрипт выполняется атомарно: другие клиенты видят все его эффекты или никаких, а сервер на время выполнения заблокирован, поэтому скрипт держат коротким. Все ключи передаются в `KEYS`. `TIME` даёт одни часы для всех экземпляров. `PEXPIRE` удаляет корзины, успевшие полностью наполниться.

```csharp
var result = (RedisResult[])(await redis.ScriptEvaluateAsync(
    Script,
    [(RedisKey)(prefix + key)],
    [limit.Capacity, limit.RefillPerSecond, cost]).WaitAsync(cancellationToken))!;

return new Decision((int)result[0] == 1, (int)result[1], TimeSpan.FromMilliseconds((long)result[2]));
```

Файл: [Ex02_Redis.cs](code/Examples/Ex02_Redis.cs).

Проверка: тридцать одновременных запросов от трёх «экземпляров» при ёмкости 5 — проходят ровно пять.

> Числа Lua в ответах превращаются в целые и теряют дробную часть: `return 3.9` даёт 3. Поэтому число токенов хранится строкой, а в ответе только целые.

## Запуск интеграционных тестов

```bash
docker run --rm -d --name rl-redis -p 127.0.0.1:6379:6379 redis:8-alpine
dotnet test
docker stop rl-redis
```

Без Redis на `127.0.0.1:6379` эти тесты помечаются как пропущенные.

## Ответ клиенту

```csharp
if (!decision.Allowed)
{
    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
    // Whole seconds, rounded up: rounding down invites a request that fails again.
    context.Response.Headers.RetryAfter =
        Math.Max(1, (int)Math.Ceiling(decision.RetryAfter.TotalSeconds)).ToString(CultureInfo.InvariantCulture);
    return;
}
```

429 означает «слишком много запросов»; `Retry-After` подсказывает, когда повторить. Определяйте клиента по тому, что он не может подделать. Файл: [Ex05_Middleware.cs](code/Examples/Ex05_Middleware.cs).

## Когда хранилище отказало

```csharp
try
{
    // The limiter is on every request; it must not become the slowest part of it.
    return await shared.AcquireAsync(key, cost, cancellationToken).AsTask()
        .WaitAsync(timeout, clock, cancellationToken);
}
// A caller's mistake or cancellation is not a store failure and must not be "allowed".
catch (Exception e) when (e is not ArgumentException && !cancellationToken.IsCancellationRequested)
{
    Interlocked.Increment(ref _failures);
    return onFailure switch
    {
        OnFailure.Allow => new Decision(true, 0, TimeSpan.Zero),
        OnFailure.UseFallback when fallback is not null =>
            await fallback.AcquireAsync(key, cost, cancellationToken),
        _ => new Decision(false, 0, TimeSpan.FromSeconds(1)),
    };
}
```

| Политика | Подходит | Риск |
|---|---|---|
| Пропускать (fail open) | важна доступность, лимит — «из вежливости» | защищаемая система открыта |
| Запрещать (fail closed) | попытки входа, дорогие операции | ограничитель роняет сервис |
| Локальный запасной лимит | компромисс | N экземпляров дают N локальных лимитов |

Ожидание ограничено таймаутом. У клиентской библиотеки собственные таймауты по умолчанию 5000 мс — слишком долго для проверки на каждом запросе. Таймаут прекращает ожидание, а не команду: Redis всё ещё может выполнить её и списать токен. Файл: [Ex03_Resilient.cs](code/Examples/Ex03_Resilient.cs).

## Частые ошибки

- ❌ `GET`, расчёт, `SET` из приложения → ✅ один скрипт.
- ❌ Часы каждого экземпляра → ✅ часы хранилища.
- ❌ Ключи без срока жизни → ✅ удалять простаивающие корзины.
- ❌ Новое соединение на запрос → ✅ один общий `ConnectionMultiplexer`.

## Связанные материалы

- [Базовая теория](basics.ru.md)
- [Экспертная теория](expert.ru.md)

## Источники

- [Redis: scripting with Lua](https://redis.io/docs/latest/develop/programmability/eval-intro/)
- [Redis: Lua API](https://redis.io/docs/latest/develop/programmability/lua-api/)
- [Redis: TIME](https://redis.io/docs/latest/commands/time/)
- [RFC 6585, status 429](https://www.rfc-editor.org/rfc/rfc6585#section-4)
- [ASP.NET Core rate limiting](https://learn.microsoft.com/aspnet/core/performance/rate-limit)
- [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis)
