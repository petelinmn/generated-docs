---
title: "Federation и Shovel: основы"
lang: ru
track: rabbitmq
kind: module
doc: basics
slug: 12-federation-and-shovel
part: 3
tags: [rabbitmq, shovel, federation, upstream, downstream, multi-datacenter]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Federation и Shovel: основы

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [overview](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [08 · Кластеризация](../08-clustering/basics.ru.md), [11 · Безопасность](../11-security/basics.ru.md) · **Время:** ~1,5 ч

## Как проверялся код

Два **независимых** (не объединённых в кластер) брокера RabbitMQ 4.4.1 в Docker: `rmqa` (порт 5691) и `rmqb` (порт 5692) в одной сети; на обоих включены плагины `rabbitmq_shovel`, `rabbitmq_shovel_management`, `rabbitmq_federation`, `rabbitmq_federation_management`. Клиент — pika 1.5.0. Выводы скопированы из запусков; **это учебный стенд на одной машине** — реальная глобальная сеть с задержками и разрывами не воспроизводилась. Команды `rabbitmqctl` выполняются внутри контейнера. Для связи между брокерами использовалась учебная учётная запись `guest` (в образе она доступна по сети); в рабочей системе заведите отдельного пользователя с минимальными правами и TLS ([модуль 11](../11-security/basics.ru.md)).

## Зачем соединять брокеры

Кластер ([модуль 08](../08-clustering/basics.ru.md)) рассчитан на быструю надёжную сеть внутри одного дата-центра. Когда брокеры стоят в разных дата-центрах, принадлежат разным командам или нужно перенести сообщения между системами, их связывают плагинами:

| Плагин | Что делает |
|---|---|
| **Shovel** | «лопата»: читает сообщения из источника (очередь или обменник) и записывает в приёмник на другом брокере |
| **Federation** | «федерация»: нижестоящий (downstream) обменник или очередь получает копии сообщений вышестоящего (upstream) брокера |

Оба плагина работают между **независимыми** брокерами: у каждого свои пользователи, vhost-ы и очереди.

## Shovel: из очереди в очередь

Динамический shovel задаётся параметром среды выполнения. На брокере `rmqb` он перекладывает сообщения из очереди `q.src` брокера `rmqa` в очередь `q.dst` брокера `rmqb`:

```text
$ rabbitmqctl set_parameter shovel s1 '{"src-protocol":"amqp091","src-uri":"amqp://USER:PASS@rmqa","src-queue":"q.src","dest-protocol":"amqp091","dest-uri":"amqp://USER:PASS@rmqb","dest-queue":"q.dst"}'
Setting runtime parameter "s1" for component "shovel" to "{"src-protocol": "amqp091", "src-uri": "amqp://rmqa", "src-queue": "q.src", "dest-protocol": "amqp091", "dest-uri": "amqp://rmqb", "dest-queue": "q.dst"}" in vhost "/" ...
```

(В выводе брокера учётные данные из URI скрыты.) До включения shovel в `q.src` лежали пять сообщений:

```text
A q.src: 0 B q.dst: 5
Shovel status on node rabbit@rmqb
name | pending | type | state | source | destination | vhost | last_changed | termination_reason | ...
s1 | 0 | dynamic | running | amqp://rmqa | amqp://rmqb | / | 2026-11-24 16:09:02 |  | ...
```

Все пять ушли из `q.src` в `q.dst` и shovel перешёл в состояние `running`. Новые сообщения переносятся непрерывно:

```text
after 3 more: A q.src: 0 B q.dst: 8
B received in order: ['m0', 'm1', 'm2', 'm3', 'm4', 'm5', 'm6', 'm7']
```

Порядок сохранён. Сообщения **перемещаются**, а не копируются: из источника они удаляются после подтверждения приёмником.

## Federation: общий обменник

Федерация делает так, чтобы нижестоящий обменник получал сообщения, опубликованные в вышестоящем. Настройка на нижестоящем брокере `rmqb`: верхний брокер (upstream) и политика, которая применяет федерацию к обменникам по шаблону имени:

```text
$ rabbitmqctl set_parameter federation-upstream up-a '{"uri":"amqp://USER:PASS@rmqa"}'
$ rabbitmqctl set_policy fed "^fed\." '{"federation-upstream-set":"all"}' --apply-to exchanges
```

Обменник `fed.ex` объявлен на обоих брокерах; на `rmqb` к нему привязана очередь `fed.q`. Публикация на `rmqa`:

```text
B fed.q after publishing on A: 1
```

Сообщение, опубликованное на `rmqa`, оказалось в очереди на `rmqb`. Состояние связи видно в `federation_status` (в проверке — `status => running`, `type => exchange`).

## Shovel или federation

| Нужно | Выбор |
|---|---|
| Перекладывать сообщения из очереди в очередь (в том числе между разными системами или протоколами) | Shovel |
| Распространять опубликованные сообщения между брокерами по обменнику | Federation |
| Одна логическая система внутри дата-центра | кластер ([модуль 08](../08-clustering/basics.ru.md)) |

## Типичные ошибки новичков

- ❌ Растягивать кластер между дата-центрами → ✅ отдельные брокеры и Shovel/Federation.
- ❌ Хранить учётные данные связи в открытом виде и использовать `guest` → ✅ отдельный пользователь, TLS.
- ❌ Ждать, что Shovel скопирует сообщения → ✅ он переносит: из источника они удаляются.
- ❌ Забыть объявить обменник/очередь на обоих концах → ✅ топологию создают заранее.
- ❌ Не следить за состоянием связи → ✅ `shovel_status`, `federation_status`.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — сбой связи, политика и ограничения
- Раньше: [11 · Безопасность](../11-security/basics.ru.md), [08 · Кластеризация](../08-clustering/basics.ru.md)
- Следующий модуль: [13 · Внутреннее устройство](../../overview.ru.md)

## Источники

- [Shovel Plugin](https://www.rabbitmq.com/docs/shovel)
- [Federation Plugin](https://www.rabbitmq.com/docs/federation)
- [Dynamic Shovels](https://www.rabbitmq.com/docs/shovel-dynamic)
