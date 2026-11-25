---
title: "Упражнения: агрегации"
lang: ru
track: opensearch
kind: module
doc: exercises
slug: 04-aggregations
part: 1
tags: [aggregations, buckets, metrics, cardinality, composite-aggregation]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Упражнения: агрегации

🌐 **Русский** · [English](exercises.en.md)

> Нужны данные и функция `agg` из [базовой теории](basics.ru.md).

## 1. Выручка по городам

Посчитайте количество проданных штук по городам.

<details><summary>Подсказка</summary>

`terms` + `sum`.

</details>

<details><summary>Решение</summary>

```python
r = agg({"c": {"terms": {"field": "city"}, "aggs": {"q": {"sum": {"field": "qty"}}}}})
print({b["key"]: b["q"]["value"] for b in r["c"]["buckets"]})
```

</details>

## 2. Продажи по месяцам

Выведите только непустые месяцы в формате `yyyy-MM`.

<details><summary>Подсказка</summary>

`min_doc_count`.

</details>

<details><summary>Решение</summary>

```python
r = agg({"m": {"date_histogram": {"field": "day", "calendar_interval": "month", "format": "yyyy-MM", "min_doc_count": 1}}})
print([(b["key_as_string"], b["doc_count"]) for b in r["m"]["buckets"]])
```

</details>

## 3. Самый дорогой товар

Для каждого товара найдите город с самой высокой ценой.

<details><summary>Подсказка</summary>

`top_hits`.

</details>

<details><summary>Решение</summary>

```python
r = agg({"p": {"terms": {"field": "product"}, "aggs": {"t": {"top_hits": {"size": 1, "sort": [{"price": "desc"}]}}}}})
print({b["key"]: b["t"]["hits"]["hits"][0]["_source"]["city"] for b in r["p"]["buckets"]})
```

</details>

## 4. Все сочетания

Выгрузите все пары город–товар страницами по 2.

<details><summary>Подсказка</summary>

`composite` и `after_key`.

</details>

<details><summary>Решение</summary>

```python
comp = {"size": 2, "sources": [{"city": {"terms": {"field": "city"}}}, {"product": {"terms": {"field": "product"}}}]}
while True:
    res = agg({"c": {"composite": comp}})["c"]
    if not res["buckets"]:
        break
    print([(b["key"]["city"], b["key"]["product"]) for b in res["buckets"]])
    comp["after"] = res["after_key"]
```

</details>

## 5. Доля города

Для запроса по Осло выведите долю его документов от всех.

<details><summary>Подсказка</summary>

`global`.

</details>

<details><summary>Решение</summary>

```python
r = client.search(index="sales", body={"size": 0, "query": {"term": {"city": "Oslo"}}, "aggs": {"all": {"global": {}}}})
print(round(r["hits"]["total"]["value"] / r["aggregations"]["all"]["doc_count"], 2))
```

</details>
