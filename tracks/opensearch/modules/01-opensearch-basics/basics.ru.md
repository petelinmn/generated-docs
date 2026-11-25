---
title: "Основы OpenSearch: базовая теория"
lang: ru
track: opensearch
kind: module
doc: basics
slug: 01-opensearch-basics
part: 1
tags: [opensearch, index, document, shard, replica, refresh, bulk, optimistic-concurrency]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Основы OpenSearch: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [overview](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Зачем это нужно

Реляционная база плохо отвечает на вопрос «найди товары, похожие на "беспроводная мышь", и покажи самые подходящие первыми». OpenSearch строит [обратный индекс](../02-mappings-and-analysis/basics.ru.md) и вычисляет релевантность.

## Основные понятия

| Понятие | Что это |
|---|---|
| Кластер | один или несколько узлов с общим состоянием |
| Узел | один процесс OpenSearch |
| Индекс | набор документов с общими настройками и отображением |
| Документ | JSON-объект с уникальным в индексе `_id` |
| Основной шард (primary) | часть индекса; каждый шард — это индекс Lucene |
| Реплика (replica) | копия основного шарда на другом узле |

## Подключение

Узел запускается командой из [обзора трека](../../overview.ru.md#окружение).

```python
from opensearchpy import OpenSearch

client = OpenSearch(hosts=[{"host": "localhost", "port": 9200}])
print(client.info()["version"]["number"])   # 3.10.1
client.indices.create(index="products",
    body={"settings": {"number_of_shards": 1, "number_of_replicas": 0}})
```

Те же действия через `curl`:

```bash
curl -X PUT "localhost:9200/products/_doc/1?refresh=true" \
  -H "Content-Type: application/json" -d '{"name":"Blue mug","price":12.5}'
curl "localhost:9200/products/_search?q=name:mug&pretty"
curl "localhost:9200/_cat/indices?v"
```

## CRUD

```python
r = client.index(index="products", id="1", body={"name": "Blue mug", "price": 12.5})
# result: created, _version: 1
r = client.index(index="products", id="1", body={"name": "Blue mug", "price": 13.0})
# result: updated, _version: 2
r = client.update(index="products", id="1", body={"doc": {"price": 14.0}})
# result: updated, _version: 3
r = client.update(index="products", id="1", body={"doc": {"price": 14.0}})
# result: noop, _version: 3
print(client.get(index="products", id="1")["_source"])
# {'name': 'Blue mug', 'price': 14.0}
client.delete(index="products", id="1")          # result: deleted
```

- `index` с занятым `_id` **заменяет** документ целиком.
- `update` с `doc` объединяет поля; если ничего не изменилось, результат `noop` и версия не растёт.
- Без `id` OpenSearch генерирует идентификатор сам (в эксперименте — 20 символов).
- `create` с занятым `_id` отвечает `409 version_conflict_engine_exception`.

## Когда документ виден поиску

Поиск видит документ после **refresh**. Чтение по `_id` (`get`) видит его сразу.

```python
client.indices.put_settings(index="products", body={"index": {"refresh_interval": "-1"}})
client.index(index="products", id="9", body={"name": "Hidden"})
print(client.get(index="products", id="9")["found"])   # True
print(client.count(index="products")["count"])         # does not include document 9
client.indices.refresh(index="products")
print(client.count(index="products")["count"])         # includes it
client.indices.put_settings(index="products", body={"index": {"refresh_interval": "1s"}})
```

По умолчанию `index.refresh_interval` равен `1s`. Параметр запроса `refresh=wait_for` заставляет запись дождаться ближайшего обновления; `refresh=true` обновляет немедленно и для массовой записи слишком дорог.

## Массовая запись

```python
from opensearchpy import helpers

actions = [{"_op_type": "index", "_index": "products", "_id": str(i), "name": f"Item {i}"} for i in range(10, 15)]
actions.append({"_op_type": "create", "_index": "products", "_id": "10", "name": "dup"})
ok, errors = helpers.bulk(client, actions, raise_on_error=False, refresh=True)
print(ok)                                               # 5
print([(e["create"]["_id"], e["create"]["status"]) for e in errors])   # [('10', 409)]
```

`_bulk` не атомарен: одни операции могут пройти, другие — нет. Проверяйте ошибки каждой.

## Здоровье кластера

| Статус | Значение |
|---|---|
| `green` | все основные шарды и реплики назначены |
| `yellow` | основные назначены, часть реплик — нет |
| `red` | часть основных шардов не назначена |

Индекс, созданный автоматически первой записью, получил 1 основной шард и 1 реплику. На одном узле реплику разместить негде, поэтому статус `yellow`.

## Частые ошибки новичков

- Ожидать, что поиск увидит документ сразу после записи.
- Использовать `index` там, где нужно изменить одно поле.
- Не проверять ошибки `_bulk`.
- Полагаться на автоматически созданный индекс в рабочей системе.
- Открывать в сеть узел с отключённой безопасностью.

## Шпаргалка

| Действие | REST |
|---|---|
| создать индекс | `PUT /<index>` |
| записать | `PUT /<index>/_doc/<id>` или `POST /<index>/_doc` |
| только создать | `PUT /<index>/_create/<id>` |
| прочитать | `GET /<index>/_doc/<id>` |
| изменить | `POST /<index>/_update/<id>` |
| удалить | `DELETE /<index>/_doc/<id>` |
| пакет | `POST /_bulk` |
| состояние | `GET /_cluster/health`, `GET /_cat/indices?v` |

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [02 · Отображения и анализ текста](../02-mappings-and-analysis/basics.ru.md)
- [Python](../../../python/overview.ru.md)

## Источники

- [Index document API](https://opensearch.org/docs/latest/api-reference/document-apis/index-document/)
- [Bulk API](https://opensearch.org/docs/latest/api-reference/document-apis/bulk/)
- [Cluster health](https://opensearch.org/docs/latest/api-reference/cluster-api/cluster-health/)
