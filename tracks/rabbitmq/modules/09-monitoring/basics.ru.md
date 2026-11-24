---
title: "Мониторинг и эксплуатация: основы"
lang: ru
track: rabbitmq
kind: module
doc: basics
slug: 09-monitoring
part: 2
tags: [rabbitmq, monitoring, management-api, rabbitmqctl, queue-depth, unacked-messages]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Мониторинг и эксплуатация: основы

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [overview](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [03 · Очереди и доставка](../03-queues-and-delivery/basics.ru.md), [08 · Кластеризация](../08-clustering/basics.ru.md) · **Время:** ~1,5 ч

## Как проверялся код

Один узел RabbitMQ 4.4.1 в Docker (`rabbitmq:4-management`, порты 5672, 15672 и 15692) и pika 1.5.0; выводы скопированы из запусков. Помощник `api(path)` вызывает HTTP API управления (`http://localhost:15672/api…`, пользователь `guest`/`guest` учебного образа) и возвращает разобранный JSON, `sh(*cmd)` выполняет команду внутри контейнера (`docker exec rabbit …`). Для опыта в очередь `mon` опубликовано 10 сообщений, а потребитель с `prefetch_count=3` взял три и не подтвердил.

## Что смотреть в первую очередь

| Что | Зачем |
|---|---|
| глубина очереди (`messages_ready`) | копится ли работа |
| неподтверждённые (`messages_unacknowledged`) | сколько сообщений «в работе» и не завис ли потребитель |
| потребители (`consumers`) | кто-то вообще читает очередь |
| соединения и каналы | утечки, кто подключён |
| оповещения (alarms) | память и диск |

## HTTP API управления

Плагин управления (management) даёт веб-интерфейс на порту 15672 и HTTP API по тому же адресу:

```python
q = api("/queues/%2F/mon")        # %2F is the default vhost "/"
print({k: q.get(k) for k in ("messages", "messages_ready", "messages_unacknowledged", "consumers", "state", "type")})
```

```text
{'messages': 10, 'messages_ready': 7, 'messages_unacknowledged': 3, 'consumers': 1, 'state': 'running', 'type': 'classic'}
```

`messages = messages_ready + messages_unacknowledged`: 7 готовых плюс 3 у потребителя. Те же числа — на уровне vhost:

```text
vhost totals: {'messages': 10, 'messages_ready': 7, 'messages_unacknowledged': 3}
```

В `message_stats` очереди лежат счётчики и скорости (`ack`, `deliver`, `deliver_get`, `get` …).

> Статистика в management обновляется с задержкой: перед чтением в опыте пришлось подождать несколько секунд. Не сравнивайте числа API с числами, полученными только что другим способом.

## Соединения, каналы, потребители

```text
connections: [('172.17.0.1:48330 -> 17', None), ('172.17.0.1:48346 -> 17', 'monitor-demo')]
channels: [(1, 0, 0), (1, 3, 3)]
consumers: [('mon', 3, True)]
```

- У соединения можно задать читаемое имя (`client_properties={"connection_name": "monitor-demo"}`): по нему легко найти приложение.
- Канал показывает `prefetch_count` (3) и число неподтверждённых (3): потребитель держит три сообщения.
- Потребитель — очередь `mon`, prefetch 3, `ack_required=True`.

## Командная строка

```text
$ rabbitmqctl list_queues name messages_ready messages_unacknowledged consumers
name  messages_ready  messages_unacknowledged  consumers
mon   7               3                        1

$ rabbitmq-diagnostics check_running
RabbitMQ on node rabbit@4ff649c7c4dc is fully booted and running
```

`rabbitmqctl list_queues` и `list_connections`/`list_channels`/`list_consumers` работают без HTTP; `rabbitmq-diagnostics` проверяет состояние узла.

## Проверка здоровья

```text
healthcheck: {'status': 'ok'}
```

`GET /api/health/checks/alarms` вернул `ok` при отсутствии оповещений; при тревоге он отвечает кодом 503 ([продвинутая теория](advanced.ru.md#оповещения-памяти-и-диска)).

## Типичные ошибки новичков

- ❌ Смотреть только `messages` → ✅ отдельно `messages_ready` и `messages_unacknowledged`.
- ❌ Не называть соединения → ✅ `connection_name` в `client_properties`.
- ❌ Считать числа API мгновенными → ✅ статистика обновляется с задержкой.
- ❌ Оставить `guest`/`guest` → ✅ отдельный пользователь с минимальными правами ([модуль 11](../11-security/overview.ru.md)).
- ❌ Мониторить только очереди → ✅ ещё и узлы: память, диск, оповещения.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — Prometheus, оповещения
- Раньше: [08 · Кластеризация](../08-clustering/basics.ru.md), [03 · Очереди и доставка](../03-queues-and-delivery/basics.ru.md)
- Следующий модуль: [10 · Управление потоком и производительность](../10-performance/overview.ru.md)

## Источники

- [Management Plugin](https://www.rabbitmq.com/docs/management)
- [Monitoring](https://www.rabbitmq.com/docs/monitoring)
- [rabbitmqctl](https://www.rabbitmq.com/docs/man/rabbitmqctl.8)
