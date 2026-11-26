---
title: "Rate limiting"
lang: ru
track: csharp
kind: recipe
doc: overview
slug: rate-limiting
part: 2
levels: [1, 2, 3, 4]
tags: [rate-limiting, throttling, aspnet-core, http-429, token-bucket, partitioned-limiter]
versions: ".NET 11, C# 15; ASP.NET Core 11.0"
status: draft
updated: 2026-11-22
---

# Rate limiting

🌐 **Русский** · [English](overview.en.md)

## Коротко

Рецепт: ограничить частоту запросов к API встроенным middleware ASP.NET Core. Превышение отвечается `429 Too Many Requests` с `Retry-After`. Алгоритмы — fixed/sliding window, token bucket и concurrency; лимит задаётся на точку (`RequireRateLimiting`) и разбивается по ключу (пользователь, API-ключ, IP). Счётчики лежат в памяти процесса, поэтому на нескольких экземплярах точную квоту делает шлюз или общее хранилище.

## Ключевые идеи

- `AddRateLimiter` + `UseRateLimiter()` + `RequireRateLimiting("policy")`.
- Fixed window — просто, но допускает до 2N на стыке окон; token bucket допускает всплеск при ограниченной средней скорости.
- Concurrency limiter защищает тяжёлые операции.
- Ключ раздела (`RateLimitPartition`) — справедливость между клиентами.
- `OnRejected` и `Retry-After` — понятный ответ клиенту.
- Лимиты in-memory: при N экземплярах до N × лимит.
- Health checks исключайте из лимитов.

## Минимальный пример

```csharp
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.AddFixedWindowLimiter("fixed", f => { f.PermitLimit = 3; f.Window = TimeSpan.FromSeconds(2); f.QueueLimit = 0; });
});
app.UseRateLimiter();
app.MapGet("/fixed", () => "ok").RequireRateLimiting("fixed");
```

## Когда применять, а когда нет

- ✅ Публичные и аутентификационные точки: защита от перебора и злоупотреблений.
- ✅ Тяжёлые операции: concurrency limiter.
- ✅ Квоты по ключам API (на шлюзе или в приложении).
- ❌ Единственный способ защиты от DDoS (нужны сеть и шлюз).
- ❌ Точная квота на нескольких экземплярах только встроенным лимитером.
- ❌ Лимит без `Retry-After` и мониторинга.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Минимум](basics.ru.md) | задача, `AddRateLimiter`, fixed window, алгоритмы |
| [2. Production](advanced.ru.md) | лимит по пользователю, `Retry-After`, очереди, глобальный и вложенный лимиты |
| [3. Масштаб и грани](expert.ru.md) | несколько экземпляров, шлюзы и Redis, подвохи, альтернативы, 10 вопросов с собеседований |
| [4. Самопроверка](quiz.ru.md) | 10 вопросов и [4 упражнения](exercises.ru.md) с решениями |
| 5. Запускаемые примеры | ещё не создан — `/learn csharp rate-limiting level 5` |
| 6. Тесты | ещё не создан — `/learn csharp rate-limiting level 6` |

## Связанные материалы

- Пререквизиты: [14 · ASP.NET Core: Minimal API](../../modules/14-aspnet-core-minimal-api/overview.ru.md)
- Рядом: [JWT-аутентификация](../jwt-auth/overview.ru.md), [Устойчивый HttpClient](../http-client-resilience/overview.ru.md), [Кэширование](../caching/overview.ru.md)
- То же в других треках: [Python](../../../python/overview.ru.md) — ограничение частоты в FastAPI
- Рецепты и библиотеки: [Polly](../../libraries/polly/overview.ru.md), [Фоновые задачи](../background-jobs/overview.ru.md), [Загрузка файлов](../file-upload/overview.ru.md), [Пагинация и фильтрация](../pagination-filtering/overview.ru.md), [Real-time через SignalR](../signalr-realtime/overview.ru.md)
- [System design: паттерны надёжности](../../../system-design/modules/09-reliability-patterns/overview.ru.md)
