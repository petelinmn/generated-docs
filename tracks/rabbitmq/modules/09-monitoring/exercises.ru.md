---
title: "Упражнения: мониторинг и эксплуатация"
lang: ru
track: rabbitmq
kind: module
doc: exercises
slug: 09-monitoring
part: 2
tags: [rabbitmq, monitoring, prometheus, alarms, management-api]
status: draft
updated: 2026-11-24
---

# Упражнения: мониторинг и эксплуатация

🌐 **Русский** · [English](exercises.en.md)

> Решайте сами; подсказки и решения — под спойлерами. Нужен контейнер `rabbitmq:4-management` с портами 5672, 15672 и 15692 (учебная одноразовая среда, пользователь `guest`/`guest`). Не меняйте пороги памяти на рабочем брокере.

## 1. 🟢 Глубина и неподтверждённые

**Задача:** опубликуйте 10 сообщений в очередь `mon`, запустите потребителя с `prefetch_count=3` без подтверждения и прочитайте `/api/queues/%2F/mon`. Выведите `messages`, `messages_ready`, `messages_unacknowledged`, `consumers`.

**Ожидаемый результат:** `10`, `7`, `3`, `1` (подождите несколько секунд: статистика обновляется с задержкой).

<details><summary>Подсказка</summary>

Basic-авторизация `guest:guest`, vhost `/` кодируется как `%2F`.

</details>

<details><summary>Решение</summary>

См. [основы](basics.ru.md#http-api-управления).

</details>

## 2. 🟢 Найдите приложение

**Задача:** откройте соединение с `connection_name="monitor-demo"` и найдите его в `/api/connections`.

**Ожидаемый результат:** в списке есть соединение с `client_properties.connection_name == "monitor-demo"`.

<details><summary>Подсказка</summary>

`pika.ConnectionParameters("localhost", client_properties={"connection_name": "monitor-demo"})`.

</details>

<details><summary>Решение</summary>

См. [основы](basics.ru.md#соединения-каналы-потребители).

</details>

## 3. 🟡 Метрики Prometheus

**Задача:** прочитайте `http://localhost:15692/metrics` и найдите `rabbitmq_queue_messages_ready`, `rabbitmq_connections`. Затем найдите ту же очередь в `/metrics/per-object`.

**Ожидаемый результат:** `rabbitmq_queue_messages_ready 7` в сводных, `rabbitmq_queue_messages_ready{vhost="/",queue="mon"} 7` — в per-object.

<details><summary>Подсказка</summary>

Фильтруйте строки по префиксу имени метрики.

</details>

<details><summary>Решение</summary>

См. [продвинутую теорию](advanced.ru.md#prometheus).

</details>

## 4. 🟡 Тревога по памяти

**Задача:** на одноразовом брокере задайте `rabbitmqctl set_vm_memory_high_watermark absolute 10MB`, опубликуйте сообщение, снимите ответ `/api/health/checks/alarms` и перехватите `Connection.Blocked`. Верните порог (`0.6`) и убедитесь, что тревога ушла.

**Ожидаемый результат:** `503`, `reason=low on memory`, после сброса `200` и `running`.

<details><summary>Подсказка</summary>

В pika у соединения есть `add_on_connection_blocked_callback`.

</details>

<details><summary>Решение</summary>

См. [продвинутую теорию](advanced.ru.md#оповещения-памяти-и-диска). Не забудьте вернуть порог.

</details>

## 5. 🔴 Ловушка устаревшей команды

**Задача:** при активной тревоге по памяти выполните `rabbitmq-diagnostics check_local_alarms` и `rabbitmq-diagnostics alarms`. Сравните и объясните расхождение.

**Ожидаемый результат:** первая печатает, что она устарела и ничего не делает («no local alarms»), вторая — `Memory alarm on node …`.

<details><summary>Подсказка</summary>

Читайте первую строку вывода.

</details>

<details><summary>Решение</summary>

В 4.4.1 `check_local_alarms` — устаревшая заглушка; для проверки тревог используйте `rabbitmq-diagnostics alarms` или HTTP-проверку `/api/health/checks/alarms`.

</details>
