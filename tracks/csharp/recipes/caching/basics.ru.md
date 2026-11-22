---
title: "Кэширование: минимальное решение"
lang: ru
track: csharp
kind: recipe
doc: basics
slug: caching
part: 2
tags: [caching, memory-cache, hybrid-cache, output-cache, expiration]
versions: ".NET 11, C# 15; Microsoft.Extensions.Caching.Hybrid 11.0.1"
status: draft
updated: 2026-11-22
---

# Кэширование: минимальное решение

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Минимум** · [2. Production](advanced.ru.md) · [3. Масштаб и грани](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [10 · async/await](../../modules/10-async-await/overview.ru.md), [14 · ASP.NET Core: Minimal API](../../modules/14-aspnet-core-minimal-api/overview.ru.md). **Время:** ~30 мин.

## Задача

Не вычислять и не запрашивать одно и то же снова и снова: ответ БД, результат внешнего вызова, отрендеренная страница. Кэш снижает задержку и нагрузку, но привносит риск устаревших данных, поэтому у каждой записи должен быть срок жизни и понятный способ сброса.

## Три инструмента

| Инструмент | Где хранит | Когда |
|---|---|---|
| `IMemoryCache` | в памяти процесса | простой локальный кэш, один экземпляр |
| `HybridCache` | L1 в памяти + (необязательно) L2 в `IDistributedCache` | современный выбор: защита от «давки», теги, общий кэш |
| Output caching | готовые HTTP-ответы | кэшировать ответ эндпоинта целиком |

## `IMemoryCache`

```csharp
builder.Services.AddMemoryCache();

var a = await mem.GetOrCreateAsync("k", async e =>
{
    e.AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(300);
    return await Load();           // an expensive call
});
```

Проверено: два последовательных обращения — `v1 v1`, загрузка выполнена **один** раз; после истечения срока (400 мс) — новая загрузка (`v2`, всего 2 загрузки).

## `HybridCache`

```bash
dotnet add package Microsoft.Extensions.Caching.Hybrid
```

```csharp
builder.Services.AddHybridCache();

var value = await hc.GetOrCreateAsync("h", async ct => await Load(ct));
await hc.RemoveAsync("h");
```

Проверено: срок жизни записи (`Expiration = 1 с`) истёк → перезагрузка (`v2`); `RemoveAsync` → следующее обращение снова грузит (`v3`).

## Output caching

Кэш готового ответа эндпоинта (`Microsoft.AspNetCore.OutputCaching`, входит в ASP.NET Core):

```csharp
builder.Services.AddOutputCache(o => o.AddPolicy("short", p => p.Expire(TimeSpan.FromSeconds(1)).Tag("items")));
app.UseOutputCache();
app.MapGet("/time", () => $"call {Interlocked.Increment(ref n)}").CacheOutput("short");
```

Проверено: два запроса подряд — `call 1`, `call 1` (обработчик вызван один раз); после истечения 1 с — `call 3`; эндпоинт без `CacheOutput` вызывается каждый раз (`call 4`, `call 5`).

## Частые ошибки новичка

- ❌ Кэш без срока жизни → ✅ всегда задавайте `Expiration`/`AbsoluteExpiration`.
- ❌ Ключ без параметров запроса (`"products"` для разных фильтров) → ✅ ключ включает все входные данные.
- ❌ Кэширование данных конкретного пользователя под общим ключом → ✅ утечка данных между пользователями.
- ❌ «Сначала проверить, потом записать» вручную → ✅ `GetOrCreateAsync`/`HybridCache`, иначе давка ([production](advanced.ru.md#давка-cache-stampede)).
- ❌ Кэшировать всё подряд → ✅ только дорогое и часто читаемое.

## Связанные материалы

- Дальше: [production](advanced.ru.md) — давка, теги, L2, инвалидация.
- [14 · ASP.NET Core: Minimal API](../../modules/14-aspnet-core-minimal-api/overview.ru.md) — middleware и endpoints.
- [15 · EF Core](../../modules/15-ef-core/overview.ru.md) — что кэшировать из БД.
- [Устойчивый HttpClient](../http-client-resilience/overview.ru.md) — кэш как деградация при сбое зависимости.
- [Python](../../../python/overview.ru.md) — `functools.cache` и кэши в веб-фреймворках.

## Источники

- [Memory caching in ASP.NET Core](https://learn.microsoft.com/aspnet/core/performance/caching/memory)
- [HybridCache library](https://learn.microsoft.com/aspnet/core/performance/caching/hybrid)
- [Output caching middleware](https://learn.microsoft.com/aspnet/core/performance/caching/output)
