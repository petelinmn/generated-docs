---
title: "Federation и Shovel: продвинутый уровень"
lang: ru
track: rabbitmq
kind: module
doc: advanced
slug: 12-federation-and-shovel
part: 3
tags: [rabbitmq, shovel, federation, reconnect, shovel-status, federation-status, policy]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Federation и Shovel: продвинутый уровень

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

Опирается на [базовую теорию](basics.ru.md) (тот же стенд из двух брокеров). Код запускался на RabbitMQ 4.4.1 с pika 1.5.0.

## Сбой источника

Что делает shovel, когда источник недоступен? Брокер `rmqa` остановили (`docker stop rmqa`):

```text
s1 | dynamic | terminated |  |  | / | 2026-11-24 16:09:09 | "failed to connect to source"
```

Shovel перешёл в состояние `terminated` с причиной `failed to connect to source`. После `docker start rmqa` и публикации сообщения `after-restart` в `q.src` через несколько секунд сообщение оказалось в `q.dst`:

```text
after A restart, B q.dst: 1
```

То есть shovel сам восстановил связь и продолжил перекладывать сообщения. Чтобы убедиться, что он работает, смотрите `rabbitmqctl shovel_status` (состояние `running`, счётчик `forwarded`).

> ⚠️ Проверить: интервал повторного подключения (`reconnect-delay`, по документации 5 секунд), режимы подтверждения (`ack-mode`: `on-confirm` по умолчанию, `on-publish`, `no-ack`) и потеря/дублирование сообщений при сбое между подтверждением и удалением из источника здесь не проверялись. Shovel даёт доставку «минимум один раз», поэтому приёмник должен переносить дубликаты ([модуль 05](../05-reliability/advanced.ru.md#идемпотентный-потребитель)).

## Управление shovel

| Действие | Команда |
|---|---|
| создать | `rabbitmqctl set_parameter shovel <имя> '<json>'` |
| состояние | `rabbitmqctl shovel_status` |
| удалить | `rabbitmqctl clear_parameter shovel <имя>` |

После `clear_parameter` shovel остановлен и удалён:

```text
Clearing runtime parameter "s1" for component "shovel" on vhost "/" ...
```

Shovel хранится как параметр в брокере-хозяине, поэтому он переживает перезапуск (так сказано в документации; здесь перезапуск `rmqb` не проверялся).

## Федерация: связь и состояние

Состояние связей показывает `federation_status`:

```text
Listing federation links on node rabbit@rmqb...
[#{error => <<>>, id => <<"945470fc">>, status => running, type => exchange,
   exchange => <<"fed.ex">>, queue => <<>>, uri => <<"amqp://rmqa">>, vhost => <<"/">>, ...}]
```

Связь создаётся для обменника, подходящего под шаблон политики (`^fed\.`), и показывает `status => running`. Сообщение, опубликованное прямо на нижестоящем брокере, попало в его локальную очередь: в `fed.q` их стало два (одно из `rmqa`, одно локальное). Дошло ли оно до `rmqa`, здесь не проверялось (там нет привязанной очереди):

```text
B fed.q after publishing on A: 1
A fed.ex received downstream publish? A has no queue bound; B fed.q: 2
```

Федерация по умолчанию однонаправленная: от вышестоящего брокера к нижестоящему.

> ⚠️ Проверить: двунаправленная федерация (upstream в обе стороны), федерация очередей (federated queues), набор вышестоящих брокеров (`federation-upstream-set`) с несколькими элементами, `max-hops` и поведение при обрыве связи здесь не проверялись.

## Что выбрать для дата-центров

| Сценарий | Подход |
|---|---|
| Перенести накопившуюся очередь в другой брокер | Shovel (с последующим удалением) |
| Постоянная доставка из одного дата-центра в другой | Shovel из очереди в очередь или Federation |
| События, которые нужны нескольким дата-центрам | Federation обменника |
| Один дата-центр | кластер |

## Лучшие практики

- Отдельный пользователь с минимальными правами для связи; TLS между брокерами.
- Приёмник идемпотентен: возможны дубликаты.
- Мониторьте `shovel_status` и `federation_status` и глубину очередей на обоих концах ([модуль 09](../09-monitoring/advanced.ru.md)).
- Топология (обменники, очереди, привязки) создаётся заранее на обоих концах.
- Не растягивайте кластер между дата-центрами.

## Частые ошибки

- ❌ `guest` в URI связи → ✅ отдельный пользователь.
- ❌ Считать shovel «репликацией» → ✅ он перемещает сообщения.
- ❌ Игнорировать состояние `terminated` → ✅ тревога по `shovel_status`.
- ❌ Ждать двунаправленности от одной связи federation → ✅ настройка с обеих сторон (не проверялась здесь).
- ❌ Забыть про дубликаты → ✅ идемпотентный приёмник.

## Связанные материалы

- Раньше: [основы](basics.ru.md)
- Дальше: [экспертная теория](expert.ru.md)
- [05 · Надёжность](../05-reliability/advanced.ru.md), [09 · Мониторинг](../09-monitoring/advanced.ru.md)
- Следующий модуль: [13 · Внутреннее устройство](../../overview.ru.md)

## Источники

- [Shovel Plugin](https://www.rabbitmq.com/docs/shovel)
- [Federation Plugin](https://www.rabbitmq.com/docs/federation)
- [Federated Exchanges](https://www.rabbitmq.com/docs/federated-exchanges)
