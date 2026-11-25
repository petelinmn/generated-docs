---
title: "Exercises: relevance"
lang: en
track: opensearch
kind: module
doc: exercises
slug: 06-relevance-tuning
part: 2
tags: [relevance, bm25, boosting, function-score, synonyms]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Exercises: relevance

🌐 [Русский](exercises.ru.md) · **English**

> You need the data and the `search` function from the [fundamentals](basics.en.md).

## 1. Title matters more

Search `search` over two fields with the title weighing three times as much.

<details><summary>Hint</summary>

`fields` with `^`.

</details>

<details><summary>Solution</summary>

```python
print(search({"multi_match": {"query": "search", "fields": ["title^3", "body"]}}))
```

</details>

## 2. Gentle popularity

Add a small view-count bonus to the text score.

<details><summary>Hint</summary>

`log1p`, `boost_mode: sum`, `factor`.

</details>

<details><summary>Solution</summary>

```python
print(search({"function_score": {"query": body_q, "boost_mode": "sum",
    "field_value_factor": {"field": "views", "modifier": "log1p", "factor": 0.001}}}))
```

</details>

## 3. Recency without zeroing

Raise recent articles without erasing the relevance of old ones.

<details><summary>Hint</summary>

Two functions: a constant 1 and a `gauss`, `score_mode: sum`.

</details>

<details><summary>Solution</summary>

```python
print(search({"function_score": {"query": body_q, "score_mode": "sum", "functions": [
    {"weight": 1},
    {"gauss": {"published": {"origin": "2026-11-25", "scale": "180d"}}}]}}))
```

</details>

## 4. A new synonym

The query `tv` must find "Television stand". Do it with a search analyzer.

<details><summary>Hint</summary>

A new index with `synonym_graph`.

</details>

<details><summary>Solution</summary>

```python
client.indices.create(index="art-tv", body={
    "settings": {"number_of_replicas": 0, "analysis": {
        "filter": {"s": {"type": "synonym_graph", "synonyms": ["tv, television"]}},
        "analyzer": {"q": {"type": "custom", "tokenizer": "standard", "filter": ["lowercase", "s"]}}}},
    "mappings": {"properties": {"title": {"type": "text", "search_analyzer": "q"}}}})
client.index(index="art-tv", id="1", body={"title": "Television stand"}, params={"refresh": "true"})
print(client.search(index="art-tv", body={"query": {"match": {"title": "tv"}}})["hits"]["total"]["value"])
```

</details>

## 5. A metric

Compute precision@2 for the query `search` on `body`, with documents 1 and 2 relevant.

<details><summary>Hint</summary>

`_rank_eval`.

</details>

<details><summary>Solution</summary>

```python
r = client.transport.perform_request("POST", "/art/_rank_eval", body={
    "requests": [{"id": "q", "request": {"query": body_q}, "ratings": [
        {"_index": "art", "_id": "1", "rating": 1}, {"_index": "art", "_id": "2", "rating": 1}]}],
    "metric": {"precision": {"k": 2, "relevant_rating_threshold": 1}}})
print(r["metric_score"])
```

</details>
