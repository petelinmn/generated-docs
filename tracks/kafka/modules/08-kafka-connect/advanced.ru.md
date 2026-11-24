---
title: "Kafka Connect: продвинутый уровень"
lang: ru
track: kafka
kind: module
doc: advanced
slug: 08-kafka-connect
part: 2
tags: [kafka-connect, smt, single-message-transform, connector-lifecycle, config-validation, offsets]
versions: "Apache Kafka 4.3.1 (Kafka Connect, режим distributed)"
status: draft
updated: 2026-11-24
---

# Kafka Connect: продвинутый уровень

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

Опирается на [базовую теорию](basics.ru.md) (тот же воркер Connect 4.3.1 в режиме distributed).

## Жизненный цикл коннектора

Коннектором управляют через REST API: пауза, возобновление, перезапуск, удаление.

```python
rest("PUT", "/connectors/file-src/pause")
rest("PUT", "/connectors/file-src/resume")
```

```text
pause: 202
state: PAUSED
topic while paused: 3 records
resume: 202
topic after resume: 4 records
```

- Команды приняты с кодом `202` (выполняются асинхронно).
- Пока коннектор `PAUSED`, новая строка файла (`while paused`) **не попала** в тему (3 записи); после `resume` она появилась (4 записи): коннектор продолжил с места остановки.

Позиции коннектора (оффсеты источника) хранятся в теме `connect-offsets`. Запрос `GET /connectors/file-src/offsets` в нашем опыте вернул `{"offsets": []}`.

> ⚠️ Проверить: почему список позиций пуст для `FileStreamSourceConnector` (особенность этого учебного коннектора или момент запроса), здесь не выяснено; семантика сброса позиций (`DELETE …/offsets`) не проверялась.

## Преобразования одиночных записей (SMT)

**SMT** (single message transforms) изменяют каждую запись на лету, не требуя кода. Цепочка задаётся в конфигурации: `transforms=wrap,addsrc`.

```python
"transforms": "wrap,addsrc",
"transforms.wrap.type": "org.apache.kafka.connect.transforms.HoistField$Value", "transforms.wrap.field": "line",
"transforms.addsrc.type": "org.apache.kafka.connect.transforms.InsertField$Value",
"transforms.addsrc.static.field": "origin", "transforms.addsrc.static.value": "in2.txt"
```

```text
smt topic: ['{"line":"secret-1","origin":"in2.txt"}']
```

- `HoistField$Value` завернул строку в объект `{"line": "secret-1"}`;
- `InsertField$Value` добавил статическое поле `origin`.

Результат — JSON-объект вместо голой строки. Преобразования применяются по порядку.

## Проверка конфигурации

Воркер проверяет конфигурацию коннектора **до** создания:

```text
bad config: 400 {"error_code": 400, "message": "Connector configuration is invalid and contains the following 1 error(s):\nMissing required configuration \"topic\" which has no default value.\nYou can also find ...
```

Отсутствие обязательного параметра `topic` даёт `400` с понятным описанием, коннектор не создаётся.

## Конвертеры

Конвертеры (`key.converter`, `value.converter`) превращают данные Connect в байты Kafka и обратно. В опыте использовался `JsonConverter` с `schemas.enable=false`, поэтому в теме лежат обычные JSON-значения; для схем и реестра используют Avro-конвертер ([модуль 06](../06-schemas-and-serialization/basics.ru.md)).

> ⚠️ Проверить: Avro-конвертер и Schema Registry с Connect здесь не проверялись (нужен отдельный набор библиотек в `plugin.path`).

## Лучшие практики

- Несколько воркеров, RF=3 у внутренних тем (`connect-offsets`, `connect-configs`, `connect-status`).
- Конфигурацию коннекторов храните в репозитории и применяйте через REST/CI.
- Проверяйте `status` и состояние задач; настройте тревоги на `FAILED`.
- Преобразования — для простых изменений; сложную логику выносите в потоковую обработку.
- Для рабочих данных берите специализированные коннекторы, а не `FileStream*`.

## Частые ошибки

- ❌ Ожидать, что пауза теряет данные источника → ✅ источник продолжит с места остановки (проверено).
- ❌ Складывать логику в цепочку SMT → ✅ SMT для простых правок.
- ❌ Не проверять `status` задач → ✅ задача может упасть при живом коннекторе.
- ❌ Использовать JSON без схем при эволюции формата → ✅ Avro + реестр.

## Связанные материалы

- Раньше: [основы](basics.ru.md)
- Дальше: [экспертная теория](expert.ru.md)
- [06 · Схемы и сериализация](../06-schemas-and-serialization/advanced.ru.md)
- Следующий модуль: [09 · Мониторинг и эксплуатация](../../overview.ru.md)

## Источники

- [Apache Kafka: Connect transformations](https://kafka.apache.org/documentation/#connect_transforms)
- [Apache Kafka: Connect REST API](https://kafka.apache.org/documentation/#connect_rest)
- [Apache Kafka: Kafka Connect](https://kafka.apache.org/documentation/#connect)
