---
title: "Index lifecycle: expert theory"
lang: en
track: opensearch
kind: module
doc: expert
slug: 08-index-lifecycle
part: 2
tags: [index-lifecycle, data-stream, ism, retention, rollover]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Index lifecycle: expert theory

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

## ISM actions

As listed by the `plugins.index_state_management.allow_list` setting and the documentation:

| Group | Actions |
|---|---|
| Switching indices | `rollover` |
| Writes and access | `read_only`, `read_write`, `open`, `close`, `search_only` |
| Compaction | `force_merge`, `shrink`, `rollup` |
| Placement | `allocation`, `replica_count`, `index_priority` |
| Safety and removal | `snapshot`, `delete`, `convert_index_to_remote` |
| Other | `alias`, `notification` |

`rollover`, `read_only`, `force_merge` and `delete` were run here.

## Action retries

A stored policy gets defaults on every action: `retry` with `count: 3`, `backoff: exponential`, `delay: 1m`. A failed index is resumed with `_plugins/_ism/retry/<index>`; for an index not in that state the call answers `This index is not in failed state`.

## Transition and rollover conditions

| Where | Conditions |
|---|---|
| `transitions.conditions` | `min_index_age`, `min_rollover_age`, `min_state_age`, `min_doc_count`, `min_size`, `cron` |
| `rollover` | `min_index_age`, `min_doc_count`, `min_size`, `min_primary_shard_size` |

## Settings

| Setting (`plugins.index_state_management.`) | Default |
|---|---|
| `enabled` | `true` |
| `job_interval` | `5` (minutes) |
| `jitter` | `0.6` |
| `coordinator.sweep_period` | `10m` |
| `history.enabled` | `true` |
| `history.rollover_retention_period` | `30d` |

## Under the hood

Policies live in the `.opendistro-ism-config` system index (visible in the delete response), and the job scheduler plugin runs the jobs. The `opendistro.*` names are the older aliases of the `plugins.*` settings.

## Gotchas

1. Delete by `_id` on the stream name is rejected with the same message as an overwrite: only `create` is allowed.
2. A late document with an old date lands in the current index, so retention by backing index follows arrival, not `@timestamp`.
3. An empty index waiting on a `min_doc_count` rollover shows `Pending rollover` in `explain`.
4. An `ism_template` attached the policy to the hidden `.ds-logs-web-000001` (the test run listed both name patterns).
5. Repeating the policy `PUT` without `if_seq_no` and `if_primary_term` returns 409.
6. `_reindex` into a stream without `op_type: create` fails with a 400.

> ⚠️ Verify: the `shrink`, `rollup`, `snapshot` and `allocation` actions, notifications, the `cron`, `min_state_age`, `min_rollover_age` and `min_size` conditions, and whether `logs-web*` alone (without a `.ds-` pattern) in `ism_template` captures backing indices, were not run.

## Interview questions

1. Why delete by index rather than by query?
<details><summary>Answer</summary>

Dropping an index frees space at once; deleted documents are only marked until segments merge.
</details>

2. What is a data stream?
<details><summary>Answer</summary>

A name over backing indices: writes to the newest, reads from all, `@timestamp` required.
</details>

3. Data stream or rollover alias?
<details><summary>Answer</summary>

A stream for appended events: less setup. An alias if you need writes by `_id` in the current index or have no time field.
</details>

4. What is an ISM policy made of?
<details><summary>Answer</summary>

A default state, states with actions and transitions, optionally `ism_template` and an error notification.
</details>

5. Why did the policy not act at once?
<details><summary>Answer</summary>

The job runs on a schedule (5 minutes plus jitter), a step per run.
</details>

6. How do you see where an index is?
<details><summary>Answer</summary>

`_plugins/_ism/explain/<index>`: state, action, message.
</details>

7. You changed a policy: what about current indices?
<details><summary>Answer</summary>

They keep the old version; use `change_policy`.
</details>

8. `min_index_age` vs `min_state_age` vs `min_rollover_age`?
<details><summary>Answer</summary>

Age since index creation, time in the state, time since rollover.
</details>

9. What does `force_merge` give?
<details><summary>Answer</summary>

Fewer segments on an index no longer written: less overhead, deleted docs purged.
</details>

10. What do you do with a stuck action?
<details><summary>Answer</summary>

Read the `explain` message and fix the cause; for a failed index use `_plugins/_ism/retry`.
</details>

## Related materials

- [Advanced theory](advanced.en.md) · [Quiz](quiz.en.md)
- [04 · Time aggregations](../04-aggregations/advanced.en.md)

## Sources

- [ISM API](https://docs.opensearch.org/latest/im-plugin/ism/api/)
- [Policies: actions and transitions](https://docs.opensearch.org/latest/im-plugin/ism/policies/)
