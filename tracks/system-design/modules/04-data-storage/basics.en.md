---
title: "Data storage: basics"
lang: en
track: system-design
kind: module
doc: basics
slug: 04-data-storage
part: 1
tags: [system-design, data-modeling, database-index, b-tree, lsm-tree, query-plan, nosql]
versions: ".NET 11, C# 15; Microsoft.Data.Sqlite 11.0.0"
status: draft
updated: 2026-11-26
---

# Data storage: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** SQL basics · **Time:** ~2 h

## Why it matters

Changing the store later is expensive: queries, transactions and operations all change. So the choice starts with questions: what do we read, by which key, in what order, how often do we write.

## Storage models

| Model | Strength | Typical use | Examples |
|---|---|---|---|
| Relational | relations, transactions, ad hoc queries | core application data | PostgreSQL, SQL Server |
| Key-value | access by key | cache, sessions | Redis |
| Document | a nested object as a whole | catalogs, profiles | MongoDB |
| Wide-column | a heavy write stream by partition key | events, messages | Cassandra |
| Search index | full-text search | searching catalogs and logs | [OpenSearch](../../../opensearch/modules/01-opensearch-basics/overview.en.md) |
| Object storage | large files | media, backups | S3, Azure Blob Storage |

Without a strong reason, start with a relational database. Several stores in one system are acceptable, but each has to be operated and kept in agreement.

## Access patterns

For a chat: "the last 20 messages of a chat", "add a message", "find by text". The first two point to the key `(chat_id, sent_at)`, the third to a separate search index.

## Indexes

An index is a separate ordered structure: it finds a row by value without looking through the whole table. Every write updates all indexes.

```csharp
// Every write pays for every index.
public void Upsert(Order order)
{
    if (_rows.TryGetValue(order.Id, out Order? old)) _byTotal.Remove((old.Total, old.Id));
    _rows[order.Id] = order;
    _byTotal.Add((order.Total, order.Id));
}
```

File: [Ex02_SecondaryIndex.cs](code/Examples/Ex02_SecondaryIndex.cs). Thanks to the ordering, the range "total from 10 to 250" is read without going through all orders.

## The query plan

SQLite answers `EXPLAIN QUERY PLAN` with lines: `SCAN` looks at everything, `SEARCH` only at the needed part. For the latest-messages query without an index the plan contains `SCAN messages` and a temporary B-tree for sorting; with the index `(chat_id, sent_at)` it is `SEARCH messages USING INDEX ix_chat` with no sorting. The tests of [Ex03_QueryPlans.cs](code/Examples/Ex03_QueryPlans.cs) show this. The output format may change between versions: it is for humans.

## Beginner mistakes

- ❌ Choosing a database by popularity → ✅ by access patterns.
- ❌ Keeping files in rows of the main table → ✅ object storage and a link.
- ❌ Not looking at the plan → ✅ check on realistically sized data.
- ❌ Assuming an index is free → ✅ it takes space and slows writes.

## Related materials

- [Advanced theory](advanced.en.md)
- [SQL queries in PostgreSQL](../../../postgresql/modules/02-sql-queries/overview.en.md)

## Sources

- [SQLite: EXPLAIN QUERY PLAN](https://www.sqlite.org/eqp.html)
- [SQLite: Query Planning](https://www.sqlite.org/queryplanner.html)
