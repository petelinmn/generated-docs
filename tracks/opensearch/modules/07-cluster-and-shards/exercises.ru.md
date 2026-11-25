---
title: "Упражнения: кластер и шарды"
lang: ru
track: opensearch
kind: module
doc: exercises
slug: 07-cluster-and-shards
part: 2
tags: [cluster, shard, replica, shard-allocation, high-availability]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Упражнения: кластер и шарды

🌐 **Русский** · [English](exercises.en.md)

> Нужны кластер из трёх узлов, `client` и индекс `orders` из [базовой теории](basics.ru.md).

## 1. Карта шардов

Посчитайте, сколько копий шардов `orders` лежит на каждом узле.

<details><summary>Подсказка</summary>

`cat.shards` с `format="json"`.

</details>

<details><summary>Решение</summary>

```python
from collections import Counter

print(Counter(s["node"] for s in client.cat.shards(index="orders", format="json")))
```

</details>

## 2. Прочитать причину

Создайте индекс с 3 репликами и выведите объяснения, почему копия не назначена.

<details><summary>Подсказка</summary>

`cluster.allocation_explain`.

</details>

<details><summary>Решение</summary>

```python
client.indices.create(index="ex-explain", body={"settings": {"number_of_shards": 1, "number_of_replicas": 3}})
client.cluster.health(index="ex-explain", wait_for_status="yellow", wait_for_events="languid")
r = client.cluster.allocation_explain(body={"index": "ex-explain", "shard": 0, "primary": False})
print(r["allocate_explanation"])
print({d["decider"] for n in r["node_allocation_decisions"] for d in n["deciders"]})
client.indices.delete(index="ex-explain")
```

</details>

## 3. Освободить узел

Уберите все шарды с `os-node2`, дождитесь конца переноса и верните настройку.

<details><summary>Подсказка</summary>

`exclude._name`, `wait_for_no_relocating_shards`.

</details>

<details><summary>Решение</summary>

```python
key = "cluster.routing.allocation.exclude._name"
client.cluster.put_settings(body={"transient": {key: "os-node2"}})
client.cluster.health(wait_for_events="languid", wait_for_no_relocating_shards=True)
print(sorted({s["node"] for s in client.cat.shards(format="json") if s["node"]}))
client.cluster.put_settings(body={"transient": {key: None}})
```

</details>

## 4. Неполный ответ

Напишите функцию подсчёта, которая бросает исключение, если ответили не все шарды.

<details><summary>Подсказка</summary>

Сравните `successful` и `total`.

</details>

<details><summary>Решение</summary>

```python
def strict_count(index):
    r = client.count(index=index)
    if r["_shards"]["successful"] < r["_shards"]["total"]:
        raise RuntimeError(f"partial result: {r['_shards']}")
    return r["count"]

print(strict_count("orders"))
```

</details>

## 5. Уменьшить число шардов

Сделайте из индекса с 4 шардами индекс с 2.

<details><summary>Подсказка</summary>

Запрет записи, сбор копий на одном узле, `shrink`.

</details>

<details><summary>Решение</summary>

```python
client.indices.create(index="ex-big", body={"settings": {"number_of_shards": 4}})
client.indices.put_settings(index="ex-big", body={"index.blocks.write": True,
    "index.number_of_replicas": 0, "index.routing.allocation.require._name": "os-node1"})
client.cluster.health(index="ex-big", wait_for_events="languid", wait_for_status="green", wait_for_no_relocating_shards=True)
client.indices.shrink(index="ex-big", target="ex-small", body={"settings": {"index.number_of_shards": 2,
    "index.number_of_replicas": 1, "index.routing.allocation.require._name": None, "index.blocks.write": None}})
print(client.cluster.health(index="ex-small", wait_for_status="green")["status"])
client.indices.delete(index="ex-big,ex-small")
```

</details>
