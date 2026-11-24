---
title: "Dead-lettering и повторы: продвинутый уровень"
lang: ru
track: rabbitmq
kind: module
doc: advanced
slug: 06-dead-lettering-and-retries
part: 2
tags: [rabbitmq, retry, delayed-retry, x-death, parking-lot, poison-message, delivery-limit]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Dead-lettering и повторы: продвинутый уровень

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

Опирается на [базовую теорию](basics.ru.md). Код запускался на RabbitMQ 4.4.1 с pika 1.5.0.

## Повтор с задержкой

Немедленный возврат неудачного сообщения (`requeue=True`) бьёт по тому же сбою в ту же миллисекунду. Нужна **задержка**. Её собирают из двух очередей и двух DLX:

```text
main --(reject, requeue=False)--> rx --> retry (TTL 300 мс) --(expired)--> default exchange --> main
```

```python
ch.exchange_declare("rx", "direct")
ch.queue_declare("main", durable=True, arguments={
    "x-dead-letter-exchange": "rx", "x-dead-letter-routing-key": "retry"})
ch.queue_declare("retry", durable=True, arguments={
    "x-message-ttl": 300,
    "x-dead-letter-exchange": "", "x-dead-letter-routing-key": "main"})
ch.queue_bind("retry", "rx", "retry")
ch.queue_declare("parking", durable=True)
```

В `retry` нет потребителей: сообщение лежит там `x-message-ttl` мс, «умирает» по сроку и возвращается в `main` через обменник по умолчанию. Потребитель считает число попыток по `x-death` и после третьей отправляет сообщение в очередь для ручного разбора (`parking`):

```python
def on_msg(chn, m, p, b):
    deaths = p.headers.get("x-death", []) if p.headers else []
    n = sum(d["count"] for d in deaths if d["queue"] == "main")   # rejections so far
    attempts.append((time.perf_counter(), n))
    if n >= 3:
        chn.basic_publish("", "parking", b, pika.BasicProperties(headers={"attempts": n + 1}))
        chn.basic_ack(m.delivery_tag)
    else:
        chn.basic_reject(m.delivery_tag, requeue=False)
```

```text
attempts: [(0.04, 0), (0.35, 1), (0.65, 2), (0.95, 3)]
parked: 1
parking headers: {'attempts': 4}
```

Четыре попытки с интервалом около 0,3 с (TTL очереди `retry`): счётчик `n` растёт 0, 1, 2, 3, на третьем повторе сообщение уходит в `parking`.

### Что накапливается в `x-death`

После двух циклов у сообщения две записи, у каждой свой `count`:

```text
{'count': 2, 'reason': 'expired', 'queue': 'retry', 'routing-keys': ['retry']}
{'count': 2, 'reason': 'rejected', 'queue': 'main', 'routing-keys': ['main']}
{'x-first-death-exchange': '', 'x-first-death-queue': 'main', 'x-first-death-reason': 'rejected', 'x-last-death-exchange': 'rx', 'x-last-death-queue': 'retry', 'x-last-death-reason': 'expired'}
```

Запись группируется по паре (очередь, причина), а `count` — сколько раз. Число отказов в `main` — это `count` записи с `queue == "main"` и `reason == "rejected"`. Суммируя все записи очереди `main`, вы смешаете причины, поэтому в боевом коде фильтруйте ещё и по `reason`.

## DLX без маршрута

Если указанный DLX не существует или нет подходящей привязки, сообщение **исчезает**, а ошибки нет:

```text
orphan DLX depth: 0 open: True
```

Канал остался открытым, сообщения нигде нет. Поэтому DLX и очередь для разбора создают заранее, а мониторят глубину очереди разбора.

## Переменная задержка

TTL очереди `retry` один на всех. Для нарастающей задержки (backoff) заводят несколько очередей с разным TTL (например, `retry.1s`, `retry.10s`, `retry.60s`) и по `x-death`-счётчику выбирают ключ маршрутизации. TTL отдельного сообщения (`expiration`) работает только на голове очереди ([модуль 03](../03-queues-and-delivery/advanced.ru.md#ttl-сообщений)), поэтому для очереди задержки с разными значениями он не годится.

## Delivery limit quorum-очередей

Для quorum-очередей в документации описан `x-delivery-limit` (и ключ политики `delivery-limit`): после превышения числа доставок сообщение отбрасывается или уходит в DLX. Тест: quorum-очередь с `x-delivery-limit: 3` и DLX, потребитель в цикле делает `nack(requeue=True)`.

```text
q1 deliveries: 140 dead-lettered: None None
```

Лимит не сработал: за 2 с было 140 доставок, в очередь разбора ничего не пришло. Позже причину нашли ([модуль 07](../07-queue-types/advanced.ru.md#delivery-limit)): **`basic_nack(requeue=True)` не увеличивает счётчик доставок, а `basic_reject(requeue=True)` и обрыв канала — увеличивают**. С `reject` тот же тест даёт 4 доставки и сообщение в DLX с причиной `delivery_limit`:

```text
C consumer+reject: deliveries 4 [None, 1, 2, 3] dead 1
```

> ⚠️ Проверить: почему `nack` не считается, здесь не выяснено (наблюдение, а не документированная гарантия). Для защиты от «ядовитых» сообщений используйте `reject` или `requeue=False` с DLX.

## Лучшие практики

- На каждую рабочую очередь — DLX и очередь для разбора (`parking`/`dead`).
- Неисправимые ошибки (формат, валидация) — сразу в DLX; временные (сеть, блокировка) — повтор с задержкой.
- Число попыток считать по `x-death` (фильтр по очереди **и** причине) или своим заголовком.
- После исчерпания попыток — в очередь ручного разбора с контекстом ошибки в заголовках.
- Мониторить глубину очередей разбора и `retry`.
- Обработчик идемпотентен: повтор может прийти после частичного успеха.

## Частые ошибки

- ❌ Повтор без задержки → ✅ очередь `retry` с TTL.
- ❌ Бесконечный цикл main → retry → main → ✅ предел попыток и `parking`.
- ❌ Суммировать `count` по всем записям → ✅ фильтр по очереди и причине.
- ❌ Положиться на delivery limit без проверки → ✅ проверить на своей версии.
- ❌ DLX без очереди → ✅ создать топологию заранее.

## Связанные материалы

- Раньше: [основы](basics.ru.md)
- Дальше: [экспертная теория](expert.ru.md)
- [03 · Очереди и доставка](../03-queues-and-delivery/advanced.ru.md) — TTL и лимиты длины
- [05 · Надёжность](../05-reliability/advanced.ru.md) — идемпотентный потребитель
- Рецепт `task-queue-with-retries`: [дорожная карта](../../overview.ru.md)

## Источники

- [Dead Letter Exchanges](https://www.rabbitmq.com/docs/dlx)
- [Quorum Queues: Poison Message Handling](https://www.rabbitmq.com/docs/quorum-queues)
- [Time-To-Live and Expiration](https://www.rabbitmq.com/docs/ttl)
