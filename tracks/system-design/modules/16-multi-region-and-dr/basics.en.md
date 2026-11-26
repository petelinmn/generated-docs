---
title: "Multi-region and recovery: basics"
lang: en
track: system-design
kind: module
doc: basics
slug: 16-multi-region-and-dr
part: 3
tags: [system-design, disaster-recovery, multi-region, failover, rpo-rto, backups, crdt]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Multi-region and recovery: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** replication, availability · **Time:** ~2 h

## Why it matters

Almost everyone has backups, not everyone has a tested recovery plan. During an outage it turns out the backup is incomplete and recovery takes a day.

## RPO and RTO

- **RPO** (recovery point objective) — the maximum acceptable data loss measured in time.
- **RTO** (recovery time objective) — the maximum acceptable time to restore operations.

With asynchronous replication you lose what had not arrived yet:

```csharp
public static IReadOnlyList<Write> LostOnFailover(IEnumerable<Write> writes, TimeSpan failureAt, TimeSpan lag) =>
    [.. writes.Where(w => w.At <= failureAt && w.At > failureAt - lag)];
```

Downtime adds up from detection, the decision, promoting the replica and redirecting clients. In the test 1 + 10 + 3 + 1 minutes make 15 minutes, and the human decision takes the most. File: [Ex01_Recovery.cs](code/Examples/Ex01_Recovery.cs).

## Strategies

| Strategy | What runs in the recovery region | Price and speed |
|---|---|---|
| Backup and restore | only data backups | cheapest, longest to recover |
| Pilot light | data is replicated, applications are off | must be switched on and scaled |
| Warm standby | a scaled down but working copy | only needs scaling |
| Multi-site active/active | all regions serve traffic | the most expensive and complex |

Actual RPO and RTO depend on the system: measure them in drills.

## A replica is not a backup

An accidental delete is replicated as faithfully as everything else. Point-in-time recovery stops before the mistake:

```csharp
foreach (Write w in log.Where(w => w.At <= moment).OrderBy(w => w.At))
    if (w.Value is null) state.Remove(w.Key);
    else state[w.Key] = w.Value;
```

## Beginner mistakes

- ❌ "We have a replica, no backups needed" → ✅ you need both.
- ❌ Backups in the same region and account → ✅ separate and protected from deletion.
- ❌ Never testing recovery → ✅ restore from a backup regularly.
- ❌ One RTO for everything → ✅ by how critical a function is.

## Related materials

- [Advanced theory](advanced.en.md)
- [Replication and partitioning](../06-replication-and-partitioning/overview.en.md)

## Sources

- [AWS: disaster recovery options](https://docs.aws.amazon.com/whitepapers/latest/disaster-recovery-workloads-on-aws/disaster-recovery-options-in-the-cloud.html)
- [Azure Well-Architected: disaster recovery](https://learn.microsoft.com/azure/well-architected/reliability/disaster-recovery)
