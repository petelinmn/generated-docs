---
title: "Безопасность: основы"
lang: ru
track: rabbitmq
kind: module
doc: basics
slug: 11-security
part: 3
tags: [rabbitmq, security, vhost, users, permissions, access-refused, user-tags]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Безопасность: основы

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [overview](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [02 · Обменники и маршрутизация](../02-exchanges-and-routing/basics.ru.md), [09 · Мониторинг](../09-monitoring/basics.ru.md) · **Время:** ~1,5 ч

## Как проверялся код

Один узел RabbitMQ 4.4.1 в Docker (`rabbitmq:4-management`) и pika 1.5.0; выводы скопированы из запусков. Помощник `sh(*cmd)` выполняет команду внутри контейнера (`docker exec rabbit …`), `conn(user, pw, vhost)` открывает соединение с заданными учётными данными. **Все учётные записи — учебные и удаляются после опыта.** Пароли в примерах — заглушки.

## Три уровня доступа

| Что | Для чего |
|---|---|
| **vhost** (виртуальный хост) | изолированное пространство имён: свои обменники, очереди, привязки и права |
| **пользователь** | учётная запись с паролем |
| **права** (`configure`/`write`/`read`) | три регулярных выражения на пару «пользователь — vhost» |

Регулярное выражение сопоставляется с **именем** ресурса (обменника или очереди):

| Право | Что разрешает |
|---|---|
| `configure` | объявлять и удалять ресурсы |
| `write` | публиковать в обменник (проверено) |
| `read` | читать из очереди (по документации; здесь проверялись только `configure` и `write`) |

> ⚠️ Проверить: какие права нужны для привязок (`queue_bind`) и чтения, здесь не проверялось.

## Настройка

```text
$ rabbitmqctl add_vhost shop
$ rabbitmqctl add_user alice pw1
$ rabbitmqctl add_user bob pw2
$ rabbitmqctl set_permissions -p shop alice "^app\..*" "^app\..*" "^app\..*"
$ rabbitmqctl set_permissions -p shop bob "^$" "^$" "^app\.read$"
$ rabbitmqctl set_user_tags alice monitoring
```

```text
$ rabbitmqctl list_permissions -p shop
user    configure   write       read
alice   ^app\..*    ^app\..*    ^app\..*
bob     ^$          ^$          ^app\.read$
```

`alice` работает только с ресурсами `app.*`, `bob` может лишь читать очередь `app.read`; пустое выражение `^$` не разрешает ничего.

## Что происходит при отказе

```python
conn("alice", "nope")              # wrong password
conn("carol", "x")                 # unknown user
conn("alice", "pw1", "/")          # no permissions on that vhost
```

```text
wrong password -> ProbableAuthenticationError ConnectionClosedByBroker: (403) 'ACCESS_REFUSED - Login was refused using authentication mechanism PLAIN. For details se…
unknown user -> ProbableAuthenticationError ConnectionClosedByBroker: (403) 'ACCESS_REFUSED - Login was refused using authentication mechanism PLAIN. For details se…
alice on vhost '/' (no permissions) -> ProbableAccessDeniedError ConnectionClosedByBroker: (530) "NOT_ALLOWED - access to vhost '/' refused for user 'alice'" …
```

- Неверный пароль и неизвестный пользователь дают **одинаковую** ошибку `403 ACCESS_REFUSED`: брокер не подсказывает, существует ли пользователь.
- Отсутствие прав на vhost — `530 NOT_ALLOWED`, закрывается соединение.

## Права на операции

```text
alice declares app.q -> ok
alice declares other.q (outside regex) -> ChannelClosedByBroker (403, "ACCESS_REFUSED - configure access to queue 'other.q' in vhost 'shop' refused for user 'alice'")
bob declares app.read (no configure) -> ChannelClosedByBroker (403, "ACCESS_REFUSED - configure access to queue 'app.read' in vhost 'shop' refused for user 'bob'")
bob publishes (no write) -> RuntimeError channel closed by broker
```

Нарушение прав закрывает **канал** с `403 ACCESS_REFUSED`; соединение остаётся.

### Публикация в очередь через обменник по умолчанию

Публикация `basic_publish("", "app.read", …)` идёт в обменник по умолчанию (`amq.default`), и для неё нужно право `write` на него. У `alice` выражение `^app\..*` его не покрывает, поэтому канал закрылся без ошибки на стороне вызова:

```text
publish to default exchange: channel open = False
publish to app.ex: open = True depth = 1
after granting write on amq.default: open = True depth = 2
```

Публикация через `app.ex` (под выражение) сработала; после выдачи права на `amq.default` сработала и публикация в очередь напрямую. **Публикация асинхронна, поэтому отказ виден только по закрытию канала** ([модуль 05](../05-reliability/advanced.ru.md#ошибки-при-которых-подтверждения-не-будет)).

## Теги и управление

Теги (`monitoring`, `administrator`, …) определяют доступ к плагину управления:

```text
mgmt alice 200
mgmt bob HTTP Error 401: Unauthorized
mgmt guest 200
```

Пользователь с тегом `monitoring` читает API, пользователь без тегов в API не допускается (401), а `guest` имеет тег `administrator`.

## Пользователь `guest`

В образе `rabbitmq:4-management` параметр `loopback_users` пуст (`{loopback_users,[]}`), поэтому `guest`/`guest` принимается и с хоста через проброшенный порт. **Это учебная настройка:** в нормальной установке `guest` разрешён только с `localhost`, а в рабочей системе его удаляют или меняют пароль.

> ⚠️ Проверить: поведение `guest` по умолчанию вне образа Docker (ограничение `localhost`) взято из документации и здесь не проверялось.

## Типичные ошибки новичков

- ❌ Оставить `guest`/`guest` → ✅ создать своих пользователей и удалить `guest`.
- ❌ Выдавать `.*` везде → ✅ минимальные регулярные выражения под приложение.
- ❌ Одно приложение — один общий vhost → ✅ vhost-ы для изоляции.
- ❌ Забыть право на `amq.default` при публикации в очередь → ✅ право `write` на него или публикация через свой обменник.
- ❌ Ожидать исключение при отказе в публикации → ✅ смотреть закрытие канала.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — TLS
- Раньше: [09 · Мониторинг и эксплуатация](../09-monitoring/basics.ru.md), [02 · Обменники и маршрутизация](../02-exchanges-and-routing/basics.ru.md)
- Следующий модуль: [12 · Federation и Shovel](../12-federation-and-shovel/overview.ru.md)
- Аналог в другом треке: [Linux](../../../linux/overview.ru.md)

## Источники

- [Authentication, Authorisation, Access Control](https://www.rabbitmq.com/docs/access-control)
- [Virtual Hosts](https://www.rabbitmq.com/docs/vhosts)
- [Production Checklist](https://www.rabbitmq.com/docs/production-checklist)
