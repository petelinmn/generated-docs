---
title: "Квиз: векторный поиск"
lang: ru
track: opensearch
kind: module
doc: quiz
slug: 09-vector-search
part: 2
tags: [vector-search, knn, embeddings, hybrid-search, hnsw]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Квиз: векторный поиск

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте сами, затем раскройте ответ. 🟢 [базовый](basics.ru.md) · 🟡 [продвинутый](advanced.ru.md) · 🔴 [экспертный](expert.ru.md).

## 1. 🟢 Что хранит поле `knn_vector`?

- A) текст произвольной длины
- B) хеш документа
- C) массив чисел фиксированной длины
- D) список слов

<details><summary>Ответ</summary>

**C.**
→ [Базовая теория](basics.ru.md)

</details>

## 2. 🟢 `k: 5`, `size: 2`. Сколько документов в `hits.hits` (один шард)?

- A) 2
- B) 5
- C) 10
- D) 7

<details><summary>Ответ</summary>

**A.**
→ [Базовая теория](basics.ru.md)

</details>

## 3. 🟢 Вектор запроса короче, чем в mapping:

- A) дополняется нулями
- B) лишние измерения игнорируются
- C) пустой результат
- D) ошибка

<details><summary>Ответ</summary>

**D.**
→ [Базовая теория](basics.ru.md)

</details>

## 4. 🟢 Метрика по умолчанию?

- A) `hamming`
- B) `l2`
- C) `cosinesimil`
- D) `innerproduct`

<details><summary>Ответ</summary>

**B.**
→ [Базовая теория](basics.ru.md)

</details>

## 5. 🟡 Где поставить фильтр, чтобы получить k подходящих?

- A) в `aggs`
- B) внутри `knn`
- C) в `post_filter`
- D) в `bool.filter` снаружи

<details><summary>Ответ</summary>

**B.**
→ [Продвинутая теория](advanced.ru.md)

</details>

## 6. 🟡 Какая метрика не зависит от длины вектора?

- A) `l2`
- B) `innerproduct`
- C) `l1`
- D) `cosinesimil`

<details><summary>Ответ</summary>

**D.**
→ [Продвинутая теория](advanced.ru.md)

</details>

## 7. 🟡 Что верно о RRF?

- [ ] A) использует позиции, а не оценки
- [ ] B) по умолчанию `rank_constant` 60
- [ ] C) требует нормализации `min_max`
- [ ] D) работает только с одним подзапросом

<details><summary>Ответ</summary>

**A, B.**
→ [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Чем считать точных соседей?

- A) `match_all`
- B) `rescore` по BM25
- C) `script_score` с `knn_score`
- D) `knn` с большим `size`

<details><summary>Ответ</summary>

**C.**
→ [Продвинутая теория](advanced.ru.md)

</details>

## 9. 🔴 `hybrid` без search pipeline:

- A) включается RRF
- B) оценки нормализуются сами
- C) возвращается только первый подзапрос
- D) в ответе служебные значения и повторы

<details><summary>Ответ</summary>

**D.**
→ [Экспертная теория](expert.ru.md)

</details>

## 10. 🔴 Оценка памяти HNSW на вектор:

- A) `m * k` байт
- B) `1.1 * (4*d + 8*m)` байт
- C) `d` байт
- D) `4*d` бит

<details><summary>Ответ</summary>

**B.**
→ [Экспертная теория](expert.ru.md)

</details>

## 11. 🔴 Какой движок нельзя использовать для нового индекса в 3.x?

- A) `lucene`
- B) все доступны
- C) `nmslib`
- D) `faiss`

<details><summary>Ответ</summary>

**C.**
→ [Экспертная теория](expert.ru.md)

</details>

## 12. 🔴 Открытый вопрос: по запросу с артикулом векторный поиск выдаёт «похожие», но не нужный товар. Что делать?

<details><summary>Ответ</summary>

Эмбеддинги плохо различают точные коды. Добавить полнотекстовый или `keyword`-подзапрос и объединить через `hybrid`; для запросов вида артикула можно искать только точно; проверить на наборе оценённых запросов.
→ [Продвинутая теория](advanced.ru.md)

</details>
