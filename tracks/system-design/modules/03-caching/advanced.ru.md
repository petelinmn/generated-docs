---
title: "Кэширование: продвинутая теория"
lang: ru
track: system-design
kind: module
doc: advanced
slug: 03-caching
part: 1
tags: [system-design, caching, cache-aside, cache-invalidation, cache-stampede, lru-cache, hybrid-cache]
versions: ".NET 11, C# 15; Microsoft.Extensions.Caching.Hybrid 11.0.1"
status: draft
updated: 2026-11-26
---

# Кэширование: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Стратегии записи

| Стратегия | Как работает | Риск |
|---|---|---|
| Cache-aside | приложение читает и инвалидирует | гонка чтения и записи |
| Read-through | кэш сам загружает при промахе | логика загрузки в слое кэша |
| Write-through | запись в кэш и источник вместе | медленнее запись |
| Write-behind | сначала кэш, источник позже | потеря данных при сбое |
| Refresh-ahead | обновление до истечения | лишняя работа для редких ключей |

## Cache stampede

Популярный ключ истёк, и все запросы одновременно пошли в базу. Первое средство — объединять одновременные загрузки одного ключа (single-flight).

```csharp
// Lazy guarantees one invocation even if GetOrAdd creates several candidates.
var flight = _inFlight.GetOrAdd(key, _ => new Lazy<Task<TValue>>(load));
try
{
    return await flight.Value;
}
finally
{
    // Remove only our own entry, so a failed load is retried by the next caller.
    _inFlight.TryRemove(new KeyValuePair<TKey, Lazy<Task<TValue>>>(key, flight));
}
```

Защита действует в пределах процесса: десять экземпляров сделают до десяти загрузок. Второе средство — разброс TTL (jitter), чтобы ключи, записанные вместе, не истекали вместе.

```csharp
public static TimeSpan WithJitter(TimeSpan ttl, double ratio, Random random) =>
    ttl * (1 + ratio * (2 * random.NextDouble() - 1));
```

Файл: [Ex02_Stampede.cs](code/Examples/Ex02_Stampede.cs). Третье — отдавать устаревшее значение, пока идёт фоновое обновление (stale-while-revalidate).

## Вытеснение

| Политика | Удаляется | Слабое место |
|---|---|---|
| LRU | давно не использованное | один полный проход вымывает горячие ключи |
| LFU | редко используемое | медленно забывает бывшие популярные |
| FIFO | самое старое по записи | не учитывает обращения |
| Random | случайное | непредсказуемо, но дёшево |

LRU за O(1) — это словарь плюс двусвязный список: при обращении узел переносится в начало, при переполнении удаляется хвост. Реализация: [Ex03_Lru.cs](code/Examples/Ex03_Lru.cs).

## HTTP-кэширование

| Директива | Смысл |
|---|---|
| `max-age` | сколько секунд ответ свеж |
| `s-maxage` | то же для общих кэшей, важнее `max-age` |
| `no-cache` | сохранять можно, но использовать только после проверки у сервера |
| `no-store` | не сохранять |
| `private` | не хранить в общем кэше |
| `stale-while-revalidate` | можно отдать устаревший ответ и обновить в фоне |
| `stale-if-error` | можно отдать устаревший ответ при ошибке сервера |

## L1 и L2

L1 быстр и не требует сети, но у каждого экземпляра свой. `RemoveByTagAsync` в `HybridCache` действует на текущий экземпляр и L2; память других экземпляров он не затрагивает. Задавайте короткий `LocalCacheExpiration`: он ограничивает время расхождения.

## Лучшие практики

- Измеряйте долю попаданий, задержку источника и число вытеснений.
- Проектируйте поведение при отказе кэша: таймаут, ограничение нагрузки на базу.
- Версионируйте ключи, когда меняется формат значения.
- Ограничивайте размер значения и число ключей.

## Частые ошибки

- ❌ Прогревать кэш одинаковым TTL → ✅ добавлять jitter.
- ❌ Не кэшировать «не найдено» при частых запросах несуществующих ключей → ✅ негативное кэширование с коротким TTL.
- ❌ Горячий ключ только в L2 → ✅ добавить L1 или копии ключа.
- ❌ Кэшировать персональный ответ в CDN → ✅ `private` или `no-store`.

## Сравнение с другими треками

- Настройка `IMemoryCache`, `HybridCache` и output caching — [рецепт C#](../../../csharp/recipes/caching/overview.ru.md).
- Кэширование промпта у LLM — другая механика с той же идеей повторного использования: [Claude: prompt caching](../../../claude/modules/05-prompt-caching/overview.ru.md).

## Связанные материалы

- [Базовая теория](basics.ru.md)
- [Экспертная теория](expert.ru.md)

## Источники

- [HybridCache в ASP.NET Core](https://learn.microsoft.com/aspnet/core/performance/caching/hybrid)
- [RFC 9111: HTTP Caching](https://www.rfc-editor.org/rfc/rfc9111)
- [RFC 5861: stale-while-revalidate и stale-if-error](https://www.rfc-editor.org/rfc/rfc5861)
