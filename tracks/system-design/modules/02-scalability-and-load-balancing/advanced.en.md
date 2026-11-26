---
title: "Scaling and load balancing: advanced"
lang: en
track: system-design
kind: module
doc: advanced
slug: 02-scalability-and-load-balancing
part: 1
tags: [system-design, scaling, scale-out, load-balancing, consistent-hashing, yarp]
versions: ".NET 11, C# 15; Yarp.ReverseProxy 2.3.0"
status: draft
updated: 2026-11-26
---

# Scaling and load balancing: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## Weighted round robin

If nodes differ in power, they get weights. The naive version gives the heavy node a burst of requests in a row; the "smooth" one spreads them more evenly.

```csharp
int best = 0;
for (int i = 0; i < nodes.Length; i++)
{
    _current[i] += nodes[i].Weight;
    if (_current[i] > _current[best]) best = i;
}
_current[best] -= _total;
return nodes[best].Node;
```

With weights a=5, b=1, c=1 the first seven picks are `aabacaa`.

## Load-aware algorithms

Least connections picks the node with the fewest active requests but scans all nodes. Power of two choices takes two random nodes and picks the less loaded one.

```csharp
int a = random.Next(nodes.Count);
int b = random.Next(nodes.Count - 1);
if (b >= a) b++;
return nodes[a].Active <= nodes[b].Active ? nodes[a] : nodes[b];
```

An experiment: 100,000 never-finishing requests over 100 nodes with `Random(42)`. A random pick gave from 923 to 1074 requests per node, two choices gave from 997 to 1002. In YARP `PowerOfTwoChoices` is the default policy.

## Consistent hashing

When requests with the same key must reach the same node (a cache, a shard), `hash % N` is bad: when `N` changes almost everything moves. Consistent hashing places nodes and keys on a ring; a key belongs to the first node clockwise.

```csharp
int i = Array.BinarySearch(_points, StableHash.Of(key));
if (i < 0) i = ~i;                         // first point clockwise from the key
return _ring[_points[i % _points.Length]]; // wrap around the end of the ring
```

Going from 4 to 5 nodes with 100,000 keys: `% N` moves 80.1% of keys, the ring moves 15.8%. Each node is represented by many virtual points, otherwise the shares are uneven:

| Virtual points per node | Shares of four nodes |
|---|---|
| 1 | 7.2% · 23.6% · 32.4% · 36.8% |
| 10 | 16.1% · 21.0% · 25.6% · 37.3% |
| 100 | 23.9% · 24.4% · 25.5% · 26.1% |
| 1000 | 24.2% · 25.0% · 25.3% · 25.4% |

File: [Ex02_ConsistentHashing.cs](code/Examples/Ex02_ConsistentHashing.cs). The hash must be stable across processes: `string.GetHashCode()` is not suitable.

## Health checks

- **Active:** the balancer polls an endpoint itself.
- **Passive:** it watches the failures of real requests.

In YARP both kinds are disabled by default. The built-in active policy is `ConsecutiveFailures` (default interval 15 s, timeout 10 s, threshold 2), the passive one is `TransportFailureRate`.

## Session affinity

Pinning a client to a node (sticky sessions) lets you live with in-memory state for a while, but it hurts evenness, and the state is lost when the node fails. In YARP it is disabled by default.

## Best practices

- A readiness check answers "I can take requests", not "all dependencies are perfect".
- On shutdown, first stop receiving new requests, then wait for the active ones (connection draining).
- Keep spare capacity for a node failure: N+1 or N+2.
- Do not use affinity instead of moving state out.

## Common mistakes

- ❌ A health check that fails because of a shared database → all nodes drop out at once. ✅ Separate the checks.
- ❌ Retries at every layer → an avalanche. ✅ Retries in one place, with a limit.
- ❌ Autoscaling by CPU only for an I/O-bound service → ✅ a metric that reflects the work queue.
- ❌ Long-lived connections with L4 → skew after adding nodes. ✅ L7 or a limit on connection lifetime.

## Comparison with other tracks

- Balancing inside a cluster — [Kubernetes: Services](../../../kubernetes/modules/06-services/overview.en.md).
- Partitions as another way to split load — [Kafka: basics](../../../kafka/modules/01-kafka-basics/overview.en.md).
- Resilience of outgoing calls — [resilient HttpClient](../../../csharp/recipes/http-client-resilience/overview.en.md).

## Related materials

- [Basics](basics.en.md)
- [Expert theory](expert.en.md)

## Sources

- [YARP: load balancing](https://learn.microsoft.com/aspnet/core/fundamentals/servers/yarp/load-balancing)
- [YARP: health checks](https://learn.microsoft.com/aspnet/core/fundamentals/servers/yarp/dests-health-checks)
- [YARP: session affinity](https://learn.microsoft.com/aspnet/core/fundamentals/servers/yarp/session-affinity)
