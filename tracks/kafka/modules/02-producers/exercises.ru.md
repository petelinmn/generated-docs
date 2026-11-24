---
title: "Упражнения: производители"
lang: ru
track: kafka
kind: module
doc: exercises
slug: 02-producers
part: 1
tags: [kafka, producer, partitioning, acks, idempotence, compression]
status: draft
updated: 2026-11-24
---

# Упражнения: производители

🌐 **Русский** · [English](exercises.en.md)

> Решайте сами; подсказки и решения — под спойлерами. Нужен одноразовый брокер `apache/kafka:4.3.1` и `confluent-kafka`. Решения запускались на Apache Kafka 4.3.1. Числа у вас будут другими.

## 1. 🟢 Явная партиция

**Задача:** отправьте три записи с одним ключом, указав `partition=2`. Выведите партиции из обратных вызовов.

**Ожидаемый результат:** `[2, 2, 2]`.

<details><summary>Подсказка</summary>

`produce(topic, key=…, value=…, partition=2, on_delivery=cb)`.

</details>

<details><summary>Решение</summary>

См. [основы](basics.ru.md#явная-партиция).

</details>

## 2. 🟢 Что с `acks=0`

**Задача:** отправьте 20 000 записей с `acks=0`, `1` и `all` и выведите смещение из отчёта первых записей.

**Ожидаемый результат:** при `acks=0` смещение `None`, при `1` и `all` — числа.

<details><summary>Подсказка</summary>

Собирайте `msg.offset()` в обратном вызове.

</details>

<details><summary>Решение</summary>

См. [основы](basics.ru.md#подтверждения-брокера-acks).

</details>

## 3. 🟡 Идемпотентность

**Задача:** создайте производитель с `enable.idempotence=True` и `acks=1`, затем с `acks=all`. Выведите результат.

**Ожидаемый результат:** первая попытка — `KafkaException` с `_INVALID_ARG`; вторая — успех.

<details><summary>Подсказка</summary>

Ошибка возникает при создании `Producer`, а не при отправке.

</details>

<details><summary>Решение</summary>

См. [продвинутую теорию](advanced.ru.md#идемпотентный-производитель).

</details>

## 4. 🟡 Цена `linger.ms`

**Задача:** замерьте задержку доставки одной записи при `linger.ms=0` и `100` (пять замеров каждого).

**Ожидаемый результат:** около 1 мс против около 100 мс.

<details><summary>Подсказка</summary>

`produce` с `on_delivery`, затем `poll(0.001)` до срабатывания.

</details>

<details><summary>Решение</summary>

См. [продвинутую теорию](advanced.ru.md#пакеты-и-задержка-lingerms).

</details>

## 5. 🔴 Ошибки доставки

**Задача:** (а) отправьте запись размером 2 МБ; (б) отправьте запись производителем, подключённым к несуществующему брокеру (`localhost:1`, `message.timeout.ms=3000`). Определите, где и с каким кодом проявляется каждая ошибка.

**Ожидаемый результат:** (а) `MSG_SIZE_TOO_LARGE` при `produce`; (б) `_MSG_TIMED_OUT` в обратном вызове.

<details><summary>Подсказка</summary>

Оберните `produce` в `try/except KafkaException`; для (б) вызовите `flush(10)`.

</details>

<details><summary>Решение</summary>

См. [основы](basics.ru.md#ошибки).

</details>
