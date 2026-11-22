---
title: "Кэширование: production"
lang: ru
track: csharp
kind: recipe
doc: advanced
slug: caching
part: 2
tags: [caching, cache-stampede, hybrid-cache, distributed-cache, invalidation, tags]
versions: ".NET 11, C# 15; Microsoft.Extensions.Caching.Hybrid 11.0.1"
status: draft
updated: 2026-11-22
---

# Кэширование: production

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Минимум](basics.ru.md) · **2. Production** · [3. Масштаб и грани](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [минимальное решение](basics.ru.md). **Время:** ~45 мин.

## Давка (cache stampede)

Когда запись истекла, а N запросов пришли одновременно, наивный кэш запустит N одинаковых дорогих загрузок. Проверено на 10 одновременных вызовах для пустого ключа:

| Кэш | Загрузок |
|---|---|
| `IMemoryCache.GetOrCreateAsync` | **10** — фабрики не синхронизируются |
| `HybridCache.GetOrCreateAsync` | **1** — остальные ждут первую загрузку |

Это главная причина брать `HybridCache`. С `IMemoryCache` защиту (блокировку по ключу, `SemaphoreSlim`) нужно писать самому.

## Теги и инвалидация

`HybridCache` позволяет пометить записи тегами и сбросить группу:

```csharp
await hc.GetOrCreateAsync("p1", async ct => await Load(), tags: ["products"]);
await hc.GetOrCreateAsync("p2", async ct => await Load(), tags: ["products"]);
await hc.RemoveByTagAsync("products");
```

Проверено: после `RemoveByTagAsync("products")` обе записи загрузились заново (2 перезагрузки). Так же работает `EvictByTagAsync` в output cache (проверено: после эвикции тега следующий запрос выполнил обработчик — `call 2`).

## Двухуровневый кэш: L1 + L2

`HybridCache` держит L1 (память процесса) и, если зарегистрирован `IDistributedCache` (Redis, SQL Server…), L2, общий для экземпляров:

```csharp
builder.Services.AddStackExchangeRedisCache(o => o.Configuration = "localhost:6379");   // L2
builder.Services.AddHybridCache();
```

> ⚠️ Проверить: Redis в этом материале не запускался (нет сервера); пакет и параметры подключения сверьте с документацией. Двухуровневое поведение проверено на обёртке над `MemoryDistributedCache` (ниже).

Проверено на двух «экземплярах» с общим L2: первый загрузил `Ann`, второй получил `Ann` из L2 без загрузки (всего 1 загрузка).

**Подвох:** если зарегистрировать как L2 сам `MemoryDistributedCache` (`AddDistributedMemoryCache()`), `HybridCache` его **не использует**: во втором экземпляре загрузка выполнилась повторно (`Bob`, 2 загрузки), в L2 ничего не попало. Проверено; поведение объяснимо — это кэш процесса, а не общий. Для общего L2 нужен настоящий распределённый кэш.

## Инвалидация между экземплярами

`RemoveAsync` на одном экземпляре удаляет запись из L2 и своего L1, но **L1 других экземпляров не сбрасывается**: проверено — после `RemoveAsync` на первом экземпляре второй всё ещё вернул `Ann` из своего L1 (загрузок не прибавилось). Поэтому:

- задавайте короткий `LocalCacheExpiration` (L1), чтобы устаревание было ограничено;
- для строгой согласованности не кэшируйте или добавляйте механизм оповещений (pub/sub, шина).

## Параметры записи

```csharp
services.AddHybridCache(o => o.DefaultEntryOptions = new HybridCacheEntryOptions
{
    Expiration = TimeSpan.FromMinutes(5),            // L2 (and overall) lifetime
    LocalCacheExpiration = TimeSpan.FromMinutes(1)   // L1 lifetime
});
```

`IMemoryCache` управляет размером: при `SizeLimit` каждая запись обязана указать `Size`. Проверено: лимит 2 и три записи размера 1 — вытеснена последняя из-за переполнения (`a=True b=True c=False`, `Count=2`); запись **без** `Size` при заданном `SizeLimit` — `InvalidOperationException`.

## Что кэшировать, а что нет

| Хорошо | Плохо |
|---|---|
| справочники, редко меняющиеся | данные с жёсткой согласованностью (баланс) |
| результаты дорогих запросов | персональные данные под общим ключом |
| ответы внешних API (с учётом их лимитов) | записи без понятной инвалидации |

## Best practices

| Правило | Почему |
|---|---|
| Срок жизни на каждую запись | ограничивает устаревание |
| Ключ включает все входы | нет коллизий |
| `HybridCache` вместо ручного `IMemoryCache` + блокировок | защита от давки |
| Короткий L1 при нескольких экземплярах | L1 не инвалидируется между узлами |
| Метрики попаданий/промахов | видно пользу кэша ([22](../../modules/22-observability/overview.ru.md)) |
| Не кэшировать ошибки надолго | не закрепить сбой |

## Связанные материалы

- Назад: [минимум](basics.ru.md). Дальше: [масштаб и грани](expert.ru.md).
- [22 · Наблюдаемость](../../modules/22-observability/overview.ru.md) — метрики кэша.
- [Rate limiting](../rate-limiting/overview.ru.md) — снижение нагрузки без отказов.
- [Фоновые задачи](../background-jobs/overview.ru.md) — прогрев кэша.

## Источники

- [HybridCache library](https://learn.microsoft.com/aspnet/core/performance/caching/hybrid)
- [Distributed caching](https://learn.microsoft.com/aspnet/core/performance/caching/distributed)
- [Memory caching: size limit](https://learn.microsoft.com/aspnet/core/performance/caching/memory)
