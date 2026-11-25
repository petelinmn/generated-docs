---
title: "Упражнения: релевантность"
lang: ru
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

# Упражнения: релевантность

🌐 **Русский** · [English](exercises.en.md)

> Нужны данные и функция `search` из [базовой теории](basics.ru.md).

## 1. Заголовок важнее

Найдите `search` по двум полям так, чтобы заголовок весил втрое.

<details><summary>Подсказка</summary>

`fields` с `^`.

</details>

<details><summary>Решение</summary>

```python
print(search({"multi_match": {"query": "search", "fields": ["title^3", "body"]}}))
```

</details>

## 2. Мягкая популярность

Добавьте к текстовой оценке небольшой бонус за просмотры.

<details><summary>Подсказка</summary>

`log1p`, `boost_mode: sum`, `factor`.

</details>

<details><summary>Решение</summary>

```python
print(search({"function_score": {"query": body_q, "boost_mode": "sum",
    "field_value_factor": {"field": "views", "modifier": "log1p", "factor": 0.001}}}))
```

</details>

## 3. Свежесть без обнуления

Поднимите свежие статьи, не стирая релевантность старых.

<details><summary>Подсказка</summary>

Две функции: константа 1 и `gauss`, `score_mode: sum`.

</details>

<details><summary>Решение</summary>

```python
print(search({"function_score": {"query": body_q, "score_mode": "sum", "functions": [
    {"weight": 1},
    {"gauss": {"published": {"origin": "2026-11-25", "scale": "180d"}}}]}}))
```

</details>

## 4. Новый синоним

Запрос `tv` должен находить «Television stand». Сделайте это поисковым анализатором.

<details><summary>Подсказка</summary>

Новый индекс с `synonym_graph`.

</details>

<details><summary>Решение</summary>

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

## 5. Метрика

Посчитайте precision@2 для запроса `search` по `body`, считая релевантными документы 1 и 2.

<details><summary>Подсказка</summary>

`_rank_eval`.

</details>

<details><summary>Решение</summary>

```python
r = client.transport.perform_request("POST", "/art/_rank_eval", body={
    "requests": [{"id": "q", "request": {"query": body_q}, "ratings": [
        {"_index": "art", "_id": "1", "rating": 1}, {"_index": "art", "_id": "2", "rating": 1}]}],
    "metric": {"precision": {"k": 2, "relevant_rating_threshold": 1}}})
print(r["metric_score"])
```

</details>
