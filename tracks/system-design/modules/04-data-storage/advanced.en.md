---
title: "Data storage: advanced"
lang: en
track: system-design
kind: module
doc: advanced
slug: 04-data-storage
part: 1
tags: [system-design, data-modeling, database-index, b-tree, lsm-tree, query-plan, nosql]
versions: ".NET 11, C# 15; Microsoft.Data.Sqlite 11.0.0"
status: verified
updated: 2026-11-26
---

# Data storage: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## Composite and covering indexes

The index `(chat_id, sent_at)` is ordered by chat first, then by time within a chat. So it serves a filter by chat together with sorting by time. The index `(sent_at, chat_id)` does not give a `SEARCH` for the same query.

A covering index contains all columns of the query, and the table is not read at all:

```sql
CREATE INDEX ix_cover ON messages (chat_id, sent_at, body);
```

The plan contains `COVERING INDEX ix_cover`. The price is a copy of `body` in the index.

## B-tree and LSM

| | B-tree | LSM tree |
|---|---|---|
| Write | modify a page in place | to memory, then immutable files |
| Read by key | one path down the tree | possibly several structures |
| Background work | page maintenance | compaction |
| Fits well | mixed load, ranges | write-heavy load |

In an LSM tree a delete is also a write: a tombstone that hides older values.

```csharp
// A delete is a write of a tombstone: older segments are immutable.
public void Delete(string key) => Write(key, null);
```

Reads go from new structures to old ones, and a missing key has to be looked for everywhere:

```csharp
for (int i = _segments.Count - 1; i >= 0; i--) // newest wins
{
    structuresChecked++;
    if (_segments[i].TryGetValue(key, out value)) return value;
}
```

Compaction merges segments and throws away old versions. File: [Ex01_LsmStore.cs](code/Examples/Ex01_LsmStore.cs). Real implementations add a write-ahead log and Bloom filters.

## Normalization and denormalization

A normalized schema stores each fact once. A denormalized one copies the author's name into a message to avoid a join. The copy has to be updated; decide who does it and how far it may lag.

## Best practices

- Write down queries and their frequency before the schema.
- Check plans on a volume close to the real one.
- Drop unused indexes.
- Plan for migrations: the schema will change.

## Common mistakes

- ❌ Three separate indexes instead of one composite → ✅ an index shaped for the query.
- ❌ A function over a column in `WHERE` → ✅ an expression index or transformed data.
- ❌ A random primary key where the database orders rows by it → ✅ ordered identifiers (for example UUID v7).
- ❌ Writing to two databases from code one after another → ✅ one source of truth and change propagation.

## Comparison with other tracks

- Types and constraints — [DDL in PostgreSQL](../../../postgresql/modules/03-data-modeling-ddl/overview.en.md).
- Access from .NET — [EF Core](../../../csharp/modules/15-ef-core/overview.en.md), [Dapper](../../../csharp/libraries/dapper/overview.en.md).
- A different index structure, the inverted one — [OpenSearch: text analysis](../../../opensearch/modules/02-mappings-and-analysis/overview.en.md).

## Related materials

- [Basics](basics.en.md)
- [Expert theory](expert.en.md)

## Sources

- [SQLite: Query Planning](https://www.sqlite.org/queryplanner.html)
- [Azure: data partitioning](https://learn.microsoft.com/azure/architecture/best-practices/data-partitioning)
