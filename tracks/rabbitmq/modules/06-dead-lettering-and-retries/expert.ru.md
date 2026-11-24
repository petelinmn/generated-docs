---
title: "Dead-lettering и повторы: экспертный уровень"
lang: ru
track: rabbitmq
kind: module
doc: expert
slug: 06-dead-lettering-and-retries
part: 2
tags: [rabbitmq, dead-letter-exchange, x-death, retry, poison-message, message-ttl, parking-lot]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Dead-lettering и повторы: экспертный уровень

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · [2. Продвинутый](advanced.ru.md) · **3. Эксперт** · [4. Квиз](quiz.ru.md)

Опирается на [базовую](basics.ru.md) и [продвинутую](advanced.ru.md) теорию. «Проверено» означает наблюдалось на RabbitMQ 4.4.1 с pika 1.5.0.

## Справочник

### Аргументы очереди для dead-lettering

| Аргумент | Эффект | Проверено |
|---|---|---|
| `x-dead-letter-exchange` | куда отправлять «мёртвые» сообщения | да |
| `x-dead-letter-routing-key` | ключ маршрутизации в DLX; иначе остаётся исходный | да |
| `x-dead-letter-strategy` (`at-most-once`/`at-least-once`) | гарантия dead-lettering | не проверялся |
| `x-delivery-limit` (quorum) | предел доставок | **не сработал** в тесте, см. [продвинутую теорию](advanced.ru.md#delivery-limit-quorum-очередей) |

### Причины (`reason` в `x-death`)

| Причина | Когда | Проверено |
|---|---|---|
| `rejected` | `reject`/`nack` с `requeue=False` | да |
| `expired` | истёк TTL сообщения/очереди | да |
| `maxlen` | вытеснено по `x-max-length` с `drop-head` | да |
| `delivery_limit` | превышен delivery limit (quorum) | нет |

### Заголовки, которые добавляет брокер

| Заголовок | Содержимое | Проверено |
|---|---|---|
| `x-death` | список записей: `queue`, `reason`, `count`, `exchange`, `routing-keys` (и время) | да |
| `x-first-death-queue/-reason/-exchange` | первая смерть | да |
| `x-last-death-queue/-reason/-exchange` | последняя смерть | да |

Запись `x-death` группируется по (очередь, причина) и накапливает `count`; порядок записей — от последней к первой (в тесте запись `retry/expired` шла перед `main/rejected`).

### Что происходит с сообщением

| Свойство | Поведение | Проверено |
|---|---|---|
| ключ маршрутизации | заменяется `x-dead-letter-routing-key` | да |
| тело и свойства | сохраняются | да |
| `delivery_mode`, `message_id`, свои заголовки | по документации сохраняются | не проверялось |
| DLX не существует | сообщение исчезает, ошибки нет | да |
| при `maxlen`+`drop-head` | «мёртвое» — самое старое | да |

## Внутреннее устройство

- **Dead-lettering — это публикация брокером.** Сообщение заново публикуется в DLX с ключом `x-dead-letter-routing-key`; дальше всё как у обычной публикации: можно цепочкой, можно в fanout.
- **`x-death` — часть сообщения.** Брокер обновляет заголовок при каждом цикле; потребитель использует его как счётчик попыток.
- **Очередь задержки без потребителей.** Сообщение «стареет» внутри очереди и «умирает» по TTL: просрочка — просто ещё одна причина dead-lettering.
- **TTL сообщения только у головы.** В очереди с разными TTL просроченное сообщение за живым ждёт ([модуль 03](../03-queues-and-delivery/expert.ru.md#5-ttl-за-долгой-головой)); для очередей задержки используйте один TTL на очередь.

> ⚠️ Проверить: гарантии `x-dead-letter-strategy` и то, как dead-lettering ведёт себя при отказе узла, здесь не проверялись; `delivery_limit` — см. выше.

## Подвохи

### 1. Reject без DLX

Сообщение пропадает навсегда (проверено: глубина 0).

### 2. DLX без маршрута

Сообщение исчезает без ошибки и без следа (проверено).

### 3. Суммирование `count`

Сумма `count` по всем записям смешивает причины и очереди; фильтруйте по очереди и причине.

### 4. Бесконечный цикл main → retry → main

Без предела попыток сообщение ходит кругом; считайте попытки и отправляйте в `parking`.

### 5. Вытесняется старое

При `maxlen` с `drop-head` в DLX уходит самое старое сообщение, а не новое (проверено: `first`).

### 6. Delivery limit

В проверке quorum-очередь с `x-delivery-limit: 3` и `nack(requeue=True)` не отправила сообщение в DLX за 140 доставок.

### 7. Задержка зависит от TTL очереди

Один TTL на очередь; разные задержки — разные очереди.

## Компромиссы и альтернативы

| Выбор | Плюсы | Минусы |
|---|---|---|
| DLX + очередь задержки | стандартные средства | очередь на каждую задержку |
| Свой счётчик в заголовке | независим от `x-death` | нужно переопубликовывать сообщение |
| Плагин delayed-message-exchange | любая задержка на сообщение | отдельный плагин; здесь не проверялся |
| Delivery limit (quorum) | брокер считает попытки | в тесте не сработал; требует проверки |
| Повтор в коде потребителя | просто | блокирует воркер, теряется при падении |

## Вопросы с собеседований

<details><summary>1. Что такое DLX?</summary>

Обменник, куда брокер отправляет сообщения очереди, ставшие «мёртвыми»: отклонённые без возврата, просроченные, вытесненные по длине.

→ [Базовая теория](basics.ru.md#зачем-нужен-dead-lettering)

</details>

<details><summary>2. Подвох: что произойдёт при `reject(requeue=False)` в очереди без DLX?</summary>

Сообщение отбрасывается навсегда.

→ [Основы](basics.ru.md#без-dlx-сообщение-исчезает)

</details>

<details><summary>3. Что лежит в `x-death`?</summary>

Список записей (очередь, причина, `count`, обменник, ключи маршрутизации), накапливающих историю «смертей».

→ [Основы](basics.ru.md#отклонённое-сообщение)

</details>

<details><summary>4. Как сделать повтор с задержкой?</summary>

Рабочая очередь → DLX → очередь `retry` без потребителей с TTL и DLX обратно в рабочую.

→ [Продвинутая теория](advanced.ru.md#повтор-с-задержкой)

</details>

<details><summary>5. Как посчитать число попыток?</summary>

По `x-death`: `count` записи с очередью `main` и причиной `rejected`, либо своим заголовком.

→ [Продвинутая теория](advanced.ru.md#что-накапливается-в-x-death)

</details>

<details><summary>6. Подвох: DLX не существует. Что с сообщением?</summary>

Оно исчезает; ошибки нет, канал открыт.

→ [Продвинутая теория](advanced.ru.md#dlx-без-маршрута)

</details>

<details><summary>7. Что становится «мёртвым» при `x-max-length` с `drop-head`?</summary>

Самое старое сообщение (причина `maxlen`).

→ [Основы](basics.ru.md#другие-причины)

</details>

<details><summary>8. Что такое «ядовитое» сообщение?</summary>

Сообщение, обработка которого всегда падает; его нужно вывести из цикла, а не возвращать в очередь.

→ [Продвинутая теория](advanced.ru.md#лучшие-практики)

</details>

<details><summary>9. Подвох: можно ли доверять delivery limit quorum-очереди?</summary>

Только после проверки на своей версии: в тесте предел 3 не сработал за 140 доставок.

→ [Подвох 6](#6-delivery-limit)

</details>

<details><summary>10. Как сделать нарастающую задержку?</summary>

Несколько очередей `retry` с разными TTL и выбор ключа по счётчику попыток.

→ [Продвинутая теория](advanced.ru.md#переменная-задержка)

</details>

<details><summary>11. Почему нельзя использовать `expiration` сообщения для очереди задержки с разными значениями?</summary>

TTL сообщения срабатывает только у головы очереди; короткий TTL за длинным ждёт.

→ [Модуль 03](../03-queues-and-delivery/expert.ru.md#5-ttl-за-долгой-головой)

</details>

<details><summary>12. Подвох: ключ маршрутизации после dead-lettering — исходный или `x-dead-letter-routing-key`?</summary>

Если задан `x-dead-letter-routing-key`, то он (проверено: `dead`); иначе остаётся исходный.

→ [Основы](basics.ru.md#отклонённое-сообщение)

</details>

## Шпаргалка

```text
queue args : x-dead-letter-exchange, x-dead-letter-routing-key
reasons    : rejected | expired | maxlen | (delivery_limit - quorum, untested here)
x-death    : [{queue, reason, count, exchange, routing-keys}] + x-first-death-* / x-last-death-*
retry      : main -DLX-> retry(TTL, no consumers) -DLX-> main ; cap attempts -> parking queue
```

- DLX и очередь разбора — на каждую рабочую очередь.
- Повтор — с задержкой и пределом.
- Не полагайтесь на непроверенный delivery limit.

## Связанные материалы

- Предыдущий уровень: [продвинутая теория](advanced.ru.md)
- Раньше: [05 · Надёжность](../05-reliability/expert.ru.md)
- Дальше: [07 · Типы очередей](../../overview.ru.md)

## Источники

- [Dead Letter Exchanges](https://www.rabbitmq.com/docs/dlx)
- [Quorum Queues](https://www.rabbitmq.com/docs/quorum-queues)
- [Time-To-Live and Expiration](https://www.rabbitmq.com/docs/ttl)
