---
title: "Dead-lettering и повторы: основы"
lang: ru
track: rabbitmq
kind: module
doc: basics
slug: 06-dead-lettering-and-retries
part: 2
tags: [rabbitmq, dead-letter-exchange, dlx, x-death, poison-message, message-ttl]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Dead-lettering и повторы: основы

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [overview](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [03 · Очереди и доставка](../03-queues-and-delivery/basics.ru.md), [05 · Надёжность](../05-reliability/basics.ru.md) · **Время:** ~1,5 ч

## Как проверялся код

Все фрагменты запускались на настоящем RabbitMQ 4.4.1 в Docker с pika 1.5.0; выводы скопированы из запусков. Помощники: `get(q)` ждёт до 2 с и возвращает `(method, props, body)` первого сообщения очереди (через `basic_get` с `auto_ack`), `show(p)` печатает заголовок `x-death` в сокращённом виде (`queue`, `reason`, `count`, `exchange`, `routing-keys`).

## Зачем нужен dead-lettering

Сообщение, которое нельзя обработать, не должно ни зацикливаться (`requeue=True`, [модуль 03](../03-queues-and-delivery/expert.ru.md#7-циклы-возврата)), ни исчезать. **Dead letter exchange (DLX)** — обменник, куда брокер перенаправляет «мёртвые» сообщения очереди. Оттуда они попадают в обычную очередь для разбора.

Сообщение становится «мёртвым» (dead-lettered), если:

| Причина (`reason`) | Когда |
|---|---|
| `rejected` | потребитель сделал `reject`/`nack` с `requeue=False` |
| `expired` | истёк TTL сообщения или очереди |
| `maxlen` | сообщение вытеснено из очереди по `x-max-length` (`drop-head`) |

> Четвёртая причина, `delivery_limit`, относится к quorum-очередям ([модуль 07 в дорожной карте](../../overview.ru.md)). В [продвинутой теории](advanced.ru.md#delivery-limit-quorum-очередей) описано, что получилось при проверке.

## Настройка

DLX задаётся аргументами очереди: `x-dead-letter-exchange` и, по желанию, `x-dead-letter-routing-key` (иначе остаётся исходный ключ):

```python
ch.exchange_declare("dlx", "direct")
ch.queue_declare("dead", durable=True)
ch.queue_bind("dead", "dlx", "dead")
ch.queue_declare("work", durable=True, arguments={
    "x-dead-letter-exchange": "dlx",
    "x-dead-letter-routing-key": "dead",
})
```

## Отклонённое сообщение

```python
ch.basic_publish("", "work", b"bad")
m, _, _ = ch.basic_get("work")
ch.basic_reject(m.delivery_tag, requeue=False)
m, p, b = get("dead")
print("rejected:", b, show(p))
print("first-death:", p.headers.get("x-first-death-reason"), p.headers.get("x-first-death-queue"), p.headers.get("x-last-death-reason"))
print("routing key now:", m.routing_key)
```

```text
rejected: b'bad' [{'count': 1, 'reason': 'rejected', 'queue': 'work', 'exchange': '', 'routing-keys': ['work']}]
first-death: rejected work rejected
routing key now: dead
```

Брокер добавил заголовок `x-death` — список записей «кто, почему и сколько раз» — и сводные поля `x-first-death-*` и `x-last-death-*`. Ключ маршрутизации стал `dead` (из `x-dead-letter-routing-key`).

## Без DLX сообщение исчезает

Если у очереди нет DLX, `reject` с `requeue=False` просто отбрасывает сообщение:

```text
plain queue depth: 0
```

Поэтому на каждую рабочую очередь заводят DLX и очередь для разбора.

## Другие причины

Просроченное (`x-message-ttl` 200 мс) и вытесненное (`x-max-length: 1`) сообщения тоже попадают в DLX; причина записывается в `x-death`:

```text
expired: b'old' [{'count': 1, 'reason': 'expired', 'queue': 'ttlq', 'exchange': '', 'routing-keys': ['ttlq']}]
maxlen: b'first' [{'count': 1, 'reason': 'maxlen', 'queue': 'mlq', 'exchange': '', 'routing-keys': ['mlq']}]
```

При `maxlen` и `drop-head` «мёртвым» становится **самое старое** сообщение (`first`), а не новое.

## Типичные ошибки новичков

- ❌ Рабочая очередь без DLX → ✅ сообщения при `reject` пропадают; заведите DLX.
- ❌ `nack(requeue=True)` для неисправимой ошибки → ✅ `requeue=False` и DLX.
- ❌ DLX без привязанной очереди → ✅ сообщение молча исчезает ([продвинутая теория](advanced.ru.md#dlx-без-маршрута)).
- ❌ Не смотреть `x-death` → ✅ там причина и число попыток.
- ❌ Ожидать, что DLX сам повторит обработку → ✅ повтор нужно собрать (задержка + возврат), см. [продвинутую теорию](advanced.ru.md).

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — повторы с задержкой, «ядовитые» сообщения
- Раньше: [05 · Надёжность](../05-reliability/basics.ru.md), [03 · Очереди и доставка](../03-queues-and-delivery/basics.ru.md)
- Следующие модули: [07 · Типы очередей](../../overview.ru.md)

## Источники

- [Dead Letter Exchanges](https://www.rabbitmq.com/docs/dlx)
- [Time-To-Live and Expiration](https://www.rabbitmq.com/docs/ttl)
- [Queue Length Limits](https://www.rabbitmq.com/docs/maxlength)
