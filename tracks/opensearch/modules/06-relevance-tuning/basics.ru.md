---
title: "Релевантность: базовая теория"
lang: ru
track: opensearch
kind: module
doc: basics
slug: 06-relevance-tuning
part: 2
tags: [relevance, bm25, boosting, function-score, synonyms]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Релевантность: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [overview](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Данные для примеров

```python
from opensearchpy import helpers

client.indices.create(index="art", body={
    "settings": {"number_of_shards": 1, "number_of_replicas": 0, "analysis": {
        "filter": {"syn": {"type": "synonym_graph", "synonyms": ["laptop, notebook computer", "mice => mouse"]}},
        "analyzer": {"search_syn": {"type": "custom", "tokenizer": "standard", "filter": ["lowercase", "syn"]}}}},
    "mappings": {"properties": {"title": {"type": "text", "search_analyzer": "search_syn"},
        "body": {"type": "text"}, "views": {"type": "integer"}, "published": {"type": "date"}}}})
rows = [
    ("1", "Search engines", "search search search", 10, "2026-11-01"),
    ("2", "Search", "a very long article about many things where search appears just once among lots of other words here", 1000, "2026-06-01"),
    ("3", "Cooking", "search for the best recipe", 5000, "2025-01-01"),
    ("4", "Laptop review", "the laptop is fine", 50, "2026-11-20"),
    ("5", "Notebook computer deals", "cheap notebook computer", 70, "2026-10-01"),
    ("6", "Paper notebook", "a notebook for writing", 20, "2026-10-01"),
    ("7", "Mouse guide", "choose a mouse", 30, "2026-09-01"),
]
fields = ("title", "body", "views", "published")
helpers.bulk(client, [{"_index": "art", "_id": i, **dict(zip(fields, rest))} for i, *rest in rows], refresh=True)

def search(query, **extra):
    r = client.search(index="art", body={"query": query, **extra})
    return [(h["_id"], round(h["_score"], 3)) for h in r["hits"]["hits"]]

body_q = {"match": {"body": "search"}}
```

## BM25

```python
search(body_q)      # [('1', 0.657), ('3', 0.396), ('2', 0.2)]
```

Документ 1 первый: слово встречается трижды в коротком поле. Документ 2 последний: одно вхождение в длинном поле. Три фактора: редкость терма (idf), частота с насыщением и длина поля относительно средней.

## Несколько полей

```python
base = {"query": "search", "fields": ["title", "body"]}
search({"multi_match": base})                              # [('1', 0.657), ('2', 0.652), ('3', 0.396)]  best_fields
search({"multi_match": {**base, "type": "most_fields"}})   # [('1', 1.17), ('2', 0.852), ('3', 0.396)]
search({"multi_match": {**base, "tie_breaker": 0.3}})      # [('1', 0.811), ('2', 0.712), ('3', 0.396)]
search({"multi_match": {**base, "fields": ["title^5", "body"]}})  # [('2', 3.259), ('1', 2.563), ('3', 0.396)]
```

| `type` | Как складывает оценки |
|---|---|
| `best_fields` (по умолчанию) | берёт лучшее поле; `tie_breaker` добавляет долю остальных |
| `most_fields` | суммирует поля |
| `cross_fields` | рассматривает поля как одно |

`title^5` умножает вклад заголовка на 5 — и порядок первых двух документов меняется.

## Повысить или понизить

```python
search({"bool": {"must": body_q, "should": {"range": {"published": {"gte": "2026-10-01", "boost": 2}}}}})
# [('1', 2.657), ('3', 0.396), ('2', 0.2)]     fresh documents get a bonus
search({"boosting": {"positive": body_q, "negative": {"match": {"body": "recipe"}}, "negative_boost": 0.1}})
# [('1', 0.657), ('2', 0.2), ('3', 0.04)]      pushed down, not removed
```

## Частые ошибки новичков

- Сравнивать `_score` разных запросов: оценка не нормирована.
- Исключать нежелательное через `must_not`, когда нужно лишь понизить.
- Подбирать веса по одному запросу.

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [03 · Поисковые запросы](../03-search-queries/expert.ru.md)

## Источники

- [Multi-match query](https://docs.opensearch.org/latest/query-dsl/full-text/multi-match/)
- [Boosting query](https://docs.opensearch.org/latest/query-dsl/compound/boosting/)
