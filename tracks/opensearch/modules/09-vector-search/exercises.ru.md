---
title: "Упражнения: векторный поиск"
lang: ru
track: opensearch
kind: module
doc: exercises
slug: 09-vector-search
part: 2
tags: [vector-search, knn, embeddings, hybrid-search, hnsw]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Упражнения: векторный поиск

🌐 **Русский** · [English](exercises.en.md)

> Нужны данные, `search` и `knn` из [базовой теории](basics.ru.md).

## 1. Две вещи для еды

Найдите двух соседей вектора `[0, 0, 1]`.

<details><summary>Подсказка</summary>

`knn(vector, k=2)`.

</details>

<details><summary>Решение</summary>

```python
print(search(knn([0.0, 0.0, 1.0], k=2)))
```

</details>

## 2. Дешёвые соседи

Три ближайших к `Q` дешевле 30.

<details><summary>Подсказка</summary>

Фильтр внутри `knn`.

</details>

<details><summary>Решение</summary>

```python
print(search(knn(k=3, filter={"range": {"price": {"lt": 30}}})))
```

</details>

## 3. Сравнить с точным

Напишите функцию recall: доля точных топ-3 в приближённом топ-3.

<details><summary>Подсказка</summary>

`script_score` и множества.

</details>

<details><summary>Решение</summary>

```python
def recall(vector, k=3):
    exact = {"size": k, "query": {"script_score": {"query": {"match_all": {}}, "script": {
        "lang": "knn", "source": "knn_score",
        "params": {"field": "v", "query_value": vector, "space_type": "l2"}}}}}
    truth = {i for i, _ in search(exact)}
    found = {i for i, _ in search(knn(vector, k=k))}
    return len(truth & found) / len(truth)

print(recall(Q))
```

</details>

## 4. Поровну

Гибридный поиск с равными весами.

<details><summary>Подсказка</summary>

Конвейер с `[0.5, 0.5]`.

</details>

<details><summary>Решение</summary>

```python
client.transport.perform_request("PUT", "/_search/pipeline/half", body={"phase_results_processors": [
    {"normalization-processor": {"normalization": {"technique": "min_max"},
     "combination": {"technique": "arithmetic_mean", "parameters": {"weights": [0.5, 0.5]}}}}]})
print(search({"query": {"hybrid": {"queries": [
    {"match": {"title": "mouse"}}, {"knn": {"v": {"vector": [0.0, 1.0, 0.0], "k": 3}}}]}}},
    search_pipeline="half"))
```

</details>

## 5. Сколько памяти

Оцените память для 2 млн векторов из 768 измерений, `m = 16`, с одной репликой.

<details><summary>Подсказка</summary>

Формула из экспертного уровня, умножить на число копий.

</details>

<details><summary>Решение</summary>

```python
def hnsw_bytes(vectors, dimension, m=16, replicas=1):
    return 1.1 * (4 * dimension + 8 * m) * vectors * (1 + replicas)

print(round(hnsw_bytes(2_000_000, 768) / 1e9, 2), "GB")
```

</details>
