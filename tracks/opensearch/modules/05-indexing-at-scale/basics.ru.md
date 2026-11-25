---
title: "Индексация в большом объёме: базовая теория"
lang: ru
track: opensearch
kind: module
doc: basics
slug: 05-indexing-at-scale
part: 2
tags: [bulk, index-template, alias, reindex, rollover]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Индексация в большом объёме: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [overview](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Пакетная запись

```python
from opensearchpy import helpers

def docs(index, n):
    return ({"_index": index, "_id": str(i), "n": i, "msg": f"event {i}"} for i in range(n))

client.indices.create(index="events", body={"settings": {"number_of_replicas": 0}})
print(helpers.bulk(client, docs("events", 1000), chunk_size=500, refresh=True))   # (1000, [])
```

Тело `_bulk` — строки JSON (NDJSON): строка действия, затем строка документа. Помощник `helpers.bulk` формирует их сам.

## Сколько это даёт

Замер на одном узле в Docker (один шард, без реплик, короткие документы, один поток, подключение по `127.0.0.1`). Это порядок величин, а не ориентир для вашей системы:

| Способ | Документов | Скорость или время |
|---|---|---|
| по одному запросу | 1000 | 5.0 с |
| по одному с `refresh=true` | 1000 | 12.7 с |
| `_bulk` по 1 | 1000 | ≈ 200 док/с |
| `_bulk` по 100 | 20000 | ≈ 15 000 док/с |
| `_bulk` по 1000 | 20000 | ≈ 44 000 док/с |
| `_bulk` по 5000 | 20000 | ≈ 45 000 док/с |

Начиная с некоторого размера пакета рост прекращается. Размер подбирают измерением; предел тела HTTP-запроса `http.max_content_length` — `100mb`.

## Ошибки в пакете

```python
client.indices.create(index="app-v", body={"mappings": {"properties": {"n": {"type": "long"}}}})
bad = [{"_index": "app-v", "n": 1}, {"_index": "app-v", "n": "x"}]
[(ok, item["index"]["status"]) for ok, item in helpers.streaming_bulk(client, bad, raise_on_error=False)]
# [(True, 201), (False, 400)]
try:
    helpers.bulk(client, [{"_index": "app-v", "n": "x"}])
except helpers.BulkIndexError as e:
    print(len(e.errors))     # 1
```

`helpers.bulk` по умолчанию бросает `BulkIndexError`; успешные операции пакета при этом уже выполнены.

## Псевдонимы

```python
client.indices.create(index="items-v1", body={"settings": {"number_of_replicas": 0}})
client.index(index="items-v1", id="1", body={"price": "10"}, params={"refresh": "true"})
client.indices.put_alias(index="items-v1", name="items")
client.index(index="items", id="2", body={"price": "20"}, params={"refresh": "true"})
print(client.count(index="items-v1")["count"])   # 2: written through the alias
```

Псевдоним — второе имя одного или нескольких индексов.

## Смена структуры без простоя

```python
client.indices.create(index="items-v2", body={"settings": {"number_of_replicas": 0},
    "mappings": {"properties": {"price": {"type": "integer"}}}})
r = client.reindex(body={"source": {"index": "items-v1"}, "dest": {"index": "items-v2"}},
                   params={"refresh": "true"})
print(r["total"], r["created"])                  # 2 2
client.indices.update_aliases(body={"actions": [
    {"remove": {"index": "items-v1", "alias": "items"}},
    {"add": {"index": "items-v2", "alias": "items"}}]})
print(list(client.indices.get_alias(name="items")))   # ['items-v2']
```

Действия одного вызова `_aliases` применяются атомарно: читатели не увидят момента без индекса.

## Частые ошибки новичков

- Писать по одному документу.
- Обращаться из приложения напрямую к `items-v1`.
- Ожидать, что `reindex` перенесёт отображение: целевой индекс создают заранее.
- Забыть, что записи, пришедшие во время `reindex`, нужно догнать.

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [01 · Основы](../01-opensearch-basics/basics.ru.md)

## Источники

- [Bulk API](https://docs.opensearch.org/latest/api-reference/document-apis/bulk/)
- [Index aliases](https://docs.opensearch.org/latest/im-plugin/index-alias/)
- [Reindex data](https://docs.opensearch.org/latest/im-plugin/reindex-data/)
