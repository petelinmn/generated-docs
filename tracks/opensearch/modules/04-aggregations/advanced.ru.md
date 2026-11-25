---
title: "Агрегации: продвинутая теория"
lang: ru
track: opensearch
kind: module
doc: advanced
slug: 04-aggregations
part: 1
tags: [aggregations, buckets, metrics, cardinality, composite-aggregation]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Агрегации: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

Данные и функция `agg` — из [базовой теории](basics.ru.md).

## Конвейерные агрегации

Считаются по результатам других агрегаций, путь к ним задаёт `buckets_path`.

```python
months = {"date_histogram": {"field": "day", "calendar_interval": "month", "format": "yyyy-MM"}, "aggs": {
    "q": {"sum": {"field": "qty"}},
    "cum": {"cumulative_sum": {"buckets_path": "q"}},
    "d": {"derivative": {"buckets_path": "q", "gap_policy": "insert_zeros"}}}}
agg({"h": months, "best": {"max_bucket": {"buckets_path": "h>q"}}})
# q: 4, 0, 9     cum: 4, 4, 13     d: -, -4, 9     best: 9.0 (2026-03)
agg({"c": {"terms": {"field": "city"}, "aggs": {
    "q": {"sum": {"field": "qty"}},
    "big": {"bucket_selector": {"buckets_path": {"q": "q"}, "script": "params.q > 3"}}}}})
# Oslo (q 4) and Riga (q 8); Bern is dropped
```

`gap_policy` по умолчанию — `skip`: в эксперименте при пустом феврале с `skip` производная не получила значения ни для февраля, ни для марта.

## Лучший документ в группе: `top_hits`

```python
r = agg({"c": {"terms": {"field": "city"}, "aggs": {
    "top": {"top_hits": {"size": 1, "sort": [{"price": "desc"}], "_source": ["product", "price"]}}}}})
# Oslo: desk 100 · Riga: lamp 30 · Bern: desk 120
```

## Обход всех корзин: `composite`

`terms` отдаёт только верхние значения, а `composite` листает все сочетания по `after_key`.

```python
sources = [{"city": {"terms": {"field": "city"}}}, {"product": {"terms": {"field": "product"}}}]
after, pages = None, []
while True:
    comp = {"size": 3, "sources": sources}
    if after:
        comp["after"] = after
    res = agg({"c": {"composite": comp}})["c"]
    if not res["buckets"]:
        break
    pages.append([(b["key"]["city"], b["key"]["product"], b["doc_count"]) for b in res["buckets"]])
    after = res["after_key"]
```

## Точность `terms`

Каждый шард присылает свои лучшие `shard_size` значений, координатор их объединяет. На 5 шардах и 3000 документах с 60 значениями (случайные данные с фиксированным seed):

| Параметры | Топ-5 | `doc_count_error_upper_bound` |
|---|---|---|
| точный подсчёт | k00 230, k01 147, k02 116, k03 98, k05 92 | — |
| `size 5, shard_size 5` | k00 224, k01 137, k02 81, k05 57, k03 44 | 110 |
| `size 5` (по умолчанию) | k00 230, k01 145, k02 109, k03 98, k04 86 | 63 |
| `size 5, shard_size 100` | совпадает с точным | 0 |

При настройках по умолчанию в пятёрку попало `k04` вместо `k05`. `show_term_doc_count_error: true` показывает оценку ошибки по каждой корзине.

## Точность `cardinality`

50 000 различных значений на одном шарде:

| `precision_threshold` | Результат |
|---|---|
| 100 | 51394 |
| 3000 | 50741 |
| 40000 | 49929 |
| по умолчанию | 50741 |

## Частые ошибки

- Выдавать приближённый топ за точный.
- Задавать огромный `size` у `terms` вместо `composite`.
- Сортировать `terms` по возрастанию счётчика: самые редкие значения на разных шардах различаются, ошибка не ограничена.

## Связанные материалы

- [Базовая теория](basics.ru.md) · [Эксперт](expert.ru.md)
- [01 · Основы](../01-opensearch-basics/advanced.ru.md) — маршрутизация и шарды
- [06 · Релевантность](../06-relevance-tuning/overview.ru.md)

## Источники

- [Pipeline aggregations](https://docs.opensearch.org/latest/aggregations/pipeline/)
- [Composite aggregation](https://docs.opensearch.org/latest/aggregations/bucket/composite/)
- [Cardinality](https://docs.opensearch.org/latest/aggregations/metric/cardinality/)
