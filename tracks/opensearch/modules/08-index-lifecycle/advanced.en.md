---
title: "Index lifecycle: advanced theory"
lang: en
track: opensearch
kind: module
doc: advanced
slug: 08-index-lifecycle
part: 2
tags: [index-lifecycle, data-stream, ism, retention, rollover]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Index lifecycle: advanced theory

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

`client` and the `api` function are from the [fundamentals](basics.en.md).

## Index State Management

ISM is a plugin that walks each index through **states**. A state runs its **actions**, then its **transition** conditions are checked.

```python
policy = {"policy": {
    "description": "logs",
    "default_state": "hot",
    "states": [
        {"name": "hot", "actions": [{"rollover": {"min_index_age": "1d", "min_primary_shard_size": "30gb"}}],
         "transitions": [{"state_name": "warm", "conditions": {"min_index_age": "7d"}}]},
        {"name": "warm", "actions": [{"read_only": {}}, {"force_merge": {"max_num_segments": 1}}],
         "transitions": [{"state_name": "delete", "conditions": {"min_index_age": "30d"}}]},
        {"name": "delete", "actions": [{"delete": {}}], "transitions": []}],
    "ism_template": [{"index_patterns": ["logs-web*"], "priority": 100}]}}

api("PUT", "/_plugins/_ism/policies/logs", policy)["_id"]        # 'logs'

api("PUT", "/_index_template/web", {"index_patterns": ["logs-web*"], "data_stream": {}, "priority": 100,
    "template": {"settings": {"number_of_shards": 1, "number_of_replicas": 0}}})
client.index(index="logs-web", body={"@timestamp": "2026-11-25T10:00:00Z"})["result"]   # 'created'
api("GET", "/_plugins/_ism/explain/logs-web")      # which policy, state and action each index is in
```

- `ism_template` attaches the policy to **new** indices by name pattern.
- To existing indices you add it with `_plugins/_ism/add/<index>`.

## How fast it works

The job runs every `plugins.index_state_management.job_interval` minutes (5 by default) plus a random `jitter` (0.6, up to 60% of the interval). For experiments you lower it:

```python
client.cluster.put_settings(body={"transient": {"plugins.index_state_management.job_interval": 1}})
```

```text
test policy: rollover at 2 docs, warm at min_index_age 2m, delete at 5m, job_interval 1 minute
times are seconds since the first check; they vary because of jitter

 80   .ds-logs-web-000001   policy initialised, state hot
150   .ds-logs-web-000001   rolled over, .ds-logs-web-000002 appears
200   .ds-logs-web-000001   transitioning to warm
250-450                     read_only, then force_merge (several runs)
500   .ds-logs-web-000001   transitioning to delete
550                         .ds-logs-web-000001 is gone
```

Even with a one-minute interval every step takes its own run. Do not expect minute accuracy.

## Rollover of an ordinary index

A data stream needs nothing extra. An ordinary index needs a write alias and the `rollover_alias` setting:

```python
api("PUT", "/_plugins/_ism/policies/roll", {"policy": {"description": "v1", "default_state": "hot",
    "states": [{"name": "hot", "actions": [{"rollover": {"min_doc_count": 1}}], "transitions": []}]}})
client.indices.create(index="app-000001", body={
    "settings": {"number_of_replicas": 0, "plugins.index_state_management.rollover_alias": "app"},
    "aliases": {"app": {"is_write_index": True}}})
client.index(index="app", body={})["_index"]                      # 'app-000001'
api("POST", "/_plugins/_ism/add/app-000001", {"policy_id": "roll"})["updated_indices"]   # 1
```

After the job ran, `app-000002` appeared and `is_write_index` moved to it. Without the setting an index stays in the `rollover` action with `Missing rollover_alias index setting`.

## Changing a policy

```python
got = api("GET", "/_plugins/_ism/policies/roll")
changed = {"policy": {"description": "v2", "default_state": "hot",
    "states": [{"name": "hot", "actions": [{"rollover": {"min_doc_count": 5}}], "transitions": []}]}}
refused_status = None
try:
    api("PUT", "/_plugins/_ism/policies/roll", changed)           # no sequence numbers
except Exception as e:
    refused_status = e.status_code
refused_status                                                    # 409
api("PUT", "/_plugins/_ism/policies/roll", changed,
    if_seq_no=got["_seq_no"], if_primary_term=got["_primary_term"])["_version"]    # 2
```

Indices already attached keep the previous version: in the run an index rolled over at the old threshold of 1 document although the policy already said 5. To move them, call `_plugins/_ism/change_policy/<index>`.

## Fixing a document in a stream

`_update_by_query` and `_delete_by_query` work on the stream name. A single fix goes to the backing index with `if_seq_no` and `if_primary_term`; without them the write is rejected. `_reindex` into a stream needs `op_type: create`.

## Common mistakes

- No `rollover` in a stream policy: one endless index.
- Reading `min_index_age` as time in a state: it is the index age.
- Leaving `job_interval` at 1 on a production cluster.

## Related materials

- [Fundamentals](basics.en.md) · [Expert](expert.en.md)
- [07 · Cluster and shards](../07-cluster-and-shards/advanced.en.md)

## Sources

- [Index State Management](https://docs.opensearch.org/latest/im-plugin/ism/index/)
- [Policies](https://docs.opensearch.org/latest/im-plugin/ism/policies/)
- [ISM settings](https://docs.opensearch.org/latest/im-plugin/ism/settings/)
