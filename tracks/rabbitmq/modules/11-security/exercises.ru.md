---
title: "Упражнения: безопасность"
lang: ru
track: rabbitmq
kind: module
doc: exercises
slug: 11-security
part: 3
tags: [rabbitmq, security, permissions, tls, vhost]
status: draft
updated: 2026-11-24
---

# Упражнения: безопасность

🌐 **Русский** · [English](exercises.en.md)

> Решайте сами; подсказки и решения — под спойлерами. Работайте на одноразовом брокере (`docker exec rabbit …`), пароли — учебные. После упражнений удаляйте пользователей и vhost-ы.

## 1. 🟢 Vhost, пользователь, права

**Задача:** создайте vhost `shop`, пользователя `alice` с паролем и права `^app\..*` на `configure`, `write` и `read`. Выведите `list_permissions -p shop`.

**Ожидаемый результат:** строка `alice  ^app\..*  ^app\..*  ^app\..*`.

<details><summary>Подсказка</summary>

`set_permissions -p shop alice <configure> <write> <read>`.

</details>

<details><summary>Решение</summary>

См. команды в [основах](basics.ru.md#настройка).

</details>

## 2. 🟢 Отказы при подключении

**Задача:** подключитесь (а) с неверным паролем, (б) от неизвестного пользователя, (в) пользователем без прав на vhost `/`. Запишите код каждой ошибки.

**Ожидаемый результат:** (а) и (б) — `403 ACCESS_REFUSED`; (в) — `530 NOT_ALLOWED`.

<details><summary>Подсказка</summary>

`pika.PlainCredentials(user, password)` и `virtual_host=…`.

</details>

<details><summary>Решение</summary>

См. [основы](basics.ru.md#что-происходит-при-отказе).

</details>

## 3. 🟡 Права на ресурсы

**Задача:** под `alice` объявите `app.q` и `other.q`. Что получится в каждом случае и что закроется?

**Ожидаемый результат:** `app.q` создана; `other.q` — `403 ACCESS_REFUSED`, канал закрыт.

<details><summary>Подсказка</summary>

После ошибки откройте новый канал.

</details>

<details><summary>Решение</summary>

См. [основы](basics.ru.md#права-на-операции).

</details>

## 4. 🟡 Обменник по умолчанию

**Задача:** под `alice` опубликуйте в очередь `app.read` через `""`, затем через обменник `app.ex`, затем выдайте `write` на `amq.default` и повторите. Выведите состояние каналов и глубину.

**Ожидаемый результат:** сначала канал закрыт; через `app.ex` — работает; после выдачи права публикация в очередь работает.

<details><summary>Подсказка</summary>

Регулярное выражение для `write`: `^(app\..*|amq\.default)$`.

</details>

<details><summary>Решение</summary>

См. [основы](basics.ru.md#публикация-в-очередь-через-обменник-по-умолчанию).

</details>

## 5. 🔴 TLS

**Задача:** выпустите учебный CA и сертификат сервера для `localhost` (с расширениями `keyUsage`, `subjectAltName`), включите слушатель 5671 и подключитесь клиентом с проверкой. Затем проверьте отказы: неизвестный CA, неверный хост, обычный AMQP на TLS-порт.

**Ожидаемый результат:** подключение с проверкой открывается; три отказа.

<details><summary>Подсказка</summary>

Сертификаты в контейнер — `docker cp`; конфигурация в `conf.d/20-tls.conf`; в клиенте `ssl.create_default_context(cafile=…)`.

</details>

<details><summary>Решение</summary>

Команды и вывод — в [продвинутой теории](advanced.ru.md#учебные-сертификаты). Сертификаты одноразовые, не используйте их в рабочей системе.

</details>
