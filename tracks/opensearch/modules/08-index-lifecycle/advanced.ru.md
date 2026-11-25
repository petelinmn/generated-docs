---
title: "Жизненный цикл индексов: продвинутая теория"
lang: ru
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

# Жизненный цикл индексов: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

`client` и функция `api` — из [базовой теории](basics.ru.md).

## Index State Management

ISM — плагин, который ведёт каждый индекс по **состояниям**. В состоянии выполняются **действия**, затем проверяются условия **переходов**.

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

- `ism_template` привязывает политику к **новым** индексам по шаблону имени.
- К существующим политику добавляют через `_plugins/_ism/add/<index>`.

## Скорость работы

Задание запускается раз в `plugins.index_state_management.job_interval` минут (по умолчанию 5) плюс случайная добавка `jitter` (0.6, то есть до 60% интервала). Для опытов интервал уменьшают:

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

Даже с интервалом в минуту каждый шаг занимает отдельный запуск. Не рассчитывайте на точность до минут.

## Rollover обычного индекса

Для data stream настраивать нечего. Обычному индексу нужен псевдоним записи и настройка `rollover_alias`:

```python
api("PUT", "/_plugins/_ism/policies/roll", {"policy": {"description": "v1", "default_state": "hot",
    "states": [{"name": "hot", "actions": [{"rollover": {"min_doc_count": 1}}], "transitions": []}]}})
client.indices.create(index="app-000001", body={
    "settings": {"number_of_replicas": 0, "plugins.index_state_management.rollover_alias": "app"},
    "aliases": {"app": {"is_write_index": True}}})
client.index(index="app", body={})["_index"]                      # 'app-000001'
api("POST", "/_plugins/_ism/add/app-000001", {"policy_id": "roll"})["updated_indices"]   # 1
```

После запуска задания появился `app-000002`, а `is_write_index` перешёл на него. Без настройки индекс остаётся в действии `rollover` с сообщением `Missing rollover_alias index setting`.

## Изменение политики

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

Привязанные индексы продолжают работать по прежней версии: в прогоне индекс переехал по старому порогу в 1 документ, хотя в политике уже стояло 5. Чтобы перевести их, вызывают `_plugins/_ism/change_policy/<index>`.

## Исправить документ в потоке

`_update_by_query` и `_delete_by_query` по имени потока работают. Точечная правка делается в backing-индексе с `if_seq_no` и `if_primary_term`; без них запись туда отклоняется. `_reindex` в поток требует `op_type: create`.

## Частые ошибки

- Не задать `rollover` в политике для потока и получить один бесконечный индекс.
- Считать `min_index_age` временем в состоянии: это возраст индекса.
- Оставить `job_interval` равным 1 на боевом кластере.

## Связанные материалы

- [Базовая теория](basics.ru.md) · [Эксперт](expert.ru.md)
- [07 · Кластер и шарды](../07-cluster-and-shards/advanced.ru.md)

## Источники

- [Index State Management](https://docs.opensearch.org/latest/im-plugin/ism/index/)
- [Policies](https://docs.opensearch.org/latest/im-plugin/ism/policies/)
- [ISM settings](https://docs.opensearch.org/latest/im-plugin/ism/settings/)
