---
title: "Упражнения: жизненный цикл индексов"
lang: ru
track: opensearch
kind: module
doc: exercises
slug: 08-index-lifecycle
part: 2
tags: [index-lifecycle, data-stream, ism, retention, rollover]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Упражнения: жизненный цикл индексов

🌐 **Русский** · [English](exercises.en.md)

> Нужны `client` и `api` из [базовой теории](basics.ru.md).

## 1. Свой поток

Создайте поток `metrics-cpu`, запишите документ и выведите имя backing-индекса.

<details><summary>Подсказка</summary>

Шаблон с `data_stream`.

</details>

<details><summary>Решение</summary>

```python
api("PUT", "/_index_template/metrics", {"index_patterns": ["metrics-*"], "data_stream": {}, "priority": 100,
    "template": {"settings": {"number_of_replicas": 0}}})
print(client.index(index="metrics-cpu", body={"@timestamp": "2026-11-25T10:00:00Z", "v": 1},
                   params={"refresh": "true"})["_index"])
```

</details>

## 2. Исправить значение

Замените значение `v` на 2 во всех документах потока.

<details><summary>Подсказка</summary>

`_update_by_query`.

</details>

<details><summary>Решение</summary>

```python
r = client.update_by_query(index="metrics-cpu", body={"script": {"source": "ctx._source.v = 2"}},
                           params={"refresh": "true"})
print(r["updated"])
```

</details>

## 3. Хранить два поколения

Напишите функцию, удаляющую все backing-индексы, кроме двух последних. Проверьте после двух rollover.

<details><summary>Подсказка</summary>

Список из `_data_stream`.

</details>

<details><summary>Решение</summary>

```python
def trim(stream, keep=2):
    names = [i["index_name"] for i in api("GET", f"/_data_stream/{stream}")["data_streams"][0]["indices"]]
    for name in names[:-keep]:
        client.indices.delete(index=name)
    return names[-keep:]

api("POST", "/metrics-cpu/_rollover")
api("POST", "/metrics-cpu/_rollover")
print(trim("metrics-cpu"))
```

</details>

## 4. Политика удаления

Создайте политику: удалять индексы `metrics-*` старше 14 дней.

<details><summary>Подсказка</summary>

Два состояния и `ism_template`.

</details>

<details><summary>Решение</summary>

```python
api("PUT", "/_plugins/_ism/policies/metrics-retention", {"policy": {"description": "14 days", "default_state": "keep",
    "states": [
        {"name": "keep", "actions": [], "transitions": [{"state_name": "gone", "conditions": {"min_index_age": "14d"}}]},
        {"name": "gone", "actions": [{"delete": {}}], "transitions": []}],
    "ism_template": [{"index_patterns": ["metrics-*"], "priority": 100}]}})
print(api("GET", "/_plugins/_ism/policies/metrics-retention")["policy"]["default_state"])
```

</details>

## 5. Привязать к существующему

Привяжите эту политику к текущему индексу записи и покажите `explain`.

<details><summary>Подсказка</summary>

`_plugins/_ism/add`.

</details>

<details><summary>Решение</summary>

```python
name = api("GET", "/_data_stream/metrics-cpu")["data_streams"][0]["indices"][-1]["index_name"]
print(api("POST", f"/_plugins/_ism/add/{name}", {"policy_id": "metrics-retention"}))
print(api("GET", f"/_plugins/_ism/explain/{name}")[name])      # policy_id may take a few seconds to appear
```

</details>
