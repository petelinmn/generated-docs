---
title: "Кэширование"
lang: ru
track: csharp
kind: recipe
doc: overview
slug: caching
part: 2
levels: [1, 2, 3, 4]
tags: [caching, memory-cache, hybrid-cache, output-cache, cache-stampede, redis]
versions: ".NET 11, C# 15; Microsoft.Extensions.Caching.Hybrid 11.0.1"
status: draft
updated: 2026-11-22
---

# Кэширование

🌐 **Русский** · [English](overview.en.md)

## Коротко

Рецепт: не вычислять одно и то же повторно. `IMemoryCache` — простой локальный кэш; `HybridCache` — L1 в памяти плюс необязательный общий L2 (`IDistributedCache`, например Redis), защита от давки и теги; output caching — кэш готовых HTTP-ответов. Главные риски — устаревшие данные, давка и несогласованность между экземплярами. Redis в этом материале не запускался.

## Ключевые идеи

- У каждой записи — срок жизни и ключ, включающий все входные данные.
- `GetOrCreateAsync` — cache-aside в одну строку.
- `IMemoryCache` не защищает от давки (10 одновременных → 10 загрузок), `HybridCache` — защищает (→ 1).
- Теги: `RemoveByTagAsync` (Hybrid), `EvictByTagAsync` (output cache).
- L1 не инвалидируется между экземплярами — задавайте короткий `LocalCacheExpiration`.
- `AddDistributedMemoryCache()` не годится как общий L2.
- `null` кэшируется — осторожно с негативным кэшированием.

## Минимальный пример

```csharp
builder.Services.AddHybridCache();
var value = await hc.GetOrCreateAsync("key", async ct => await Load(ct));
```

## Когда применять, а когда нет

- ✅ Дорогие и часто читаемые данные: справочники, агрегаты, внешние API.
- ✅ Целые ответы публичных эндпоинтов (output cache).
- ✅ Деградация при сбое зависимости.
- ❌ Данные с жёсткой согласованностью (балансы, остатки).
- ❌ Персональные данные под общим ключом.
- ❌ Кэш без метрик и инвалидации.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Минимум](basics.ru.md) | `IMemoryCache`, `HybridCache`, output caching |
| [2. Production](advanced.ru.md) | давка, теги, L1+L2, инвалидация между экземплярами, размер |
| [3. Масштаб и грани](expert.ru.md) | несколько экземпляров, холодный старт, подвохи, 10 вопросов с собеседований |
| [4. Самопроверка](quiz.ru.md) | 10 вопросов и [4 упражнения](exercises.ru.md) с решениями |
| 5. Запускаемые примеры | не сгенерирован — `/learn csharp caching level 5` |
| 6. Тесты | не сгенерирован — `/learn csharp caching level 6` |

## Связанные материалы

- Пререквизиты: [10 · async/await](../../modules/10-async-await/overview.ru.md), [14 · ASP.NET Core: Minimal API](../../modules/14-aspnet-core-minimal-api/overview.ru.md)
- Рядом: [15 · EF Core](../../modules/15-ef-core/overview.ru.md), [19 · Производительность](../../modules/19-performance/overview.ru.md), [Устойчивый HttpClient](../http-client-resilience/overview.ru.md), [Rate limiting](../rate-limiting/overview.ru.md)
- В других треках: [Python](../../../python/overview.ru.md) — `functools.cache`
- Рецепты и библиотеки: [Пагинация и фильтрация](../pagination-filtering/overview.ru.md)
