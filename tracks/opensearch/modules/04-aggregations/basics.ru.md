---
title: "Агрегации: базовая теория"
lang: ru
track: opensearch
kind: module
doc: basics
slug: 04-aggregations
part: 1
tags: [aggregations, buckets, metrics, cardinality, composite-aggregation]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Агрегации: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [overview](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Данные для примеров

```python
from opensearchpy import helpers

client.indices.create(index="sales", body={"settings": {"number_of_shards": 1, "number_of_replicas": 0},
    "mappings": {"properties": {"city": {"type": "keyword"}, "product": {"type": "keyword"},
        "price": {"type": "double"}, "qty": {"type": "integer"}, "day": {"type": "date"},
        "tags": {"type": "keyword"}}}})
rows = [("Riga", "mug", 10, 2, "2026-01-05", ["a", "b"]), ("Riga", "lamp", 30, 1, "2026-01-20", ["a"]),
        ("Riga", "mug", 10, 5, "2026-03-02", []), ("Oslo", "mug", 12, 1, "2026-01-11", ["b"]),
        ("Oslo", "desk", 100, 1, "2026-03-15", ["c"]), ("Oslo", "lamp", 35, 2, "2026-03-16", []),
        ("Bern", "desk", 120, 1, "2026-03-30", ["a"])]
fields = ("city", "product", "price", "qty", "day", "tags")
helpers.bulk(client, [{"_index": "sales", "_id": str(i), **dict(zip(fields, row))}
                      for i, row in enumerate(rows, 1)], refresh=True)

def agg(aggs, query=None):
    body = {"size": 0, "aggs": aggs}
    if query:
        body["query"] = query
    return client.search(index="sales", body=body)["aggregations"]
```

`size: 0` отключает выдачу самих документов: нужны только сводки.

## Метрики

```python
agg({"s": {"stats": {"field": "price"}}})
# count 7, min 10.0, max 120.0, avg 45.29, sum 317.0
agg({"u": {"cardinality": {"field": "city"}}})                          # 3
agg({"p": {"percentiles": {"field": "price", "percents": [50, 95]}}})    # 50: 30.0, 95: 120.0
```

Также `avg`, `min`, `max`, `sum`, `value_count`, `extended_stats`.

## Корзины

```python
agg({"c": {"terms": {"field": "city"}}})            # Oslo 3, Riga 3, Bern 1
agg({"c": {"terms": {"field": "city", "size": 1}}}) # Oslo 3, sum_other_doc_count 4
agg({"h": {"histogram": {"field": "price", "interval": 50}}})   # 0: 5, 50: 0, 100: 2
agg({"r": {"range": {"field": "price", "ranges": [{"to": 30}, {"from": 30, "to": 100}, {"from": 100}]}}})
# *-30: 3, 30-100: 2, 100-*: 2     ("from" is included, "to" is not)
agg({"h": {"date_histogram": {"field": "day", "calendar_interval": "month", "format": "yyyy-MM"}}})
# 2026-01: 3, 2026-02: 0, 2026-03: 4
```

- `terms` по умолчанию возвращает 10 самых частых значений; остальные суммируются в `sum_other_doc_count`.
- Документ с массивом попадает в несколько корзин, поэтому сумма по корзинам может превышать число документов.

## Вложенные агрегации

```python
agg({"c": {"terms": {"field": "city", "order": {"rev": "desc"}}, "aggs": {
    "rev": {"sum": {"script": "doc['price'].value * doc['qty'].value"}},
    "top": {"terms": {"field": "product", "size": 1}}}}})
# Oslo: rev 182, top desk · Bern: rev 120, top desk · Riga: rev 100, top mug
```

## Область действия

```python
riga = {"term": {"city": "Riga"}}
agg({"a": {"avg": {"field": "price"}},
     "all": {"global": {}, "aggs": {"a": {"avg": {"field": "price"}}}}}, riga)
# a: 16.67 (Riga only)     all.a: 45.29 (ignores the query)
agg({"in_riga": {"filter": riga, "aggs": {"a": {"avg": {"field": "price"}}}}})   # 3 docs, avg 16.67
agg({"f": {"filters": {"filters": {"cheap": {"range": {"price": {"lt": 30}}}, "riga": riga}}}})
# cheap 3, riga 3
agg({"t": {"terms": {"field": "tags", "missing": "none"}}})   # a 3, b 2, none 2, c 1
agg({"m": {"missing": {"field": "tags"}}})                    # 2
```

Пустой массив считается отсутствующим значением.

## Частые ошибки новичков

- Агрегация по `text`: ошибка 400; нужно `keyword`-поле.
- Считать, что `terms` вернул все значения.
- Ожидать, что `post_filter` сузит агрегации (он применяется после них).

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [03 · Поисковые запросы](../03-search-queries/basics.ru.md)

## Источники

- [Aggregations](https://docs.opensearch.org/latest/aggregations/)
- [Terms aggregation](https://docs.opensearch.org/latest/aggregations/bucket/terms/)
- [Date histogram](https://docs.opensearch.org/latest/aggregations/bucket/date-histogram/)
