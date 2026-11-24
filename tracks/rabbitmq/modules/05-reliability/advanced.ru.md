---
title: "Надёжность: продвинутый уровень"
lang: ru
track: rabbitmq
kind: module
doc: advanced
slug: 05-reliability
part: 2
tags: [rabbitmq, publisher-confirms, nack, transactions, idempotency, message-id, throughput]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Надёжность: продвинутый уровень

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

Опирается на [базовую теорию](basics.ru.md) (помощник `Confirmed`). Код запускался на RabbitMQ 4.4.1 с pika 1.5.0.

## Nack от брокера

Брокер может ответить `nack`: например, очередь с `x-overflow: reject-publish` переполнена ([модуль 03](../03-queues-and-delivery/advanced.ru.md#ограничения-длины-и-overflow)). Это сигнал издателю повторить позже или отказаться:

```python
ch.queue_declare("lim", durable=True, arguments={"x-max-length": 1, "x-overflow": "reject-publish"})
print("first:", pub.publish("", "lim", b"1"), "second:", pub.publish("", "lim", b"2"))
```

```text
first: ack second: nack
```

Без confirms издатель не узнал бы об отказе.

## Ошибки, при которых подтверждения не будет

Публикация в несуществующий обменник закрывает канал (`404 NOT_FOUND`), и подтверждения не приходит совсем:

```text
missing exchange: timeout channel open: False
```

Поэтому у ожидания подтверждения всегда должен быть таймаут, а после закрытия канала нужно открыть новый и включить confirms заново. Все неподтверждённые публикации считаются **неизвестными**: они могли дойти, а могли нет.

## Пропускная способность

Ждать подтверждения после каждого сообщения дорого. Три способа публикации 300 сообщений (на Docker Desktop под Windows; цифры зависят от окружения, важны соотношения):

```text
300 one-by-one confirmed: 13.43s
300 pipelined confirms: 0.048s acked=300
300 no confirms (fire-and-forget): 0.005s
```

Конвейерный вариант отправляет всё подряд и считает подтверждения в обратном вызове (`multiple=True` подтверждает сразу пачку):

```python
acked, ev, lock = [0], threading.Event(), threading.Lock()
def on_confirm(frame):
    with lock:
        if isinstance(frame.method, pika.spec.Basic.Ack):
            acked[0] += (frame.method.delivery_tag - acked[0]) if frame.method.multiple else 1
        if acked[0] >= N: ev.set()
ch2.confirm_delivery(on_confirm)
for i in range(N): ch2.basic_publish("", "rel", b"x")
ev.wait(10)
```

Почти в 300 раз быстрее, но этот счётчик учитывает только `ack`: в боевом коде нужно хранить множество неподтверждённых тегов (из `on_publish`) и обрабатывать `nack` и обрыв соединения: неподтверждённые публикуются повторно.

## Транзакции

В AMQP есть транзакции (`tx_select`, `tx_commit`, `tx_rollback`), но в новом `pika.Connection` их **нет**: у канала нет методов `tx_*`. Они доступны только в устаревшем `BlockingConnection`. Проверено:

```text
tx before commit: 0
after rollback: 0
after commit: 1
100 tx commits: 4.50s
```

Сообщения не видны до `tx_commit`, `tx_rollback` отбрасывает их. Но коммит каждой публикации стоит дорого (100 коммитов — 4,5 с против 0,05 с конвейерных confirms на 300 сообщений), поэтому используйте publisher confirms, а транзакции оставьте для редких случаев «всё или ничего» над несколькими сообщениями.

## Идемпотентный потребитель

Дубликаты неизбежны. Если у сообщения есть `message_id`, потребитель помнит обработанные и пропускает повторы:

```python
for i in range(3):
    ch.basic_publish("", "idem", b"pay", pika.BasicProperties(message_id="order-42"))
seen, processed = set(), []
def cb(chn, m, p, b):
    if p.message_id not in seen:
        processed.append(p.message_id); seen.add(p.message_id)   # do the work here
    chn.basic_ack(m.delivery_tag)
```

```text
processed: ['order-42']
```

Три копии — одна обработка. В памяти это работает лишь пока жив процесс; в продакшене идентификаторы хранят в базе вместе с результатом работы **в одной транзакции** (или делают саму операцию идемпотентной: «установить статус», а не «увеличить счётчик»).

## Лучшие практики

- Confirms на всех издателях, важных для бизнеса; таймаут ожидания.
- Пакетная или асинхронная отправка вместо «один → ждём».
- `mandatory`, если маршрут может отсутствовать.
- Неподтверждённые после обрыва повторять; потребитель идемпотентен.
- Persistent + durable для данных, которые нельзя терять.

## Частые ошибки

- ❌ Ждать confirm после каждого сообщения при большом потоке → ✅ конвейер с тегами.
- ❌ Без таймаута ждать confirm → ✅ таймаут и повторное открытие канала.
- ❌ Транзакции вместо confirms → ✅ confirms.
- ❌ Идемпотентность только в памяти процесса → ✅ в базе, вместе с результатом.
- ❌ Повторять публикацию без `message_id` → ✅ иначе дубликаты не распознать.

## Связанные материалы

- Раньше: [основы](basics.ru.md)
- Дальше: [экспертная теория](expert.ru.md)
- Следующий модуль: [06 · Dead-lettering и повторы](../06-dead-lettering-and-retries/overview.ru.md)

## Источники

- [Consumer Acknowledgements and Publisher Confirms](https://www.rabbitmq.com/docs/confirms)
- [Publishers](https://www.rabbitmq.com/docs/publishers)
- [pika: publisher confirms](https://pika.readthedocs.io/en/stable/)
