---
title: "Обмен сообщениями и модель AMQP: продвинутый уровень"
lang: ru
track: rabbitmq
kind: module
doc: advanced
slug: 01-messaging-basics
part: 1
tags: [rabbitmq, amqp, message-properties, connection-parameters, heartbeat, virtual-host]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Обмен сообщениями и модель AMQP: продвинутый уровень

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

Опирается на [базовую теорию](basics.ru.md). Все выводы получены на настоящем брокере RabbitMQ 4.4.1 с pika 1.5.0.

## Свойства сообщения

У сообщения есть тело (байты, в которые брокер не заглядывает) и **свойства** (properties) — стандартные метаданные, которые едут вместе с ним:

```python
props = pika.BasicProperties(
    content_type="application/json", content_encoding="utf-8",
    delivery_mode=pika.DeliveryMode.Persistent, message_id="m-1", correlation_id="c-1",
    reply_to="replies", type="order.created", app_id="shop", timestamp=1700000000,
    priority=3, headers={"retries": 2, "src": "web"})
ch.basic_publish("", "props", json.dumps({"id": 7}).encode(), props)
```

Что получил потребитель:

```text
b'{"id": 7}'
{'content_type': 'application/json', 'content_encoding': 'utf-8', 'delivery_mode': 2, 'message_id': 'm-1', 'correlation_id': 'c-1', 'reply_to': 'replies', 'type': 'order.created', 'app_id': 'shop', 'timestamp': 1700000000, 'priority': 3, 'headers': {'retries': 2, 'src': 'web'}, 'expiration': None, 'user_id': None}
```

| Свойство | Назначение |
|---|---|
| `content_type`, `content_encoding` | как декодировать тело (`application/json`) |
| `delivery_mode` | `1` transient, `2` persistent ([модуль 03](../03-queues-and-delivery/overview.ru.md)) |
| `message_id` | идентификатор для дедупликации и трассировки |
| `correlation_id`, `reply_to` | запрос/ответ (модуль 04 ([дорожная карта](../../overview.ru.md))) |
| `type`, `app_id` | вид события, отправитель |
| `timestamp` | секунды с эпохи, выставляет производитель |
| `priority` | для очередей с приоритетом |
| `expiration` | TTL одного сообщения в миллисекундах, **строкой** |
| `headers` | произвольная таблица; используется обменниками headers |
| `user_id` | должен совпадать с подключённым пользователем, иначе брокер откажет |

Брокер считает тело непрозрачным. Договоритесь о формате (JSON с `content_type`, либо Protobuf/Avro) и кладите версию в `type` или заголовок, чтобы потребители могли развиваться.

`pika` принимает тело `str` и отправляет его текст; `int` вызывает `TypeError: basic_publish body must be bytes or str, not int`. Предпочитайте явные `bytes`.

## Параметры соединения

```python
params = pika.ConnectionParameters(
    "localhost", virtual_host="/",
    credentials=pika.PlainCredentials("guest", "guest"), heartbeat=30)
conn = pika.Connection(params)
```

- **`virtual_host`** — пространство имён обменников, очередей и прав; на одном брокере их может быть много. По умолчанию `/`.
- **`heartbeat`** — интервал в секундах, с которым каждая сторона доказывает, что жива; мёртвый собеседник обнаруживается по пропущенным heartbeat. У брокера по умолчанию 60 с.
- Сбои подключения проявляются разными исключениями:

```text
bad login: ProbableAuthenticationError ConnectionClosedByBroker: (403) 'ACCESS_REFUSED - Login was refused using authentication mechanism P…
no broker: AMQPConnectionError
bad vhost: ProbableAccessDeniedError ConnectionClosedByBroker: (530) 'NOT_ALLOWED - vhost nope not found' (host='localhost', port=5672)
```

Ошибка аутентификации (403) и отсутствие виртуального хоста (530) — разные проблемы и разные исключения; недоступный порт — это `AMQPConnectionError`.

## Ошибки асинхронны

`basic_publish` не ждёт брокера. Ошибка сообщается позже закрытием канала:

```python
ch2.basic_publish("", "uid", b"x", pika.BasicProperties(user_id="someone"))   # connected as guest
time.sleep(0.5)
print("ch2 closed:", ch2.is_closed)
```

```text
ch2 closed: True
```

Сам вызов вернулся нормально. Не считайте «нет исключения» признаком «принято»: следите за обработчиками закрытия канала, используйте **publisher confirms** (модуль 05 в [дорожной карте](../../overview.ru.md)), а для недоставляемых сообщений — флаг `mandatory` ([модуль 02](../02-exchanges-and-routing/overview.ru.md)).

## Имена и очереди, созданные сервером

- Имена — строки до 255 байт (короткие строки AMQP); используйте строчные имена через точку (`orders.created`).
- Пустое имя просит брокера **сгенерировать** его: `queue_declare("", exclusive=True)` вернул имя, начинающееся с `amq.gen`. Такие очереди — для личных очередей ответов или подписок рассылки; они исчезают вместе с соединением.
- Имена, начинающиеся с `amq.`, зарезервированы за брокером.

## Потоки и каналы

`pika.Connection` потокобезопасно: несколько потоков могут вызывать `basic_publish` одновременно, а обработчики потребителей выполняются в рабочем потоке на канал — по одному сообщению, по порядку. Медленный обработчик не останавливает heartbeat (у старого `BlockingConnection` такая проблема была). Всё равно держите канал для одной цели: ошибка на уровне канала убивает всё, что на нём было.

> ⚠️ Проверить: автоматическое переподключение и восстановление потребителей у `pika.Connection` не исследовались; считайте, что после потери соединения переподключаться и заново объявлять топологию нужно самим.

## Лучшие практики

- Одно соединение на процесс; отдельные соединения для публикации и потребления, если обратное давление на одно не должно блокировать другое.
- Объявляйте топологию (обменники, очереди, привязки) при старте в одном месте, идемпотентно.
- Ставьте `content_type`, `message_id` и `timestamp` на каждое сообщение.
- Обрабатывайте закрытие канала и соединения; переподключайтесь с задержкой.

## Частые ошибки

- ❌ Считать публикацию без исключения принятой → ✅ confirms и обработчики закрытия.
- ❌ Указывать `expiration` числом → ✅ строка миллисекунд.
- ❌ Зашивать учётные данные в код → ✅ переменные окружения.
- ❌ Делить канал между производителями несвязанного трафика → ✅ одна цель на канал.

## Связанные материалы

- Дальше: [экспертная теория](expert.ru.md)
- Следующий модуль: [02 · Обменники и маршрутизация](../02-exchanges-and-routing/advanced.ru.md)

## Источники

- [AMQP 0-9-1 Model Explained](https://www.rabbitmq.com/tutorials/amqp-concepts)
- [Connections](https://www.rabbitmq.com/docs/connections)
- [Heartbeats](https://www.rabbitmq.com/docs/heartbeats)
- [Virtual Hosts](https://www.rabbitmq.com/docs/vhosts)
