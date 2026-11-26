---
title: "Multi-region and recovery: advanced"
lang: en
track: system-design
kind: module
doc: advanced
slug: 16-multi-region-and-dr
part: 3
tags: [system-design, disaster-recovery, multi-region, failover, rpo-rto, backups, crdt]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Multi-region and recovery: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## Failover and failback

```csharp
if (Active == _primary && _failures >= failAfter) Active = secondary;
// Returning needs a longer streak: flapping is worse than staying on the secondary.
else if (Active == secondary && _successes >= recoverAfter) Active = _primary;
```

You switch after a streak of failures and return after a longer streak of successes. File: [Ex02_Failover.cs](code/Examples/Ex02_Failover.cs). The example controls only routing: before a real database goes back, the data written on the secondary also has to be moved.

## Who decides

Automation is faster but may switch because of a broken link and end up with two leaders. You need a quorum with a third observation point and [fencing](../12-consensus-and-coordination/basics.en.md) of the old leader.

## Active-active

| Approach | Idea |
|---|---|
| A user's home region | all of a user's writes go to one region |
| A global write leader | reads are local, writes go to one region |
| Write anywhere | merge rules are needed |

Last write wins loses changes: a basket `book, pen` and a later `book, mug` give a result without `pen`. An observed-remove set removes only the additions it has seen:

```csharp
public void Remove(string item)
{
    foreach (var (_, tag) in _added.Where(a => a.Item == item)) _removed.Add(tag);
}
```

File: [Ex03_ActiveActive.cs](code/Examples/Ex03_ActiveActive.cs).

## Drills

Run recovery on a schedule, measure the real RPO and RTO, write down what went wrong.

## Best practices

- Describe infrastructure as code so a region can be rebuilt.
- Check quotas and capacity in the recovery region in advance.
- Watch replication lag and alert at the RPO threshold.
- Keep the failover runbook available without the primary region.

## Common mistakes

- ❌ The standby depends on a service in the primary region → ✅ check all dependencies.
- ❌ DNS with a long TTL → ✅ account for caching in the RTO.
- ❌ Failback without a plan → ✅ it is a separate process of its own.
- ❌ Forgotten secrets and configuration in the standby → ✅ replicate them too.

## Comparison with other tracks

- Linking brokers across sites — [RabbitMQ](../../../rabbitmq/modules/12-federation-and-shovel/overview.en.md).
- Deploying from a repository — [GitOps](../../../kubernetes/modules/20-gitops/overview.en.md).

## Related materials

- [Basics](basics.en.md)
- [Expert theory](expert.en.md)

## Sources

- [AWS: disaster recovery options](https://docs.aws.amazon.com/whitepapers/latest/disaster-recovery-workloads-on-aws/disaster-recovery-options-in-the-cloud.html)
