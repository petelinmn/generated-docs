---
title: "Упражнения: индексация в большом объёме"
lang: ru
track: opensearch
kind: module
doc: exercises
slug: 05-indexing-at-scale
part: 2
tags: [bulk, index-template, alias, reindex, rollover]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Упражнения: индексация в большом объёме

🌐 **Русский** · [English](exercises.en.md)

> Нужен узел из [обзора трека](../../overview.ru.md#окружение) и объект `client` из [модуля 01](../01-opensearch-basics/basics.ru.md).

## 1. Загрузка

Загрузите 500 документов в `catalog-v1` и проверьте счётчик.

<details><summary>Подсказка</summary>

`helpers.bulk`.

</details>

<details><summary>Решение</summary>

```python
from opensearchpy import helpers

client.indices.create(index="catalog-v1", body={"settings": {"number_of_replicas": 0}})
helpers.bulk(client, ({"_index": "catalog-v1", "_id": str(i), "price": str(i)} for i in range(500)), refresh=True)
print(client.count(index="catalog-v1")["count"])
```

</details>

## 2. Переезд

Сделайте `price` числом и переключите псевдоним `catalog` на новый индекс.

<details><summary>Подсказка</summary>

Новый индекс, `reindex`, `_aliases`.

</details>

<details><summary>Решение</summary>

```python
client.indices.put_alias(index="catalog-v1", name="catalog")
client.indices.create(index="catalog-v2", body={"settings": {"number_of_replicas": 0}, "mappings": {"properties": {"price": {"type": "integer"}}}})
client.reindex(body={"source": {"index": "catalog-v1"}, "dest": {"index": "catalog-v2"}}, params={"refresh": "true"})
client.indices.update_aliases(body={"actions": [{"remove": {"index": "catalog-v1", "alias": "catalog"}}, {"add": {"index": "catalog-v2", "alias": "catalog"}}]})
print(client.count(index="catalog", body={"query": {"range": {"price": {"gte": 490}}}})["count"])
```

</details>

## 3. Ошибочные документы

Загрузите смесь верных и неверных документов в `catalog` и соберите причины отказов.

<details><summary>Подсказка</summary>

`streaming_bulk` с `raise_on_error=False`.

</details>

<details><summary>Решение</summary>

```python
mixed = [{"_index": "catalog", "price": v} for v in (1, "two", 3)]
failed = [item["index"]["error"]["type"] for ok, item in helpers.streaming_bulk(client, mixed, raise_on_error=False) if not ok]
print(failed)
```

</details>

## 4. Шаблон

Сделайте так, чтобы каждый новый индекс `events-*` получал 0 реплик и поле `at` типа `date`.

<details><summary>Подсказка</summary>

`put_index_template`.

</details>

<details><summary>Решение</summary>

```python
client.indices.put_index_template(name="t-events", body={"index_patterns": ["events-*"], "template": {
    "settings": {"number_of_replicas": 0}, "mappings": {"properties": {"at": {"type": "date"}}}}})
client.indices.create(index="events-1")
print(client.indices.get_mapping(index="events-1")["events-1"]["mappings"]["properties"]["at"])
client.indices.delete_index_template(name="t-events")
```

</details>

## 5. Rollover

Настройте псевдоним записи и выполните rollover после трёх документов.

<details><summary>Подсказка</summary>

`is_write_index` и `max_docs`.

</details>

<details><summary>Решение</summary>

```python
client.indices.create(index="app-ex-000001", body={"settings": {"number_of_replicas": 0}, "aliases": {"app-ex": {"is_write_index": True}}})
for i in range(3):
    client.index(index="app-ex", body={"i": i}, params={"refresh": "true"})
r = client.indices.rollover(alias="app-ex", body={"conditions": {"max_docs": 3}})
print(r["rolled_over"], r["new_index"])
```

</details>
