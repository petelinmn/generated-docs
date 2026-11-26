---
title: "Scaling and load balancing"
lang: en
track: system-design
kind: module
doc: overview
slug: 02-scalability-and-load-balancing
part: 1
levels: [1, 2, 3, 4, 5, 6]
tags: [system-design, scaling, scale-out, load-balancing, consistent-hashing, yarp]
versions: ".NET 11, C# 15; Yarp.ReverseProxy 2.3.0"
status: verified
updated: 2026-11-26
---

# Scaling and load balancing

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

When one server is not enough, you either give the load to a bigger machine or split it between several. In the second case someone has to choose where to send each request, and the service must not care.

## Key ideas

- Vertical scaling is simpler but has a ceiling; horizontal scaling requires a stateless service.
- A load balancer works either at the connection level (L4) or at the HTTP level (L7).
- Round robin is good for similar requests; with varying durations, load-aware algorithms help.
- Power of two choices gives an almost even distribution in O(1).
- Consistent hashing moves only part of the keys when a node is added.
- Health checks take broken nodes out.
- Adding nodes does not give linear growth: queues and coordination get in the way.

## Minimal example

```csharp
uint i = unchecked((uint)Interlocked.Increment(ref _next));
return nodes[(int)(i % (uint)nodes.Count)];
```

## When to use and when not

- ✅ You need fault tolerance or growth beyond one machine.
- ✅ Zero-downtime rollout.
- ✅ A single entry point for several services.
- ❌ The bottleneck is one database: more application copies only add pressure.
- ❌ State in process memory that cannot be moved out.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | scale up and scale out, stateless, L4 and L7, round robin, YARP |
| [2. Advanced](advanced.en.md) | weighted and load-aware algorithms, consistent hashing, health checks, affinity |
| [3. Expert](expert.en.md) | Amdahl's law and USL, YARP reference, gotchas, questions |
| [4. Self-check](quiz.en.md) | a quiz and [exercises](exercises.en.md) |
| [5. Runnable examples](code/Examples/) | balancers, a hash ring, a YARP gateway |
| [6. Tests](code/Tests/) | `dotnet test` inside `code/` |

## Related materials

- Prerequisites: [design process](../01-design-process/overview.en.md)
- Next: [caching](../03-caching/overview.en.md)
- The same idea in other tracks: [Kubernetes: Services](../../../kubernetes/modules/06-services/overview.en.md), [Kubernetes: autoscaling](../../../kubernetes/modules/15-autoscaling/overview.en.md)
- [Replication and partitioning](../06-replication-and-partitioning/overview.en.md)
