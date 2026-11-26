---
title: "Кэширование: базовая теория"
lang: ru
track: system-design
kind: module
doc: basics
slug: 03-caching
part: 1
tags: [system-design, caching, cache-aside, cache-invalidation, cache-stampede, lru-cache, hybrid-cache]
versions: ".NET 11, C# 15; Microsoft.Extensions.Caching.Hybrid 11.0.1"
status: verified
updated: 2026-11-26
---

# Кэширование: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** async/await, DI в .NET · **Время:** ~2 ч

## Зачем это нужно

Если 99% запросов обслуживает кэш, при 10 000 запросах в секунду до базы доходит 100. Но верно и обратное: когда кэш отказывает, база внезапно получает все 10 000.

## Уровни кэша

| Где | Что кэширует | Кто управляет |
|---|---|---|
| Браузер | ответы HTTP | заголовки `Cache-Control` |
| CDN и прокси | статика и публичные ответы | заголовки и настройки CDN |
| Память приложения (L1) | объекты | код |
| Распределённый кэш (L2), например Redis | сериализованные значения | код |
| База данных | страницы данных и индексов | СУБД |

## Cache-aside

Приложение само заполняет кэш при промахе, а после записи удаляет запись.

```csharp
public decimal GetPrice(int id)
{
    if (cache.TryGet(id, out decimal cached)) return cached;
    decimal price = store.Read(id);
    cache.Set(id, price, Ttl);
    return price;
}

public void SetPrice(int id, decimal price)
{
    store.Write(id, price);
    cache.Remove(id); // delete, do not update: concurrent writers could leave the older value
}
```

Файл: [Ex01_CacheAside.cs](code/Examples/Ex01_CacheAside.cs).

## TTL и инвалидация

TTL (time to live) — срок, после которого запись считается отсутствующей. Время берётся из `TimeProvider`, поэтому тесты сдвигают часы, а не ждут. Тесты показывают главное ограничение: если кто-то пишет в базу в обход сервиса, кэш отдаёт старое значение, пока не истечёт TTL. TTL — страховка на случай пропущенной инвалидации.

## HybridCache

`HybridCache` из пакета `Microsoft.Extensions.Caching.Hybrid` объединяет L1 и необязательный L2 и выполняет cache-aside одним вызовом.

```csharp
public ValueTask<string> GetNameAsync(int id, Task gate, CancellationToken ct = default) =>
    cache.GetOrCreateAsync(
        $"product:{id}",
        async _ =>
        {
            Interlocked.Increment(ref _loads);
            await gate; // stands in for a slow database query
            return $"Product {id}";
        },
        tags: ["products"],
        cancellationToken: ct);
```

Десять одновременных вызовов с пустым кэшем выполняют загрузку один раз. `RemoveByTagAsync("products")` делает записи с этим тегом недействительными. Файл: [Ex04_HybridCache.cs](code/Examples/Ex04_HybridCache.cs).

## Ошибки новичка

- ❌ Кэш без TTL → ✅ всегда ограничивать срок.
- ❌ Ключ без части входных данных (язык, пользователь) → ✅ всё, что влияет на результат, входит в ключ.
- ❌ Сначала инвалидировать, потом писать в базу → ✅ сначала запись, затем удаление.
- ❌ Считать кэш хранилищем → ✅ система должна пережить его очистку.

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [Рецепт кэширования в ASP.NET Core](../../../csharp/recipes/caching/overview.ru.md)

## Источники

- [HybridCache в ASP.NET Core](https://learn.microsoft.com/aspnet/core/performance/caching/hybrid)
- [Паттерн Cache-Aside](https://learn.microsoft.com/azure/architecture/patterns/cache-aside)
