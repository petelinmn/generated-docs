---
title: "Индексация в большом объёме: продвинутая теория"
lang: ru
track: opensearch
kind: module
doc: advanced
slug: 05-indexing-at-scale
part: 2
tags: [bulk, index-template, alias, reindex, rollover]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Индексация в большом объёме: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

Требуются данные и `helpers` из [базовой теории](basics.ru.md).

## Шаблоны индексов

```python
client.transport.perform_request("PUT", "/_component_template/c-common", body={"template": {
    "settings": {"number_of_replicas": 0}, "mappings": {"properties": {"@timestamp": {"type": "date"}}}}})
client.indices.put_index_template(name="t-logs", body={
    "index_patterns": ["logs-*"], "priority": 10, "composed_of": ["c-common"],
    "template": {"settings": {"number_of_shards": 2},
                 "mappings": {"properties": {"level": {"type": "keyword"}}}, "aliases": {"all-logs": {}}}})
client.index(index="logs-2026.01", body={"level": "INFO", "@timestamp": "2026-01-01T00:00:00Z"})
# the new index got: 2 shards, 0 replicas, fields @timestamp and level, alias all-logs
client.indices.put_index_template(name="t-logs", body={"index_patterns": ["logs-*"], "priority": 10,
    "template": {"settings": {"number_of_shards": 3}}})
print(client.indices.get_settings(index="logs-2026.01")["logs-2026.01"]["settings"]["index"]["number_of_shards"])
# 2: existing indexes are not changed
```

Компонентный шаблон — переиспользуемая часть. Два шаблона с пересекающимся образцом и одинаковым `priority` создать нельзя (ошибка 400). Что получит будущий индекс, показывает `_index_template/_simulate_index/<имя>`.

## Псевдоним на несколько индексов

```python
for name in ("app-a", "app-b"):
    client.indices.create(index=name, body={"settings": {"number_of_replicas": 0}})
    client.indices.put_alias(index=name, name="app-pair")
client.index(index="app-pair", body={})        # error 400: no write index is defined for alias
client.indices.update_aliases(body={"actions": [{"add": {"index": "app-b", "alias": "app-pair", "is_write_index": True}}]})
print(client.index(index="app-pair", body={})["_index"])    # app-b
client.get(index="app-pair", id="1")           # error 400: alias has more than one index
```

## Псевдоним с фильтром

```python
client.indices.create(index="items-f", body={"settings": {"number_of_replicas": 0}})
for i, price in enumerate((10, 20, 30), 1):
    client.index(index="items-f", id=str(i), body={"price": price}, params={"refresh": "true"})
client.indices.put_alias(index="items-f", name="items-expensive", body={"filter": {"range": {"price": {"gte": 20}}}})
print(client.count(index="items-expensive")["count"])    # 2
```

Фильтр псевдонима — удобство, а не защита: прямой доступ к индексу он не запрещает.

## Rollover

```python
client.indices.create(index="app-log-000001", body={"settings": {"number_of_replicas": 0},
    "aliases": {"app-log": {"is_write_index": True}}})
client.index(index="app-log", body={"a": 1}, params={"refresh": "true"})
r = client.indices.rollover(alias="app-log", body={"conditions": {"max_docs": 5}})
print(r["rolled_over"])                          # False
r = client.indices.rollover(alias="app-log", body={"conditions": {"max_docs": 1}})
print(r["rolled_over"], r["new_index"])          # True app-log-000002
```

Условия проверяются только в момент вызова; автоматизацию даёт ISM (модуль 08 дорожной карты).

## Долгий reindex

```python
import time

task = client.reindex(body={"source": {"index": "events"}, "dest": {"index": "events-copy"}},
                      params={"wait_for_completion": "false", "slices": "auto"})
while not (state := client.tasks.get(task_id=task["task"]))["completed"]:
    time.sleep(0.2)
print(state["response"]["total"], state["response"]["created"])   # 1000 1000
```

## Подмножество и преобразование

```python
r = client.reindex(params={"refresh": "true"}, body={
    "source": {"index": "events", "query": {"range": {"n": {"lt": 10}}}, "_source": ["n"]},
    "dest": {"index": "events-small"},
    "script": {"source": "ctx._source.n *= 2"}})
print(r["total"])                                # 10
```

## Повторный запуск reindex

| `dest.op_type` | Повтор по двум уже скопированным документам |
|---|---|
| по умолчанию (`index`) | `updated: 2` |
| `create` + `conflicts: proceed` | `created: 0`, `version_conflicts: 2` |

## Настройки на время первичной загрузки

Обычно на время загрузки ставят `refresh_interval: -1` и `number_of_replicas: 0`, затем возвращают. Пока реплик нет, потеря узла означает потерю данных шарда: так делают, только если загрузку можно повторить. `index.translog.durability: async` ещё ускоряет запись, но при сбое теряются подтверждённые операции за последние секунды.

## Связанные материалы

- [Базовая теория](basics.ru.md) · [Эксперт](expert.ru.md)
- [01 · Основы](../01-opensearch-basics/expert.ru.md) — translog, refresh
- [07 · Кластер и шарды](../07-cluster-and-shards/overview.ru.md)
- [08 · Жизненный цикл индексов](../08-index-lifecycle/overview.ru.md)

## Источники

- [Index templates](https://docs.opensearch.org/latest/im-plugin/index-templates/)
- [Rollover](https://docs.opensearch.org/latest/api-reference/index-apis/rollover/)
- [Reindex API](https://docs.opensearch.org/latest/api-reference/document-apis/reindex/)
