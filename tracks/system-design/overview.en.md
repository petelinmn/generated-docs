---
title: "Architecture and system design"
lang: en
track: system-design
kind: track
doc: overview
slug: system-design
tags: [system-design, architecture, dotnet]
versions: ".NET 11, C# 15"
default_level: 6
status: draft
updated: 2026-11-26
---

# Architecture and system design

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

System design is the skill of turning requirements ("a messenger for 10 million users") into the structure of a system: which components are needed, how they talk, where data lives, what happens on failure and what it costs. There is no single right answer: there are trade-offs, and you need to name them and back them with numbers. Every idea in this track is shown in C#/.NET code that you can run and check with tests. The structure of code *inside* one application is covered separately: [C#: application architecture](../csharp/modules/21-architecture/overview.en.md).

## Environment

| What | Tool |
|---|---|
| SDK | .NET 11 SDK or newer — `dotnet --version`; modules target `net11.0` |
| Editor | VS Code + C# Dev Kit, Rider or Visual Studio |
| Module code | `code/` — a `Code.slnx` solution: `Examples/`, `Solutions/`, `Tests/` (xUnit v3) and `global.json` with the Microsoft.Testing.Platform mode |
| Run all module tests | `dotnet test` inside `code/` |
| Run one example | `dotnet test --filter "FullyQualifiedName~<Name>"` |

## Roadmap

Learning progress is tracked in [PROGRESS.md](../../PROGRESS.md). Modules are generated at detail level **6** (all levels) by default, see [detail levels](../../README.en.md#detail-levels).

### Part 1. Fundamentals

- [01 · Design process and estimation](modules/01-design-process/overview.en.md) — requirements, back-of-the-envelope math, SLI/SLO, availability, percentiles
- [02 · Scaling and load balancing](modules/02-scalability-and-load-balancing/overview.en.md) — vertical and horizontal, stateless, balancing algorithms, consistent hashing, YARP
- [03 · Caching](modules/03-caching/overview.en.md) — cache levels, cache-aside, TTL and invalidation, eviction, cache stampede, HybridCache
- [04 · Data storage](modules/04-data-storage/overview.en.md) — relational and NoSQL stores, indexes, B-tree and LSM, choosing by access patterns
- [05 · API design](modules/05-api-design/overview.en.md) — REST, gRPC, versioning, idempotency, pagination

### Part 2. Advanced topics

- [06 · Replication and partitioning](modules/06-replication-and-partitioning/overview.en.md) — leader/follower, quorums, choosing a shard key, hot keys
- [07 · Consistency and CAP](modules/07-consistency-and-cap/overview.en.md) — consistency models, CAP and PACELC, transaction isolation
- [08 · Messaging and events](modules/08-messaging-and-events/overview.en.md) — queues and logs, delivery semantics, idempotent consumers, backpressure
- [09 · Reliability patterns](modules/09-reliability-patterns/overview.en.md) — timeouts, retry, circuit breaker, bulkhead, load shedding, rate limiting
- [10 · Distributed transactions](modules/10-distributed-transactions/overview.en.md) — 2PC, saga, outbox and inbox
- [11 · Architecture styles](modules/11-architecture-styles/overview.en.md) — monolith, modular monolith, microservices, serverless, ADR

### Part 3. Expert topics

- [12 · Consensus and coordination](modules/12-consensus-and-coordination/overview.en.md) — Raft, leader election, distributed locks, fencing tokens, logical clocks
- [13 · Event sourcing and CQRS](modules/13-event-sourcing-and-cqrs/overview.en.md) — event streams, projections, event versioning
- [14 · Observability and SLOs](modules/14-observability-and-slo/overview.en.md) — metrics, tracing, error budgets, alerts
- [15 · Security architecture](modules/15-security-architecture/overview.en.md) — threat modeling, service-to-service authentication and authorization, secrets, zero trust
- [16 · Multi-region and recovery](modules/16-multi-region-and-dr/overview.en.md) — RPO and RTO, active-active, conflicts, drills
- [17 · Case studies](modules/17-design-case-studies/overview.en.md) — complete interview-style walkthroughs

## Libraries

- [YARP](libraries/yarp/overview.en.md) — a reverse proxy and API gateway on .NET
- [Orleans](libraries/orleans/overview.en.md) — virtual actors for distributed applications

## Recipes

- [URL shortener](recipes/url-shortener/overview.en.md) — short code generation, cache, analytics
- [Distributed rate limiter](recipes/distributed-rate-limiter/overview.en.md) — one limit shared by several instances
- [News feed](recipes/news-feed/overview.en.md) — fan-out on write and on read
- [Notification service](recipes/notification-service/overview.en.md) — queues, retries, deduplication

## Projects

- [URL shortener under load](projects/url-shortener-service/overview.en.md) — API, cache, several instances, a load test

## Level 3 expansion plan

All modules exist at level 6, but the expert files are still shorter than a reference should be. Modules 01 and 02 are already expanded. Below is what remains to be written; each item is closed with `/learn system-design <module> level 3`, after which the module is checked with `/verify`.

| Module | What to add to the expert theory |
|---|---|
| 03 · Caching | L1 invalidation through pub/sub, cache warming, CDNs and purge by key, Redis Cluster and hot keys, serialization cost |
| 04 · Data storage | columnar and time-series stores, MVCC and VACUUM, the write-ahead log, compaction strategies, secondary indexes in distributed databases |
| 05 · API design | GraphQL, webhooks (signatures, retries), API gateways, rate limit headers, OpenAPI contract testing |
| 06 · Replication and partitioning | chain replication, sloppy quorum and hinted handoff, rebalancing logical partitions, cross-shard queries and global indexes |
| 07 · Consistency and CAP | the hierarchy of consistency models, isolation in SQL Server and MySQL, how SSI works, a CRDT catalog, classifying databases by PACELC |
| 08 · Messaging and events | Kafka transactions and exactly-once, RabbitMQ quorum queues and streams, a schema registry, retry topics, consumer rebalancing |
| 09 · Reliability patterns | adaptive concurrency limits, hedging, deadlines in gRPC, chaos engineering, degradation and feature flags |
| 10 · Distributed transactions | workflow engines, CDC for the outbox, distributed transactions in .NET, the TCC pattern, saga timeouts and versions |
| 11 · Architecture styles | strategic DDD and context maps, service mesh, BFF and API gateway, micro-frontends, branch by abstraction |
| 12 · Consensus and coordination | Paxos and Raft, cluster membership changes, Raft log snapshots, ZooKeeper and etcd primitives, hybrid logical clocks |
| 13 · Event sourcing and CQRS | ready-made event stores, rebuilding projections without downtime, snapshot strategies, implementing crypto-shredding |
| 14 · Observability and SLOs | the OpenTelemetry Collector, exemplars, continuous profiling, log sampling, synthetic monitoring |
| 15 · Security | OAuth 2.0 and OIDC flows, mTLS and certificate rotation, secret stores, the supply chain (SBOM, signing), tenant isolation, an encryption key hierarchy |
| 16 · Multi-region | global databases and their consistency levels, DNS and anycast failover time, game days, immutable backups |
| 17 · Case studies | chat, a distributed rate limiter, search autocomplete, file storage, a payment system, geo search |

## Related tracks

- [C# / .NET](../csharp/overview.en.md) — the language and platform of the examples
- [Kubernetes](../kubernetes/overview.en.md) — where services run and scale
- [PostgreSQL](../postgresql/overview.en.md) — relational storage
- [Kafka](../kafka/overview.en.md) and [RabbitMQ](../rabbitmq/overview.en.md) — message brokers
- [OpenSearch](../opensearch/overview.en.md) — search

## Sources

- [Azure Architecture Center: architecture styles](https://learn.microsoft.com/azure/architecture/guide/architecture-styles/)
- [Azure Architecture Center: cloud design patterns](https://learn.microsoft.com/azure/architecture/patterns/)
- [Google SRE Book: Service Level Objectives](https://sre.google/sre-book/service-level-objectives/)
