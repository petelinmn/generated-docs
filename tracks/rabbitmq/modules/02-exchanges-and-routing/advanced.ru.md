---
title: "Обменники и маршрутизация: продвинутый уровень"
lang: ru
track: rabbitmq
kind: module
doc: advanced
slug: 02-exchanges-and-routing
part: 1
tags: [rabbitmq, exchange, mandatory, alternate-exchange, unroutable-messages, exchange-binding]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Обменники и маршрутизация: продвинутый уровень

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

Опирается на [базовую теорию](basics.ru.md) (те же помощники `drain` и `setup`). Все выводы получены на настоящем RabbitMQ 4.4.1.

## Немаршрутизируемые сообщения и `mandatory`

Сообщение, не подошедшее ни одной привязке, отбрасывается без ошибки. Если терять его нельзя, публикуйте с `mandatory=True`: тогда брокер **вернёт** сообщение издателю через `basic.return`. В pika зарегистрируйте обработчик возврата:

```python
returns = []
ch.add_on_return_callback(lambda c, m, p, b: returns.append((m.reply_code, m.reply_text, m.routing_key, b)))
ch.basic_publish("d", "nokey", b"x", mandatory=True)
time.sleep(0.5)
print("returned:", returns)
```

```text
returned: [(312, 'NO_ROUTE', 'nokey', b'x')]
```

Возврат асинхронен, поэтому издателю нужен обработчик, а чтобы узнать, что сообщение *было* маршрутизировано, — publisher confirms ([модуль 05](../05-reliability/overview.ru.md).

## Alternate exchange

Обменник может указать **альтернативный обменник** (alternate exchange): сообщения, которые он не смог маршрутизировать, уходят туда, а не исчезают. Так «мёртвый» трафик собирается в одной очереди для разбора:

```python
ch.exchange_declare("unrouted", exchange_type="fanout")
uq = ch.queue_declare("", exclusive=True).method.queue
ch.queue_bind(uq, "unrouted")
ch.exchange_declare("main", exchange_type="direct", arguments={"alternate-exchange": "unrouted"})
mq = ch.queue_declare("", exclusive=True).method.queue
ch.queue_bind(mq, "main", routing_key="ok")
ch.basic_publish("main", "ok", b"routed")
ch.basic_publish("main", "bad", b"unrouted")
print("main:", drain(mq), "alt:", drain(uq))
```

```text
main: ['routed'] alt: ['unrouted']
```

Делайте альтернативный обменник `fanout`, чтобы на втором шаге снова ничего не терялось.

## Привязки обменник-обменник

Обменники можно привязывать к обменникам, строя граф маршрутизации: грубый обменник рассылает в topic-обменники и так далее.

```python
ch.exchange_declare("src", exchange_type="fanout")
ch.exchange_declare("dst", exchange_type="topic")
ch.exchange_bind("dst", "src")
q = ch.queue_declare("", exclusive=True).method.queue
ch.queue_bind(q, "dst", routing_key="#")
ch.basic_publish("src", "k", b"hop")
print("e2e:", drain(q))
```

```text
e2e: ['hop']
```

## Одна очередь, несколько подходящих привязок

Если подходят несколько привязок одной очереди, очередь всё равно получает **одну** копию. Тест с `a.b` и `a.#`, привязанными к одной очереди, и ключом `a.b` оставил в ней ровно одно сообщение.

## Internal и auto-delete обменники

- **`internal=True`:** производители не могут публиковать в него; он служит целью только для привязок других обменников. Публикация в такой обменник в тесте закрыла канал.
- **`auto_delete=True`:** обменник удаляется после снятия последней привязки. После последней отвязки пассивное объявление вернуло `404`.

## Правила объявления

- Повторное объявление с другим типом падает: `406 PRECONDITION_FAILED - inequivalent arg 'type' for exchange 'd' … received 'fanout' but current is 'direct'`. То же относится к другим аргументам.
- Неизвестный тип отклоняется: `406 PRECONDITION_FAILED - unknown exchange type 'nonsense'`.
- Обе ошибки закрывают канал.
- Имена с префиксом `amq.` клиент объявить не может: `403 ACCESS_REFUSED - exchange name 'amq.custom' contains reserved prefix 'amq.*'`.
- Обменник по умолчанию нельзя удалить или привязать к нему: `403 ACCESS_REFUSED - operation not permitted on the default exchange`.

## Лучшие практики

- Объявляйте обменники и привязки в одной процедуре запуска, общей для производителей и потребителей.
- Предпочитайте `topic` с дисциплинированными ключами (`<домен>.<сущность>.<событие>`) множеству `direct`-обменников.
- Важным обменникам задавайте alternate exchange или публикуйте с `mandatory`.
- Держите ключи маршрутизации короткими, строчными, без пробелов.

## Частые ошибки

- ❌ Считать, что отброшенное сообщение вызывает ошибку → ✅ `mandatory` или alternate exchange.
- ❌ Объявлять обменник только у потребителя → ✅ производитель может стартовать первым и опубликовать в пустоту.
- ❌ Привязать очередь дважды и ждать две копии → ✅ будет одна копия.

## Связанные материалы

- Дальше: [экспертная теория](expert.ru.md)
- Раньше: [01 · Обмен сообщениями и модель AMQP](../01-messaging-basics/advanced.ru.md)
- Следующий модуль: [03 · Очереди и доставка](../03-queues-and-delivery/advanced.ru.md)

## Источники

- [Exchanges](https://www.rabbitmq.com/docs/exchanges)
- [Alternate Exchanges](https://www.rabbitmq.com/docs/ae)
- [Publishers](https://www.rabbitmq.com/docs/publishers)
