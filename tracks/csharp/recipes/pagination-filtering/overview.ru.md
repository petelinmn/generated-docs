---
title: "Пагинация и фильтрация"
lang: ru
track: csharp
kind: recipe
doc: overview
slug: pagination-filtering
part: 2
levels: [1, 2, 3, 4]
tags: [pagination, filtering, ef-core, offset-pagination, keyset-pagination, cursor]
versions: ".NET 11, C# 15; Microsoft.EntityFrameworkCore.Sqlite 11.0.0"
status: draft
updated: 2026-11-22
---

# Пагинация и фильтрация

🌐 **Русский** · [English](overview.en.md)

## Коротко

Рецепт: отдавать записи порциями и с фильтрами через EF Core. Offset (`Skip/Take`) прост и даёт номера страниц, но деградирует на глубоких страницах и даёт дубликаты при изменениях. Keyset (курсор «после такого-то») стабилен и быстр при подходящем индексе, но не умеет прыжок на страницу N и требует tie-breaker. Все примеры проверены запуском на EF Core 11 + SQLite, включая замер на миллионе строк.

## Ключевые идеи

- Всегда явный `OrderBy` (лучше по уникальному ключу).
- Пагинация — в SQL-запросе, не в памяти.
- Фильтры собираются по частям (`Where` отложенно).
- Offset: дубликат на границе после вставки; на позиции 900 000 из 1 000 000 — 15 мс против 0 мс у keyset.
- Keyset по неуникальному полю требует `Id` как tie-breaker.
- Курсор непрозрачный, размер страницы ограничен, `size + 1` вместо `COUNT`.
- `^1` нельзя внутри выражения запроса.

## Минимальный пример

```csharp
var items = await db.Products.OrderBy(p => p.Id).Skip((page - 1) * size).Take(size).ToListAsync();
```

## Когда применять, а когда нет

- ✅ Любой список, который может вырасти (товары, заказы, события).
- ✅ Offset: админки, небольшие наборы, номера страниц.
- ✅ Keyset: ленты, бесконечная прокрутка, большие таблицы.
- ❌ Offset на больших изменчивых таблицах.
- ❌ Пагинация без сортировки.
- ❌ Динамические фильтры из произвольных полей клиента.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Минимум](basics.ru.md) | offset, keyset, фильтры, SQL |
| [2. Production](advanced.ru.md) | дубликаты, стоимость, tie-breaker, API с курсором, индексы |
| [3. Масштаб и грани](expert.ru.md) | total, поиск, выгрузки, подвохи, 10 вопросов с собеседований |
| [4. Самопроверка](quiz.ru.md) | 10 вопросов и [4 упражнения](exercises.ru.md) с решениями |
| 5. Запускаемые примеры | не сгенерирован — `/learn csharp pagination-filtering level 5` |
| 6. Тесты | не сгенерирован — `/learn csharp pagination-filtering level 6` |

## Связанные материалы

- Пререквизиты: [07 · LINQ](../../modules/07-linq/overview.ru.md), [15 · EF Core](../../modules/15-ef-core/overview.ru.md), [14 · ASP.NET Core: Minimal API](../../modules/14-aspnet-core-minimal-api/overview.ru.md)
- Рядом: [Кэширование](../caching/overview.ru.md), [Rate limiting](../rate-limiting/overview.ru.md), [19 · Производительность](../../modules/19-performance/overview.ru.md)
- В других треках: [Python](../../../python/overview.ru.md) — пагинация в API на Python
- Рецепты и библиотеки: [Dapper](../../libraries/dapper/overview.ru.md), [Mapperly](../../libraries/mapperly/overview.ru.md)
- [System design: проектирование API](../../../system-design/modules/05-api-design/overview.ru.md)
