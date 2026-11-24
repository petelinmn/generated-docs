---
title: "Шаблоны обмена сообщениями: продвинутый уровень"
lang: ru
track: rabbitmq
kind: module
doc: advanced
slug: 04-messaging-patterns
part: 1
tags: [rabbitmq, rpc, direct-reply-to, timeout, request-expiration, competing-consumers]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Шаблоны обмена сообщениями: продвинутый уровень

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

Опирается на [базовую теорию](basics.ru.md). Код запускался на RabbitMQ 4.4.1 с pika 1.5.0.

## Direct reply-to

Вместо создания очереди ответов на каждого клиента можно использовать псевдоочередь `amq.rabbitmq.reply-to`: клиент подписывается на неё и указывает её в `reply_to`; брокер сам подставляет уникальный адрес и доставляет ответ на тот же канал, минуя настоящие очереди.

```python
cx.basic_consume("amq.rabbitmq.reply-to", lambda chn, m, p, b: out.append((p.correlation_id, b.decode())), auto_ack=True)
cx.basic_publish("", "rpc", b"12", pika.BasicProperties(reply_to="amq.rabbitmq.reply-to", correlation_id="d1"))
```

```text
direct reply-to: [('d1', '144')]
```

Правила, проверенные запуском:

- Подписка должна быть с `auto_ack=True`: с ручным подтверждением брокер закрывает канал — `406 PRECONDITION_FAILED - reply consumer cannot acknowledge`.
- Подписаться нужно **до** публикации запроса с таким `reply_to`: публикация без подписки закрыла канал (проверено: `is_open` стал `False`).
- Ответ приходит только на тот же канал, который отправил запрос. Если клиент упал, ответ пропадает: у него нет очереди, где он мог бы подождать.

> ⚠️ Проверить: последнее утверждение взято из документации и здесь не проверялось запуском.

## RPC-клиент с таймаутом

Брокер не знает, что вы ждёте ответа. Если сервер недоступен, клиент без таймаута повиснет. Клиент ниже хранит ожидающие запросы по `correlation_id`, ждёт события с таймаутом и ставит запросу `expiration`, равный таймауту:

```python
class RpcClient:
    def __init__(self):
        self.c = P(); self.ch = self.c.channel(); self.pending = {}
        self.ch.basic_consume("amq.rabbitmq.reply-to", self._on_reply, auto_ack=True)

    def _on_reply(self, ch, m, p, body):
        slot = self.pending.pop(p.correlation_id, None)   # unknown id -> ignored
        if slot: slot[1].append(body.decode()); slot[0].set()

    def call(self, body, timeout=1.0):
        cid = str(uuid.uuid4()); ev = threading.Event(); box = []
        self.pending[cid] = (ev, box)
        self.ch.basic_publish("", "rpc", body, pika.BasicProperties(
            reply_to="amq.rabbitmq.reply-to", correlation_id=cid,
            expiration=str(int(timeout * 1000))))          # stale requests expire
        if not ev.wait(timeout):
            self.pending.pop(cid, None)
            raise TimeoutError(f"no reply in {timeout}s")
        return box[0]
```

```text
timeout: no reply in 0.3s
stale request still queued: 0
reply: cba
processed by server: ['abc']
timeout: no reply in 0.3s
pending after late reply: 0
```

Разберём по строкам:

1. Сервера нет: клиент получил `TimeoutError`.
2. `expiration` убрал просроченный запрос из очереди: когда сервер появился, он обработал только новый запрос (`processed by server: ['abc']`), а не устаревший.
3. Медленный сервер ответил после таймаута: клиент уже удалил `correlation_id` из `pending`, поэтому поздний ответ молча отброшен.

Без `expiration` запрос от давно ушедшего клиента дождался бы сервера и был бы выполнен зря — а если запрос меняет состояние, то и опасно.

## Выбор шаблона

| Задача | Шаблон | Почему |
|---|---|---|
| Распределить тяжёлую работу | work queue | каждое сообщение — одному воркеру |
| Оповестить всех | pub/sub (fanout) | каждому подписчику своя копия |
| Оповестить заинтересованных | direct / topic | подписчик выбирает ключи |
| Получить результат | RPC | ответ привязан к запросу |
| Результат не нужен | work queue или pub/sub | RPC лишний, связывает сервисы по времени |

RPC превращает асинхронную систему в синхронную: клиент ждёт, значит доступность клиента зависит от сервера. Если ответ не нужен немедленно, лучше отправить команду и получить результат отдельным событием.

## Лучшие практики

- Work queue: durable-очередь, persistent-сообщения, ручной ack после работы, `prefetch_count` по длительности задач.
- Pub/sub: временные exclusive-очереди для «живых» подписчиков и именованные durable-очереди для тех, кто не должен терять сообщения, пока отключён.
- RPC: таймаут клиента, `expiration` у запроса, проверка `correlation_id`, идемпотентный сервер.
- Один обменник на тип события и привязки на стороне подписчиков: издатель не знает потребителей.

## Частые ошибки

- ❌ RPC без таймаута → ✅ таймаут и `expiration`.
- ❌ Подписка на `amq.rabbitmq.reply-to` с ручным ack → ✅ `auto_ack=True`.
- ❌ Публикация запроса до подписки на reply-to → ✅ сначала подписаться.
- ❌ Временная очередь для подписчика, который не должен ничего пропускать → ✅ именованная durable-очередь.
- ❌ Считать ответ принадлежащим запросу без проверки `correlation_id` → ✅ сверять id.

## Связанные материалы

- Раньше: [основы](basics.ru.md)
- Дальше: [экспертная теория](expert.ru.md)
- [03 · Очереди и доставка](../03-queues-and-delivery/advanced.ru.md) — TTL и expiration сообщений
- Следующий модуль: [05 · Надёжность](../05-reliability/overview.ru.md)

## Источники

- [Direct Reply-to](https://www.rabbitmq.com/docs/direct-reply-to)
- [Time-To-Live and Expiration](https://www.rabbitmq.com/docs/ttl)
- [RabbitMQ tutorial: Remote procedure call](https://www.rabbitmq.com/tutorials/tutorial-six-python)
