---
title: "Кластер и шарды: базовая теория"
lang: ru
track: opensearch
kind: module
doc: basics
slug: 07-cluster-and-shards
part: 2
tags: [cluster, shard, replica, shard-allocation, high-availability]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Кластер и шарды: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [overview](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Слова, без которых не обойтись

| Термин | Что это |
|---|---|
| узел (node) | один процесс OpenSearch |
| кластер (cluster) | узлы с общим `cluster.name` |
| управляющий узел (cluster manager) | выбранный узел, который хранит и меняет состояние кластера |
| первичный шард (primary) | часть индекса, принимающая запись первой |
| реплика (replica) | копия первичного шарда на другом узле; тоже отвечает на поиск |

## Кластер из трёх узлов

Нужны Docker и примерно 3 ГБ свободной памяти.

```bash
# disposable local cluster, security disabled: never expose it
docker network create osnet
for i in 1 2 3; do
  zone=$(echo "a b c" | cut -d' ' -f$i)
  port=""; [ $i = 1 ] && port="-p 127.0.0.1:9200:9200"
  docker run -d --name os-node$i --network osnet $port \
    -e cluster.name=learn -e node.name=os-node$i -e node.attr.zone=$zone \
    -e discovery.seed_hosts=os-node1,os-node2,os-node3 \
    -e cluster.initial_cluster_manager_nodes=os-node1,os-node2,os-node3 \
    -e DISABLE_SECURITY_PLUGIN=true -e DISABLE_INSTALL_DEMO_CONFIG=true \
    -e "OPENSEARCH_JAVA_OPTS=-Xms512m -Xmx512m" opensearchproject/opensearch:3.10.1
done
# cleanup: docker rm -f os-node1 os-node2 os-node3 && docker network rm osnet
```

```python
from opensearchpy import OpenSearch

client = OpenSearch(hosts=[{"host": "localhost", "port": 9200}])
client.cluster.health(wait_for_nodes="3")

client.indices.create(index="orders", body={"settings": {"number_of_shards": 3, "number_of_replicas": 1}})
for i in range(1, 31):
    client.index(index="orders", id=str(i), body={"n": i})
client.indices.refresh(index="orders")

def health(index="orders"):
    h = client.cluster.health(index=index, wait_for_status="green")
    return h["status"], h["active_shards"], h["unassigned_shards"]

health()                      # ('green', 6, 0)
```

3 первичных шарда и по одной реплике — всего 6 копий.

## Где лежат шарды

```python
print(client.cat.shards(index="orders", h="shard,prirep,docs,node", s="shard,prirep", v=True))
```

```text
shard prirep docs node
0     p         6 os-node2
0     r         6 os-node1
1     p        13 os-node1
1     r        13 os-node3
2     p        11 os-node3
2     r        11 os-node2
```

Две копии одного шарда никогда не оказываются на одном узле. Документ попадает в шард по хешу значения маршрутизации (по умолчанию `_id`), поэтому поровну не всегда: здесь 6, 13 и 11 документов.

## Цвет кластера

| Статус | Значит |
|---|---|
| green | все копии назначены |
| yellow | все первичные есть, часть реплик нет: данные доступны, запас прочности меньше |
| red | хотя бы одного первичного нет: часть данных недоступна |

## Что будет, если узел пропал

```text
$ docker stop os-node3

status yellow, 2 nodes, active_shards 4, unassigned_shards 2, delayed_unassigned_shards 2
... 75 seconds later
status green, active_shards 6, unassigned_shards 0      the missing replicas were rebuilt on the two remaining nodes
```

Сначала кластер ждёт: узел может вернуться после перезапуска. По умолчанию задержка `index.unassigned.node_left.delayed_timeout` равна `1m`, затем реплики строятся заново. Верните узел: `docker start os-node3`.

## Число шардов и реплик

```python
from opensearchpy.exceptions import RequestError

client.indices.put_settings(index="orders", body={"number_of_replicas": 2})
health()                      # ('green', 9, 0)      replicas can be changed at any time

try:
    client.indices.put_settings(index="orders", body={"number_of_shards": 6})
except RequestError as e:
    print(e.status_code)      # 400                  primaries are fixed at creation

client.indices.put_settings(index="orders", body={"number_of_replicas": 1})
```

## Частые ошибки новичков

- Много мелких шардов «на вырост»: каждый стоит памяти и участвует в поиске.
- Считать реплики резервной копией: удаление и порча данных повторяются на репликах. Копии делают снимками (snapshots).
- Паниковать из-за yellow на одном узле: реплике там просто негде разместиться.

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [01 · Основы OpenSearch](../01-opensearch-basics/basics.ru.md)

## Источники

- [Cluster health API](https://docs.opensearch.org/latest/api-reference/cluster-api/cluster-health/)
- [Creating a cluster](https://docs.opensearch.org/latest/tuning-your-cluster/)
