---
title: "Упражнения: кластеризация и высокая доступность"
lang: ru
track: rabbitmq
kind: module
doc: exercises
slug: 08-clustering
part: 2
tags: [rabbitmq, cluster, quorum, high-availability, maintenance-mode]
status: draft
updated: 2026-11-24
---

# Упражнения: кластеризация и высокая доступность

🌐 **Русский** · [English](exercises.en.md)

> Решайте сами; подсказки и решения — под спойлерами. Нужен Docker и трёхузловой стенд из [базовой теории](basics.ru.md#как-проверялся-код) (учебная одноразовая среда; в конце удалите контейнеры и сеть: `docker rm -f rmq1 rmq2 rmq3; docker network rm rmqnet`).

## 1. 🟢 Соберите кластер

**Задача:** запустите три контейнера, присоедините `rmq2` и `rmq3` к `rmq1` и выведите `cluster_status`.

**Ожидаемый результат:** в `Running Nodes` три узла `rabbit@rmq1`, `rabbit@rmq2`, `rabbit@rmq3`.

<details><summary>Подсказка</summary>

На присоединяемых узлах: `stop_app`, `join_cluster rabbit@rmq1`, `start_app`; у всех одинаковый `RABBITMQ_ERLANG_COOKIE`.

</details>

<details><summary>Решение</summary>

Команды — в [основах](basics.ru.md#что-такое-кластер).

</details>

## 2. 🟢 Где живут очереди

**Задача:** объявите classic-очередь `cl` на `rmq1` и quorum-очередь `qu`. Выведите `rabbitmqctl list_queues name type leader members`.

**Ожидаемый результат:** у `cl` один член `rabbit@rmq1`, у `qu` — три члена и лидер.

<details><summary>Подсказка</summary>

Подключайтесь к порту 5681 (узел `rmq1`).

</details>

<details><summary>Решение</summary>

См. вывод в [основах](basics.ru.md#где-живёт-очередь).

</details>

## 3. 🟡 Отказ одного узла

**Задача:** опубликуйте по сообщению в обе очереди, остановите `rmq1` и подключитесь к `rmq3`. Проверьте доступность `cl` и `qu`, опубликуйте и прочитайте сообщения из `qu`. Затем запустите `rmq1` и проверьте `cl`.

**Ожидаемый результат:** `cl` недоступна (404), `qu` работает; после запуска `rmq1` сообщение в `cl` на месте.

<details><summary>Подсказка</summary>

Publish в `cl` с `delivery_mode=2`; после `docker start rmq1` подождите, пока узел поднимется.

</details>

<details><summary>Решение</summary>

См. [основы](basics.ru.md#отказ-узла).

</details>

## 4. 🟡 Потеря большинства

**Задача:** остановите `rmq1` и `rmq2` и опубликуйте в `qu` с подтверждением и таймаутом 6 с. Затем верните узлы и повторите.

**Ожидаемый результат:** `timeout`, после восстановления — `ack`.

<details><summary>Подсказка</summary>

Используйте помощник `Pub` из [модуля 05](../05-reliability/basics.ru.md#как-проверялся-код).

</details>

<details><summary>Решение</summary>

См. [основы](basics.ru.md#большинство-и-кворум).

</details>

## 5. 🔴 Безопасное обслуживание

**Задача:** выполните `check_if_node_is_quorum_critical` на `rmq3`, затем `rabbitmq-upgrade drain`, проверьте `cluster_status` и верните узел через `revive`.

**Ожидаемый результат:** узел «не критичен», затем `marked for maintenance`, затем снова `not under maintenance`.

<details><summary>Подсказка</summary>

Статус — раздел `Maintenance status` в `cluster_status`.

</details>

<details><summary>Решение</summary>

См. [продвинутую теорию](advanced.ru.md#режим-обслуживания).

</details>
