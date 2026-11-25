---
title: "Exercises: aggregations"
lang: en
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

# Exercises: aggregations

🌐 [Русский](exercises.ru.md) · **English**

> You need the data and the `agg` function from the [fundamentals](basics.en.md).

## 1. Revenue by city

Count the units sold per city.

<details><summary>Hint</summary>

`terms` + `sum`.

</details>

<details><summary>Solution</summary>

```python
r = agg({"c": {"terms": {"field": "city"}, "aggs": {"q": {"sum": {"field": "qty"}}}}})
print({b["key"]: b["q"]["value"] for b in r["c"]["buckets"]})
```

</details>

## 2. Monthly sales

Print only non-empty months as `yyyy-MM`.

<details><summary>Hint</summary>

`min_doc_count`.

</details>

<details><summary>Solution</summary>

```python
r = agg({"m": {"date_histogram": {"field": "day", "calendar_interval": "month", "format": "yyyy-MM", "min_doc_count": 1}}})
print([(b["key_as_string"], b["doc_count"]) for b in r["m"]["buckets"]])
```

</details>

## 3. The most expensive item

For each product find the city with the highest price.

<details><summary>Hint</summary>

`top_hits`.

</details>

<details><summary>Solution</summary>

```python
r = agg({"p": {"terms": {"field": "product"}, "aggs": {"t": {"top_hits": {"size": 1, "sort": [{"price": "desc"}]}}}}})
print({b["key"]: b["t"]["hits"]["hits"][0]["_source"]["city"] for b in r["p"]["buckets"]})
```

</details>

## 4. All combinations

Export all city–product pairs in pages of 2.

<details><summary>Hint</summary>

`composite` and `after_key`.

</details>

<details><summary>Solution</summary>

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

## 5. A city's share

For a query on Oslo print its share of all documents.

<details><summary>Hint</summary>

`global`.

</details>

<details><summary>Solution</summary>

```python
r = client.search(index="sales", body={"size": 0, "query": {"term": {"city": "Oslo"}}, "aggs": {"all": {"global": {}}}})
print(round(r["hits"]["total"]["value"] / r["aggregations"]["all"]["doc_count"], 2))
```

</details>
