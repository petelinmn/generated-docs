---
title: "Индексация в большом объёме: экспертная теория"
lang: ru
track: opensearch
kind: module
doc: expert
slug: 05-indexing-at-scale
part: 2
tags: [bulk, index-template, alias, reindex, rollover]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Индексация в большом объёме: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · [2. Продвинутый](advanced.ru.md) · **3. Эксперт** · [4. Квиз](quiz.ru.md)

## Что происходит с пакетом

Координирующий узел разбирает пакет, группирует операции по шардам и рассылает их основным шардам; те применяют и передают репликам. Запись обслуживает пул потоков `write`: на тестовом узле 12 потоков (зависит от числа процессоров) и очередь 10000. Отклонения видны в столбце `rejected`.

```python
client.cat.thread_pool(thread_pool_patterns="write", params={"v": "true", "h": "name,size,queue_size,rejected"})
```

> ⚠️ Проверить: поведение при переполнении очереди (код 429) взято из документации и не воспроизводилось.

## Сегменты и слияния

Каждый refresh с новыми данными создаёт сегмент, фоновые слияния объединяют их. После 1000 одиночных записей в тестовых индексах было по нескольку сегментов, причём с `refresh=true` большая часть документов уже оказалась объединена: слияния расходовали работу, чем и объясняется часть замедления.

## Справочник операций

| API | Назначение | Ограничение |
|---|---|---|
| `_bulk` | пакет операций | не атомарен |
| `_reindex` | копирование документов | нужен `_source` |
| `_aliases` | атомарное изменение псевдонимов | |
| `_rollover` | новый индекс по условиям | нужен псевдоним записи или data stream |
| `_split` | больше шардов | источник заблокирован на запись |
| `_shrink` | меньше шардов | то же; копии всех шардов на одном узле |
| `_clone` | копия индекса | источник заблокирован на запись |
| `_close` / `_open` | закрыть и открыть индекс | |
| `_tasks` | долгие операции | |

## Split

```python
client.indices.create(index="app-src", body={"settings": {"number_of_shards": 1, "number_of_replicas": 0}})
client.index(index="app-src", body={"a": 1}, params={"refresh": "true"})
client.indices.put_settings(index="app-src", body={"index.blocks.write": True})
client.index(index="app-src", body={})          # 403 cluster_block_exception
client.indices.split(index="app-src", target="app-split",
                     body={"settings": {"index.number_of_shards": 2, "index.number_of_replicas": 0}})
```

## Подвохи

1. Изменение шаблона не трогает существующие индексы.
2. Псевдоним на несколько индексов без `is_write_index` принимает чтение, но не запись; `get` по нему не работает.
3. Пока псевдоним указывает на один индекс, запись через него работает без `is_write_index` — и ломается после добавления второго.
4. Повторный `reindex` по умолчанию перезаписывает документы, в том числе изменённые уже в новом индексе.
5. `BulkIndexError` не означает, что не записалось ничего.
6. Shrink на большее число шардов — ошибка 400.

## Вопросы с собеседований

1. Почему пакетная запись быстрее?
<details><summary>Ответ</summary>

Меньше сетевых обходов и реже синхронизация translog: она выполняется на запрос.
</details>

2. Атомарен ли `_bulk`?
<details><summary>Ответ</summary>

Нет.
</details>

3. Что делать с ответом 429?
<details><summary>Ответ</summary>

Это перегрузка очереди записи: повторить с растущей задержкой и снизить параллелизм.
</details>

4. Когда применяется шаблон индекса?
<details><summary>Ответ</summary>

Только при создании индекса.
</details>

5. Как изменить тип поля без простоя?
<details><summary>Ответ</summary>

Новый индекс, reindex, догнать изменения, атомарно переключить псевдоним.
</details>

6. Что копирует `reindex`?
<details><summary>Ответ</summary>

Документы из `_source`; настройки и отображение — нет.
</details>

7. Что такое `is_write_index`?
<details><summary>Ответ</summary>

Отмечает индекс псевдонима, принимающий записи.
</details>

8. Зачем rollover?
<details><summary>Ответ</summary>

Держать индексы ограниченного размера и удалять старые целиком.
</details>

9. Почему удалить индекс дешевле, чем `delete_by_query`?
<details><summary>Ответ</summary>

Удаляются файлы, а не помечаются документы с последующими слияниями.
</details>

10. Split или shrink?
<details><summary>Ответ</summary>

Split увеличивает число шардов, shrink уменьшает; в обоих случаях исходный индекс закрывают на запись.
</details>

11. Как следить за долгим reindex?
<details><summary>Ответ</summary>

`wait_for_completion=false` и Tasks API.
</details>

12. Как обеспечить идемпотентную загрузку?
<details><summary>Ответ</summary>

Детерминированный `_id`; при необходимости внешняя версия.
</details>

## Связанные материалы

- [Продвинутая теория](advanced.ru.md) · [Квиз](quiz.ru.md)
- [Темы и хранение в Kafka](../../../kafka/modules/04-topics-and-retention/expert.ru.md) — тоже сегменты и удаление целиком

## Источники

- [Split index](https://docs.opensearch.org/latest/api-reference/index-apis/split/)
- [Shrink index](https://docs.opensearch.org/latest/api-reference/index-apis/shrink-index/)
- [Tasks API](https://docs.opensearch.org/latest/api-reference/tasks/)
