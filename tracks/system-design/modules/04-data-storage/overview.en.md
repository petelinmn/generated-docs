---
title: "Data storage"
lang: en
track: system-design
kind: module
doc: overview
slug: 04-data-storage
part: 1
levels: [1, 2, 3, 4, 5, 6]
tags: [system-design, data-modeling, database-index, b-tree, lsm-tree, query-plan, nosql]
versions: ".NET 11, C# 15; Microsoft.Data.Sqlite 11.0.0"
status: verified
updated: 2026-11-26
---

# Data storage

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

A store is chosen by how the data is accessed: which queries, how many writes and reads, what consistency is needed. This module explains what happens inside (indexes, B-tree, LSM) and why the choice depends on it.

## Key ideas

- Write down the access patterns first, then pick the store.
- An index speeds up reads and slows down writes.
- The column order of a composite index decides which queries it serves.
- A B-tree updates pages in place, an LSM tree appends and merges later.
- NoSQL is several different models, not one.
- Denormalization speeds up reads at the cost of keeping copies in agreement.
- The query plan tells the truth, guesses do not.

## Minimal example

```sql
CREATE INDEX ix_chat ON messages (chat_id, sent_at);
SELECT body FROM messages WHERE chat_id = 1 ORDER BY sent_at DESC LIMIT 20;
```

## When to use and when not

- ✅ Designing a new schema or choosing a database.
- ✅ A query got slow as the data grew.
- ❌ Choosing "NoSQL because it scales" without access patterns.
- ❌ An index on every column "just in case".

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | storage models, access patterns, indexes |
| [2. Advanced](advanced.en.md) | composite and covering indexes, B-tree and LSM, denormalization |
| [3. Expert](expert.en.md) | amplification, reference, plan gotchas, questions |
| [4. Self-check](quiz.en.md) | a quiz and [exercises](exercises.en.md) |
| [5. Runnable examples](code/Examples/) | a toy LSM, a secondary index, SQLite plans |
| [6. Tests](code/Tests/) | `dotnet test` inside `code/` |

## Related materials

- Prerequisites: [design process](../01-design-process/overview.en.md)
- Next: [replication and partitioning](../06-replication-and-partitioning/overview.en.md), [API design](../05-api-design/overview.en.md)
- In other tracks: [data modeling in PostgreSQL](../../../postgresql/modules/03-data-modeling-ddl/overview.en.md), [EF Core](../../../csharp/modules/15-ef-core/overview.en.md), [OpenSearch](../../../opensearch/modules/01-opensearch-basics/overview.en.md)
- [Joins and subqueries in PostgreSQL](../../../postgresql/modules/04-joins-and-subqueries/overview.en.md)
- [Indexes in PostgreSQL](../../../postgresql/modules/05-indexes/overview.en.md)
- [JSON in PostgreSQL](../../../postgresql/modules/08-json-and-operators/overview.en.md)
- [Query plans in PostgreSQL](../../../postgresql/modules/09-performance-explain/overview.en.md)
