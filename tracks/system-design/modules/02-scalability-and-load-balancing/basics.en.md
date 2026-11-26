---
title: "Scaling and load balancing: basics"
lang: en
track: system-design
kind: module
doc: basics
slug: 02-scalability-and-load-balancing
part: 1
tags: [system-design, scaling, scale-out, load-balancing, consistent-hashing, yarp]
versions: ".NET 11, C# 15; Yarp.ReverseProxy 2.3.0"
status: draft
updated: 2026-11-26
---

# Scaling and load balancing: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** HTTP, ASP.NET Core at the Minimal API level · **Time:** ~2 h

## Why it matters

One server is a load ceiling and a single point of failure. Several copies solve both problems, if something can distribute requests between them.

## Vertical and horizontal scaling

| | Vertical (scale up) | Horizontal (scale out) |
|---|---|---|
| What we do | more CPU and memory | more instances |
| Code changes | usually none | a stateless service is required |
| Ceiling | the largest machine | coordination and shared resources |
| Machine failure | downtime | the others keep working |

## A stateless service

An instance must not keep anything the next request will need: sessions, uploaded files, counters. That goes into shared storage (a database, a distributed cache, object storage). Then any instance can handle any request.

## L4 and L7 load balancers

| | L4 | L7 |
|---|---|---|
| Sees | IPs and ports, TCP/UDP | HTTP: path, headers, cookies |
| Decision is made | per connection | per request |
| Capabilities | fast and simple | routing, TLS termination, retries, transforms |

With HTTP/2 and gRPC the difference matters: many requests share one connection, and an L4 balancer sends all of them to one instance.

## Round robin

Requests are handed out in a circle. The counter is shared by all threads, so it is incremented atomically.

```csharp
public string Pick()
{
    // The uint cast keeps the index non-negative after the counter overflows.
    uint i = unchecked((uint)Interlocked.Increment(ref _next));
    return nodes[(int)(i % (uint)nodes.Count)];
}
```

File: [Ex01_Balancers.cs](code/Examples/Ex01_Balancers.cs).

## A load balancer with YARP

YARP (Yet Another Reverse Proxy) is a reverse proxy library for ASP.NET Core. Its configuration consists of routes and clusters; a cluster is a set of destination addresses.

```csharp
var cluster = new ClusterConfig
{
    ClusterId = "api",
    LoadBalancingPolicy = LoadBalancingPolicies.RoundRobin,
    Destinations = backendAddresses
        .Index()
        .ToDictionary(d => $"d{d.Index}", d => new DestinationConfig { Address = d.Item }),
};
builder.Services.AddReverseProxy().LoadFromMemory([route], [cluster]);

var app = builder.Build();
app.MapReverseProxy();
```

The example [Ex04_YarpGateway.cs](code/Examples/Ex04_YarpGateway.cs) starts two backends and a gateway on free ports of `127.0.0.1`; the test sends four requests and gets two answers from each. Usually the same configuration lives in `appsettings.json` and is attached with `LoadFromConfig`.

## Beginner mistakes

- ❌ Keeping the session in memory with several instances → ✅ shared storage.
- ❌ One load balancer without a backup → ✅ it has to be redundant too.
- ❌ Scaling the application when the database cannot cope → ✅ find the bottleneck first.
- ❌ Assuming request order is preserved → ✅ do not depend on ordering across instances.

## Related materials

- [Advanced theory](advanced.en.md)
- [Kubernetes: Services](../../../kubernetes/modules/06-services/overview.en.md)

## Sources

- [YARP: load balancing](https://learn.microsoft.com/aspnet/core/fundamentals/servers/yarp/load-balancing)
- [YARP: overview](https://learn.microsoft.com/aspnet/core/fundamentals/servers/yarp/yarp-overview)
