---
title: "Векторный поиск: продвинутая теория"
lang: ru
track: opensearch
kind: module
doc: advanced
slug: 09-vector-search
part: 2
tags: [vector-search, knn, embeddings, hybrid-search, hnsw]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Векторный поиск: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

Данные, `search` и `knn` — из [базовой теории](basics.ru.md).

## Фильтрация

```python
furniture = {"term": {"category": "furniture"}}
search(knn(k=2, filter=furniture))                       # [('4', 0.4938), ('3', 0.4357)]
search({**knn(k=2), "post_filter": furniture})           # []
search({"query": {"bool": {"must": knn(k=2)["query"], "filter": furniture}}})    # []
```

Фильтр **внутри** `knn` ищет соседей среди подходящих. `post_filter` и `bool.filter` снаружи отсеивают уже найденные `k` соседей — здесь это две периферии, и не остаётся ничего.

## Порог вместо k

```python
search(knn(max_distance=0.05))       # [('1', 0.995), ('6', 0.9852), ('2', 0.9569)]
search(knn(min_score=0.9))           # the same three
```

Задаётся ровно одно из `k`, `max_distance`, `min_score`. Для `l2` расстояние здесь — квадрат евклидова.

## Точный поиск

```python
exact = {"script_score": {"query": furniture, "script": {"lang": "knn", "source": "knn_score",
         "params": {"field": "v", "query_value": Q, "space_type": "l2"}}}}
search({"query": exact})             # [('4', 0.4938), ('3', 0.4357)]
```

Перебирает все документы, прошедшие запрос: точно, но стоимость растёт с их числом. Годится для небольших наборов и для проверки полноты (recall) приближённого поиска. Работает и без `index.knn`.

## Метрики

```python
for space in ("l2", "cosinesimil", "innerproduct"):
    make("s-" + space, {"name": "hnsw", "space_type": space, "engine": "faiss"})

search(knn(k=3), "s-l2")                       # [('1', 0.995), ('6', 0.9852), ('2', 0.9569)]
search(knn(k=3), "s-cosinesimil")              # [('1', 0.999), ('6', 0.9951), ('2', 0.9868)]
search(knn(k=3), "s-innerproduct")             # [('1', 1.78), ('6', 1.71), ('2', 1.64)]
long = [8.5, 1.5, 0]                           # the same direction, ten times longer
search(knn(long, k=3), "s-l2")                 # [('1', 0.0165), ('6', 0.0161), ('2', 0.0158)]
search(knn(long, k=3), "s-cosinesimil")        # [('1', 0.999), ('6', 0.9951), ('2', 0.9868)]
search(knn(long, k=3), "s-innerproduct")       # [('1', 8.8), ('6', 8.1), ('2', 7.4)]
```

`cosinesimil` не зависит от длины вектора, `l2` и `innerproduct` зависят. Берите ту метрику, на которую рассчитана модель.

## Гибридный поиск

Оценки BM25 и k-NN живут в разных шкалах, поэтому `bool.should` просто смешает числа. Запрос `hybrid` вместе с конвейером поиска (search pipeline) приводит их к общей шкале.

```python
text = {"match": {"title": "mouse"}}
vector = {"knn": {"v": {"vector": [0.0, 1.0, 0.0], "k": 3}}}      # "furniture"
search({"query": text})              # [('1', 0.468), ('6', 0.468)]
search({"query": vector})            # [('3', 0.9804), ('4', 0.9174), ('2', 0.5051)]

def pipeline(name, processor):
    client.transport.perform_request("PUT", "/_search/pipeline/" + name,
                                     body={"phase_results_processors": [processor]})

def norm(text_weight):
    return {"normalization-processor": {"normalization": {"technique": "min_max"},
            "combination": {"technique": "arithmetic_mean",
                            "parameters": {"weights": [text_weight, round(1 - text_weight, 2)]}}}}

pipeline("vector-heavy", norm(0.3))
pipeline("text-heavy", norm(0.7))
pipeline("rrf", {"score-ranker-processor": {"combination": {"technique": "rrf"}}})

hybrid = {"query": {"hybrid": {"queries": [text, vector]}}}
search(hybrid, search_pipeline="vector-heavy")  # [('3', 0.7), ('4', 0.6073), ('1', 0.3), ('6', 0.3), ('2', 0.0007)]
search(hybrid, search_pipeline="text-heavy")    # [('1', 0.7), ('6', 0.7), ('3', 0.3), ('4', 0.2603), ('2', 0.0003)]
search(hybrid, search_pipeline="rrf")           # [('1', 0.0164), ('3', 0.0164), ('4', 0.0161), ('6', 0.0161), ('2', 0.0159)]
```

- **Нормализация** (`min_max`) с весами: веса идут в порядке подзапросов и в сумме дают 1.
- **RRF** (reciprocal rank fusion): складывает `1 / (rank_constant + позиция)`, оценки не нужны; `rank_constant` по умолчанию 60.
- Конвейер можно сделать умолчанием индекса: `index.search.default_pipeline`.

## Частые ошибки

- Выполнить `hybrid` без конвейера: в ответе служебные значения.
- Фильтровать снаружи и получать меньше `k`.
- Подбирать веса на глаз вместо набора оценённых запросов.

## Связанные материалы

- [Базовая теория](basics.ru.md) · [Эксперт](expert.ru.md)
- [06 · Релевантность: измерение качества](../06-relevance-tuning/expert.ru.md)

## Источники

- [Filtering in k-NN search](https://docs.opensearch.org/latest/vector-search/filter-search-knn/index/)
- [Exact k-NN with a scoring script](https://docs.opensearch.org/latest/vector-search/vector-search-techniques/knn-score-script/)
- [Hybrid search](https://docs.opensearch.org/latest/vector-search/ai-search/hybrid-search/index/)
- [Score ranker processor](https://docs.opensearch.org/latest/search-plugins/search-pipelines/score-ranker-processor/)
