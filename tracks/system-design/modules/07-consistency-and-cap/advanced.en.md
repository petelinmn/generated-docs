---
title: "Consistency and CAP: advanced"
lang: en
track: system-design
kind: module
doc: advanced
slug: 07-consistency-and-cap
part: 2
tags: [system-design, consistency, cap-theorem, isolation-levels, mvcc, write-skew, crdt]
versions: ".NET 11, C# 15"
status: verified
updated: 2026-11-27
---

# Consistency and CAP: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## Isolation levels in PostgreSQL

| Level | Dirty read | Nonrepeatable | Phantom | Serialization anomaly |
|---|---|---|---|---|
| Read uncommitted | not in PostgreSQL | possible | possible | possible |
| Read committed (default) | no | possible | possible | possible |
| Repeatable read | no | no | not in PostgreSQL | possible |
| Serializable | no | no | no | no |

Repeatable read in PostgreSQL is implemented as snapshot isolation. The application must retry transactions on serialization failures; for Serializable that is SQLSTATE `40001`. In other database systems the same names may mean something else.

## Snapshot isolation and write skew

A transaction reads a snapshot taken at its start; when the same row is written, the first committer wins.

```csharp
IEnumerable<string> mustBeUnchanged = isolation == Isolation.Serializable
    ? _writes.Keys.Union(_reads)
    : _writes.Keys;
if (_writes.Count > 0 && mustBeUnchanged.Any(k => store.ChangedSince(k, snapshot))) return false;
```

The rule: at least one doctor is on call. Two transactions see two, each removes *its own* — the rows differ, there is no write conflict, and nobody is left on call. Validating what was read rejects the second one. File: [Ex01_Isolation.cs](code/Examples/Ex01_Isolation.cs). This models the idea; real database systems track dependencies differently.

## PACELC

On a partition (P) you choose between availability (A) and consistency (C), else (E) between latency (L) and consistency (C). Synchronous replication pays in latency every day, not only during failures.

## Session guarantees

```csharp
public string? Read(Replica replica)
{
    if (replica.Version < _seen) return null;
    _seen = replica.Version;
    return replica.Value;
}
```

This is monotonic reads: the client does not accept an answer older than what it has already seen. File: [Ex03_Sessions.cs](code/Examples/Ex03_Sessions.cs).

## Best practices

- State your invariants and check what protects each one.
- Protect uniqueness and stock levels with database constraints.
- Retry the whole transaction, including the reads.
- Set the acceptable staleness per operation.

## Common mistakes

- ❌ Retrying only the `COMMIT` → ✅ the whole transaction.
- ❌ A side effect (an email) inside a retried transaction → ✅ after the commit, through an outbox.
- ❌ "Check then insert" without a unique index → ✅ a constraint in the database.
- ❌ One consistency level for the whole system → ✅ per operation.

## Comparison with other tracks

- Concurrency tokens — [EF Core](../../../csharp/modules/15-ef-core/overview.en.md).
- The same class of races inside a process — [concurrency](../../../csharp/modules/18-concurrency/overview.en.md).

## Related materials

- [Basics](basics.en.md)
- [Expert theory](expert.en.md)

## Sources

- [PostgreSQL: transaction isolation](https://www.postgresql.org/docs/current/transaction-iso.html)
