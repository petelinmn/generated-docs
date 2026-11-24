---
title: "Очереди и доставка: основы"
lang: ru
track: rabbitmq
kind: module
doc: basics
slug: 03-queues-and-delivery
part: 1
tags: [rabbitmq, queue, acknowledgement, prefetch, durability, redelivery, persistent-message]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Очереди и доставка: основы

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [overview](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [01 · Обмен сообщениями и модель AMQP](../01-messaging-basics/basics.ru.md), [02 · Обменники и маршрутизация](../02-exchanges-and-routing/basics.ru.md) · **Время:** ~1,5 ч

## Как проверялся код

Весь код запускался на настоящем RabbitMQ 4.4.1 в Docker с pika 1.5.0; выводы скопированы из запусков. Помощники ниже: `ch` — канал, `count(q)` ждёт 0,3 с и возвращает число готовых сообщений `q` пассивным объявлением, `fresh(q, **kw)` удаляет и заново объявляет durable-очередь.

## Ack, nack, reject

На каждую доставку потребитель сообщает, что произошло:

- `basic_ack(tag)` — готово; брокер забывает сообщение.
- `basic_nack(tag, requeue=…)` — сбой (можно отказаться от многих сразу через `multiple=True`).
- `basic_reject(tag, requeue=…)` — сбой (одно сообщение).

С `requeue=True` сообщение возвращается в очередь; с `requeue=False` отбрасывается (или уходит в dead-letter, если у очереди есть DLX).

```python
fresh("jobs")
for i in range(3):
    ch.basic_publish("", "jobs", f"m{i}".encode())
m1, _, b1 = ch.basic_get("jobs")
m2, _, b2 = ch.basic_get("jobs")
m3, _, b3 = ch.basic_get("jobs")
print("ready while three are unacked:", count("jobs"))
ch.basic_ack(m1.delivery_tag)
ch.basic_nack(m2.delivery_tag, requeue=True)
ch.basic_reject(m3.delivery_tag, requeue=False)
print("after ack / nack(requeue) / reject(drop):", count("jobs"))
m, _, b = ch.basic_get("jobs")
print("requeued message:", b, "redelivered:", m.redelivered)
ch.basic_ack(m.delivery_tag)
```

```text
ready while three are unacked: 0
after ack / nack(requeue) / reject(drop): 1
requeued message: b'm1' redelivered: True
```

Неподтверждённые сообщения не «готовы»: ими владеет потребитель. Сообщения были `m0`, `m1`, `m2`: `m0` подтверждено (исчезло), `m1` отклонено с возвратом (снова в очереди, `redelivered: True`), `m2` отклонено без возврата (отброшено).

## Умерший потребитель возвращает сообщения

Если канал или соединение закрываются с неподтверждёнными сообщениями, брокер возвращает их:

```python
ch.basic_publish("", "jobs", b"crash")
c2 = pika.Connection(pika.ConnectionParameters("localhost"))
mm, _, _ = c2.channel().basic_get("jobs")      # takes it, never acks
c2.close()
time.sleep(0.5)
m, _, b = ch.basic_get("jobs")
print("after consumer died:", b, "redelivered:", m.redelivered)
```

```text
after consumer died: b'crash' redelivered: True
```

Это основа обработки «минимум один раз»: сообщение может обработаться дважды, поэтому обработчики должны переносить дубликаты.

## Prefetch и справедливость

По умолчанию брокер присылает столько сообщений, сколько может. `basic_qos(prefetch_count=N)` ограничивает число **неподтверждённых** сообщений на потребителя (на канал). Тест: шесть сообщений, два потребителя, A никогда не подтверждает, B подтверждает сразу, `prefetch_count=1`:

```text
prefetch=1: {'A': ['t0'], 'B': ['t1', 't2', 't3', 't4', 't5']}
```

A взял одно сообщение и дальше пропускался, потому что исчерпал квоту; остальное сделал B. Без лимита:

```text
prefetch=0 (unlimited): {'A': ['t0', 't1', 't2', 't3', 't4', 't5'], 'B': []}
```

A подписался первым, ему сразу отдали всё, и он на нём сидел. Задавайте небольшой prefetch (часто 1 для медленных задач, больше для быстрых), чтобы работа распределялась.

## Durable и persistent

Перезапуск брокера (проверено `docker restart`) стирает то, что не сохранено:

| Что | Переживает перезапуск |
|---|---|
| не-durable очередь | нет (а 4.4 не создаст её, если она не exclusive) |
| durable-очередь | сама очередь — да |
| сообщение в ней | только если persistent (`delivery_mode=2`) |

```python
ch.queue_declare("dur", durable=True)
ch.basic_publish("", "dur", b"transient")
ch.basic_publish("", "dur", b"persistent", pika.BasicProperties(delivery_mode=pika.DeliveryMode.Persistent))
# docker restart rabbit
```

```text
before restart: 2
after restart: 1
[b'persistent']
```

«Persistent» само по себе не гарантирует сохранности: сообщение всё ещё может потеряться между публикацией и записью брокером. Этот разрыв закрывают publisher confirms ([модуль 05](../05-reliability/overview.ru.md).

## Типичные ошибки новичков

- ❌ Durable-очередь с transient-сообщениями → ✅ ещё и `delivery_mode=2`.
- ❌ Нет prefetch при медленных задачах → ✅ `basic_qos(prefetch_count=1)`.
- ❌ Забыть подтвердить → ✅ неподтверждённые копятся и возвращаются при разрыве.
- ❌ Бесконечно возвращать «ядовитое» сообщение → ✅ `requeue=False` плюс DLX ([модуль 06](../06-dead-lettering-and-retries/overview.ru.md).
- ❌ Подтверждать до завершения работы → ✅ подтверждать последним.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — TTL, лимиты длины, срок жизни
- Раньше: [02 · Обменники и маршрутизация](../02-exchanges-and-routing/basics.ru.md)
- Следующий модуль: [04 · Шаблоны обмена сообщениями](../04-messaging-patterns/overview.ru.md)

## Источники

- [Consumer Acknowledgements and Publisher Confirms](https://www.rabbitmq.com/docs/confirms)
- [Consumer Prefetch](https://www.rabbitmq.com/docs/consumer-prefetch)
- [Queues](https://www.rabbitmq.com/docs/queues)
- [Reliability Guide](https://www.rabbitmq.com/docs/reliability)
