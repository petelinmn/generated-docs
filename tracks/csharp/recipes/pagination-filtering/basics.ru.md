---
title: "Пагинация и фильтрация: минимальное решение"
lang: ru
track: csharp
kind: recipe
doc: basics
slug: pagination-filtering
part: 2
tags: [pagination, filtering, ef-core, offset-pagination, keyset-pagination, linq]
versions: ".NET 11, C# 15; Microsoft.EntityFrameworkCore.Sqlite 11.0.0"
status: draft
updated: 2026-11-22
---

# Пагинация и фильтрация: минимальное решение

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Минимум** · [2. Production](advanced.ru.md) · [3. Масштаб и грани](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [07 · LINQ](../../modules/07-linq/overview.ru.md), [15 · EF Core](../../modules/15-ef-core/overview.ru.md), [14 · ASP.NET Core: Minimal API](../../modules/14-aspnet-core-minimal-api/overview.ru.md). **Время:** ~30 мин.

## Задача

Вернуть из API список записей не целиком, а порциями (страницами) и с фильтрами. Без пагинации большой список перегружает БД, сеть и клиента. Есть два основных способа: **offset** (`Skip/Take`, номер страницы) и **keyset** (курсор: «записи после такого-то»).

## Offset-пагинация

```csharp
int page = 2, size = 10;
var q = db.Products.OrderBy(p => p.Id).Skip((page - 1) * size).Take(size);
var items = await q.ToListAsync();
var total = await db.Products.CountAsync();
```

Проверено на 25 записях (SQLite): страница 2 — `P11…P20`, `total = 25`. Сгенерированный SQL:

```sql
SELECT "p"."Id", "p"."Category", "p"."Name", "p"."Price"
FROM "Products" AS "p"
ORDER BY "p"."Id"
LIMIT @p OFFSET @p
```

(в выводе `ToQueryString()` оба параметра названы `@p` со значением 10 — это особенность отображения: в примере `OFFSET = 10` и `LIMIT = 10`).

**Обязательно задавайте `OrderBy`**: без явной сортировки порядок строк не гарантирован, и страницы могут пересекаться или терять записи.

## Keyset-пагинация

Вместо номера страницы — «значение последнего элемента»:

```csharp
async Task<List<Product>> Keyset(int afterId, int take) =>
    await db.Products.Where(p => p.Id > afterId).OrderBy(p => p.Id).Take(take).ToListAsync();
```

Проверено: три вызова подряд (`afterId = 0`, затем `Id` последнего элемента) вернули 10, 10 и 5 записей. SQL — `WHERE "p"."Id" > 10 ORDER BY "p"."Id" LIMIT @p`: БД идёт по индексу сразу к нужному месту, а не отсчитывает пропускаемые строки.

## Фильтрация

Условия добавляют по необходимости, собирая запрос по частям (запрос не выполняется, пока вы не вызвали `ToListAsync`):

```csharp
var filtered = db.Products.AsQueryable();
if (cat is not null) filtered = filtered.Where(p => p.Category == cat);
if (min is not null) filtered = filtered.Where(p => p.Price >= min);
```

Проверено: категория `x` и цена от 20 дали 9 записей; SQL `WHERE "p"."Category" = @cat AND ef_compare("p"."Price", @min) >= 0` (SQLite хранит `decimal` не нативно, поэтому EF добавляет вызов `ef_compare` — на других СУБД он отсутствует).

## Частые ошибки новичка

- ❌ `Skip/Take` без `OrderBy` → ✅ всегда явная сортировка (лучше по уникальному ключу).
- ❌ Возвращать всё и пагинировать в памяти (`ToList().Skip()`) → ✅ пагинация в запросе до `ToListAsync`.
- ❌ Размер страницы от клиента без ограничения → ✅ `Math.Clamp` ([production](advanced.ru.md#api-с-курсором)).
- ❌ `Count` на каждый запрос для огромных таблиц → ✅ решить, нужен ли точный total.
- ❌ Строить фильтры конкатенацией SQL-строк → ✅ LINQ-предикаты (параметризация защищает от SQL-инъекций).

## Связанные материалы

- Дальше: [production](advanced.ru.md) — API с курсором, составной ключ, стабильность.
- [07 · LINQ](../../modules/07-linq/overview.ru.md) — `IQueryable` и отложенное выполнение.
- [15 · EF Core](../../modules/15-ef-core/overview.ru.md) — запросы и трекинг.
- [14 · ASP.NET Core: Minimal API](../../modules/14-aspnet-core-minimal-api/overview.ru.md) — параметры запроса.
- [Кэширование](../caching/overview.ru.md) — кэш страниц.
- [Python](../../../python/overview.ru.md) — пагинация в API на Python.

## Источники

- [EF Core: Pagination](https://learn.microsoft.com/ef/core/querying/pagination)
- [EF Core: Querying data](https://learn.microsoft.com/ef/core/querying/)
- [EF Core: SQLite provider limitations](https://learn.microsoft.com/ef/core/providers/sqlite/limitations)
