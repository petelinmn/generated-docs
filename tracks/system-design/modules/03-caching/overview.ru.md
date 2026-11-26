---
title: "Кэширование"
lang: ru
track: system-design
kind: module
doc: overview
slug: 03-caching
part: 1
levels: [1, 2, 3, 4, 5, 6]
tags: [system-design, caching, cache-aside, cache-invalidation, cache-stampede, lru-cache, hybrid-cache]
versions: ".NET 11, C# 15; Microsoft.Extensions.Caching.Hybrid 11.0.1"
status: draft
updated: 2026-11-26
---

# Кэширование

🌐 **Русский** · [English](overview.en.md)

## Коротко

Кэш хранит копию данных ближе и дешевле, чем источник. Он снижает задержку и разгружает базу, а взамен приносит устаревшие данные и новые виды отказов. Здесь — архитектурный взгляд; настройка кэшей ASP.NET Core разобрана в [рецепте](../../../csharp/recipes/caching/overview.ru.md).

## Ключевые идеи

- Кэш есть на каждом уровне: браузер, CDN, приложение, общий кэш, база.
- Cache-aside — самый частый шаблон: читаем из кэша, при промахе из источника.
- TTL ограничивает устаревание, инвалидация после записи его сокращает.
- Объём ограничен: нужна политика вытеснения.
- Истечение популярного ключа вызывает stampede.
- L1 в памяти быстр, но у каждого экземпляра свой.
- Доля попаданий определяет, сколько запросов дойдёт до источника.

## Минимальный пример

```csharp
if (cache.TryGet(id, out decimal cached)) return cached;
decimal price = store.Read(id);
cache.Set(id, price, Ttl);
return price;
```

## Когда применять, а когда нет

- ✅ Читают намного чаще, чем пишут.
- ✅ Результат дорого вычислять.
- ✅ Небольшое устаревание допустимо.
- ❌ Решения, требующие актуальных данных: списание денег, остатки.
- ❌ Каждый запрос уникален: попаданий не будет.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | уровни кэша, cache-aside, TTL, инвалидация, HybridCache |
| [2. Продвинутая теория](advanced.ru.md) | стратегии записи, stampede, вытеснение, HTTP-кэш, L1 и L2 |
| [3. Экспертная теория](expert.ru.md) | гонки, справочник, подвохи .NET, вопросы |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| [5. Запускаемые примеры](code/Examples/) | cache-aside, single-flight, LRU, HybridCache |
| [6. Тесты](code/Tests/) | `dotnet test` в папке `code/` |

## Связанные материалы

- Пререквизиты: [масштабирование и балансировка](../02-scalability-and-load-balancing/overview.ru.md)
- Практика в .NET: [рецепт кэширования](../../../csharp/recipes/caching/overview.ru.md)
- Оценки нагрузки: [процесс проектирования](../01-design-process/overview.ru.md)
