---
title: "Упражнения: темы, хранение и компактация"
lang: ru
track: kafka
kind: module
doc: exercises
slug: 04-topics-and-retention
part: 1
tags: [kafka, retention, log-compaction, tombstone, delete-records]
status: draft
updated: 2026-11-24
---

# Упражнения: темы, хранение и компактация

🌐 **Русский** · [English](exercises.en.md)

> Решайте сами; подсказки и решения — под спойлерами. Нужен одноразовый брокер `apache/kafka:4.3.1` с ускоренными проверками (см. [как проверялся код](basics.ru.md#как-проверялся-код)) и `confluent-kafka`. Решения запускались на Apache Kafka 4.3.1.

## 1. 🟢 Срок хранения

**Задача:** создайте тему с `retention.ms=4000` и `segment.ms=1500`, запишите пять сообщений и следите за `(low, high)` по прошествии 8 секунд.

**Ожидаемый результат:** `(0, 5)`, затем `(5, 5)`.

<details><summary>Подсказка</summary>

`get_watermark_offsets` у `Consumer` с любым `group.id`.

</details>

<details><summary>Решение</summary>

См. [основы](basics.ru.md#срок-хранения).

</details>

## 2. 🟢 Ручное удаление

**Задача:** запишите десять сообщений и сдвиньте `low_watermark` до 6 командой `kafka-delete-records.sh`.

**Ожидаемый результат:** `(0, 10)`, затем `(6, 10)`.

<details><summary>Подсказка</summary>

JSON: `{"partitions": [{"topic": "dr", "partition": 0, "offset": 6}], "version": 1}`; файл положите в контейнер `docker cp`.

</details>

<details><summary>Решение</summary>

См. [основы](basics.ru.md#удалить-записи-вручную).

</details>

## 3. 🟡 Компактация

**Задача:** создайте тему с `cleanup.policy=compact`, обновите три ключа по четыре раза и прочитайте тему после очистки.

**Ожидаемый результат:** по одной последней записи на ключ, с исходными смещениями (9, 10, 11).

<details><summary>Подсказка</summary>

Нужны закрытые сегменты: `segment.ms=1000` и пара записей после паузы; ждите 8–10 секунд.

</details>

<details><summary>Решение</summary>

См. [продвинутую теорию](advanced.ru.md#компактация-журнала).

</details>

## 4. 🟡 Tombstone

**Задача:** в compact-теме удалите ключ `k2` записью со значением `None` и прочитайте тему.

**Ожидаемый результат:** старые значения `k2` пропали, осталась запись `(k2, None)`.

<details><summary>Подсказка</summary>

`produce(topic, key=b"k2", value=None)`.

</details>

<details><summary>Решение</summary>

См. [продвинутую теорию](advanced.ru.md#могильные-записи-tombstones).

</details>

## 5. 🔴 Добавление партиций

**Задача:** тема из трёх партиций, восемь ключей `user0…user7`. Запишите их, увеличьте число партиций до шести и запишите снова **новым** производителем. Какие ключи сменили партицию? Что делает старый производитель?

**Ожидаемый результат:** часть ключей (в проверке `user0`, `user2`, `user3`, `user5`) в других партициях; старый производитель пишет по-старому.

<details><summary>Подсказка</summary>

`AdminClient.create_partitions([NewPartitions(t, 6)])`; не создавайте нового производителя для «старого» эксперимента.

</details>

<details><summary>Решение</summary>

См. [продвинутую теорию](advanced.ru.md#добавление-партиций-и-ключи).

</details>
