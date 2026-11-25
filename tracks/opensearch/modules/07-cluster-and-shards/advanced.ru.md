---
title: "Кластер и шарды: продвинутая теория"
lang: ru
track: opensearch
kind: module
doc: advanced
slug: 07-cluster-and-shards
part: 2
tags: [cluster, shard, replica, shard-allocation, high-availability]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Кластер и шарды: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

Кластер, `client` и индекс `orders` — из [базовой теории](basics.ru.md).

## Почему шард не назначен

```python
client.indices.put_settings(index="orders", body={"number_of_replicas": 3})   # 4 copies, 3 nodes
client.cluster.health(index="orders", wait_for_status="yellow", wait_for_events="languid")["status"]   # 'yellow'
r = client.cluster.allocation_explain(body={"index": "orders", "shard": 0, "primary": False})
r["can_allocate"]                                                  # 'no'
any(d["decider"] == "same_shard" for n in r["node_allocation_decisions"] for d in n["deciders"])   # True
client.indices.put_settings(index="orders", body={"number_of_replicas": 1})
```

## Освободить узел перед обслуживанием

```python
client.cluster.put_settings(body={"transient": {"cluster.routing.allocation.exclude._name": "os-node3"}})
client.cluster.health(wait_for_events="languid", wait_for_no_relocating_shards=True)
print(client.cat.shards(index="orders", h="node"))                # no os-node3
client.cluster.put_settings(body={"transient": {"cluster.routing.allocation.exclude._name": None}})
```

При короткой перезагрузке перемещать всё не обязательно: `cluster.routing.allocation.enable: primaries` запрещает назначать реплики, а после возврата узла настройку сбрасывают. С ней новый индекс остаётся yellow, и `allocation/explain` называет причину.

## Зоны доступности

```python
client.cluster.put_settings(body={"persistent": {
    "cluster.routing.allocation.awareness.attributes": "zone",
    "cluster.routing.allocation.awareness.force.zone.values": "a,b,c"}})
```

Копии шарда распределяются по значениям `node.attr.zone`. Наблюдение на трёх зонах при остановке узла зоны `c`:

| Копий на шард | Через 75 секунд |
|---|---|
| 2 (1 реплика) | green: копия восстановлена в другой зоне |
| 3 (2 реплики) | yellow: решение `awareness` не даёт сложить две копии в одну зону |

Принудительные значения (`force`) нужны именно для этого: при потере зоны оставшиеся не перегружаются третьими копиями.

## Своя маршрутизация

```python
client.indices.create(index="tenants", body={"settings": {"number_of_shards": 3},
                                             "mappings": {"_routing": {"required": True}}})
try:
    client.index(index="tenants", id="1", body={})
except RequestError as e:
    print(e.error)                                # routing_missing_exception
client.index(index="tenants", id="1", body={}, routing="acme", refresh=True)
client.search(index="tenants", routing="acme")["_shards"]["total"]   # 1
client.search(index="tenants")["_shards"]["total"]                   # 3
```

Поиск по одному шарду дешевле, но крупный арендатор делает его «горячим». Тот же `routing` нужен при чтении и удалении по `_id`.

## Сколько шардов

- Ориентир AWS: 10–50 ГиБ на шард; меньше — когда важна задержка поиска, больше — для логов.
- Там же: не более 25 шардов на ГиБ heap узла.
- Это отправные точки, а не гарантии: проверяйте нагрузочным тестом на своих данных.
- Первичные шарды масштабируют запись и объём, реплики — чтение и надёжность.

## Частые ошибки

- Забыть снять `exclude` или `allocation.enable` после работ.
- Два управляющих узла-кандидата: потеря любого лишает кластер большинства. Берите три.
- Маршрутизация по полю с перекосом значений.

## Связанные материалы

- [Базовая теория](basics.ru.md) · [Эксперт](expert.ru.md)
- [05 · Индексация: rollover и split](../05-indexing-at-scale/advanced.ru.md)
- [08 · Жизненный цикл индексов](../08-index-lifecycle/overview.ru.md)

## Источники

- [Cluster allocation explain](https://docs.opensearch.org/latest/api-reference/cluster-api/cluster-allocation/)
- [Cluster settings](https://docs.opensearch.org/latest/install-and-configure/configuring-opensearch/cluster-settings/)
- [Routing metadata field](https://docs.opensearch.org/latest/field-types/metadata-fields/routing/)
- [Choosing the number of shards (AWS)](https://docs.aws.amazon.com/opensearch-service/latest/developerguide/bp-sharding.html)
