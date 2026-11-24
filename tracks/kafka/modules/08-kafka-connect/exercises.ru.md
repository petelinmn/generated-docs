---
title: "Упражнения: Kafka Connect"
lang: ru
track: kafka
kind: module
doc: exercises
slug: 08-kafka-connect
part: 2
tags: [kafka-connect, connector, smt, rest-api]
status: draft
updated: 2026-11-24
---

# Упражнения: Kafka Connect

🌐 **Русский** · [English](exercises.en.md)

> Решайте сами; подсказки и решения — под спойлерами. Нужны кластер Kafka и воркер Connect в режиме distributed с `connect-file-*.jar` в `plugin.path` (см. [как проверялся код](basics.ru.md#как-проверялся-код)); REST — на `http://localhost:18083`. Решения запускались на Kafka 4.3.1. По окончании удалите контейнер `connect`.

## 1. 🟢 Источник

**Задача:** создайте коннектор `FileStreamSourceConnector` для файла `/tmp/in.txt` (две строки) и темы `connect-test`. Выведите статус и записи темы.

**Ожидаемый результат:** код `201`, состояния `RUNNING`, две записи (`"line one"`, `"line two"`).

<details><summary>Подсказка</summary>

`POST /connectors` с телом `{"name": …, "config": {…}}`.

</details>

<details><summary>Решение</summary>

См. [основы](basics.ru.md#источник-файл--тема).

</details>

## 2. 🟢 Приёмник

**Задача:** создайте `FileStreamSinkConnector` из темы `connect-test` в файл `/tmp/out.txt`. Проверьте содержимое файла.

**Ожидаемый результат:** строки совпадают со строками исходного файла.

<details><summary>Подсказка</summary>

Параметр темы — `topics`.

</details>

<details><summary>Решение</summary>

См. [основы](basics.ru.md#приёмник-тема--файл).

</details>

## 3. 🟡 Пауза и возобновление

**Задача:** поставьте источник на паузу, допишите строку в файл, посмотрите тему, затем возобновите и посмотрите снова.

**Ожидаемый результат:** во время паузы новой записи нет, после `resume` она появляется.

<details><summary>Подсказка</summary>

`PUT /connectors/<имя>/pause` и `/resume`; пауза и возобновление асинхронны (`202`).

</details>

<details><summary>Решение</summary>

См. [продвинутую теорию](advanced.ru.md#жизненный-цикл-коннектора).

</details>

## 4. 🟡 Преобразования

**Задача:** создайте источник с цепочкой `HoistField$Value` (поле `line`) и `InsertField$Value` (статическое поле `origin`). Выведите запись темы.

**Ожидаемый результат:** `{"line":"secret-1","origin":"in2.txt"}`.

<details><summary>Подсказка</summary>

`transforms=wrap,addsrc`; параметры `transforms.<имя>.…`.

</details>

<details><summary>Решение</summary>

См. [продвинутую теорию](advanced.ru.md#преобразования-одиночных-записей-smt).

</details>

## 5. 🔴 Неверная конфигурация

**Задача:** создайте `FileStreamSourceConnector` без параметра `topic`. Какой код и сообщение вернёт воркер, создан ли коннектор?

**Ожидаемый результат:** `400`, сообщение о недостающем `topic`, коннектор не создан.

<details><summary>Подсказка</summary>

Проверьте `GET /connectors` после попытки.

</details>

<details><summary>Решение</summary>

См. [продвинутую теорию](advanced.ru.md#проверка-конфигурации).

</details>
