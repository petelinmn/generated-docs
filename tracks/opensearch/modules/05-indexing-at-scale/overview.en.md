---
title: "Indexing at scale"
lang: en
track: opensearch
kind: module
doc: overview
slug: 05-indexing-at-scale
part: 2
tags: [bulk, index-template, alias, reindex, rollover]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
levels: [1, 2, 3, 4]
status: draft
updated: 2026-11-25
---

# Indexing at scale

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

How to load a lot of data and change an index's structure without downtime: `_bulk`, **index templates**, **aliases**, **reindex** and **rollover**.

## Key ideas

- Batch writes are orders of magnitude faster than one-by-one writes.
- A template applies only when an index is created.
- An application talks to an alias, not to a concrete index.
- `reindex` copies documents, not settings or mappings.
- Rollover switches writes to a new index when conditions are met.

## Minimal example

```python
client.indices.create(index="catalog-v1", body={"aliases": {"catalog": {}}})
# later: build catalog-v2, reindex, then switch atomically
client.indices.update_aliases(body={"actions": [
    {"remove": {"index": "catalog-v1", "alias": "catalog"}},
    {"add": {"index": "catalog-v2", "alias": "catalog"}}]})
```

## When to use

Initial loads, mapping changes, time-based indexes (logs, events).

## Material levels

| Level | File | Add with |
|---|---|---|
| 1. Fundamentals | [basics.en.md](basics.en.md) | |
| 2. Advanced theory | [advanced.en.md](advanced.en.md) | |
| 3. Expert theory | [expert.en.md](expert.en.md) | |
| 4. Self-check | [quiz.en.md](quiz.en.md), [exercises.en.md](exercises.en.md) | |
| 5. Runnable examples | — | `/learn opensearch 05-indexing-at-scale level 5` |
| 6. Tests | — | `/learn opensearch 05-indexing-at-scale level 6` |

## Related materials

- [OpenSearch track](../../overview.en.md)
- [01 · OpenSearch basics](../01-opensearch-basics/overview.en.md)
- [02 · Mappings and text analysis](../02-mappings-and-analysis/overview.en.md)
- [Kafka producers](../../../kafka/modules/02-producers/overview.en.md) — batching too
