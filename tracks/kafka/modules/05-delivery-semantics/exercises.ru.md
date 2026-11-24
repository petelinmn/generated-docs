---
title: "Упражнения: семантика доставки"
lang: ru
track: kafka
kind: module
doc: exercises
slug: 05-delivery-semantics
part: 2
tags: [kafka, exactly-once, transactions, isolation-level, fencing]
status: draft
updated: 2026-11-24
---

# Упражнения: семантика доставки

🌐 **Русский** · [English](exercises.en.md)

> Решайте сами; подсказки и решения — под спойлерами. Нужен одноразовый брокер `apache/kafka:4.3.1` и `confluent-kafka`. Решения запускались на Apache Kafka 4.3.1.

## 1. 🟢 Подтверждение и отмена

**Задача:** в одной теме запишите транзакцией два сообщения, затем отмените транзакцию с двумя сообщениями (с `flush()` перед отменой), затем подтвердите транзакцию с одним. Выведите `(low, high)`.

**Ожидаемый результат:** `(0, 8)`.

<details><summary>Подсказка</summary>

Метки подтверждения и отмены занимают смещения.

</details>

<details><summary>Решение</summary>

См. [основы](basics.ru.md#транзакции-производителя).

</details>

## 2. 🟢 Режимы чтения

**Задача:** прочитайте тему из упражнения 1 с `read_committed` и `read_uncommitted`. Сравните результаты.

**Ожидаемый результат:** первый — три подтверждённые записи; второй — ещё и две отменённые.

<details><summary>Подсказка</summary>

Параметр потребителя `isolation.level`; окно чтения не меньше 5–8 секунд.

</details>

<details><summary>Решение</summary>

См. [основы](basics.ru.md#транзакции-производителя).

</details>

## 3. 🟡 Открытая транзакция

**Задача:** откройте транзакцию, запишите сообщение `in-flight` с `flush()` и, не подтверждая, прочитайте тему в обоих режимах. Затем подтвердите и прочитайте снова.

**Ожидаемый результат:** `in-flight` виден только в `read_uncommitted`, а после `commit_transaction()` — и в `read_committed`.

<details><summary>Подсказка</summary>

Читайте до `commit_transaction()` и после.

</details>

<details><summary>Решение</summary>

См. [основы](basics.ru.md#открытая-транзакция).

</details>

## 4. 🟡 Конвейер «ровно один раз»

**Задача:** тема `in` с шестью записями. Реализуйте цикл чтение→верхний регистр→запись в `out` пачками по три в транзакции с `send_offsets_to_transaction`. Имитируйте сбой первой пачки (отмена) и повторите.

**Ожидаемый результат:** в `out` (в `read_committed`) шесть записей `M0…M5`, каждая один раз.

<details><summary>Подсказка</summary>

Смещение для фиксации — `offset + 1` последней записи; у потребителя `enable.auto.commit=False`, `isolation.level=read_committed`.

</details>

<details><summary>Решение</summary>

См. [продвинутую теорию](advanced.ru.md#чтение--обработка--запись-ровно-один-раз).

</details>

## 5. 🔴 Зомби

**Задача:** создайте двух производителей с одним `transactional.id` и вызовите `init_transactions()` у обоих. Попробуйте провести транзакцию первым. Какая ошибка и что с ней делать?

**Ожидаемый результат:** `_FENCED`, `fatal() == True`; перезапуск процесса.

<details><summary>Подсказка</summary>

Ошибка — `KafkaException`; код — `e.args[0].name()`.

</details>

<details><summary>Решение</summary>

См. [продвинутую теорию](advanced.ru.md#зомби-и-ограждение-fencing).

</details>
