---
title: "Упражнения: основы OpenSearch"
lang: ru
track: opensearch
kind: module
doc: exercises
slug: 01-opensearch-basics
part: 1
tags: [opensearch, index, document, shard, replica, refresh, bulk, optimistic-concurrency]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Упражнения: основы OpenSearch

🌐 **Русский** · [English](exercises.en.md)

> Нужен узел из [обзора трека](../../overview.ru.md#окружение) и объект `client` из [базовой теории](basics.ru.md).

## 1. Первый индекс

Создайте индекс `books` с 1 шардом без реплик, запишите три книги, прочитайте одну по `_id` и выведите `_cat/indices`.

<details><summary>Подсказка</summary>

`indices.create`, `index`, `get`, `cat.indices`.

</details>

<details><summary>Решение</summary>

```python
client.indices.create(index="books", body={"settings": {"number_of_shards": 1, "number_of_replicas": 0}})
for i, title in enumerate(["Dune", "Emma", "Ulysses"], 1):
    client.index(index="books", id=str(i), body={"title": title}, params={"refresh": "true"})
print(client.get(index="books", id="2")["_source"])
print(client.cat.indices(index="books", params={"v": "true"}))
```

</details>

## 2. Видимость

Покажите, что при `refresh_interval = -1` документ читается по `_id`, но не считается поиском.

<details><summary>Подсказка</summary>

Сравните `count` до и после `indices.refresh`.

</details>

<details><summary>Решение</summary>

```python
client.indices.put_settings(index="books", body={"index": {"refresh_interval": "-1"}})
client.index(index="books", id="4", body={"title": "Solaris"})
print(client.get(index="books", id="4")["found"], client.count(index="books")["count"])
client.indices.refresh(index="books")
print(client.count(index="books")["count"])
client.indices.put_settings(index="books", body={"index": {"refresh_interval": "1s"}})
```

</details>

## 3. Счётчик без гонок

Увеличьте поле `views` документа так, чтобы параллельные вызовы не теряли обновления. Решите двумя способами.

<details><summary>Подсказка</summary>

Скрипт с `retry_on_conflict` либо цикл с `if_seq_no`.

</details>

<details><summary>Решение</summary>

```python
from opensearchpy.exceptions import ConflictError

# 1) server-side script
client.update(index="books", id="1", params={"retry_on_conflict": 5}, body={
    "script": {"source": "ctx._source.views = (ctx._source.views == null ? 0 : ctx._source.views) + 1"}})

# 2) optimistic loop
while True:
    d = client.get(index="books", id="1")
    src = d["_source"] | {"views": d["_source"].get("views", 0) + 1}
    try:
        client.index(index="books", id="1", body=src,
                     params={"if_seq_no": d["_seq_no"], "if_primary_term": d["_primary_term"]})
        break
    except ConflictError:
        continue
```

</details>

## 4. Пакет с ошибками

Отправьте пакет, в котором часть операций `create` конфликтует, и выведите список `_id` отклонённых.

<details><summary>Подсказка</summary>

`helpers.bulk(..., raise_on_error=False)`.

</details>

<details><summary>Решение</summary>

```python
from opensearchpy import helpers

actions = [{"_op_type": "create", "_index": "books", "_id": str(i), "title": "t"} for i in range(1, 7)]
ok, errors = helpers.bulk(client, actions, raise_on_error=False, refresh=True)
print(ok, [e["create"]["_id"] for e in errors])
```

</details>

## 5. Идемпотентный приёмник

Напишите функцию `apply(event)` для событий `{id, version, title}`: повторные и устаревшие события не должны менять данные.

<details><summary>Подсказка</summary>

`version_type=external`, перехват `ConflictError`.

</details>

<details><summary>Решение</summary>

```python
from opensearchpy.exceptions import ConflictError

def apply(event):
    try:
        client.index(index="books", id=event["id"], body={"title": event["title"]},
                     params={"version": event["version"], "version_type": "external"})
        return True
    except ConflictError:
        return False   # stale or duplicate

print(apply({"id": "x", "version": 5, "title": "new"}))   # True
print(apply({"id": "x", "version": 4, "title": "old"}))   # False
print(apply({"id": "x", "version": 5, "title": "new"}))   # False
```

</details>
