---
title: "Index lifecycle"
lang: en
track: opensearch
kind: module
doc: overview
slug: 08-index-lifecycle
part: 2
tags: [index-lifecycle, data-stream, ism, retention, rollover]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
levels: [1, 2, 3, 4]
status: draft
updated: 2026-11-25
---

# Index lifecycle

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

How to keep time-based data that only grows: **data streams** for writing and **ISM** for automatic rollover, compaction and deletion.

## Key ideas

- Dropping an index is cheaper than deleting documents.
- A data stream is one name over many backing indices.
- An ISM policy is states, actions and transitions.
- ISM runs on a schedule with a delay, not instantly.
- A managed index keeps the policy version it was attached with.

## Minimal example

```python
api("GET", "/_data_stream/logs-app")
api("GET", "/_plugins/_ism/explain/logs-app")
```

## When to use

Logs, metrics, events. Not for a product catalogue or other mutable data.

## Material levels

| Level | File | Add with |
|---|---|---|
| 1. Fundamentals | [basics.en.md](basics.en.md) | |
| 2. Advanced theory | [advanced.en.md](advanced.en.md) | |
| 3. Expert theory | [expert.en.md](expert.en.md) | |
| 4. Self-check | [quiz.en.md](quiz.en.md), [exercises.en.md](exercises.en.md) | |
| 5. Runnable examples | — | `/learn opensearch 08-index-lifecycle level 5` |
| 6. Tests | — | `/learn opensearch 08-index-lifecycle level 6` |

## Related materials

- [OpenSearch track](../../overview.en.md)
- [05 · Indexing at scale](../05-indexing-at-scale/overview.en.md)
- [07 · Cluster and shards](../07-cluster-and-shards/overview.en.md)
- [Topics and retention in Kafka](../../../kafka/modules/04-topics-and-retention/overview.en.md) — retention counterpart
