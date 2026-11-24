---
title: "Federation и Shovel: экспертный уровень"
lang: ru
track: rabbitmq
kind: module
doc: expert
slug: 12-federation-and-shovel
part: 3
tags: [rabbitmq, shovel, federation, upstream, policy, shovel-status, multi-datacenter]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Federation и Shovel: экспертный уровень

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · [2. Продвинутый](advanced.ru.md) · **3. Эксперт** · [4. Квиз](quiz.ru.md)

Опирается на [базовую](basics.ru.md) и [продвинутую](advanced.ru.md) теорию. «Проверено» означает наблюдалось на двух независимых брокерах RabbitMQ 4.4.1 в Docker.

## Справочник

### Shovel

| Элемент | Значение | Проверено |
|---|---|---|
| плагины | `rabbitmq_shovel`, `rabbitmq_shovel_management` | да |
| создание | `rabbitmqctl set_parameter shovel <имя> '<json>'` | да |
| ключи JSON | `src-protocol`, `src-uri`, `src-queue`, `dest-protocol`, `dest-uri`, `dest-queue` | да (`amqp091`) |
| состояние | `rabbitmqctl shovel_status`: `name`, `type`, `state`, `source`, `destination`, `termination_reason`, `forwarded`, … | да |
| состояния | `running`, `terminated` | да |
| причина остановки при отказе источника | `"failed to connect to source"` | да |
| удаление | `rabbitmqctl clear_parameter shovel <имя>` | да |
| источник — обменник (`src-exchange`), `ack-mode`, `reconnect-delay`, `src-delete-after`, AMQP 1.0 | другие ключи | не проверялись |
| статический shovel (в конфигурации) | альтернатива динамическому | не проверялся |

### Federation

| Элемент | Значение | Проверено |
|---|---|---|
| плагины | `rabbitmq_federation`, `rabbitmq_federation_management` | да |
| upstream | `rabbitmqctl set_parameter federation-upstream <имя> '{"uri": "…"}'` | да |
| политика | `set_policy <имя> "<шаблон>" '{"federation-upstream-set":"all"}' --apply-to exchanges` | да |
| состояние | `rabbitmqctl federation_status`: `status`, `type`, `exchange`, `uri`, `vhost` | да |
| федеративные очереди, `max-hops`, двунаправленность, `expires`, `message-ttl` | другие возможности | не проверялись |

### Что проверено на стенде

| Опыт | Результат |
|---|---|
| shovel `A.q.src → B.q.dst`, 5 сообщений в источнике | все 5 в приёмнике; `forwarded` 5 |
| ещё 3 сообщения | источник 0, приёмник 8; порядок сохранён |
| остановка источника | `terminated`, `failed to connect to source` |
| запуск источника и публикация | сообщение дошло, shovel снова работает |
| федеративный обменник `fed.ex` | сообщение с `rmqa` в очереди `rmqb`; связь `running` |

## Внутреннее устройство

- **Shovel — клиент AMQP.** Он подключается к источнику и приёмнику как обычный потребитель и издатель и перекладывает сообщения; поэтому он работает через любые URI, доступные брокеру-хозяину.
- **Shovel живёт как параметр** (runtime parameter) на брокере-хозяине (в опыте — на `rmqb`).
- **Federation — это связь, создаваемая политикой:** политика по шаблону имени запускает связи с вышестоящими брокерами.
- **Оба плагина используют обычные AMQP-соединения** и права ([модуль 11](../11-security/expert.ru.md)).
- **Дублирование возможно:** при сбое между доставкой и подтверждением сообщение может прийти повторно.

> ⚠️ Проверить: внутренние механизмы (обработка подтверждений, поведение при разрывах и перезапуске, лимиты производительности, внутренний пул соединений и очереди федерации) описаны по документации и здесь не проверялись.

## Подвохи

### 1. Shovel перемещает, а не копирует

После переноса сообщение исчезает из источника (проверено: `q.src` пуст).

### 2. Нужна топология на обоих концах

В опытах очереди и обменники заранее создавались на обоих брокерах; делайте так же. Что произойдёт при их отсутствии, здесь не проверялось.

### 3. Учётные данные в параметре

URI с паролем хранится в параметре брокера; используйте отдельного пользователя с минимумом прав и TLS (в выводе брокер скрывает пароль).

### 4. Состояние terminated

Shovel восстановился после возвращения источника (проверено), но без мониторинга вы не узнаете о простое.

### 5. Федерация по умолчанию однонаправленная

Вышестоящий → нижестоящий; обратное направление требует отдельной настройки (здесь не проверялось).

### 6. Дубликаты

Приёмник должен быть идемпотентным ([модуль 05](../05-reliability/expert.ru.md)).

### 7. `guest` для связи

В учебном образе работает, но в рабочей системе недопустим.

## Компромиссы и альтернативы

| Выбор | Плюсы | Минусы |
|---|---|---|
| Shovel | простой перенос очередь→очередь, любые протоколы | перемещает, нет «общей» топологии |
| Federation | общий обменник между брокерами | настройка политик, односторонность по умолчанию |
| Кластер | единая система | не для WAN |
| Репликация данных на уровне приложения | полный контроль | больше кода |

## Вопросы с собеседований

<details><summary>1. Чем Shovel отличается от Federation?</summary>

Shovel перекладывает сообщения из источника в приёмник (перемещает); Federation распространяет опубликованные сообщения вышестоящих обменников на нижестоящие.

→ [Основы](basics.ru.md#зачем-соединять-брокеры)

</details>

<details><summary>2. Подвох: shovel копирует или перемещает?</summary>

Перемещает: из источника сообщения удаляются.

→ [Подвох 1](#1-shovel-перемещает-а-не-копирует)

</details>

<details><summary>3. Почему не растянуть кластер между дата-центрами?</summary>

Кластер рассчитан на быструю надёжную сеть; для WAN используют независимые брокеры и Shovel/Federation.

→ [Основы](basics.ru.md#зачем-соединять-брокеры)

</details>

<details><summary>4. Как создать динамический shovel?</summary>

`rabbitmqctl set_parameter shovel <имя> '{"src-protocol":…,"src-uri":…,"src-queue":…,"dest-protocol":…,"dest-uri":…,"dest-queue":…}'`.

→ [Основы](basics.ru.md#shovel-из-очереди-в-очередь)

</details>

<details><summary>5. Что происходит с shovel при отказе источника?</summary>

Состояние `terminated` с причиной `failed to connect to source`; после возвращения источника shovel восстановился.

→ [Продвинутая теория](advanced.ru.md#сбой-источника)

</details>

<details><summary>6. Как настроить федерацию обменника?</summary>

Параметр `federation-upstream` с URI и политика с `federation-upstream-set` по шаблону имён обменников.

→ [Основы](basics.ru.md#federation-общий-обменник)

</details>

<details><summary>7. Подвох: сообщение, опубликованное на нижестоящем брокере, попадёт на вышестоящий?</summary>

По умолчанию федерация идёт от вышестоящего к нижестоящему; обратное направление здесь не проверялось.

→ [Подвох 5](#5-федерация-по-умолчанию-однонаправленная)

</details>

<details><summary>8. Как проверить состояние связей?</summary>

`rabbitmqctl shovel_status` и `rabbitmqctl federation_status`.

→ [Продвинутая теория](advanced.ru.md#управление-shovel)

</details>

<details><summary>9. Какие гарантии доставки у shovel?</summary>

Минимум один раз: возможны дубликаты, нужен идемпотентный приёмник.

→ [Подвох 6](#6-дубликаты)

</details>

<details><summary>10. Подвох: можно ли использовать `guest` в URI связи?</summary>

В учебном образе работает, в рабочей системе нельзя: отдельный пользователь и TLS.

→ [Подвох 7](#7-guest-для-связи)

</details>

<details><summary>11. Что нужно создать заранее?</summary>

Очереди/обменники и привязки на обоих брокерах.

→ [Подвох 2](#2-нужна-топология-на-обоих-концах)

</details>

<details><summary>12. Когда выбрать Federation, а когда Shovel?</summary>

Federation — общий обменник и распространение публикаций; Shovel — перенос очередей и интеграция разных систем.

→ [Основы](basics.ru.md#shovel-или-federation)

</details>

## Шпаргалка

```text
shovel     : set_parameter shovel NAME '{src-uri,src-queue,dest-uri,dest-queue,...}' ; shovel_status ; clear_parameter shovel NAME
federation : set_parameter federation-upstream UP '{"uri":"amqp://..."}' ; set_policy P "^pat" '{"federation-upstream-set":"all"}' --apply-to exchanges ; federation_status
moves vs copies : shovel MOVES a queue's messages ; federation propagates exchange publishes downstream
```

- Независимые брокеры, отдельный пользователь, TLS, идемпотентный приёмник.
- Следите за состоянием связей.

## Связанные материалы

- Предыдущий уровень: [продвинутая теория](advanced.ru.md)
- Раньше: [11 · Безопасность](../11-security/expert.ru.md)
- Дальше: [13 · Внутреннее устройство](../../overview.ru.md)

## Источники

- [Shovel Plugin](https://www.rabbitmq.com/docs/shovel)
- [Federation Plugin](https://www.rabbitmq.com/docs/federation)
- [Dynamic Shovels](https://www.rabbitmq.com/docs/shovel-dynamic)
