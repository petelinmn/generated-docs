---
title: "Rate limiting: минимальное решение"
lang: ru
track: csharp
kind: recipe
doc: basics
slug: rate-limiting
part: 2
tags: [rate-limiting, throttling, aspnet-core, fixed-window, http-429]
versions: ".NET 11, C# 15; ASP.NET Core 11.0"
status: draft
updated: 2026-11-22
---

# Rate limiting: минимальное решение

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Минимум** · [2. Production](advanced.ru.md) · [3. Масштаб и грани](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [14 · ASP.NET Core: Minimal API](../../modules/14-aspnet-core-minimal-api/overview.ru.md). **Время:** ~30 мин.

## Задача

Ограничить частоту запросов к API: защититься от перебора паролей, случайных «зацикливаний» клиентов и перегрузки. Превышение отвечается кодом **429 Too Many Requests**. В ASP.NET Core есть встроенный middleware (`Microsoft.AspNetCore.RateLimiting`), пакеты не нужны.

## Минимальное решение

```csharp
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.AddFixedWindowLimiter("fixed", f => { f.PermitLimit = 3; f.Window = TimeSpan.FromSeconds(2); f.QueueLimit = 0; });
});

app.UseRateLimiter();
app.MapGet("/fixed", () => "ok").RequireRateLimiting("fixed");
app.MapGet("/free", () => "ok");                       // no limit
```

Политика `fixed` разрешает 3 запроса за окно в 2 секунды. Проверено на 5 одновременных запросах: 3 получили `200`, 2 — `429`; после окончания окна запрос снова проходит (`200`). Точка `/free` без политики не ограничена: десять запросов — десять `200`. `UseRateLimiter()` вызывают после маршрутизации (в минимальных API — до `Map...`); ограничение подключается к конкретным точкам через `RequireRateLimiting`.

## Алгоритмы

| Алгоритм | Идея | Когда |
|---|---|---|
| Fixed window | N запросов за фиксированное окно | просто и дёшево; допускает всплеск на стыке окон |
| Sliding window | окно скользит по сегментам | сглаживает стык окон |
| Token bucket | ведро токенов, пополняется с постоянной скоростью | допускает всплески, но ограничивает среднюю скорость |
| Concurrency | не более N одновременно | защита тяжёлых операций |

Token bucket проверен: ёмкость 2, пополнение +1 в секунду — первые два запроса `200`, третий сразу `429`, через 1,1 с снова `200`. Concurrency с лимитом 1: из трёх одновременных медленных запросов один получил `200`, два — `429`.

## Ошибки новичка

- ❌ Забыть `UseRateLimiter()` → ✅ политики сами не применяются.
- ❌ Глобальный лимит на всех вместе → ✅ ключ по пользователю или IP ([production](advanced.ru.md#ограничение-по-пользователю)).
- ❌ Не сообщать, когда повторить → ✅ заголовок `Retry-After`.
- ❌ Лимит только в приложении за балансировщиком с несколькими узлами → ✅ см. [масштаб](expert.ru.md#масштаб-несколько-экземпляров).
- ❌ Очередь без ограничения → ✅ `QueueLimit` небольшой или 0.

## Связанные материалы

- Дальше: [production](advanced.ru.md) — ключи, `Retry-After`, очереди, вложенные политики.
- [14 · ASP.NET Core: Minimal API](../../modules/14-aspnet-core-minimal-api/overview.ru.md) — конвейер middleware.
- [JWT-аутентификация](../jwt-auth/overview.ru.md) — защита входа от перебора.
- [Python](../../../python/overview.ru.md) — ограничение частоты в FastAPI.

## Источники

- [Middleware ограничения частоты](https://learn.microsoft.com/aspnet/core/performance/rate-limit)
- [`System.Threading.RateLimiting`](https://learn.microsoft.com/dotnet/api/system.threading.ratelimiting)
