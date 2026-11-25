---
title: "Indexing at scale: expert theory"
lang: en
track: opensearch
kind: module
doc: expert
slug: 05-indexing-at-scale
part: 2
tags: [bulk, index-template, alias, reindex, rollover]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Indexing at scale: expert theory

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

## What happens to a batch

The coordinating node splits the batch, groups operations by shard and sends them to primaries, which apply them and pass them to replicas. Writes are served by the `write` thread pool: 12 threads on the test node (it depends on the CPU count) and a queue of 10000. Rejections show in the `rejected` column.

```python
client.cat.thread_pool(thread_pool_patterns="write", params={"v": "true", "h": "name,size,queue_size,rejected"})
```

> ⚠️ Verify: the behaviour on queue overflow (status 429) comes from the documentation and was not reproduced.

## Segments and merges

Every refresh with new data creates a segment, and background merges combine them. After 1000 single writes the test indexes had several segments each, and with `refresh=true` most documents had already been merged: merging did work, which explains part of the slowdown.

## Operations reference

| API | Purpose | Constraint |
|---|---|---|
| `_bulk` | a batch of operations | not atomic |
| `_reindex` | copy documents | needs `_source` |
| `_aliases` | atomic alias changes | |
| `_rollover` | a new index on conditions | needs a write alias or a data stream |
| `_split` | more shards | the source is write-blocked |
| `_shrink` | fewer shards | the same; a copy of every shard on one node |
| `_clone` | a copy of an index | the source is write-blocked |
| `_close` / `_open` | close and open an index | |
| `_tasks` | long operations | |

## Split

```python
client.indices.create(index="app-src", body={"settings": {"number_of_shards": 1, "number_of_replicas": 0}})
client.index(index="app-src", body={"a": 1}, params={"refresh": "true"})
client.indices.put_settings(index="app-src", body={"index.blocks.write": True})
client.index(index="app-src", body={})          # 403 cluster_block_exception
client.indices.split(index="app-src", target="app-split",
                     body={"settings": {"index.number_of_shards": 2, "index.number_of_replicas": 0}})
```

## Gotchas

1. Changing a template does not touch existing indexes.
2. An alias over several indexes without `is_write_index` serves reads but not writes; `get` through it does not work.
3. While an alias points to one index, writing through it works without `is_write_index` — and breaks when a second one is added.
4. A repeated `reindex` overwrites documents by default, including ones already changed in the new index.
5. `BulkIndexError` does not mean nothing was written.
6. Shrinking to a larger shard count is a 400 error.

## Interview questions

1. Why are batch writes faster?
<details><summary>Answer</summary>

Fewer round trips and fewer translog syncs: a sync is done per request.
</details>

2. Is `_bulk` atomic?
<details><summary>Answer</summary>

No.
</details>

3. What do you do on a 429?
<details><summary>Answer</summary>

The write queue is overloaded: retry with backoff and lower the concurrency.
</details>

4. When is an index template applied?
<details><summary>Answer</summary>

Only when an index is created.
</details>

5. How do you change a field type without downtime?
<details><summary>Answer</summary>

A new index, reindex, catch up the changes, switch the alias atomically.
</details>

6. What does `reindex` copy?
<details><summary>Answer</summary>

Documents from `_source`; not settings or mappings.
</details>

7. What is `is_write_index`?
<details><summary>Answer</summary>

It marks the index of an alias that accepts writes.
</details>

8. Why rollover?
<details><summary>Answer</summary>

To keep indexes bounded in size and delete old ones whole.
</details>

9. Why is deleting an index cheaper than `delete_by_query`?
<details><summary>Answer</summary>

Files are removed instead of marking documents and merging later.
</details>

10. Split or shrink?
<details><summary>Answer</summary>

Split increases the shard count, shrink reduces it; in both the source is blocked for writes.
</details>

11. How do you watch a long reindex?
<details><summary>Answer</summary>

`wait_for_completion=false` and the Tasks API.
</details>

12. How do you make a load idempotent?
<details><summary>Answer</summary>

A deterministic `_id`, plus an external version if needed.
</details>

## Related materials

- [Advanced theory](advanced.en.md) · [Quiz](quiz.en.md)
- [Topics and storage in Kafka](../../../kafka/modules/04-topics-and-retention/expert.en.md) — segments and whole-file deletion too

## Sources

- [Split index](https://docs.opensearch.org/latest/api-reference/index-apis/split/)
- [Shrink index](https://docs.opensearch.org/latest/api-reference/index-apis/shrink-index/)
- [Tasks API](https://docs.opensearch.org/latest/api-reference/tasks/)
