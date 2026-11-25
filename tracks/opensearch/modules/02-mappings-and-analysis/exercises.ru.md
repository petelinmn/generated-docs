---
title: "Упражнения: отображения и анализ"
lang: ru
track: opensearch
kind: module
doc: exercises
slug: 02-mappings-and-analysis
part: 1
tags: [mapping, analyzer, inverted-index, text-keyword, nested, dynamic-mapping]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Упражнения: отображения и анализ

🌐 **Русский** · [English](exercises.en.md)

> Нужен узел из [обзора трека](../../overview.ru.md#окружение) и объект `client` из [базовой теории](basics.ru.md).

## 1. Исследуйте анализаторы

Сравните `standard`, `simple` и `english` на фразе «Running dogs' toys, v2.0».

<details><summary>Подсказка</summary>

`indices.analyze`.

</details>

<details><summary>Решение</summary>

```python
for name in ("standard", "simple", "english"):
    r = client.indices.analyze(body={"text": "Running dogs' toys, v2.0", "analyzer": name})
    print(name, [t["token"] for t in r["tokens"]])
```

</details>

## 2. Явное отображение

Создайте `notes` с полями `title` (полнотекстовое и сортируемое), `tag`, `created`; запретите незнакомые поля.

<details><summary>Подсказка</summary>

`dynamic: strict` и multi-fields.

</details>

<details><summary>Решение</summary>

```python
client.indices.create(index="notes", body={"settings": {"number_of_replicas": 0}, "mappings": {"dynamic": "strict", "properties": {
    "title": {"type": "text", "analyzer": "english", "fields": {"raw": {"type": "keyword"}}},
    "tag": {"type": "keyword"}, "created": {"type": "date"}}}})
for i, t in enumerate(["Banana", "Apple"], 1):
    client.index(index="notes", id=str(i), body={"title": t, "tag": "a"}, params={"refresh": "true"})
r = client.search(index="notes", body={"sort": [{"title.raw": "asc"}]})
print([h["_source"]["title"] for h in r["hits"]["hits"]])
```

</details>

## 3. Поиск без учёта диакритики

Сделайте так, чтобы запрос `cafe` находил `Café`.

<details><summary>Подсказка</summary>

Свой анализатор с `lowercase` и `asciifolding`.

</details>

<details><summary>Решение</summary>

```python
client.indices.create(index="places", body={
    "settings": {"analysis": {"analyzer": {"folded": {"type": "custom", "tokenizer": "standard", "filter": ["lowercase", "asciifolding"]}}}},
    "mappings": {"properties": {"name": {"type": "text", "analyzer": "folded"}}}})
client.index(index="places", id="1", body={"name": "Café Roma"}, params={"refresh": "true"})
print(client.search(index="places", body={"query": {"match": {"name": "cafe"}}})["hits"]["total"]["value"])
client.indices.delete(index="places")
```

</details>

## 4. Пара значений

У заказа есть строки `{sku, qty}`. Найдите заказы, где у `sku=A` количество не меньше 5.

<details><summary>Подсказка</summary>

`nested`.

</details>

<details><summary>Решение</summary>

```python
client.indices.create(index="orders", body={"mappings": {"properties": {"lines": {"type": "nested", "properties": {
    "sku": {"type": "keyword"}, "qty": {"type": "integer"}}}}}})
client.index(index="orders", id="1", body={"lines": [{"sku": "A", "qty": 1}, {"sku": "B", "qty": 9}]})
client.index(index="orders", id="2", body={"lines": [{"sku": "A", "qty": 7}]}, params={"refresh": "true"})
r = client.search(index="orders", body={"query": {"nested": {"path": "lines", "query": {"bool": {"filter": [
    {"term": {"lines.sku": "A"}}, {"range": {"lines.qty": {"gte": 5}}}]}}}}})
print([h["_id"] for h in r["hits"]["hits"]])
client.indices.delete(index="orders")
```

</details>

## 5. Смена типа

Поле `n` в индексе `old` имеет тип `text`. Переведите данные в индекс, где оно `integer`.

<details><summary>Подсказка</summary>

Новый индекс и `reindex`.

</details>

<details><summary>Решение</summary>

```python
client.index(index="old", id="1", body={"n": "5"}, params={"refresh": "true"})
client.indices.create(index="new", body={"mappings": {"properties": {"n": {"type": "integer"}}}})
client.reindex(body={"source": {"index": "old"}, "dest": {"index": "new"}}, params={"refresh": "true"})
print(client.indices.get_mapping(index="new")["new"]["mappings"]["properties"]["n"])
print(client.search(index="new", body={"query": {"range": {"n": {"gte": 3}}}})["hits"]["total"]["value"])
client.indices.delete(index="old,new")
```

</details>
