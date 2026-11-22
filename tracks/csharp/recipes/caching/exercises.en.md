---
title: "Exercises: Caching"
lang: en
track: csharp
kind: recipe
doc: exercises
slug: caching
part: 2
tags: [caching, memory-cache, hybrid-cache, output-cache, cache-stampede]
status: draft
updated: 2026-11-22
---

# Exercises: Caching

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. The solutions were verified by running (`Microsoft.Extensions.Caching.Hybrid` 11.0.1; output cache is part of ASP.NET Core). There are no runnable versions in `code/` (level 5 does not exist).

## 1. 🟢 A cache with expiry

**Task:** cache an expensive load in `IMemoryCache` for 300 ms. Check that two calls give one load and that after a pause there is a new one.

**Expected result:** `v1 v1 loads=1`, after 400 ms — `v2 loads=2`.

<details><summary>Hint</summary>

`GetOrCreateAsync` and `e.AbsoluteExpirationRelativeToNow`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 1: memory cache with absolute expiration.
var a = await mem.GetOrCreateAsync("k", async e =>
{
    e.AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(300);
    return await Load();
});
```

Verified: `memory: v1 v1 loads=1`, `memory after expiry: v2 loads=2`.

</details>

## 2. 🟡 A stampede

**Task:** run 10 simultaneous requests for one empty key in `IMemoryCache` and in `HybridCache`. Compare the number of loads.

**Expected result:** 10 and 1.

<details><summary>Hint</summary>

`Enumerable.Range(0, 10).Select(_ => …GetOrCreateAsync(…))` and `Task.WhenAll`; count loads through `Interlocked`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 2: HybridCache coalesces concurrent loads of one key.
var ht = Enumerable.Range(0, 10)
    .Select(_ => hc.GetOrCreateAsync("h", async ct => await Load()).AsTask()).ToArray();
await Task.WhenAll(ht);
```

Verified: `memory stampede: loads=10`, `hybrid stampede: loads=1`.

</details>

## 3. 🟡 Invalidation by tag

**Task:** put two entries with the tag `products` into `HybridCache`, reset the tag and check that both loaded again.

**Expected result:** `reloads=2`.

<details><summary>Hint</summary>

`GetOrCreateAsync(key, factory, tags: ["products"])` and `RemoveByTagAsync("products")`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 3: group invalidation with tags.
await hc.GetOrCreateAsync("p1", async ct => await Load(), tags: ["products"]);
await hc.GetOrCreateAsync("p2", async ct => await Load(), tags: ["products"]);
await hc.RemoveByTagAsync("products");
```

Verified: `hybrid RemoveByTag: reloads=2`.

</details>

## 4. 🔴 A shared L2 between two instances

**Task:** create two service providers with `HybridCache` and a shared `IDistributedCache`. The second must get the first's value without loading. Then reset the key on the first and check what the second returns.

**Expected result:** `instance1=Ann instance2=Ann loads=1`; after `RemoveAsync` on the first the second still returns `Ann` (from its L1).

<details><summary>Hint</summary>

`AddDistributedMemoryCache()` will not do — `HybridCache` does not use it as L2. Wrap `MemoryDistributedCache` in your own class implementing `IDistributedCache` (in production it is Redis).

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 4: two instances sharing one L2 via a custom IDistributedCache wrapper.
var shared = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
s.AddSingleton<IDistributedCache>(new Wrap(shared));
s.AddHybridCache();

class Wrap(IDistributedCache inner) : IDistributedCache
{
    public byte[]? Get(string key) => inner.Get(key);
    public Task<byte[]?> GetAsync(string key, CancellationToken token = default) => inner.GetAsync(key, token);
    public void Refresh(string key) => inner.Refresh(key);
    public Task RefreshAsync(string key, CancellationToken token = default) => inner.RefreshAsync(key, token);
    public void Remove(string key) => inner.Remove(key);
    public Task RemoveAsync(string key, CancellationToken token = default) => inner.RemoveAsync(key, token);
    public void Set(string key, byte[] value, DistributedCacheEntryOptions options) => inner.Set(key, value, options);
    public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default) => inner.SetAsync(key, value, options, token);
}
```

Verified: `instance1=Ann instance2=Ann loads=1`; after `RemoveAsync` on the first instance the second returned `Ann`, the loads still 1.

</details>
