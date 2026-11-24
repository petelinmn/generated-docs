---
title: "Обмен сообщениями и модель AMQP: экспертный уровень"
lang: ru
track: rabbitmq
kind: module
doc: expert
slug: 01-messaging-basics
part: 1
tags: [rabbitmq, amqp, delivery-tag, channel-max, frame-max, heartbeat, default-exchange]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Обмен сообщениями и модель AMQP: экспертный уровень

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · [2. Продвинутый](advanced.ru.md) · **3. Эксперт** · [4. Квиз](quiz.ru.md)

Опирается на [базовую](basics.ru.md) и [продвинутую](advanced.ru.md) теорию. Значения в «Наблюдаемых значениях по умолчанию» считаны с живого брокера (`rabbitmqctl eval 'application:get_env(rabbit, Key).'`); части о протоколе — из описания модели AMQP 0-9-1.

## Справочник

### Наблюдаемые значения по умолчанию (RabbitMQ 4.4.1, образ Docker)

| Настройка | Значение |
|---|---|
| `max_message_size` | 16 777 216 байт (16 МиБ) |
| `channel_max` | 256 |
| `frame_max` | 131 072 байта |
| `heartbeat` | 60 с |
| `consumer_timeout` | 1 800 000 мс (30 мин) |
| `default_vhost` / `default_user` | `/` / `guest` (с тегом `administrator`) |

### Слушатели в образе

| Порт | Протокол |
|---|---|
| 5672 | AMQP 0-9-1 и AMQP 1.0 |
| 15672 | HTTP API и management UI |
| 15692 | экспортёр Prometheus |
| 25672 | связь между узлами и CLI |

### Встроенные обменники (`rabbitmqctl list_exchanges name type`)

| Имя | Тип |
|---|---|
| (пустое — обменник по умолчанию) | direct |
| `amq.direct` | direct |
| `amq.fanout` | fanout |
| `amq.headers`, `amq.match` | headers |
| `amq.topic` | topic |
| `amq.rabbitmq.log`, `amq.rabbitmq.trace` | topic |

### Классы протокола

AMQP 0-9-1 группирует операции по классам: `connection`, `channel`, `exchange`, `queue`, `basic` (publish, get, consume, ack, reject, nack, qos, return), `confirm` (publisher confirms), `tx` (транзакции). Методы клиента соответствуют им: `queue_declare` → `queue.declare`, `basic_publish` → `basic.publish` (кадр метода, кадр заголовка содержимого и кадры тела).

### Коды закрытия, встреченные ранее

| Код | Имя | Что закрывает |
|---|---|---|
| 403 | `ACCESS_REFUSED` | соединение (вход) |
| 404 | `NOT_FOUND` | канал |
| 405 | `RESOURCE_LOCKED` | канал |
| 406 | `PRECONDITION_FAILED` | канал |
| 530 | `NOT_ALLOWED` | соединение (неверный vhost) |
| 541 | `INTERNAL_ERROR` | соединение (transient-очередь отклонена) |

## Внутреннее устройство

- **Delivery tag** — на канал: начинаются с 1 и растут; подтверждение ссылается на тег на том же канале. В тесте подтверждение тега на другом канале закрыло этот другой канал (исходный остался открытым).
- **Очередь — это процесс** в среде Erlang; публикация маршрутизируется в процесс очереди, чем объясняется отставание счётчиков после публикации.
- **Кадры.** Всё передаётся кадрами до `frame_max`; большое тело делится на кадры тела. В тесте тело в 16 МиБ + 1 байт закрыло канал (соединение осталось открытым).
- **Heartbeat** работают на уровне транспорта: фактический интервал согласуется между клиентом и брокером в начале соединения.
- **Каналы мультиплексируют** одно TCP-соединение; до `channel_max` на соединение (здесь 256).

## Подвохи

### 1. `queue_declare` без `durable` на 4.4

Закрывается соединение (не только канал) с кодом 541. Старые учебники ломаются.

### 2. Пассивное объявление убивает канал

Пассивное объявление отсутствующей очереди возвращает 404 и закрывает канал: плохая проверка существования на рабочем канале.

### 3. Публикация в несуществующую очередь успешна

Через обменник по умолчанию она молча отбрасывается, без ошибки.

### 4. Счётчик отстаёт

`message_count`, прочитанный сразу после публикации, дал `1 1 2` на три публикации; через секунду — `3`.

### 5. `expiration` — строка

TTL одного сообщения задаётся как `BasicProperties(expiration="200")`, строка миллисекунд.

### 6. `user_id` проверяется

`user_id`, отличный от подключённого пользователя, закрывает канал — асинхронно.

### 7. `delivery_mode` читается целым

`delivery_mode=pika.DeliveryMode.Persistent` вернулся как `2`.

## Компромиссы и альтернативы

| Выбор | Плюсы | Минусы |
|---|---|---|
| `basic_consume` | push, малая задержка | нужна настройка prefetch ([модуль 03](../03-queues-and-delivery/expert.ru.md)) |
| `basic_get` | просто | опрос, один обмен на сообщение |
| Канал на поток | изолированные ошибки | больше каналов |
| Один общий канал | меньше ресурсов | одна ошибка убивает всё |
| RabbitMQ против системы-журнала (Kafka) | гибкая маршрутизация, подтверждение по сообщению | по умолчанию нет повторного чтения (streams добавляют) |

## Вопросы с собеседований

<details><summary>1. Каков путь сообщения в AMQP 0-9-1?</summary>

Производитель → обменник → (привязки, ключ маршрутизации) → очередь → потребитель.

→ [Базовая теория](basics.ru.md#модель-amqp)

</details>

<details><summary>2. Зачем каналы, если есть соединения?</summary>

TCP-соединение дорого; каналы мультиплексируют много логических сеансов по одному соединению.

→ [Базовая теория](basics.ru.md#соединение-и-канал)

</details>

<details><summary>3. Подвох: вы публикуете в обменник по умолчанию с ключом — именем несуществующей очереди. Что будет?</summary>

Исключения нет: сообщение отбрасывается, потому что ни одна очередь не подходит.

→ [Подвох 3](#3-публикация-в-несуществующую-очередь-успешна)

</details>

<details><summary>4. Что происходит, когда потребитель умирает без подтверждения?</summary>

Брокер возвращает сообщение в очередь и помечает его `redelivered`.

→ [Базовая теория](basics.ru.md#подтверждения)

</details>

<details><summary>5. Подвох: что закрывается при неудачном пассивном объявлении — канал или соединение?</summary>

Канал (404); соединение остаётся открытым.

→ [Подвох 2](#2-пассивное-объявление-убивает-канал)

</details>

<details><summary>6. Почему `queue_declare("q")` падает на 4.4?</summary>

Transient-очереди без exclusive отклоняются; соединение закрывается с 541. Используйте `durable=True` или `exclusive=True`.

→ [Базовая теория](basics.ru.md#очереди-должны-быть-durable-или-exclusive)

</details>

<details><summary>7. Чем `basic_get` отличается от `basic_consume`?</summary>

`basic_get` опрашивает одно сообщение за вызов; `basic_consume` подписывает, и брокер присылает сообщения.

→ [Базовая теория](basics.ru.md#получение-get-и-consume)

</details>

<details><summary>8. Подвох: почему `message_count` после публикации может быть неверным?</summary>

Публикация асинхронна, а очередь обновляет счётчик позже.

→ [Подвох 4](#4-счётчик-отстаёт)

</details>

<details><summary>9. Что такое виртуальный хост?</summary>

Пространство имён для обменников, очередей и прав на одном брокере; по умолчанию `/`.

→ [Продвинутая теория](advanced.ru.md#параметры-соединения)

</details>

<details><summary>10. Как отличить ошибку аутентификации от ошибки vhost?</summary>

По коду закрытия: 403 `ACCESS_REFUSED` (вход) против 530 `NOT_ALLOWED` (vhost не найден); pika бросает разные исключения.

→ [Продвинутая теория](advanced.ru.md#параметры-соединения)

</details>

<details><summary>11. Подвох: публикация не бросила исключения. Принято ли сообщение?</summary>

Не обязательно: ошибки приходят асинхронно закрытием канала; используйте confirms.

→ [Продвинутая теория](advanced.ru.md#ошибки-асинхронны)

</details>

<details><summary>12. Что такое delivery tag и где он действителен?</summary>

Счётчик на канал, идентифицирующий доставку; подтверждение должно использовать его на том же канале.

→ [Справочник](#внутреннее-устройство)

</details>

## Шпаргалка

```text
conn = pika.Connection(pika.ConnectionParameters(host, virtual_host="/", credentials=..., heartbeat=30))
ch = conn.channel()
ch.queue_declare(q, durable=True)            # 4.4: durable or exclusive
ch.basic_publish(exchange="", routing_key=q, body=b"...", properties=pika.BasicProperties(...))
ch.basic_consume(q, callback)  /  ch.basic_get(q)
callback(ch, method, props, body): ch.basic_ack(method.delivery_tag)
errors: 403/530/541 close the connection; 404/405/406 close the channel
```

- Подтверждайте после обработки.
- Свойства на каждом сообщении.
- Одна цель на канал.

## Связанные материалы

- Предыдущий уровень: [продвинутая теория](advanced.ru.md)
- Следующий модуль: [02 · Обменники и маршрутизация](../02-exchanges-and-routing/expert.ru.md)

## Источники

- [AMQP 0-9-1 Model Explained](https://www.rabbitmq.com/tutorials/amqp-concepts)
- [AMQP 0-9-1 Complete Reference](https://www.rabbitmq.com/amqp-0-9-1-reference)
- [Configuration](https://www.rabbitmq.com/docs/configure)
- [Networking](https://www.rabbitmq.com/docs/networking)
