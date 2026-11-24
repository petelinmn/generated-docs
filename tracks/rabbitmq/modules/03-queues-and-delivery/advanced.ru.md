---
title: "Очереди и доставка: продвинутый уровень"
lang: ru
track: rabbitmq
kind: module
doc: advanced
slug: 03-queues-and-delivery
part: 1
tags: [rabbitmq, queue, message-ttl, queue-length-limit, overflow, exclusive-queue, auto-delete-queue, queue-expiry]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Очереди и доставка: продвинутый уровень

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

Опирается на [базовую теорию](basics.ru.md) (те же помощники). Все выводы получены на настоящем RabbitMQ 4.4.1.

## TTL сообщений

TTL для всей очереди задаётся аргументом `x-message-ttl` (миллисекунды); у отдельного сообщения свой — свойство `expiration` (строка). Истёкшее сообщение отбрасывается (или уходит в dead-letter).

```python
fresh("ttl", arguments={"x-message-ttl": 300})
ch.basic_publish("", "ttl", b"short-lived")
print("ttl before:", count("ttl"), end=" ")
time.sleep(0.7)
print("after:", count("ttl"))

fresh("perm")
ch.basic_publish("", "perm", b"x", pika.BasicProperties(expiration="200"))
time.sleep(0.6)
print("per-message expiration:", count("perm"))
```

```text
ttl before: 1 after: 0
per-message expiration: 0
```

Если заданы оба, действует **меньший**: TTL очереди 5000 мс и `expiration` сообщения 200 оставили 0 сообщений через 0,8 с, а TTL очереди 200 и expiration сообщения 5000 тоже оставили 0. Но TTL отдельного сообщения применяется только у головы очереди: сообщение `short` (200 мс), опубликованное за `long` (5000 мс), через 0,8 с всё ещё считалось (2 сообщения).

## Ограничения длины и overflow

`x-max-length` (сообщений) ограничивает очередь. Когда она полна, поведение задаёт `x-overflow`:

- `drop-head` (по умолчанию): отбрасывается самое старое сообщение;
- `reject-publish`: новые сообщения отклоняются.

```python
fresh("cap", arguments={"x-max-length": 2})
for i in range(4):
    ch.basic_publish("", "cap", f"c{i}".encode())
print([ch.basic_get("cap", auto_ack=True)[2] for _ in range(2)])

fresh("cap2", arguments={"x-max-length": 2, "x-overflow": "reject-publish"})
for i in range(4):
    ch.basic_publish("", "cap2", f"c{i}".encode())
print([ch.basic_get("cap2", auto_ack=True)[2] for _ in range(2)])
```

```text
[b'c2', b'c3']
[b'c0', b'c1']
```

С `drop-head` остались два новейших; с `reject-publish` — два первых. Издатель без confirms об отказе не узнает; с confirms получит nack (модуль 05 в [дорожной карте](../../overview.ru.md)).

## Срок жизни очереди: `x-expires`

Очередь, не использовавшаяся заданное число миллисекунд, удаляется (нет потребителей, нет повторного объявления, нет `basic_get`):

```python
ch.queue_declare("idle", durable=True, arguments={"x-expires": 500})
time.sleep(1.0)
ch.queue_declare("idle", passive=True)     # ChannelClosedByBroker 404
```

```text
x-expires: ChannelClosedByBroker 404
```

## Exclusive и auto-delete очереди

- **`exclusive=True`:** доступна только объявившему соединению и удаляется при его закрытии. Попытка другого соединения даёт `405 RESOURCE_LOCKED - cannot obtain exclusive access to locked queue …`.
- **`auto_delete=True`:** удаляется, когда отписывается последний потребитель. После подписки и отмены потребителя пассивное объявление вернуло `404`.

## Несовпадение аргументов

Повторное объявление очереди с аргументами, отличающимися от текущих, падает и закрывает канал:

```text
406 PRECONDITION_FAILED - inequivalent arg 'x-message-ttl' for queue 'jobs' in vhost '/': received the value '1000' of type 'signedint' but current is none
```

Чтобы изменить аргументы очереди, её нужно удалить и создать заново (либо использовать политику для настроек, которые политики охватывают, см. экспертный уровень).

## Лучшие практики

- Всегда задавайте лимит длины или TTL для очередей, которые могут перерасти память, и оповещайте по глубине.
- Для быстрых обработчиков — `prefetch_count` в несколько сообщений, для медленных — 1.
- Для подписок на экземпляр используйте exclusive-очереди с именем от сервера.
- Никогда не меняйте аргументы объявления в коде без плана миграции.

## Частые ошибки

- ❌ Ждать, что TTL сообщения истечёт у сообщений за долгоживущим → ✅ используйте `x-message-ttl` очереди, когда истечение должно быть оперативным.
- ❌ `x-max-length` с `reject-publish` и издатели без confirms → ✅ confirms.
- ❌ Объявлять одну очередь в двух сервисах с разными аргументами → ✅ один владелец объявления.

## Связанные материалы

- Дальше: [экспертная теория](expert.ru.md)
- Раньше: [02 · Обменники и маршрутизация](../02-exchanges-and-routing/advanced.ru.md)
- Следующий модуль: 04 · Шаблоны обмена сообщениями ([дорожная карта](../../overview.ru.md))

## Источники

- [Time-To-Live and Expiration](https://www.rabbitmq.com/docs/ttl)
- [Queue Length Limits](https://www.rabbitmq.com/docs/maxlength)
- [Queues](https://www.rabbitmq.com/docs/queues)
