---
title: "Кластер и шарды: экспертная теория"
lang: ru
track: opensearch
kind: module
doc: expert
slug: 07-cluster-and-shards
part: 2
tags: [cluster, shard, replica, shard-allocation, high-availability]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Кластер и шарды: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · [2. Продвинутый](advanced.ru.md) · **3. Эксперт** · [4. Квиз](quiz.ru.md)

## Роли узлов

| Роль | Что делает |
|---|---|
| `cluster_manager` | участвует в выборах; выбранный управляет состоянием |
| `data` | хранит шарды, выполняет индексацию и поиск |
| `ingest` | выполняет ingest-конвейеры |
| `remote_cluster_client` | обращается к другим кластерам |
| пустой список | только координирует запросы |

У узлов из примера по умолчанию были `cluster_manager, data, ingest, remote_cluster_client`.

## Путь запроса

- Запись: координирующий узел вычисляет шард по маршрутизации и пересылает первичному, тот выполняет операцию и отправляет репликам.
- Поиск: по одной копии каждого шарда (фаза query), затем сборка лучших документов (фаза fetch).

## Настройки и значения по умолчанию

| Настройка | По умолчанию |
|---|---|
| `index.unassigned.node_left.delayed_timeout` | `1m` |
| `index.write.wait_for_active_shards` | `1` |
| `index.auto_expand_replicas` | `false` |
| `index.replication.type` | `DOCUMENT` |
| `cluster.max_shards_per_node` | `1000` |
| `cluster.routing.allocation.enable` | `all` |
| `cluster.routing.rebalance.enable` | `all` |
| `cluster.routing.allocation.node_concurrent_recoveries` | `2` |
| `indices.recovery.max_bytes_per_sec` | 40 MB/s |
| `cluster.routing.allocation.disk.watermark.low` / `high` / `flood_stage` | 85% / 90% / 95% |

Значения сняты с узла 3.10.1 через `include_defaults`. `transient` сбрасывается полным перезапуском кластера, `persistent` сохраняется.

## Потеря большинства

```text
$ docker stop os-node2 os-node3                      2 of 3 cluster-manager-eligible nodes are gone

GET /_cluster/health                                 503 cluster_manager_not_discovered_exception
GET /_cluster/state/blocks?local=true                "no cluster-manager", levels: ["metadata_write"]
POST /orders/_doc/100                                500 node_not_connected_exception (that shard's primary was on a stopped node)
GET /orders/_search?size=0                           200, "_shards": {"total": 3, "successful": 2, "failed": 1}
```

Оставшийся узел отвечает на чтение тем, что у него есть, но состояние кластера не меняется.

## Частичный результат

```text
index "solo": 3 primaries, 0 replicas, 30 documents; one node with a shard is stopped

GET /_cluster/health                  red
GET /solo/_count                      {"count": 24, "_shards": {"total": 3, "successful": 2, "failed": 0}}
GET /solo/_search?allow_partial_search_results=false
                                      search_phase_execution_exception: Search rejected due to missing shards [[solo][0]]
node started again                    count is 30
```

## Восстановление

`_cat/recovery` показывает тип: `existing_store` — первичный поднят с локального диска, `peer` — копия получена с другого узла. После возврата узла в нашем прогоне у `peer` было `files_percent` 0.0%: файлы повторно не копировались.

## Изменить число шардов

| Операция | Шардов в результате | Условия |
|---|---|---|
| shrink | делитель исходного | `index.blocks.write`, копии всех шардов на одном узле |
| split | кратное исходному | `index.blocks.write` |
| clone | столько же | `index.blocks.write` |
| reindex | любое | копирование документов |

## Подвохи

1. `_count` по red-индексу вернул 24 из 30 с `failed: 0`; недостачу выдаёт только `successful < total`.
2. Поиск по умолчанию отдаёт частичные результаты. Проверяйте `_shards` или запрещайте `allow_partial_search_results=false`.
3. `wait_for_active_shards=2` при одной доступной копии даёт `unavailable_shards_exception`; без параметра тот же документ записывается.
4. Клон индекса унаследовал `index.routing.allocation.require._name`, оставшийся после shrink-подготовки, и стал yellow.
5. Лимит шардов считается на кластер: при `max_shards_per_node: 5` и трёх узлах отказ пришёл на пределе 15, причём реплики тоже считаются.
6. `index.auto_expand_replicas: 0-all` на трёх узлах дал 2 реплики, что бы ни было задано в `number_of_replicas`.
7. Сразу после изменения настройки `cluster.health` может ответить раньше, чем начнётся перенос: добавляйте `wait_for_events="languid"`.
8. В opensearch-py 3.2.0 `timeout="60s"` в `cluster.health` был воспринят как таймаут соединения и вызвал ошибку, а не ушёл на сервер.

## Что не проверялось здесь

Выделенные управляющие и координирующие узлы, срабатывание disk watermarks, segment replication, кросс-кластерная репликация и поиск.

> ⚠️ Проверить: поведение перечисленного выше, а также ответы о выборах и пути записи взяты из документации и общих знаний, не из прогонов.

## Вопросы с собеседований

1. Чем отличаются green, yellow и red?
<details><summary>Ответ</summary>

Все копии назначены; нет части реплик; нет хотя бы одного первичного.
</details>

2. Почему нельзя поменять число первичных шардов?
<details><summary>Ответ</summary>

От него зависит, в какой шард попадает документ. Нужны reindex, split или shrink в новый индекс.
</details>

3. Что будет на одном узле с `number_of_replicas: 1`?
<details><summary>Ответ</summary>

Yellow: реплику нельзя положить рядом с первичным.
</details>

4. Зачем три управляющих узла-кандидата?
<details><summary>Ответ</summary>

Большинство из трёх сохраняется при потере одного и не даёт кластеру расколоться.
</details>

5. Что делает кластер после ухода узла?
<details><summary>Ответ</summary>

Повышает реплики до первичных, затем после задержки строит недостающие копии.
</details>

6. Как узнать, почему шард не назначен?
<details><summary>Ответ</summary>

`_cluster/allocation/explain`: решения (deciders) по каждому узлу.
</details>

7. Реплики заменяют снимки?
<details><summary>Ответ</summary>

Нет: они повторяют удаления и ошибки.
</details>

8. Что такое disk watermarks?
<details><summary>Ответ</summary>

Пороги заполнения диска (по умолчанию 85, 90, 95%): не размещать новые шарды, переносить, запрещать запись в индекс.
</details>

9. Увеличивают ли реплики скорость поиска?
<details><summary>Ответ</summary>

Они добавляют пропускную способность, если есть свободные узлы; один запрос не обязательно станет быстрее.
</details>

10. Чем опасны тысячи мелких шардов?
<details><summary>Ответ</summary>

Память, большое состояние кластера, долгие восстановления и широкий веер запросов.
</details>

11. Что делает shrink и что для него нужно?
<details><summary>Ответ</summary>

Создаёт индекс с меньшим числом первичных (делителем исходного); нужны запрет записи и копии всех шардов на одном узле.
</details>

12. Что такое segment replication?
<details><summary>Ответ</summary>

Режим, где реплики получают готовые сегменты вместо повторной индексации; по умолчанию `index.replication.type` — `DOCUMENT`.
</details>

## Связанные материалы

- [Продвинутая теория](advanced.ru.md) · [Квиз](quiz.ru.md)
- [06 · Релевантность: статистика шардов](../06-relevance-tuning/expert.ru.md)

## Источники

- [Cluster reroute](https://docs.opensearch.org/latest/api-reference/cluster-api/cluster-reroute/)
- [Segment replication](https://docs.opensearch.org/latest/tuning-your-cluster/availability-and-recovery/segment-replication/index/)
- [Shrink index](https://docs.opensearch.org/latest/api-reference/index-apis/shrink-index/)
- [Clone index](https://docs.opensearch.org/latest/api-reference/index-apis/clone/)
