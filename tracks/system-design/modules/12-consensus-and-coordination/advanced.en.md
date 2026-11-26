---
title: "Consensus and coordination: advanced"
lang: en
track: system-design
kind: module
doc: advanced
slug: 12-consensus-and-coordination
part: 3
tags: [system-design, consensus, raft, leader-election, distributed-lock, fencing-token, logical-clocks]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Consensus and coordination: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## Voting rules

A node gives at most one vote per term and does not vote for a candidate whose log is less up to date. Of two logs the more up to date is the one with the larger term of the last entry; with equal terms, the longer one.

```csharp
bool upToDate = candidateLogTerm > LastLogTerm
    || (candidateLogTerm == LastLogTerm && candidateLogIndex >= LastLogIndex);
if (!upToDate || (_votedFor is not null && _votedFor != candidate)) return false;
```

This way a node lacking committed entries cannot become the leader.

## Randomized timeout

So that votes are not split forever, the election timeout is picked at random from an interval; the paper gives 150–300 ms as an example.

## Committing entries

An entry is committed when the leader that created it has placed it on a majority. Entries of earlier terms are not considered committed by merely counting copies: they are committed together with an entry of the current term.

## Terms

Each node stores the current term. On seeing a larger one, a leader or candidate becomes a follower. A request with a stale term is rejected.

## Best practices

- An odd number of nodes, usually 3 or 5.
- Spread nodes across failure zones.
- Measure durations with a monotonic clock.
- Make the job under the lock idempotent.

## Common mistakes

- ❌ Splitting a cluster equally across two data centers → ✅ losing either may remove the majority; a third zone is needed.
- ❌ Many nodes "for speed" → ✅ each write waits for a majority.
- ❌ A short lease without renewal → ✅ renew early and still use fencing.
- ❌ Assuming a lock release always runs → ✅ an expiry is mandatory.

## Comparison with other tracks

- Controller and replicas — [Kafka](../../../kafka/modules/07-replication-and-availability/overview.en.md).
- etcd in the control plane — [Kubernetes](../../../kubernetes/modules/02-architecture/overview.en.md).

## Related materials

- [Basics](basics.en.md)
- [Expert theory](expert.en.md)

## Sources

- [Raft](https://raft.github.io/raft.pdf)
