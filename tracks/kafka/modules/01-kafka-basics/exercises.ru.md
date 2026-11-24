---
title: "Упражнения: основы Kafka"
lang: ru
track: kafka
kind: module
doc: exercises
slug: 01-kafka-basics
part: 1
tags: [kafka, topic, partition, offset, consumer-group]
status: draft
updated: 2026-11-24
---

# Упражнения: основы Kafka

🌐 **Русский** · [English](exercises.en.md)

> Решайте сами; подсказки и решения — под спойлерами. Нужен одноразовый брокер `apache/kafka:4.3.1` (порт 9092) и `confluent-kafka` для Python. Решения запускались на Apache Kafka 4.3.1.

## 1. 🟢 Тема и партиции

**Задача:** создайте тему `orders` с тремя партициями и выведите номера партиций.

**Ожидаемый результат:** `create: {'orders': None}` и `[0, 1, 2]`.

<details><summary>Подсказка</summary>

`AdminClient.create_topics([NewTopic(…)])`, затем `list_topics`.

</details>

<details><summary>Решение</summary>

См. код в [основах](basics.ru.md#тема-и-партиции).

</details>

## 2. 🟢 Запись с отчётом

**Задача:** опубликуйте шесть записей с ключами `user0` и `user1` попеременно и выведите партицию и смещение каждой из обратного вызова.

**Ожидаемый результат:** записи каждого ключа в одной партиции, смещения 0, 1, 2.

<details><summary>Подсказка</summary>

`on_delivery=` и `flush()`.

</details>

<details><summary>Решение</summary>

См. код в [основах](basics.ru.md#запись).

</details>

## 3. 🟡 Чтение не удаляет

**Задача:** прочитайте тему двумя разными группами (`earliest`) и убедитесь, что результаты совпадают.

**Ожидаемый результат:** `True`.

<details><summary>Подсказка</summary>

Разные `group.id`; `enable.auto.commit=False`.

</details>

<details><summary>Решение</summary>

См. код в [основах](basics.ru.md#чтение-и-смещения).

</details>

## 4. 🟡 Границы партиций

**Задача:** выведите low и high watermark каждой партиции.

**Ожидаемый результат:** `low=0` везде; `high` равен числу записей партиции.

<details><summary>Подсказка</summary>

`Consumer.get_watermark_offsets(TopicPartition(topic, p), timeout=10)`.

</details>

<details><summary>Решение</summary>

См. [основы](basics.ru.md#чтение-и-смещения).

</details>

## 5. 🔴 Позиции групп

**Задача:** прочитайте тему новой группой с `earliest`, зафиксируйте позиции (`commit(asynchronous=False)`), выведите `kafka-consumer-groups.sh --describe --group …` и повторите чтение той же группой. Объясните результат.

**Ожидаемый результат:** `LAG` 0 у каждой партиции; повторное чтение пусто.

<details><summary>Подсказка</summary>

`auto.offset.reset` действует только без сохранённой позиции.

</details>

<details><summary>Решение</summary>

См. [продвинутую теорию](advanced.ru.md#позиции-групп-потребителей).

</details>
