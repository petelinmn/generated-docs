---
title: "Упражнения: репликация и доступность"
lang: ru
track: kafka
kind: module
doc: exercises
slug: 07-replication-and-availability
part: 2
tags: [kafka, replication, isr, min-insync-replicas, leader-election]
status: draft
updated: 2026-11-24
---

# Упражнения: репликация и доступность

🌐 **Русский** · [English](exercises.en.md)

> Решайте сами; подсказки и решения — под спойлерами. Нужен одноразовый кластер из трёх брокеров Apache Kafka 4.3.1 в Docker (комбинированные узлы KRaft, см. [как проверялся код](basics.ru.md#как-проверялся-код)) и `confluent-kafka`. По окончании: `docker rm -f kafka1 kafka2 kafka3; docker network rm kfk`.

## 1. 🟢 Раскладка лидеров

**Задача:** создайте тему из трёх партиций с RF=3 и `min.insync.replicas=2`. Выведите лидеров, реплики и ISR.

**Ожидаемый результат:** лидеры распределены по брокерам, ISR каждой партиции содержит три брокера.

<details><summary>Подсказка</summary>

`kafka-topics.sh --describe --topic T` или `AdminClient.list_topics`.

</details>

<details><summary>Решение</summary>

См. [основы](basics.ru.md#репликация).

</details>

## 2. 🟢 Отказ лидера

**Задача:** остановите брокера — лидера партиции 0 — и через 8 секунд посмотрите лидера и ISR. Запишите записи с `acks=all`.

**Ожидаемый результат:** новый лидер, ISR из двух брокеров, запись успешна.

<details><summary>Подсказка</summary>

`docker stop kafkaN`; подключайтесь к живым брокерам.

</details>

<details><summary>Решение</summary>

См. [основы](basics.ru.md#отказ-лидера).

</details>

## 3. 🟡 Недостаточно реплик

**Задача:** остановите второго брокера. Запишите записи с `acks=all` и `acks=1` (`message.timeout.ms=15000`).

**Ожидаемый результат:** `acks=all` — `_MSG_TIMED_OUT`; `acks=1` — `ok`.

<details><summary>Подсказка</summary>

Подключайтесь к оставшемуся брокеру.

</details>

<details><summary>Решение</summary>

См. [основы](basics.ru.md#acks-и-mininsyncreplicas).

</details>

## 4. 🟡 Восстановление

**Задача:** запустите остановленных брокеров и через 25 секунд выведите лидеров и ISR всех партиций.

**Ожидаемый результат:** полный ISR; лидеры не обязательно вернулись к предпочтительным.

<details><summary>Подсказка</summary>

`--under-replicated-partitions` должен быть пуст.

</details>

<details><summary>Решение</summary>

См. [основы](basics.ru.md#восстановление).

</details>

## 5. 🔴 Кворум контроллеров

**Задача:** оставьте живым один брокер из трёх и попробуйте создать тему с таймаутом 8 секунд. Затем запустите остальные.

**Ожидаемый результат:** таймаут; после запуска кворум восстановился.

<details><summary>Подсказка</summary>

`create_topics([...], request_timeout=8, operation_timeout=8)`.

</details>

<details><summary>Решение</summary>

См. [продвинутую теорию](advanced.ru.md#кворум-контроллеров).

</details>
