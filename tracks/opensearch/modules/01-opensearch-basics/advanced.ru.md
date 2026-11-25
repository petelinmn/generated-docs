---
title: "Основы OpenSearch: продвинутая теория"
lang: ru
track: opensearch
kind: module
doc: advanced
slug: 01-opensearch-basics
part: 1
tags: [opensearch, index, document, shard, replica, refresh, bulk, optimistic-concurrency]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Основы OpenSearch: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Конкурентные изменения

Каждое изменение документа получает `_seq_no`; вместе с `_primary_term` он позволяет записать «только если документ не менялся».

```python
from opensearchpy.exceptions import ConflictError

d = client.get(index="products", id="1")
cond = {"if_seq_no": d["_seq_no"], "if_primary_term": d["_primary_term"]}
client.index(index="products", id="1", body=d["_source"] | {"price": 15}, params=cond)   # ok
try:
    client.index(index="products", id="1", body=d["_source"] | {"price": 99}, params=cond)
except ConflictError as e:
    print(e.status_code, e.error)   # 409 version_conflict_engine_exception
```

Это оптимистичная блокировка: при конфликте читайте заново и повторяйте. Для `update` есть параметр `retry_on_conflict`.

### Внешняя версия

Если версия ведётся в источнике (например, в базе данных), используйте `version_type=external`: запись с версией 5 прошла, следом с версией 4 — `409`. Так устаревшее событие из очереди не затрёт свежее.

## Маршрутизация

Шард документа выбирается по хешу значения маршрутизации; по умолчанию это `_id`. Можно задать своё, чтобы документы одного клиента лежали вместе и запрос шёл в один шард.

```python
client.indices.create(index="r", body={"settings": {"number_of_shards": 3, "number_of_replicas": 0}})
client.index(index="r", id="a", body={"n": 100}, params={"routing": "alice", "refresh": "true"})
client.get(index="r", id="a")                                  # NotFoundError (404)
client.get(index="r", id="a", params={"routing": "alice"})     # found
```

Если маршрутизация задана при записи, она нужна и при чтении, изменении, удалении. Крупный клиент при этом может перегрузить один шард.

## Скрипты, upsert и операции по запросу

`update` принимает скрипт (`ctx._source.n += 5`) и `doc_as_upsert: true`, чтобы создать отсутствующий документ.

```python
r = client.update_by_query(index="r", params={"refresh": "true"}, body={
    "query": {"term": {"g": 1}},
    "script": {"source": "ctx._source.n += params.by", "params": {"by": 1000}}})
# total: 15, updated: 15, version_conflicts: 0
r = client.delete_by_query(index="r", params={"refresh": "true"},
                           body={"query": {"range": {"n": {"gte": 1000}}}})
# total: 15, deleted: 15
```

`update_by_query` и `delete_by_query` не транзакционны: они работают по снимку, при конфликте версии по умолчанию прерываются, а уже выполненное не откатывается. `conflicts=proceed` продолжает и считает конфликты.

## Настройки индекса

- `number_of_replicas` меняется в любой момент: `0 → 1` на одном узле перевело индекс в `yellow`.
- `number_of_shards` на открытом индексе изменить нельзя: `400 ... Can't update non dynamic settings`. Для другого числа шардов нужны reindex, split или shrink.

## Производительность записи

- Пишите пакетами `_bulk`; размер подбирайте измерением.
- Не вызывайте `refresh=true` на каждый документ.
- При первичной загрузке увеличивают `refresh_interval` (или ставят `-1`) и временно убирают реплики, затем возвращают.

## Частые ошибки

- Чтение без `routing` документа, записанного с ним.
- «Прочитать, изменить, записать» без `if_seq_no`.
- Считать `delete_by_query` откатываемым.
- Ожидать, что `get` и `search` согласованы в одну и ту же секунду.

## Связанные материалы

- [Базовая теория](basics.ru.md) · [Эксперт](expert.ru.md)
- [Семантика доставки в Kafka](../../../kafka/modules/05-delivery-semantics/basics.ru.md) — идемпотентный приёмник
- [05 · Индексация в большом объёме](../05-indexing-at-scale/overview.ru.md)
- [07 · Кластер и шарды](../07-cluster-and-shards/overview.ru.md)

## Источники

- [Update document](https://opensearch.org/docs/latest/api-reference/document-apis/update-document/)
- [Update by query](https://opensearch.org/docs/latest/api-reference/document-apis/update-by-query/)
- [Routing field](https://opensearch.org/docs/latest/field-types/metadata-fields/routing/)
