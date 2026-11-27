---
title: "Distributed rate limiter: expert"
lang: en
track: system-design
kind: recipe
doc: expert
slug: distributed-rate-limiter
part: 2
tags: [rate-limiting, redis, token-bucket, lua, distributed-systems]
versions: ".NET 11, C# 15; StackExchange.Redis 3.4.35; Redis 8"
status: draft
updated: 2026-11-27
---

# Distributed rate limiter: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

## Fewer round trips: leases

```csharp
int held = _leases.GetValueOrDefault(key);
if (held < cost)
{
    SharedCalls++;
    var lease = await shared.AcquireAsync(key, Math.Max(batch, cost), cancellationToken);
    if (!lease.Allowed)
    {
        // Less than a batch may remain: ask for exactly what is needed.
        SharedCalls++;
        var exact = await shared.AcquireAsync(key, cost, cancellationToken);
        return exact;
    }

    held += Math.Max(batch, cost);
}

// Tokens held here are invisible to other instances until used.
_leases[key] = held - cost;
return new Decision(true, held - cost, TimeSpan.Zero);
```

Each instance takes a batch and spends it locally. In the test with a capacity of 20 and batches of 10, the first instance uses its ten, the second takes the other ten and uses one; the first is then refused although nine tokens are unused. The smaller the batch relative to the limit, the smaller the error. This example neither expires leases nor returns them. File: [Ex04_Leased.cs](code/Examples/Ex04_Leased.cs).

## Hot keys and sharding

All requests for one key go to one Redis node. A very hot limit can be split into N keys with a part each; a request picks a shard. The total is preserved, but a shard can run out while others still have tokens.

## Cluster and persistence

- The script touches one key, so it works with hash slots. Several limits in one script need the same hash tag, for example `{tenant1}:minute` and `{tenant1}:day`.
- Redis caches scripts by their SHA1; the cache is volatile, and `EVALSHA` can answer `NOSCRIPT`.
- After a restart or a failover bucket state may be lost: clients get a full bucket again.

## Reference

| Question | Choice in this recipe |
|---|---|
| Algorithm | token bucket |
| State | a hash with `tokens` and `at` |
| Atomicity | Lua script |
| Clock | Redis `TIME` |
| Expiry | refill time plus a second |
| Refusal | 429, `Retry-After` in whole seconds |
| Failure | configurable policy and timeout |

`X-RateLimit-Remaining` used here is a convention, not a standard header.

## Gotchas

- **Fractions vanish** in Lua replies.
- **A long script blocks the whole server.**
- **A timeout does not cancel** the command.
- **A negative cost adds tokens**: validate it.
- **A key taken from a header** lets a client invent a new key for every request and never be limited.
- **The in-process dictionary never evicts** keys.
- **Anonymous clients share one key** in this example: one of them can use up the limit of all.
- **Keys from user input** can be created without bound; expiry limits the damage.
- **A cost above the capacity** can never succeed, although `Retry-After` suggests waiting.

## Trade-offs and alternatives

| Approach | Accuracy | Latency | Dependency |
|---|---|---|---|
| Local only | per instance | none | none |
| Shared on every request | high | a round trip | store |
| Leases | approximate | low on average | store |
| Gateway limiter | depends on the gateway | at the edge | gateway |
| Sticky routing by client | exact while routing holds | none | balancer |

## Interview questions

**1. Why is a per-instance limit not enough?**

<details><summary>Answer</summary>

The effective limit grows with the number of instances and depends on balancing.

</details>

**2. Why a script and not `INCR`?**

<details><summary>Answer</summary>

`INCR` is enough for a fixed window; a token bucket reads, computes and writes, which must be atomic.

</details>

**3. Whose clock?**

<details><summary>Answer</summary>

One clock, here Redis `TIME`.

</details>

**4. What do you do when Redis is down?**

<details><summary>Answer</summary>

Choose per operation: open, closed or a local fallback, with a short timeout and an alert.

</details>

**5. How to handle a hot key?**

<details><summary>Answer</summary>

Shard the limit, lease batches, or reject locally what is certainly over.

</details>

**6. What does batching cost?**

<details><summary>Answer</summary>

Tokens stranded in an instance and refusals while tokens exist elsewhere.

</details>

**7. What about Redis Cluster?**

<details><summary>Answer</summary>

Keys of one script must be passed explicitly and normally belong to one hash slot.

</details>

**8. Is the limit exact after a failover?**

<details><summary>Answer</summary>

Not necessarily: replication is asynchronous and recent writes can be lost.

</details>

**9. Trick question: the check timed out. Was the token taken?**

<details><summary>Answer</summary>

Unknown: the command may still run.

</details>

**10. Where to put the limiter?**

<details><summary>Answer</summary>

At the gateway for coarse limits, in the service for business quotas.

</details>

## Related materials

- [Advanced](advanced.en.md)
- [12 · Consensus and coordination](../../modules/12-consensus-and-coordination/overview.en.md)

## Sources

- [Redis: scripting with Lua](https://redis.io/docs/latest/develop/programmability/eval-intro/)
- [Redis: Lua API](https://redis.io/docs/latest/develop/programmability/lua-api/)
- [Redis: TIME](https://redis.io/docs/latest/commands/time/)
- [RFC 6585, status 429](https://www.rfc-editor.org/rfc/rfc6585#section-4)
- [ASP.NET Core rate limiting](https://learn.microsoft.com/aspnet/core/performance/rate-limit)
- [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis)
