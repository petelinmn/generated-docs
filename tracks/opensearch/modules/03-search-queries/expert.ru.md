---
title: "Поисковые запросы: экспертная теория"
lang: ru
track: opensearch
kind: module
doc: expert
slug: 03-search-queries
part: 1
tags: [query-dsl, full-text-search, bool-query, relevance, bm25, pagination]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Поисковые запросы: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · [2. Продвинутый](advanced.ru.md) · **3. Эксперт** · [4. Квиз](quiz.ru.md)

## Из чего складывается оценка

`"explain": true` показывает расчёт BM25 для `match: {name: "mouse"}`:

```text
0.3979 weight(name:mous in 0) [PerFieldSimilarity], result of:
  0.3979 score(freq=1.0), computed as boost * idf * tf from:
    0.8755 idf, computed as log(1 + (N - n + 0.5) / (n + 0.5)) from:
      2 n, number of documents containing term
      5 N, total number of documents with field
    0.4545 tf, computed as freq / (freq + k1 * (1 - b + b * dl / avgdl)) from:
      1.0 freq    1.2 k1    0.75 b    2.0 dl    2.0 avgdl
```

- **idf**: чем реже терм, тем он ценнее.
- **tf**: повторения повышают оценку с насыщением (`k1`).
- **b**: штраф за длину поля относительно средней.

Статистика считается **по каждому шарду отдельно**, поэтому на маленьких индексах с несколькими шардами одинаковые документы могут получить разные оценки.

## Диагностика запроса

| Инструмент | Что показывает |
|---|---|
| `explain` | расчёт оценки документа |
| `_validate/query?explain=true` | как запрос переписан: `match` «Wireless Mice» по `english` → `name:wireless name:mice` |
| `"profile": true` | время по частям запроса на каждом шарде |

Второй пример поучителен: стеммер `english` не превращает `mice` в `mouse` — он отсекает суффиксы, а не знает неправильные формы.

## Каталог запросов

По документации; в этом модуле проверены `match`, `match_phrase`, `multi_match`, `query_string`, `simple_query_string`, `term`, `terms`, `range`, `exists`, `prefix`, `wildcard`, `ids`, `bool`, `constant_score`, `nested`.

| Группа | Запросы |
|---|---|
| Полнотекстовые | `match`, `match_phrase`, `match_phrase_prefix`, `match_bool_prefix`, `multi_match`, `combined_fields`, `query_string`, `simple_query_string`, `intervals` |
| Уровня термов | `term`, `terms`, `terms_set`, `range`, `exists`, `prefix`, `wildcard`, `regexp`, `fuzzy`, `ids` |
| Составные | `bool`, `boosting`, `constant_score`, `dis_max`, `function_score`, `hybrid` |
| Связанные документы | `nested`, `has_child`, `has_parent`, `parent_id` |
| Геозапросы | `geo_distance`, `geo_bounding_box`, `geo_polygon`, `geo_shape` |
| Специальные | `script`, `script_score`, `more_like_this`, `percolate`, `knn`, `neural`, `rank_feature`, `distance_feature`, `wrapper` |

> ⚠️ Проверить: каталог запросов составлен по документации; запускались только перечисленные над таблицей.

## Значения по умолчанию

| Параметр | Значение |
|---|---|
| `size` | 10 |
| `from + size` не более | 10000 |
| `track_total_hits` | точный счёт до 10000 |
| `match.operator` | `or` |
| BM25 `k1`, `b` | 1.2 и 0.75 |

## Подвохи

1. Фильтрующий `bool` даёт `_score` 0.0, `match_all` и `term` сами по себе — 1.0 (в эксперименте `term` по `keyword`-полю с одним совпадением).
2. `exists` по отсутствующему полю — пустой результат, а не ошибка.
3. Дата-математика `2026-03-01||-1M` отсчитывается от указанной даты.
4. `match` по полю с другим анализатором ищет другие термы.
5. Сортировка по `text` — ошибка 400.
6. Анализатор `english` не знает неправильных форм (`mice`).
7. `fuzziness` применяется к термам **после** анализа: опечатка `wireles` по `english` превращается в `wirel` и находит `wired`, а не `wireless`. Для нечёткого поиска заведите подполе без стемминга.

## Вопросы с собеседований

1. Чем query context отличается от filter context?
<details><summary>Ответ</summary>

В первом вычисляется оценка, во втором только да/нет, и результат фильтров может кэшироваться.
</details>

2. `match` против `term`?
<details><summary>Ответ</summary>

`match` анализирует строку запроса, `term` ищет терм как есть.
</details>

3. Как работает BM25?
<details><summary>Ответ</summary>

Сумма по термам idf, умноженного на насыщающуюся частоту с поправкой на длину поля.
</details>

4. Почему на разных шардах разные оценки?
<details><summary>Ответ</summary>

Статистика термов локальна для шарда.
</details>

5. Что делает `minimum_should_match`?
<details><summary>Ответ</summary>

Задаёт, сколько `should` обязаны совпасть.
</details>

6. Почему глубокий `from` дорог?
<details><summary>Ответ</summary>

Каждый шард собирает `from + size` лучших, координатор их сливает.
</details>

7. Что такое PIT?
<details><summary>Ответ</summary>

Замороженное представление индекса для согласованного обхода вместе с `search_after`.
</details>

8. Зачем `post_filter`?
<details><summary>Ответ</summary>

Фильтровать выдачу, не меняя агрегации.
</details>

9. Почему ведущая звёздочка в `wildcard` медленна?
<details><summary>Ответ</summary>

Нельзя сузить диапазон по началу терма, перебирается словарь.
</details>

10. Как учесть опечатки?
<details><summary>Ответ</summary>

`fuzziness: "AUTO"` — расстояние зависит от длины терма.
</details>

11. `multi_match` с `name^3`?
<details><summary>Ответ</summary>

Вклад поля `name` умножается на 3.
</details>

12. Как понять, почему документ выше другого?
<details><summary>Ответ</summary>

`explain` для обоих.
</details>

## Связанные материалы

- [Продвинутая теория](advanced.ru.md) · [Квиз](quiz.ru.md)
- [02 · Отображения и анализ](../02-mappings-and-analysis/expert.ru.md)

## Источники

- [Query DSL](https://opensearch.org/docs/latest/query-dsl/)
- [Explain API](https://opensearch.org/docs/latest/api-reference/search-apis/explain/)
- [Profile API](https://opensearch.org/docs/latest/api-reference/search-apis/profile/)
