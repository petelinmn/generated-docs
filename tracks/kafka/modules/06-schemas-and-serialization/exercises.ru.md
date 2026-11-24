---
title: "Упражнения: схемы и сериализация"
lang: ru
track: kafka
kind: module
doc: exercises
slug: 06-schemas-and-serialization
part: 2
tags: [kafka, schema-registry, avro, compatibility]
status: draft
updated: 2026-11-24
---

# Упражнения: схемы и сериализация

🌐 **Русский** · [English](exercises.en.md)

> Решайте сами; подсказки и решения — под спойлерами. Нужны брокер Kafka, Confluent Schema Registry (`confluentinc/cp-schema-registry`, порт 8081 или другой) и `pip install "confluent-kafka[avro,schemaregistry]" fastavro`. Решения запускались на Apache Kafka 4.3.1 и Schema Registry 8.4.1.

## 1. 🟢 JSON против Avro

**Задача:** закодируйте запись `{"id": 1001, "customer": "alice", "amount": 19.99}` в JSON и в Avro (`fastavro.schemaless_writer`). Сравните размеры.

**Ожидаемый результат:** `50` и `16` байт.

<details><summary>Подсказка</summary>

`fastavro.parse_schema(json.loads(V1))`, запись в `io.BytesIO`.

</details>

<details><summary>Решение</summary>

См. [основы](basics.ru.md#json-и-avro).

</details>

## 2. 🟢 Формат записи

**Задача:** сериализуйте запись `AvroSerializer` и выведите магический байт, идентификатор схемы и длину.

**Ожидаемый результат:** магический байт `0`, идентификатор схемы (в опыте `1`), всего 21 байт.

<details><summary>Подсказка</summary>

`struct.unpack(">I", payload[1:5])`.

</details>

<details><summary>Решение</summary>

См. [основы](basics.ru.md#формат-записи-с-реестром).

</details>

## 3. 🟡 Совместимость

**Задача:** зарегистрируйте `V1`, затем проверьте (`test_compatibility`) версию с полем `currency` (а) со значением по умолчанию и (б) без. Попробуйте зарегистрировать (б).

**Ожидаемый результат:** (а) `True`, (б) `False`; регистрация (б) — `409`, код `40901`.

<details><summary>Подсказка</summary>

Ошибка — `SchemaRegistryError`, поля `http_status_code` и `error_code`.

</details>

<details><summary>Решение</summary>

См. [продвинутую теорию](advanced.ru.md#эволюция-схем-и-совместимость).

</details>

## 4. 🟡 Читатели разных версий

**Задача:** запишите запись по `V2` (с `currency="EUR"`) и прочитайте её читателем без схемы читателя, читателем на `V1`; затем запись по `V1` прочитайте читателем на `V2`.

**Ожидаемый результат:** первый видит `currency`, второй — нет; третий видит `currency="USD"`.

<details><summary>Подсказка</summary>

`AvroDeserializer(sr, reader_schema_str)`.

</details>

<details><summary>Решение</summary>

См. [продвинутую теорию](advanced.ru.md#что-видят-читатели-с-разными-схемами).

</details>

## 5. 🔴 Переименование и смена типа

**Задача:** проверьте `test_compatibility` для (а) переименования поля `customer` → `client`, (б) того же с `aliases`, (в) смены типа `amount` с double на string.

**Ожидаемый результат:** (а) `False`, (б) `True`, (в) `False`.

<details><summary>Подсказка</summary>

Алиас — свойство поля нового имени: `"aliases": ["customer"]`.

</details>

<details><summary>Решение</summary>

См. [квиз, вопрос 10](quiz.ru.md#10--найдите-ошибку) и [экспертную теорию](expert.ru.md#вопросы-с-собеседований).

</details>
