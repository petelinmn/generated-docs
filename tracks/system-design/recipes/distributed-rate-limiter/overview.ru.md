---
title: "Распределённый rate limiter"
lang: ru
track: system-design
kind: recipe
doc: overview
slug: distributed-rate-limiter
part: 2
levels: [1, 2, 3, 4, 5, 6]
tags: [rate-limiting, redis, token-bucket, lua, distributed-systems]
versions: ".NET 11, C# 15; StackExchange.Redis 3.4.35; Redis 8"
status: draft
updated: 2026-11-27
---

# Распределённый rate limiter

🌐 **Русский** · [English](overview.en.md)

## Коротко

Когда сервис работает несколькими экземплярами, лимит в каждом из них умножается: три экземпляра пропускают втрое больше. Распределённый ограничитель частоты (rate limiter) хранит состояние в общем хранилище. Рецепт строит его на алгоритме token bucket в Redis и решает, что делать, когда Redis недоступен.

## Ключевые идеи

- У token bucket есть размер всплеска и средняя скорость.
- Чтение, пополнение и запись должны быть одним атомарным шагом; это делает Lua-скрипт.
- Берите часы хранилища, а не экземпляров.
- Отказывайте кодом 429 с `Retry-After`.
- Решите заранее: пропускать, запрещать или перейти на локальный лимит.
- Получение токенов пачками экономит обращения ценой точности.

## Минимальный пример

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

## Когда применять и когда нет

- ✅ Лимит, который должен соблюдаться для всего сервиса: квоты, защита общей зависимости, тарифы.
- ❌ Защита одного процесса от перегрузки: локальный ограничитель проще и быстрее.
- ❌ Точный биллинг: нужен надёжный журнал операций.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | алгоритм и ограничитель в одном процессе |
| [2. Продвинутая теория](advanced.ru.md) | Redis, middleware, поведение при отказе |
| [3. Экспертная теория](expert.ru.md) | пачки, горячие ключи, кластер, подвохи, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| [5. Запускаемые примеры](code/Examples/Ex02_Redis.cs) | папка `code/` |
| [6. Тесты](code/Tests/ExamplesTests.cs) | `dotnet test` в `code/` |

## Связанные материалы

- [09 · Паттерны надёжности](../../modules/09-reliability-patterns/overview.ru.md), [02 · Масштабирование](../../modules/02-scalability-and-load-balancing/overview.ru.md)
- В одном процессе: [ограничение частоты в ASP.NET Core](../../../csharp/recipes/rate-limiting/overview.ru.md)
- Пригодится в: [сокращатель ссылок](../url-shortener/overview.ru.md)
