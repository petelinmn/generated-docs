---
title: "Упражнения: Кэширование"
lang: ru
track: csharp
kind: recipe
doc: exercises
slug: caching
part: 2
tags: [caching, memory-cache, hybrid-cache, output-cache, cache-stampede]
status: draft
updated: 2026-11-22
---

# Упражнения: Кэширование

🌐 **Русский** · [English](exercises.en.md)

> Решайте сами, потом открывайте подсказки. Решения проверены запуском (`Microsoft.Extensions.Caching.Hybrid` 11.0.1; output cache — часть ASP.NET Core). Запускаемых версий в `code/` нет (уровня 5 нет).

## 1. 🟢 Кэш с истечением

**Задача:** кэшируйте дорогую загрузку в `IMemoryCache` на 300 мс. Проверьте, что два вызова дают одну загрузку, а после паузы — новую.

**Ожидаемый результат:** `v1 v1 loads=1`, после 400 мс — `v2 loads=2`.

<details><summary>Подсказка</summary>

`GetOrCreateAsync` и `e.AbsoluteExpirationRelativeToNow`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 1: memory cache with absolute expiration.
var a = await mem.GetOrCreateAsync("k", async e =>
{
    e.AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(300);
    return await Load();
});
```

Проверено: `memory: v1 v1 loads=1`, `memory after expiry: v2 loads=2`.

</details>

## 2. 🟡 Давка

**Задача:** запустите 10 одновременных запросов на один пустой ключ в `IMemoryCache` и в `HybridCache`. Сравните число загрузок.

**Ожидаемый результат:** 10 и 1.

<details><summary>Подсказка</summary>

`Enumerable.Range(0, 10).Select(_ => …GetOrCreateAsync(…))` и `Task.WhenAll`; счётчик загрузок через `Interlocked`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 2: HybridCache coalesces concurrent loads of one key.
var ht = Enumerable.Range(0, 10)
    .Select(_ => hc.GetOrCreateAsync("h", async ct => await Load()).AsTask()).ToArray();
await Task.WhenAll(ht);
```

Проверено: `memory stampede: loads=10`, `hybrid stampede: loads=1`.

</details>

## 3. 🟡 Инвалидация по тегу

**Задача:** положите в `HybridCache` две записи с тегом `products`, сбросьте тег и убедитесь, что обе загрузились заново.

**Ожидаемый результат:** `reloads=2`.

<details><summary>Подсказка</summary>

`GetOrCreateAsync(key, factory, tags: ["products"])` и `RemoveByTagAsync("products")`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 3: group invalidation with tags.
await hc.GetOrCreateAsync("p1", async ct => await Load(), tags: ["products"]);
await hc.GetOrCreateAsync("p2", async ct => await Load(), tags: ["products"]);
await hc.RemoveByTagAsync("products");
```

Проверено: `hybrid RemoveByTag: reloads=2`.

</details>

## 4. 🔴 Общий L2 между двумя экземплярами

**Задача:** создайте два провайдера сервисов с `HybridCache` и общим `IDistributedCache`. Второй должен получить значение первого без загрузки. Затем сбросьте ключ на первом и проверьте, что отдаёт второй.

**Ожидаемый результат:** `instance1=Ann instance2=Ann loads=1`; после `RemoveAsync` на первом второй всё ещё отдаёт `Ann` (из своего L1).

<details><summary>Подсказка</summary>

`AddDistributedMemoryCache()` не годится — `HybridCache` его не использует как L2. Оберните `MemoryDistributedCache` в собственный класс, реализующий `IDistributedCache` (в проде это Redis).

</details>

<details><summary>Решение</summary>

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

Проверено: `instance1=Ann instance2=Ann loads=1`; после `RemoveAsync` на первом экземпляре второй вернул `Ann`, загрузок по-прежнему 1.

</details>
