---
title: "Consensus and coordination: basics"
lang: en
track: system-design
kind: module
doc: basics
slug: 12-consensus-and-coordination
part: 3
tags: [system-design, consensus, raft, leader-election, distributed-lock, fencing-token, logical-clocks]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Consensus and coordination: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** replication, threads · **Time:** ~2 h

## Why it matters

If two nodes think they are the leader, the data diverges. If every instance runs a scheduled job, the customer gets three emails.

## Majority

A decision is made when a majority of the whole cluster supports it. Any two majorities intersect, so two leaders in one term are impossible.

| Nodes | Majority | Failures tolerated |
|---|---|---|
| 3 | 2 | 1 |
| 4 | 3 | 1 |
| 5 | 3 | 2 |

## Leader election in Raft

A node is a follower, a candidate or a leader. If a follower receives no heartbeat for long enough, it increments the term, votes for itself and asks for votes.

```csharp
public bool StartElection(IEnumerable<Node> reachable, int clusterSize)
{
    (Term, _votedFor) = (Term + 1, Id);
    int votes = 1 + reachable.Count(n => n.RequestVote(Term, Id, LastLogTerm, LastLogIndex));
    return votes > clusterSize / 2;
}
```

A candidate with two votes out of five loses, with three it wins. File: [Ex01_Election.cs](code/Examples/Ex01_Election.cs). This is only the voting rule, not a full Raft.

## Lease and fencing token

A lock with a limited lifetime (a lease) goes to one client. But the client may "fall asleep" (a GC pause, the network), the lease expires, another gets it, and the first wakes up and writes. Checking the expiry before writing does not help: the pause may come after the check.

```csharp
public bool TryWrite(long token, string value)
{
    if (token < _highestToken) return false; // a holder from the past

    _highestToken = token;
    Value = value;
    return true;
}
```

Each grant of the lease gets a larger number, and the storage rejects a smaller one. File: [Ex02_Fencing.cs](code/Examples/Ex02_Fencing.cs).

## Beginner mistakes

- ❌ `lock` for protection across servers → ✅ it works within one process.
- ❌ An even number of nodes for reliability → ✅ it adds no resilience.
- ❌ A lease without fencing → ✅ a check on the resource side.
- ❌ Your own leader election protocol → ✅ a ready system.

## Related materials

- [Advanced theory](advanced.en.md)
- [Consistency and CAP](../07-consistency-and-cap/overview.en.md)

## Sources

- [Raft: In Search of an Understandable Consensus Algorithm](https://raft.github.io/raft.pdf)
- [Kleppmann: How to do distributed locking](https://martin.kleppmann.com/2016/02/08/how-to-do-distributed-locking.html)
