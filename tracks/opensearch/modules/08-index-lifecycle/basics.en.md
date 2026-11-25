---
title: "Index lifecycle: fundamentals"
lang: en
track: opensearch
kind: module
doc: basics
slug: 08-index-lifecycle
part: 2
tags: [index-lifecycle, data-stream, ism, retention, rollover]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Index lifecycle: fundamentals

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## Why

Logs and metrics only keep arriving, and the recent ones are what you need. Deleting documents one by one is expensive, dropping a whole index is cheap. So data goes into a sequence of indices: a new one starts by age or size, old ones become cheaper and are deleted.

## Data stream

A data stream is one name over hidden backing indices. Writes go to the newest, searches cover all. It needs a template with `data_stream`, and every document needs an `@timestamp` field.

```python
from opensearchpy import OpenSearch
from opensearchpy.exceptions import RequestError

client = OpenSearch(hosts=[{"host": "localhost", "port": 9200}])

def api(method, path, body=None, **params):
    return client.transport.perform_request(method, path, body=body, params=params)

api("PUT", "/_index_template/logs", {"index_patterns": ["logs-app*"], "data_stream": {}, "priority": 100,
    "template": {"settings": {"number_of_shards": 1, "number_of_replicas": 0}}})

client.index(index="logs-app", body={"@timestamp": "2026-11-25T10:00:00Z", "msg": "start"},
             params={"refresh": "true"})["_index"]        # '.ds-logs-app-000001'
```

```python
ds = api("GET", "/_data_stream/logs-app")["data_streams"][0]
ds["generation"], [i["index_name"] for i in ds["indices"]]    # (1, ['.ds-logs-app-000001'])

api("POST", "/logs-app/_rollover")["new_index"]               # '.ds-logs-app-000002'
client.index(index="logs-app", body={"@timestamp": "2020-01-01T00:00:00Z", "msg": "late"},
             params={"refresh": "true"})["_index"]            # '.ds-logs-app-000002'
client.count(index="logs-app")["count"]                       # 2
```

The document dated 2020 went into the new index: the current write index is always used, not one "matching the date".

## What is not allowed

```python
def refused(call):
    try:
        call()
    except RequestError as e:
        return e.error

refused(lambda: client.index(index="logs-app", body={"msg": "no time"}))
# 'mapper_parsing_exception'
refused(lambda: client.index(index="logs-app", id="1", body={"@timestamp": "2026-11-25T10:00:00Z"}))
# 'illegal_argument_exception'    only op_type=create is allowed
client.create(index="logs-app", id="1", body={"@timestamp": "2026-11-25T10:00:00Z"}, params={"refresh": "true"})["result"]
# 'created'
refused(lambda: client.delete(index="logs-app", id="1"))
# 'illegal_argument_exception'
refused(lambda: client.indices.delete(index=".ds-logs-app-000002"))
# 'illegal_argument_exception'    the write index cannot be deleted
```

A stream is built for appending. If data is often changed by `_id`, use an ordinary index.

## Removing old data

```python
api("DELETE", "/_data_stream/logs-app")       # the stream and all its backing indices
```

Individual old backing indices can be deleted by hand too. ISM does it automatically — see the [next level](advanced.en.md).

## Common beginner mistakes

- Cleaning logs with `_delete_by_query` instead of deleting indices.
- Forgetting `@timestamp` in the log shipper.
- An index per day at low volume: hundreds of tiny shards.

## Related materials

- [Advanced theory](advanced.en.md)
- [05 · Indexing: templates, aliases, rollover](../05-indexing-at-scale/basics.en.md)

## Sources

- [Data streams](https://docs.opensearch.org/latest/im-plugin/data-streams/)
