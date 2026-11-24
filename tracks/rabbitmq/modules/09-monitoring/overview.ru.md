---
title: "Мониторинг и эксплуатация"
lang: ru
track: rabbitmq
kind: module
doc: overview
slug: 09-monitoring
part: 2
levels: [1, 2, 3, 4]
tags: [rabbitmq, monitoring, management-api, prometheus, alarms, health-check]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Мониторинг и эксплуатация

🌐 **Русский** · [English](overview.en.md)

## Коротко

Следить нужно за очередями (глубина, неподтверждённые, потребители), соединениями и каналами, а также за узлами (память, диск, оповещения). Инструменты: плагин управления (веб-интерфейс и HTTP API на 15672), `rabbitmqctl`/`rabbitmq-diagnostics` и метрики Prometheus на 15692.

## Ключевые идеи

- **`messages = messages_ready + messages_unacknowledged`**; смотрите их отдельно.
- **HTTP API:** `/api/queues/%2F/<очередь>`, `/api/connections`, `/api/channels`, `/api/consumers`.
- **Называйте соединения** (`connection_name`) — их легко найти.
- **Prometheus:** `/metrics` — сводные метрики узла, `/metrics/per-object` — по каждой очереди.
- **Тревога по памяти/диску** блокирует издателей: `Connection.Blocked` (`low on memory`), health check отвечает `503`.
- **`check_local_alarms` в 4.4.1 — устаревшая заглушка;** используйте `rabbitmq-diagnostics alarms` или `/api/health/checks/alarms`.
- **Статистика management отстаёт** на секунды.

## Минимальный пример

```python
q = api("/queues/%2F/mon")
print(q["messages_ready"], q["messages_unacknowledged"], q["consumers"])   # 7 3 1
```

Всё запускалось на настоящем RabbitMQ 4.4.1 в Docker (см. [базовую теорию](basics.ru.md)).

## Когда применять и когда нет

| Нужно | Инструмент |
|---|---|
| Быстро посмотреть очереди | management UI / `rabbitmqctl list_queues` |
| История, графики, тревоги | Prometheus + Grafana |
| Проверка живости/готовности | `/api/health/checks/alarms`, `check_running` |
| Реакция клиента на нехватку ресурсов | `Connection.Blocked` |

- ❌ Не полагайтесь на устаревший `check_local_alarms`.
- ❌ Не оставляйте `guest`/`guest` в продакшене.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | HTTP API, соединения и каналы, CLI, health check |
| [2. Продвинутая теория](advanced.ru.md) | Prometheus, оповещения памяти и диска, что тревожить |
| [3. Экспертная теория](expert.ru.md) | справочник API, CLI и метрик, подвохи, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| 5. Запускаемые примеры | пока не создан — `/learn rabbitmq 09-monitoring level 5` |
| 6. Тесты | пока не созданы — `/learn rabbitmq 09-monitoring level 6` |

## Связанные материалы

- Пререквизиты: [03 · Очереди и доставка](../03-queues-and-delivery/overview.ru.md), [08 · Кластеризация](../08-clustering/overview.ru.md)
- Дальше: [10 · Управление потоком и производительность](../../overview.ru.md)
- Аналоги в других треках: [Linux](../../../linux/overview.ru.md)
