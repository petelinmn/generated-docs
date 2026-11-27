---
title: "URL shortener: expert"
lang: en
track: system-design
kind: recipe
doc: expert
slug: url-shortener
part: 2
tags: [url-shortener, caching, redirect, rate-limiting, minimal-api]
versions: ".NET 11, C# 15; Microsoft.Extensions.Caching.Hybrid 11.0.1; Microsoft.Data.Sqlite 11.0.0"
status: draft
updated: 2026-11-27
---

# URL shortener: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

## Several instances

| Part | Behaviour with N instances | What to do |
|---|---|---|
| Database | shared | not SQLite on a network share: a server database |
| `HybridCache` | local copies are separate; add a distributed cache (`IDistributedCache`) as the second level | short `LocalCacheExpiration`, or broadcast invalidation |
| Rate limiter | counters per instance, up to N times the limit | gateway or a [distributed limiter](../distributed-rate-limiter/overview.en.md) |
| Click queue | one per instance | totals are added in the database, so they combine |

The documentation states that `RemoveAsync` removes the entry on the current server and in the distributed cache, while memory caches on other servers are not affected. The test `Cache_AnotherInstanceKeepsItsLocalCopyAfterDisable` shows it:

```csharp
public async Task DisableAsync(string code)
{
    await store.DisableAsync(code);
    await cache.RemoveAsync(Key(code));
```

## Redirect codes

| Code | Permanence | Method may change | Heuristically cacheable |
|---|---|---|---|
| 301 | permanent | yes (POST to GET) | yes |
| 302 | temporary | yes | no |
| 307 | temporary | no | no |
| 308 | permanent | no | yes |

## Growing further

- Partition links by code: every lookup knows the key.
- Choose a store for key-value reads; keep analytics in a separate one.
- Put redirects close to users; creation can stay in one region.
- Send click events to a log and aggregate them in a consumer.
- Delete expired links in batches rather than keeping them forever.

## Gotchas

- **A lookup running during a disable** can put the old value back into the cache after the key was removed; it lives until the entry expires.
- **A request path is user input:** reject strings that cannot be a code before they become cache keys.
- **A failing background flush** stops the host by default; catch and log inside the service.
- **A cached link outlives its expiry.** Check `ExpiresAt` after reading the cache.
- **A cached "not found" blocks a new alias.** Remove the key on creation; with several instances other local copies still answer 404 until they expire.
- **`$` in a regular expression** accepts a trailing newline.
- **Code length versus collisions:** the chance that one new code is taken equals stored ÷ 62^length; with a billion links and 7 characters it is about 0.028%.
- **Codes are case-sensitive** in this design; printed or dictated links may need another alphabet.
- **Reused codes:** do not give a deleted code to a new address, old copies of the link still exist.

## Trade-offs and alternatives

| Decision | Chosen | Alternative | Price of the alternative |
|---|---|---|---|
| Code | random | counter or Snowflake-like id | predictable, but no collisions |
| Redirect | 302 | 301 | fewer requests, blind spots |
| Clicks | in-memory batches | broker | durability for infrastructure |
| Store | relational | key-value | harder ad-hoc queries |
| Cache invalidation | local TTL | pub/sub | more moving parts |

## Interview questions

**1. How do you generate short codes?**

<details><summary>Answer</summary>

Random with a uniqueness check by the database, or a unique id encoded in base62; compare predictability and coordination.

</details>

**2. Why not hash the URL?**

<details><summary>Answer</summary>

A truncated hash collides, and one address can then not have several links with different owners or lifetimes.

</details>

**3. 301 or 302?**

<details><summary>Answer</summary>

301 may be cached heuristically, so clicks and changes can be missed; 302 is not.

</details>

**4. How do you scale reads?**

<details><summary>Answer</summary>

Stateless instances, local and distributed cache, read replicas, partitioning by code.

</details>

**5. What is negative caching for?**

<details><summary>Answer</summary>

So that scanning random codes does not hit the database each time for the same code.

</details>

**6. Does negative caching stop a scan of many different codes?**

<details><summary>Answer</summary>

No: each new code misses. Rate limiting and a filter such as Bloom help.

</details>

**7. A link is disabled. When do all instances stop serving it?**

<details><summary>Answer</summary>

Others may serve their local copy until it expires, here up to one minute; faster needs a broadcast.

</details>

**8. Why is click counting asynchronous?**

<details><summary>Answer</summary>

A redirect must stay fast; the price is possible loss.

</details>

**9. How would you not lose clicks?**

<details><summary>Answer</summary>

A durable log or broker, idempotent consumers.

</details>

**10. Trick question: with three instances and a limit of 30 per minute, what is the real limit?**

<details><summary>Answer</summary>

Up to 90, because counters are in each instance.

</details>

**11. What abuse must a public shortener handle?**

<details><summary>Answer</summary>

Phishing, malware, loops, mass creation, scanning for private links.

</details>

## Related materials

- [Advanced](advanced.en.md)
- [06 · Replication and partitioning](../../modules/06-replication-and-partitioning/overview.en.md), [16 · Multi-region](../../modules/16-multi-region-and-dr/overview.en.md)

## Sources

- [HybridCache](https://learn.microsoft.com/aspnet/core/performance/caching/hybrid)
- [Rate limiting middleware](https://learn.microsoft.com/aspnet/core/performance/rate-limit)
- [RFC 9110, redirection](https://www.rfc-editor.org/rfc/rfc9110#name-redirection-3xx)
- [Minimal APIs](https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis)
- [Channels](https://learn.microsoft.com/dotnet/core/extensions/channels)
- [Microsoft.Data.Sqlite errors](https://learn.microsoft.com/dotnet/standard/data/sqlite/database-errors)
