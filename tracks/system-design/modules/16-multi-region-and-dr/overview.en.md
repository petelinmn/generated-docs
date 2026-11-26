---
title: "Multi-region and recovery"
lang: en
track: system-design
kind: module
doc: overview
slug: 16-multi-region-and-dr
part: 3
levels: [1, 2, 3, 4, 5, 6]
tags: [system-design, disaster-recovery, multi-region, failover, rpo-rto, backups, crdt]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Multi-region and recovery

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

A whole region may become unavailable, and data may turn out to be corrupted. Disaster recovery (DR) answers how much data and time you are prepared to lose and how much you pay for that.

## Key ideas

- RPO is acceptable data loss, RTO is acceptable recovery time.
- A replica repeats mistakes too: a backup saves you from corrupted data.
- Strategies go from cheap and slow to expensive and fast.
- Failover is a process of several steps, each taking time.
- Automatic failover must not jump on a single error.
- Active-active requires conflict resolution.
- A recovery that was never tested does not count as existing.

## Minimal example

```csharp
public static TimeSpan Downtime(TimeSpan detect, TimeSpan decide, TimeSpan promote, TimeSpan redirect) =>
    detect + decide + promote + redirect;
```

## When to use and when not

- ✅ Downtime or data loss is expensive for the business.
- ✅ There are regulatory requirements on recovery or data location.
- ❌ Several regions before the service is reliable in one.
- ❌ Active-active without a conflict resolution plan.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | RPO and RTO, strategies, replicas and backups |
| [2. Advanced](advanced.en.md) | failover, active-active, conflicts, drills |
| [3. Expert](expert.en.md) | reference, gotchas, questions |
| [4. Self-check](quiz.en.md) | a quiz and [exercises](exercises.en.md) |
| [5. Runnable examples](code/Examples/) | failover losses, a failover controller, an OR-Set |
| [6. Tests](code/Tests/) | `dotnet test` inside `code/` |

## Related materials

- Prerequisites: [design process](../01-design-process/overview.en.md), [replication](../06-replication-and-partitioning/overview.en.md), [consistency](../07-consistency-and-cap/overview.en.md)
- Next: [case studies](../17-design-case-studies/overview.en.md)
- In other tracks: [federation and shovel in RabbitMQ](../../../rabbitmq/modules/12-federation-and-shovel/overview.en.md), [GitOps](../../../kubernetes/modules/20-gitops/overview.en.md)
