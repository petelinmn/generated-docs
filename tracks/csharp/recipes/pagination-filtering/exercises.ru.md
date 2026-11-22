---
title: "Упражнения: Пагинация и фильтрация"
lang: ru
track: csharp
kind: recipe
doc: exercises
slug: pagination-filtering
part: 2
tags: [pagination, keyset-pagination, ef-core, filtering, minimal-api]
status: draft
updated: 2026-11-22
---

# Упражнения: Пагинация и фильтрация

🌐 **Русский** · [English](exercises.en.md)

> Решайте сами, потом открывайте подсказки. Решения проверены запуском (EF Core 11.0.0 + SQLite). Запускаемых версий в `code/` нет (уровня 5 нет). Модель: `Product { Id, Name, Price, Category }`, 25 записей `P01…P25`.

## 1. 🟢 Offset-страница

**Задача:** верните страницу 2 по 10 записей и общее число записей.

**Ожидаемый результат:** `P11…P20` и `total = 25`.

<details><summary>Подсказка</summary>

`OrderBy(p => p.Id).Skip((page - 1) * size).Take(size)` и `CountAsync()`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 1: offset pagination with total.
int page = 2, size = 10;
var items = await db.Products.OrderBy(p => p.Id).Skip((page - 1) * size).Take(size).ToListAsync();
var total = await db.Products.CountAsync();
```

Проверено: `offset page 2: P11,…,P20`, `total: 25`.

</details>

## 2. 🟡 Keyset-обход

**Задача:** обойдите все 25 записей keyset-страницами по 10, передавая `Id` последнего элемента.

**Ожидаемый результат:** страницы по 10, 10 и 5 записей.

<details><summary>Подсказка</summary>

`Where(p => p.Id > afterId).OrderBy(p => p.Id).Take(take)`; `afterId` — `Id` последнего элемента предыдущей страницы.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 2: keyset pagination.
async Task<List<Product>> Keyset(int afterId, int take) =>
    await db.Products.Where(p => p.Id > afterId).OrderBy(p => p.Id).Take(take).ToListAsync();
var k1 = await Keyset(0, 10);
var k2 = await Keyset(k1[^1].Id, 10);
var k3 = await Keyset(k2[^1].Id, 10);
```

Проверено: `keyset pages: 10,10,5; last of page2=P20`. Обратите внимание, что `k1[^1]` используется **вне** выражения запроса.

</details>

## 3. 🟡 Tie-breaker

**Задача:** есть 12 записей с повторяющимися ценами. Сделайте keyset по `(Price, Id)`: 5 записей, затем следующие 5 после последней. Покажите, чем наивное `Price > lastPrice` хуже.

**Ожидаемый результат:** правильный вариант продолжает с `P06`; наивный — с `P08`.

<details><summary>Подсказка</summary>

Условие `Price > lp || (Price == lp && Id > lid)` и `OrderBy(Price).ThenBy(Id)`; значения `lp`, `lid` — в локальных переменных.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 3: keyset over a non-unique column needs a tie-breaker.
var lp = last.Price; var lid = last.Id;
var naive = await db.Products.Where(p => p.Price > lp)
    .OrderBy(p => p.Price).ThenBy(p => p.Id).Take(5).ToListAsync();
var correct = await db.Products.Where(p => p.Price > lp || (p.Price == lp && p.Id > lid))
    .OrderBy(p => p.Price).ThenBy(p => p.Id).Take(5).ToListAsync();
```

Проверено: `price keyset: after (20,5) naive-first=P08 correct-first=P06` — наивный вариант пропустил `P06` и `P07`.

</details>

## 4. 🔴 API с курсором и ограничением

**Задача:** `GET /products?cursor=&size=&category=`: страница по курсору (Base64 от `Id`), размер не больше 20, фильтр по категории, `nextCursor = null` на последней странице, `400` на битый курсор.

**Ожидаемый результат:** страницы 10, 10, 5; `size=1000` → 20 записей; `category=even` → 12; битый курсор → `400`.

<details><summary>Подсказка</summary>

Возьмите `take + 1` записей; лишнюю отбросьте и по ней узнайте, есть ли продолжение; `Math.Clamp(size ?? 10, 1, 20)`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 4: cursor API with page size clamp and validation.
app.MapGet("/products", async (ShopDb db, string? cursor, int? size, string? category) =>
{
    var take = Math.Clamp(size ?? 10, 1, 20);
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

Проверено: страницы `10, 10, 5` с курсорами `MTA=`, `MjA=`, `null`; `size=1000 clamped: 20`; `filtered even: 12`; `bad cursor: 400`.

</details>
