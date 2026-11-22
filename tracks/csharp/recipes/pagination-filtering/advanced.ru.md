---
title: "Пагинация и фильтрация: production"
lang: ru
track: csharp
kind: recipe
doc: advanced
slug: pagination-filtering
part: 2
tags: [pagination, keyset-pagination, cursor, ef-core, api-design, performance]
versions: ".NET 11, C# 15; Microsoft.EntityFrameworkCore.Sqlite 11.0.0"
status: draft
updated: 2026-11-22
---

# Пагинация и фильтрация: production

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Минимум](basics.ru.md) · **2. Production** · [3. Масштаб и грани](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [минимальное решение](basics.ru.md). **Время:** ~45 мин.

## Почему offset ломается

**Дубликаты и пропуски при изменениях.** Проверено: 20 записей, сортировка «новые первыми». Страница 1 — 10 записей (`P20…P11`); между запросами добавили `NEW`; страница 2 (`Skip(10)`) снова содержит `P11` — дубликат на границе. Keyset-вариант (`Id < последний id`) после той же вставки дубликатов не дал (0).

**Стоимость глубокого offset.** БД вынуждена пройти и отбросить все пропускаемые строки. Измерено на SQLite с 1 000 000 строк (среднее из 5 запусков, 20 записей на страницу):

| Позиция | offset | keyset |
|---|---|---|
| 0 | 0 мс | 0 мс |
| 100 000 | 1 мс | 0 мс |
| 900 000 | 15 мс | 0 мс |

Для SQLite и простого индексного ключа разница мала в абсолютных числах, но растёт линейно с позицией; на «тяжёлых» запросах (сортировка по неиндексированному полю, соединения) она в разы больше.

## Составной ключ сортировки

Если сортировка не по уникальному полю (цена, дата), в keyset нужен **tie-breaker** — уникальный ключ в конце. Проверено на 12 записях с повторяющимися ценами: после элемента `(Price = 20, Id = 5)` наивный запрос `Price > 20` пропустил остальные записи с ценой 20 (первым вернул `P08`), а правильный `Price > 20 OR (Price == 20 AND Id > 5)` продолжил с `P06`:

```csharp
var page = await db.Products
    .Where(p => p.Price > lp || (p.Price == lp && p.Id > lid))
    .OrderBy(p => p.Price).ThenBy(p => p.Id)
    .Take(5).ToListAsync();
```

> В выражении запроса нельзя использовать `^1` и индексаторы-диапазоны (`items[^1].Id`): компилятор запрещает их в деревьях выражений (ошибки CS8790/CS8791). Вынесите значение в локальную переменную.

## API с курсором

Курсор делают непрозрачным (Base64) и возвращают в ответе. Берут `size + 1` записей, чтобы узнать, есть ли следующая страница:

```csharp
app.MapGet("/products", async (ShopDb db, string? cursor, int? size, string? category) =>
{
    var take = Math.Clamp(size ?? 10, 1, 20);                       // max page size
    int afterId = 0;
    if (cursor is not null)
    {
        try { afterId = int.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(cursor))); }
        catch (Exception) { return Results.BadRequest("invalid cursor"); }
    }
    var q = db.Products.AsNoTracking().Where(p => p.Id > afterId);
    if (category is not null) q = q.Where(p => p.Category == category);
    var items = await q.OrderBy(p => p.Id).Take(take + 1).ToListAsync();
    var hasMore = items.Count > take;
    if (hasMore) items.RemoveAt(items.Count - 1);
    var next = hasMore ? Convert.ToBase64String(Encoding.UTF8.GetBytes(items[^1].Id.ToString())) : null;
    return Results.Ok(new { items = items.Select(i => i.Name), nextCursor = next });
});
```

Проверено запуском (25 записей, `size=10`): страница 1 — 10 записей и курсор `MTA=`, страница 2 — 10 и `MjA=`, страница 3 — 5 и `null`; всего 25 записей, все различны. `size=1000` ограничен до 20; фильтр `category=even` вернул 12 записей; битый курсор → `400`. Для чтения без изменений используйте `AsNoTracking()` ([15](../../modules/15-ef-core/overview.ru.md)).

## Индексы

Keyset быстр только при индексе, соответствующем `WHERE` и `ORDER BY`. Для сортировки по `(Price, Id)` нужен составной индекс `(Price, Id)`; для фильтра по `Category` — индекс по нему (или составной `(Category, Id)`).

```csharp
modelBuilder.Entity<Product>().HasIndex(p => new { p.Category, p.Id });
```

> ⚠️ Проверить: влияние конкретных индексов на планы запросов в этом материале не измерялось — проверяйте `EXPLAIN` вашей СУБД.

## Offset или keyset

| | Offset | Keyset |
|---|---|---|
| Переход на страницу N | да | нет (только вперёд/назад) |
| Стабильность при изменениях | дубликаты/пропуски | стабильно |
| Скорость на глубоких страницах | падает | постоянная |
| Сложность | низкая | выше (курсор, tie-breaker) |
| Подходит | админки, небольшие наборы | ленты, бесконечная прокрутка, большие таблицы |

## Best practices

| Правило | Почему |
|---|---|
| Явный `OrderBy` по уникальному ключу | детерминированные страницы |
| Верхний предел размера страницы | защита БД |
| Непрозрачный курсор | можно менять устройство без поломки клиентов |
| `AsNoTracking` для чтения | меньше памяти |
| Индексы под фильтр и сортировку | скорость |
| Total только если нужен | `COUNT` на больших таблицах дорог |

## Связанные материалы

- Назад: [минимум](basics.ru.md). Дальше: [масштаб и грани](expert.ru.md).
- [15 · EF Core](../../modules/15-ef-core/overview.ru.md) — индексы и трекинг.
- [19 · Производительность](../../modules/19-performance/overview.ru.md) — измерения.
- [Rate limiting](../rate-limiting/overview.ru.md) — защита от массового обхода страниц.

## Источники

- [EF Core: Pagination](https://learn.microsoft.com/ef/core/querying/pagination)
- [EF Core: Indexes](https://learn.microsoft.com/ef/core/modeling/indexes)
- [Use The Index, Luke: Fetching the next page](https://use-the-index-luke.com/no-offset)
