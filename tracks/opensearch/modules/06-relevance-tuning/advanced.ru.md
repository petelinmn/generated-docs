---
title: "Релевантность: продвинутая теория"
lang: ru
track: opensearch
kind: module
doc: advanced
slug: 06-relevance-tuning
part: 2
tags: [relevance, bm25, boosting, function-score, synonyms]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Релевантность: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

Данные и функция `search` — из [базовой теории](basics.ru.md).

## `function_score`

```python
fvf = lambda **f: {"function_score": {"query": body_q, "field_value_factor": {"field": "views", **f}}}
search(fvf())                      # [('3', 1980.068), ('2', 199.922), ('1', 6.574)]
search(fvf(modifier="log1p"))      # [('3', 1.465), ('1', 0.685), ('2', 0.6)]
search({"function_score": {"query": body_q, "boost_mode": "sum",
                           "field_value_factor": {"field": "views", "modifier": "log1p"}}})
# [('3', 4.095), ('2', 3.2), ('1', 1.699)]
```

Без модификатора число просмотров стало множителем и полностью определило порядок. `log1p` сглаживает; `boost_mode` решает, как объединить функцию с оценкой запроса (по умолчанию умножение).

## Затухание и свой расчёт

```python
search({"function_score": {"query": body_q, "functions": [
    {"gauss": {"published": {"origin": "2026-11-25", "scale": "30d", "decay": 0.5}}}]}})
# [('1', 0.422), ('2', 0.0), ('3', 0.0)]    old documents are almost zeroed
search({"script_score": {"query": body_q, "script": {"source": "_score * Math.log(2 + doc['views'].value)"}}})
# [('3', 3.373), ('1', 1.634), ('2', 1.381)]
search({"script_score": {"query": body_q, "script": {"source": "-1"}}})      # error 400
```

Отрицательная оценка запрещена. Фиксируйте `origin` в тестах: с `now` результат меняется со временем.

## `rescore`

Дорогой запрос применяется только к верхним `window_size` документам каждого шарда.

```python
search(body_q, rescore={"window_size": 2, "query": {
    "rescore_query": {"match_phrase": {"body": "best recipe"}},
    "query_weight": 1, "rescore_query_weight": 10}})
# [('3', 16.434), ('1', 0.657), ('2', 0.2)]
```

Документ 3 был вторым, то есть попал в окно из двух, и поднялся на первое место. Документ вне окна переоценён не был бы.

## Синонимы

```python
search({"match": {"title": "laptop"}})              # [('5', 1.03), ('4', 0.738)]
search({"match": {"title": "notebook computer"}})   # [('5', 1.03), ('4', 0.738)]   not "Paper notebook"
search({"match": {"title": "notebook"}})            # [('6', 0.513), ('5', 0.422)]
search({"match": {"title": "mice"}})                # [('7', 0.738)]
```

- `a, b` — равнозначные;
- `a => b` — замена слева направо;
- `synonym_graph` правильно обрабатывает многословные синонимы и предназначен для анализа запроса.

Синонимы в `search_analyzer` не требуют переиндексации документов.

## Связанные материалы

- [Базовая теория](basics.ru.md) · [Эксперт](expert.ru.md)
- [02 · Отображения и анализ](../02-mappings-and-analysis/advanced.ru.md)
- [09 · Векторный и гибридный поиск](../09-vector-search/overview.ru.md)

## Источники

- [Function score query](https://docs.opensearch.org/latest/query-dsl/compound/function-score/)
- [Script score query](https://docs.opensearch.org/latest/query-dsl/specialized/script-score/)
- [Synonym graph token filter](https://docs.opensearch.org/latest/analyzers/token-filters/synonym-graph/)
