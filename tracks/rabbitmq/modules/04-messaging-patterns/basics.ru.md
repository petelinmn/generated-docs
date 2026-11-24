---
title: "Шаблоны обмена сообщениями: основы"
lang: ru
track: rabbitmq
kind: module
doc: basics
slug: 04-messaging-patterns
part: 1
tags: [rabbitmq, work-queue, publish-subscribe, routing, rpc, correlation-id, reply-to]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Шаблоны обмена сообщениями: основы

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [overview](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [02 · Обменники и маршрутизация](../02-exchanges-and-routing/basics.ru.md), [03 · Очереди и доставка](../03-queues-and-delivery/basics.ru.md) · **Время:** ~1,5 ч

## Как проверялся код

Все фрагменты запускались на настоящем RabbitMQ 4.4.1 в Docker с pika 1.5.0; выводы скопированы из запусков. Помощник `P()` создаёт соединение: `P = lambda: pika.Connection(pika.ConnectionParameters("localhost"))`; `ch` — канал основного соединения. Обработчики `basic_consume` вызываются на рабочем потоке pika, поэтому отдельный цикл не нужен — достаточно `time.sleep` в примерах.

Четыре классических шаблона строятся из одних и тех же кирпичей (обменник, очередь, привязка) и различаются тем, **кто делит сообщения и кто их копирует**.

| Шаблон | Обменник | Очереди | Смысл |
|---|---|---|---|
| Work queue | default | одна общая | каждое сообщение получает один воркер |
| Publish/subscribe | fanout | по очереди на подписчика | каждое сообщение получают все |
| Routing | direct (или topic) | по очереди на интерес | подписчик выбирает ключи |
| RPC | default | очередь запросов + очередь ответов | запрос и ответ, связанные `correlation_id` |

## Work queue

Одна очередь, несколько воркеров: брокер раздаёт сообщения по кругу, а `prefetch_count=1` не даёт медленному воркеру копить задачи ([модуль 03](../03-queues-and-delivery/basics.ru.md#prefetch-и-справедливость)). Тест: десять задач, воркер `w1` тратит 0,5 с на задачу, `w2` — 0,05 с.

```python
ch.queue_declare("tasks", durable=True)

def worker(name, delay):
    c = P(); x = c.channel(); x.basic_qos(prefetch_count=1)
    def cb(chn, m, p, body):
        time.sleep(delay)                      # pretend to work
        done[name].append(body.decode())
        chn.basic_ack(m.delivery_tag)          # ack after the work
    x.basic_consume("tasks", cb)
    return c

w1 = worker("w1", 0.5); w2 = worker("w2", 0.05)
for i in range(10):
    ch.basic_publish("", "tasks", f"job{i}".encode(),
                     pika.BasicProperties(delivery_mode=pika.DeliveryMode.Persistent))
```

```text
work queue: {'w1': 1, 'w2': 9} total 10
```

Быстрый воркер забрал девять задач из десяти, потому что освобождался чаще: распределение идёт «по готовности», а не поровну. Без `prefetch_count` брокер отдал бы каждому по половине заранее.

## Publish/subscribe

Fanout-обменник копирует каждое сообщение во все привязанные очереди. У каждого подписчика — своя **временная очередь**: `queue_declare("", exclusive=True)` просит брокера придумать имя, а очередь исчезает вместе с соединением.

```python
ch.exchange_declare("logs", "fanout")

def sub(name):
    c = P(); x = c.channel()
    q = x.queue_declare("", exclusive=True).method.queue   # server-named
    x.queue_bind(q, "logs")
    x.basic_consume(q, lambda chn, m, p, b: got[name].append(b.decode()), auto_ack=True)
    return c, q

for i in range(3):
    ch.basic_publish("logs", "", f"log{i}".encode())
```

```text
server-named queue: True
pub/sub: {'A': ['log0', 'log1', 'log2'], 'B': ['log0', 'log1', 'log2']}
late subscriber: ['log3']
```

Имя временной очереди начинается с `amq.gen-`. Подписчик, пришедший позже, получает только новые сообщения (`log3`), а сообщение, опубликованное при **нуле** привязанных очередей, пропадает: fanout ничего не хранит.

## Маршрутизация по ключам

Direct-обменник доставляет сообщение в очереди, чей ключ привязки равен ключу маршрутизации. Один подписчик слушает только `error`, другой — все три уровня:

```python
ch.exchange_declare("sev", "direct")
# s1 binds "error"; s2 binds "info", "warning", "error"
for k in ("info", "warning", "error"):
    ch.basic_publish("sev", k, f"{k}-msg".encode())
```

```text
routing: {'errors': [('error', 'error-msg')], 'all': [('info', 'info-msg'), ('warning', 'warning-msg'), ('error', 'error-msg')]}
```

Одна очередь может иметь несколько привязок, а одно сообщение попадает в **каждую** подходящую очередь. Для иерархических ключей (`orders.eu.created`) используйте topic-обменник ([модуль 02](../02-exchanges-and-routing/basics.ru.md)).

## RPC

Запрос-ответ поверх очередей: клиент отправляет запрос в очередь `rpc` и указывает в свойствах, **куда** ответить (`reply_to`) и **как опознать** ответ (`correlation_id`). Сервер отвечает в очередь из `reply_to` и копирует `correlation_id`.

```python
def on_req(chn, m, p, body):                   # server
    r = str(fib(int(body))).encode()
    chn.basic_publish("", p.reply_to, r, pika.BasicProperties(correlation_id=p.correlation_id))
    chn.basic_ack(m.delivery_tag)

reply_q = cx.queue_declare("", exclusive=True).method.queue   # client
cx.basic_consume(reply_q, lambda chn, m, p, b: replies.__setitem__(p.correlation_id, b.decode()), auto_ack=True)
for n in (10, 20, 5):
    cid = str(uuid.uuid4())
    cx.basic_publish("", "rpc", str(n).encode(), pika.BasicProperties(reply_to=reply_q, correlation_id=cid))
```

```text
rpc: [(10, '55'), (20, '6765'), (5, '5')]
```

Очередь ответов можно использовать для многих запросов; `correlation_id` связывает ответ с запросом. Если сервер не запущен, запрос ждёт в очереди и ответа нет:

```text
no reply without server: False queued: 1
```

> RPC поверх брокера — удобный, но не бесплатный шаблон: клиент должен сам ставить таймауты и сопоставлять ответы. См. [продвинутую теорию](advanced.ru.md).

## Типичные ошибки новичков

- ❌ Один потребитель на общую очередь в pub/sub → ✅ по очереди на подписчика (иначе сообщения делятся, а не копируются).
- ❌ Публикация в fanout до появления подписчиков → ✅ сначала очереди и привязки, потом публикация.
- ❌ Не копировать `correlation_id` в ответ → ✅ клиент не сможет связать ответ с запросом.
- ❌ Work queue без prefetch и без ручного ack → ✅ `basic_qos(1)` и `basic_ack` после работы.
- ❌ Именованная очередь для временного подписчика → ✅ `queue_declare("", exclusive=True)`.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — direct reply-to, таймауты RPC, выбор шаблона
- Раньше: [03 · Очереди и доставка](../03-queues-and-delivery/basics.ru.md), [02 · Обменники и маршрутизация](../02-exchanges-and-routing/basics.ru.md)
- Следующий модуль: [05 · Надёжность](../05-reliability/overview.ru.md)
- Аналог в другом треке: [фоновые задачи в C#](../../../csharp/recipes/background-jobs/overview.ru.md)

## Источники

- [RabbitMQ tutorials](https://www.rabbitmq.com/tutorials)
- [Direct Reply-to](https://www.rabbitmq.com/docs/direct-reply-to)
- [Exchanges](https://www.rabbitmq.com/docs/exchanges)
