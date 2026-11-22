---
title: "Exercises: Pagination and filtering"
lang: en
track: csharp
kind: recipe
doc: exercises
slug: pagination-filtering
part: 2
tags: [pagination, keyset-pagination, ef-core, filtering, minimal-api]
status: draft
updated: 2026-11-22
---

# Exercises: Pagination and filtering

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. The solutions were verified by running (EF Core 11.0.0 + SQLite). There are no runnable versions in `code/` (level 5 does not exist). The model: `Product { Id, Name, Price, Category }`, 25 records `P01…P25`.

## 1. 🟢 An offset page

**Task:** return page 2 of 10 records and the total number of records.

**Expected result:** `P11…P20` and `total = 25`.

<details><summary>Hint</summary>

`OrderBy(p => p.Id).Skip((page - 1) * size).Take(size)` and `CountAsync()`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 1: offset pagination with total.
int page = 2, size = 10;
var items = await db.Products.OrderBy(p => p.Id).Skip((page - 1) * size).Take(size).ToListAsync();
var total = await db.Products.CountAsync();
```

Verified: `offset page 2: P11,…,P20`, `total: 25`.

</details>

## 2. 🟡 A keyset walk

**Task:** walk all 25 records in keyset pages of 10, passing the `Id` of the last item.

**Expected result:** pages of 10, 10 and 5 records.

<details><summary>Hint</summary>

`Where(p => p.Id > afterId).OrderBy(p => p.Id).Take(take)`; `afterId` is the `Id` of the last item of the previous page.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 2: keyset pagination.
async Task<List<Product>> Keyset(int afterId, int take) =>
    await db.Products.Where(p => p.Id > afterId).OrderBy(p => p.Id).Take(take).ToListAsync();
var k1 = await Keyset(0, 10);
var k2 = await Keyset(k1[^1].Id, 10);
var k3 = await Keyset(k2[^1].Id, 10);
```

Verified: `keyset pages: 10,10,5; last of page2=P20`. Note that `k1[^1]` is used **outside** the query expression.

</details>

## 3. 🟡 A tie-breaker

**Task:** there are 12 records with repeating prices. Make a keyset by `(Price, Id)`: 5 records, then the next 5 after the last. Show why the naive `Price > lastPrice` is worse.

**Expected result:** the correct variant continues from `P06`; the naive one — from `P08`.

<details><summary>Hint</summary>

The condition `Price > lp || (Price == lp && Id > lid)` and `OrderBy(Price).ThenBy(Id)`; keep `lp`, `lid` in local variables.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 3: keyset over a non-unique column needs a tie-breaker.
var lp = last.Price; var lid = last.Id;
var naive = await db.Products.Where(p => p.Price > lp)
    .OrderBy(p => p.Price).ThenBy(p => p.Id).Take(5).ToListAsync();
var correct = await db.Products.Where(p => p.Price > lp || (p.Price == lp && p.Id > lid))
    .OrderBy(p => p.Price).ThenBy(p => p.Id).Take(5).ToListAsync();
```

Verified: `price keyset: after (20,5) naive-first=P08 correct-first=P06` — the naive variant skipped `P06` and `P07`.

</details>

## 4. 🔴 A cursor API with a limit

**Task:** `GET /products?cursor=&size=&category=`: a page by cursor (Base64 of `Id`), size at most 20, a category filter, `nextCursor = null` on the last page, `400` on a broken cursor.

**Expected result:** pages 10, 10, 5; `size=1000` → 20 records; `category=even` → 12; a broken cursor → `400`.

<details><summary>Hint</summary>

Take `take + 1` records; drop the extra one and use it to learn whether there is a continuation; `Math.Clamp(size ?? 10, 1, 20)`.

</details>

<details><summary>Solution</summary>

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

Verified: pages `10, 10, 5` with cursors `MTA=`, `MjA=`, `null`; `size=1000 clamped: 20`; `filtered even: 12`; `bad cursor: 400`.

</details>
