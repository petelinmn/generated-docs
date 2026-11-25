---
title: "Поисковые запросы: продвинутая теория"
lang: ru
track: opensearch
kind: module
doc: advanced
slug: 03-search-queries
part: 1
tags: [query-dsl, full-text-search, bool-query, relevance, bm25, pagination]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Поисковые запросы: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

Данные и функция `search` — из [базовой теории](basics.ru.md).

## `should` и `minimum_should_match`

```python
mouse, wireless, keyboard = ({"match": {"name": w}} for w in ("mouse", "wireless", "keyboard"))
search(query={"bool": {"should": [mouse, keyboard]}})                  # ids 1, 2, 3, 5
search(query={"bool": {"filter": [{"term": {"category": "electronics"}}], "should": [mouse]}})
# [('1', 0.398), ('2', 0.0), ('3', 0.0)]    should only boosts here
search(query={"bool": {"should": [mouse, wireless, keyboard], "minimum_should_match": 2}})   # ids 1, 3
search(query={"constant_score": {"filter": {"term": {"category": "electronics"}}, "boost": 2}})  # score 2.0
```

## Глубокая постраничность: `search_after`

`from + size` ограничено `index.max_result_window` (10000). `search_after` продолжает с последнего значения сортировки; сортировка должна однозначно упорядочивать документы.

```python
r = client.search(index="shop", body={"sort": [{"price": "asc"}, {"_id": "asc"}], "size": 2})
last = r["hits"]["hits"][-1]["sort"]          # [9.5, '4']     ids 5, 4
search(sort=[{"price": "asc"}, {"_id": "asc"}], size=2, search_after=last)   # ids 1, 3
```

Сортировка по `_id` здесь для наглядности; на больших объёмах заведите отдельное `keyword`-поле с уникальным значением.

## Согласованный снимок: PIT

**Point in time** фиксирует состояние индекса на время обхода.

```python
pit = client.transport.perform_request("POST", "/shop/_search/point_in_time", params={"keep_alive": "1m"})
client.index(index="shop", id="6", body={"name": "New Mouse", "category": "electronics", "price": 1},
             params={"refresh": "true"})
r = client.search(body={"pit": {"id": pit["pit_id"], "keep_alive": "1m"}, "sort": [{"price": "asc"}]})
# ids 5, 4, 1, 3, 2  - the new document is not visible in the PIT
# a normal search returns 6, 5, 4, 1, 3, 2
client.transport.perform_request("DELETE", "/_search/point_in_time", body={"pit_id": [pit["pit_id"]]})
client.delete(index="shop", id="6", params={"refresh": "true"})
```

Старый способ — `scroll`; для постраничной выдачи пользователю он не предназначен, а контекст нужно закрывать.

## Строка запроса от пользователя

```python
search(query={"query_string": {"query": "category:electronics AND price:[20 TO 60]"}})   # ids 1, 3
search(query={"query_string": {"query": "name:(mouse"}})            # 400 query_shard_exception
search(query={"simple_query_string": {"query": "name:(mouse", "fields": ["name"]}})   # ids 1, 5, no error
```

`query_string` даёт полный синтаксис и падает на ошибке в нём; для ввода пользователя берите `match` или `simple_query_string`.

## `post_filter`: фасеты

`post_filter` отсекает результаты **после** агрегаций, поэтому счётчики категорий остаются полными.

```python
r = client.search(index="shop", body={"query": {"match": {"name": "mouse"}},
    "post_filter": {"term": {"category": "accessories"}},
    "aggs": {"c": {"terms": {"field": "category"}}}})
# hits: only id 5; buckets: accessories 1, electronics 1
```

## Количество совпадений

`hits.total` точен до 10000, дальше возвращается `"relation": "gte"`. Порог задаёт `track_total_hits`; при значении 2 ответ был `{"value": 2, "relation": "gte"}`. Нужно только число — используйте `_count`.

## Частые ошибки

- `should` рядом с `filter` без `minimum_should_match`, когда ожидалось обязательное совпадение.
- `wildcard` со звёздочкой в начале на больших индексах.
- `query_string` с вводом пользователя.
- Не закрывать PIT и `scroll`.

## Связанные материалы

- [Базовая теория](basics.ru.md) · [Эксперт](expert.ru.md)
- [01 · Основы](../01-opensearch-basics/expert.ru.md) — две фазы поиска
- [04 · Агрегации](../04-aggregations/overview.ru.md)
- [06 · Релевантность](../06-relevance-tuning/overview.ru.md)

## Источники

- [Paginate results](https://opensearch.org/docs/latest/search-plugins/searching-data/paginate/)
- [Point in time](https://opensearch.org/docs/latest/search-plugins/searching-data/point-in-time/)
- [Query string query](https://opensearch.org/docs/latest/query-dsl/full-text/query-string/)
