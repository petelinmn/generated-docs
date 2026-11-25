---
title: "Векторный поиск: экспертная теория"
lang: ru
track: opensearch
kind: module
doc: expert
slug: 09-vector-search
part: 2
tags: [vector-search, knn, embeddings, hybrid-search, hnsw]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Векторный поиск: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · [2. Продвинутый](advanced.ru.md) · **3. Эксперт** · [4. Квиз](quiz.ru.md)

Определения `search` и `hybrid` — из [базовой](basics.ru.md) и [продвинутой](advanced.ru.md) теории.

## Движки и параметры

| Параметр | По умолчанию | Значения и смысл |
|---|---|---|
| `engine` | `faiss` | `faiss`, `lucene`; `nmslib` для новых индексов запрещён с 3.0.0 |
| `method.name` | `hnsw` | `hnsw`, `ivf` (faiss, требует обучения) |
| `space_type` | `l2` | `l2`, `cosinesimil`, `innerproduct`, `l1`, `linf`, `hamming` — не все с каждым движком |
| `m` | 16 | связей на узел графа |
| `ef_construction` | 100 | точность построения |
| `ef_search` | 100 | точность поиска (faiss); lucene берёт `k` |
| `dimension` | — | 1–16 000 |
| `data_type` | `float` | `float`, `byte` (−128…127), `binary` |

## Память

Оценка документации для HNSW: `1.1 * (4 * dimension + 8 * m)` байт на вектор. Для миллиона векторов из 256 измерений при `m = 16` это около 1,27 ГБ, не считая реплик.

| Настройка | По умолчанию |
|---|---|
| `knn.memory.circuit_breaker.enabled` | `true` |
| `knn.memory.circuit_breaker.limit` | `50%` |
| `knn.algo_param.index_thread_qty` | `1` |

`GET /_plugins/_knn/stats` показывает `graph_memory_usage`, попадания и промахи кэша графов и `circuit_breaker_triggered`.

## Сжатие

Поле можно объявить с `"mode": "on_disk"` и `"compression_level": "32x"`: вектора квантуются, а кандидаты уточняются по полным значениям с диска.

> ⚠️ Проверить: здесь проверено только создание такого mapping, не качество и не расход памяти; `ivf`, PQ, бинарные векторы и lucene-специфичные ограничения не запускались.

## Эмбеддинги внутри кластера

Плагины ML Commons и neural search позволяют подключить модель: вектор вычисляется ingest-конвейером, а запрос `neural` принимает текст.

> ⚠️ Проверить: этот путь описан по документации и не запускался.

## Гибридный запрос без конвейера

```python
search(hybrid)       # no search pipeline
# [('1', -9549512000.0), ('1', -4422440400.0), ('1', 0.468), ('6', 0.468), ('1', -4422440400.0),
#  ('3', 0.9804), ('4', 0.9174), ('2', 0.5051), ('1', -9549512000.0)]
```

Огромные отрицательные числа — служебные разделители, которыми шарды отмечают границы списков подзапросов; обычно их убирает процессор нормализации или RRF. Всегда указывайте конвейер.

## Подвохи

1. `post_filter` и внешний `bool.filter` дали пустую выдачу там, где фильтр внутри `knn` нашёл два документа.
2. `k` и `min_score` вместе: ошибка `requires exactly one of k, distance or score`.
3. `hits.total` равен числу найденных соседей (при `size: 2`, `k: 5` — 5), а не числу «подходящих» документов.
4. У k-NN нет понятия «ничего не найдено»: ближайший найдётся всегда, даже совсем непохожий.
5. Веса, не дающие в сумме 1, отклоняются при создании конвейера.
6. `min_max` даёт худшему в списке почти ноль (документ `2`: 0.0007), хотя исходная оценка была 0.5.

## Вопросы с собеседований

1. Что такое эмбеддинг?
<details><summary>Ответ</summary>

Вектор от модели, в котором близость отражает похожесть объектов.
</details>

2. Точный и приближённый k-NN?
<details><summary>Ответ</summary>

Перебор всех против графа или кластеров: скорость в обмен на полноту.
</details>

3. Что такое HNSW?
<details><summary>Ответ</summary>

Многоуровневый граф соседей; `m` — связность, `ef_construction` и `ef_search` — ширина поиска при построении и запросе.
</details>

4. Как сэкономить память?
<details><summary>Ответ</summary>

Меньшая размерность, квантование (16 бит, байты, бинарное, PQ), режим `on_disk` — ценой точности или задержки.
</details>

5. Почему фильтр может вернуть меньше k?
<details><summary>Ответ</summary>

Если он применяется после поиска соседей.
</details>

6. Косинус или скалярное произведение?
<details><summary>Ответ</summary>

Косинус игнорирует длину; для нормированных векторов порядок совпадает.
</details>

7. Как объединить BM25 и векторы?
<details><summary>Ответ</summary>

`hybrid` с нормализацией и весами или RRF.
</details>

8. Когда RRF лучше нормализации?
<details><summary>Ответ</summary>

Когда распределения оценок несопоставимы или с выбросами и нечем подобрать веса.
</details>

9. Как измерить recall?
<details><summary>Ответ</summary>

Сравнить приближённую выдачу с точной через `script_score` на выборке запросов.
</details>

10. Что делать при смене модели?
<details><summary>Ответ</summary>

Новый индекс, переиндексация с новыми векторами, переключение псевдонима.
</details>

11. Что такое circuit breaker k-NN?
<details><summary>Ответ</summary>

Ограничение памяти под нативные графы; по умолчанию `knn.memory.circuit_breaker.limit` 50%.
</details>

## Связанные материалы

- [Продвинутая теория](advanced.ru.md) · [Квиз](quiz.ru.md)
- [07 · Кластер и шарды](../07-cluster-and-shards/advanced.ru.md)

## Источники

- [Methods and engines](https://docs.opensearch.org/latest/mappings/supported-field-types/knn-methods-engines/)
- [Approximate k-NN search](https://docs.opensearch.org/latest/vector-search/vector-search-techniques/approximate-knn/)
- [Normalization processor](https://docs.opensearch.org/latest/search-plugins/search-pipelines/normalization-processor/)
- [Vector quantization](https://docs.opensearch.org/latest/vector-search/optimizing-storage/knn-vector-quantization/)
