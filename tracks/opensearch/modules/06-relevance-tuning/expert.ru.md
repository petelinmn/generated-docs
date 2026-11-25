---
title: "Релевантность: экспертная теория"
lang: ru
track: opensearch
kind: module
doc: expert
slug: 06-relevance-tuning
part: 2
tags: [relevance, bm25, boosting, function-score, synonyms]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Релевантность: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · [2. Продвинутый](advanced.ru.md) · **3. Эксперт** · [4. Квиз](quiz.ru.md)

## Разбор оценки

Формула из ответа `_explain` (для запроса `match` по `body`):

`score = boost * idf * tf`, `idf = log(1 + (N - n + 0.5) / (n + 0.5))`, `tf = freq / (freq + k1 * (1 - b + b * dl / avgdl))`

```text
document 1:  idf 0.8267 (n=3, N=7)   tf 0.7952 (freq 3, dl 3,  avgdl 5.71)   score 0.6574
document 2:  idf 0.8267 (n=3, N=7)   tf 0.2418 (freq 1, dl 18, avgdl 5.71)   score 0.1999
```

## Параметры сходства

По умолчанию `k1 = 1.2`, `b = 0.75`. Их можно задать для поля:

```python
client.indices.create(index="art2", body={
    "settings": {"number_of_shards": 1, "number_of_replicas": 0, "similarity": {"nolen": {"type": "BM25", "b": 0}}},
    "mappings": {"properties": {"body": {"type": "text", "similarity": "nolen"}}}})
# the same seven bodies, the same match query:
# default: [('1', 0.657), ('3', 0.396), ('2', 0.2)]
# b = 0:   [('1', 0.59), ('2', 0.376), ('3', 0.376)]
```

## Статистика шардов

По умолчанию (`query_then_fetch`) каждый шард использует свои idf. В индексе из трёх шардов документы `0` и `1` с одинаковым текстом получили 1.133 и 1.032; с `search_type=dfs_query_then_fetch` — оба 1.109. Это стоит дополнительного обхода шардов.

## Измерение качества

```python
r = client.transport.perform_request("POST", "/art/_rank_eval", body={
    "requests": [{"id": "q1", "request": {"query": body_q},
                  "ratings": [{"_index": "art", "_id": "2", "rating": 1}]}],
    "metric": {"mean_reciprocal_rank": {"k": 3}}})
print(r["metric_score"])     # 0.333...: the relevant document is third
```

С оценками «релевантен только документ 2» метрика precision@3 тоже равна 0.333.

## Инструменты

| Инструмент | Для чего | Проверено |
|---|---|---|
| `_explain`, `"explain": true` | расчёт оценки; для несовпавшего — `no matching term` | да |
| `_name` | какие части запроса совпали | да |
| `_rank_eval` | метрики по оценённым запросам | да |
| `_analyze` | термы и позиции | да |
| `rank_feature` | эффективный учёт числового сигнала; нужно поле типа `rank_feature` | только отказ на `integer` |
| Learning to Rank | модель ранжирования плагином | ⚠️ |
| Search pipelines | обработка запроса и результата на сервере | ⚠️ |

> ⚠️ Проверить: Learning to Rank и search pipelines упомянуты по документации и не запускались.

## Подвохи

1. Сырой `field_value_factor` дал оценку 1980: текст перестал влиять на порядок.
2. `gauss` со `scale: 30d` практически обнулил оценки старых документов — а умножение на ноль стирает текстовую релевантность.
3. Скрипт с отрицательным результатом даёт ошибку 400.
4. `rescore` видит только окно: плохо отобранный кандидат не поднимется.
5. Синоним `laptop` поднял «Notebook computer deals» выше «Laptop review»: расширение запроса меняет не только полноту, но и порядок.

## Вопросы с собеседований

1. Из чего состоит BM25?
<details><summary>Ответ</summary>

idf, насыщающаяся частота терма (`k1`), нормировка по длине (`b`).
</details>

2. Что даёт `b = 0`?
<details><summary>Ответ</summary>

Отключает влияние длины поля.
</details>

3. `best_fields` против `most_fields`?
<details><summary>Ответ</summary>

Первый берёт максимум по полям, второй суммирует.
</details>

4. Зачем `tie_breaker`?
<details><summary>Ответ</summary>

Чтобы совпадения в остальных полях тоже что-то значили.
</details>

5. Как учесть популярность, не заглушив текст?
<details><summary>Ответ</summary>

Логарифм или насыщающая функция, сложение вместо умножения, ограничение вклада.
</details>

6. Что такое `rescore`?
<details><summary>Ответ</summary>

Переоценка верхних документов каждого шарда дорогим запросом.
</details>

7. Синонимы при индексации или при поиске?
<details><summary>Ответ</summary>

При поиске: не нужна переиндексация, а многословные корректно обрабатывает `synonym_graph`.
</details>

8. Почему на маленьком индексе оценки странные?
<details><summary>Ответ</summary>

Статистика по шардам; помогает `dfs_query_then_fetch` или один шард.
</details>

9. Как измерить качество поиска?
<details><summary>Ответ</summary>

Набор запросов с оценками и метрики precision@k, MRR, DCG через `_rank_eval`; в продакшене — клики и эксперименты.
</details>

10. Что такое named queries?
<details><summary>Ответ</summary>

`_name` у части запроса; в ответе `matched_queries` показывает, что совпало.
</details>

11. Можно ли сравнивать `_score` разных запросов?
<details><summary>Ответ</summary>

Нет, оценка не нормирована.
</details>

12. Что проверить, если нужный документ внизу?
<details><summary>Ответ</summary>

`explain`, термы анализатора, длину поля, веса и функции.
</details>

## Связанные материалы

- [Продвинутая теория](advanced.ru.md) · [Квиз](quiz.ru.md)
- [04 · Агрегации](../04-aggregations/basics.ru.md)

## Источники

- [Similarity](https://docs.opensearch.org/latest/field-types/mapping-parameters/similarity/)
- [Ranking evaluation API](https://docs.opensearch.org/latest/api-reference/search-apis/rank-eval/)
- [Explain API](https://docs.opensearch.org/latest/api-reference/search-apis/explain/)
