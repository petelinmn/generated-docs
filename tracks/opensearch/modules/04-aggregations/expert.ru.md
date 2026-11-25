---
title: "Агрегации: экспертная теория"
lang: ru
track: opensearch
kind: module
doc: expert
slug: 04-aggregations
part: 1
tags: [aggregations, buckets, metrics, cardinality, composite-aggregation]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Агрегации: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · [2. Продвинутый](advanced.ru.md) · **3. Эксперт** · [4. Квиз](quiz.ru.md)

## Как выполняется агрегация

Каждый шард считает частичный результат по doc values своих сегментов, координирующий узел объединяет их (reduce). Суммы и минимумы объединяются точно, а топы и число различных значений — приближённо.

## Каталог

> ⚠️ Проверить: список составлен по документации; запуском в этом модуле проверены только агрегации, показанные в примерах.

| Группа | Агрегации |
|---|---|
| Метрики | `avg`, `sum`, `min`, `max`, `value_count`, `stats`, `extended_stats`, `cardinality`, `percentiles`, `percentile_ranks`, `median_absolute_deviation`, `weighted_avg`, `top_hits`, `scripted_metric`, `geo_bounds`, `geo_centroid`, `matrix_stats` |
| Корзины | `terms`, `multi_terms`, `rare_terms`, `significant_terms`, `significant_text`, `histogram`, `date_histogram`, `auto_date_histogram`, `range`, `date_range`, `ip_range`, `filter`, `filters`, `global`, `missing`, `nested`, `reverse_nested`, `composite`, `sampler`, `diversified_sampler`, `adjacency_matrix`, `geohash_grid`, `geotile_grid`, `geo_distance` |
| Конвейерные | `avg_bucket`, `sum_bucket`, `min_bucket`, `max_bucket`, `stats_bucket`, `percentiles_bucket`, `cumulative_sum`, `derivative`, `moving_fn`, `serial_diff`, `bucket_script`, `bucket_selector`, `bucket_sort` |

## Значения по умолчанию и пределы

| Параметр | Значение |
|---|---|
| `terms.size` | 10 |
| `search.max_buckets` | 65535 |
| `gap_policy` | `skip` |

Значение `search.max_buckets` получено из настроек узла. Запрос `terms` с `size: 70000` по 50 000 значений прошёл: корзин меньше предела — важно число созданных корзин, а не `size`.

## Подвохи

1. `fixed_interval: "month"` — ошибка разбора: месяц есть только в `calendar_interval`.
2. `avg` по неотображённому полю даёт `null`, а `sum` — `0.0`: опечатка в имени поля выглядит как правдоподобный ноль.
3. `date_histogram` по умолчанию возвращает и пустые интервалы; `min_doc_count: 1` их убирает.
4. Пустая корзина даёт `sum` 0.0, но конвейерная агрегация с `skip` считает её пропуском.
5. `terms` по массиву считает документ в каждой своей корзине.
6. `post_filter` сужает выдачу, но не агрегации: при нём `hits.total` стал 2, а корзины остались полными.

## Вопросы с собеседований

1. Чем метрическая агрегация отличается от корзинной?
<details><summary>Ответ</summary>

Метрика считает число, корзинная делит документы на группы и может содержать вложенные агрегации.
</details>

2. Почему `terms` приблизителен?
<details><summary>Ответ</summary>

Шарды присылают только свои верхние `shard_size` значений.
</details>

3. Как повысить точность `terms`?
<details><summary>Ответ</summary>

Увеличить `shard_size` ценой памяти и трафика; либо направлять значения одного ключа в один шард.
</details>

4. Что показывает `sum_other_doc_count`?
<details><summary>Ответ</summary>

Сколько документов не вошло в возвращённые корзины.
</details>

5. Почему нельзя агрегировать по `text`?
<details><summary>Ответ</summary>

У него нет doc values; `fielddata` грузит термы в кучу.
</details>

6. Как получить все группы?
<details><summary>Ответ</summary>

`composite` с `after_key`.
</details>

7. `global` против `filter`?
<details><summary>Ответ</summary>

`global` игнорирует запрос, `filter` дополнительно сужает.
</details>

8. `calendar_interval` против `fixed_interval`?
<details><summary>Ответ</summary>

Календарный учитывает разную длину месяцев; фиксированный — постоянная длительность, месяца в нём нет.
</details>

9. Как отфильтровать корзины по значению метрики?
<details><summary>Ответ</summary>

`bucket_selector`.
</details>

10. Точна ли `cardinality`?
<details><summary>Ответ</summary>

Нет, это оценка алгоритмом HyperLogLog++; точность настраивается `precision_threshold`.
</details>

11. Влияет ли `post_filter` на агрегации?
<details><summary>Ответ</summary>

Нет.
</details>

12. Как снизить нагрузку дашборда?
<details><summary>Ответ</summary>

`size: 0`, фильтры в filter-контексте, меньше уровней вложенности, предварительная сводка данных.
</details>

## Связанные материалы

- [Продвинутая теория](advanced.ru.md) · [Квиз](quiz.ru.md)
- [02 · Отображения и анализ](../02-mappings-and-analysis/expert.ru.md) — doc values

## Источники

- [Метрические агрегации](https://docs.opensearch.org/latest/aggregations/metric/)
- [Корзинные агрегации](https://docs.opensearch.org/latest/aggregations/bucket/)
- [Конвейерные агрегации](https://docs.opensearch.org/latest/aggregations/pipeline/)
