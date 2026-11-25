---
title: "Векторный поиск: базовая теория"
lang: ru
track: opensearch
kind: module
doc: basics
slug: 09-vector-search
part: 2
tags: [vector-search, knn, embeddings, hybrid-search, hnsw]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Векторный поиск: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [overview](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Идея

Полнотекстовый поиск ищет совпадающие слова. Векторный — близкий смысл: модель превращает текст или картинку в массив чисел (эмбеддинг, embedding), и похожие объекты оказываются рядом. OpenSearch хранит такие векторы и находит ближайшие (k-NN, k nearest neighbours).

Здесь векторы из трёх чисел придуманы руками, чтобы результат был предсказуем и не требовал модели. Настоящие эмбеддинги — сотни и тысячи измерений от обученной модели.

## Индекс

Нужны настройка `index.knn` и поле `knn_vector` с точной размерностью.

```python
from opensearchpy import OpenSearch
from opensearchpy.exceptions import RequestError

client = OpenSearch(hosts=[{"host": "localhost", "port": 9200}])

docs = [  # id, title, category, price, toy vector: [peripherals-ness, furniture-ness, food-ness]
    ("1", "wireless mouse", "peripherals", 20, [0.9, 0.1, 0.0]),
    ("2", "wired keyboard", "peripherals", 35, [0.7, 0.3, 0.0]),
    ("3", "office chair", "furniture", 120, [0.0, 0.9, 0.1]),
    ("4", "standing desk", "furniture", 300, [0.1, 0.8, 0.2]),
    ("5", "coffee beans", "food", 15, [0.0, 0.1, 0.9]),
    ("6", "mouse pad", "peripherals", 8, [0.8, 0.2, 0.1]),
]

def make(name, method=None):
    field = {"type": "knn_vector", "dimension": 3}
    if method:
        field["method"] = method
    client.indices.create(index=name, body={
        "settings": {"index.knn": True, "number_of_shards": 1, "number_of_replicas": 0},
        "mappings": {"properties": {"title": {"type": "text"}, "category": {"type": "keyword"},
                                    "price": {"type": "integer"}, "v": field}}})
    for i, title, category, price, v in docs:
        client.index(index=name, id=i, body={"title": title, "category": category, "price": price, "v": v})
    client.indices.refresh(index=name)

make("items")

def search(body, index="items", **params):
    r = client.search(index=index, body=body, params=params)
    return [(h["_id"], round(h["_score"], 4)) for h in r["hits"]["hits"]]

Q = [0.85, 0.15, 0.0]          # "something for the computer"

def knn(vector=Q, **kw):
    return {"query": {"knn": {"v": {"vector": vector, **kw}}}}
```

## Поиск ближайших

```python
search(knn(k=3))                     # [('1', 0.995), ('6', 0.9852), ('2', 0.9569)]
search({"size": 2, **knn(k=5)})      # [('1', 0.995), ('6', 0.9852)]
```

`k` — сколько соседей искать, `size` — сколько вернуть. Оценка для метрики по умолчанию (`l2`) равна `1 / (1 + расстояние²)`: чем ближе, тем выше, максимум 1.

## Размерность строгая

```python
try:
    search(knn([1, 0], k=2))
except RequestError as e:
    print(e.error)                   # search_phase_execution_exception: Dimension should be: 3
```

И документ, и запрос должны содержать ровно столько чисел, сколько в mapping. Векторы запроса и документов должны быть от **одной и той же модели**.

## Приближённый поиск

Сравнивать запрос со всеми векторами дорого, поэтому по умолчанию строится граф HNSW: поиск быстрый, но приближённый — изредка пропускает настоящего соседа.

## Частые ошибки новичков

- Забыть `index.knn: true`: запрос `knn` отклоняется (`not built for ANN search`).
- Сменить модель, не переиндексировав документы.
- Читать оценку как вероятность или сравнивать её между метриками.

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [06 · Релевантность](../06-relevance-tuning/basics.ru.md)

## Источники

- [Vector search](https://docs.opensearch.org/latest/vector-search/)
- [k-NN vector field type](https://docs.opensearch.org/latest/mappings/supported-field-types/knn-vector/)
