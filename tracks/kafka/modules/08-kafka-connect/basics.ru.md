---
title: "Kafka Connect: базовый уровень"
lang: ru
track: kafka
kind: module
doc: basics
slug: 08-kafka-connect
part: 2
tags: [kafka-connect, connector, source-connector, sink-connector, rest-api, distributed-mode]
versions: "Apache Kafka 4.3.1 (Kafka Connect, режим distributed)"
status: draft
updated: 2026-11-24
---

# Kafka Connect: базовый уровень

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [overview](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [01 · Основы Kafka](../01-kafka-basics/basics.ru.md), [06 · Схемы и сериализация](../06-schemas-and-serialization/basics.ru.md) · **Время:** ~1,5 ч

## Как проверялся код

Рабочий процесс Kafka Connect (4.3.1) запущен в режиме **distributed** в контейнере на образе `apache/kafka:4.3.1` командой `connect-distributed.sh` в сети Docker `kfk` рядом с кластером из трёх брокеров ([модуль 07](../07-replication-and-availability/basics.ru.md#как-проверялся-код)); REST API опубликован на `http://localhost:18083`. Файл конфигурации воркера: `bootstrap.servers=kafka1:19092,…`, `group.id=connect-cluster`, `JsonConverter` для ключей и значений (`schemas.enable=false`), внутренние темы `connect-offsets`, `connect-configs`, `connect-status` с RF=3, `plugin.path=/tmp/plugins` (в него скопированы `connect-file-*.jar` из `/opt/kafka/libs`). Выводы скопированы из запусков. Помощник `rest(method, path, body)` вызывает REST API и возвращает `(код, JSON)`; `sh(cmd)` выполняет команду в контейнере `connect`; `read(topic, n)` читает тему. **Коннекторы `FileStream*` — учебные примеры из поставки Kafka**, а не рабочие решения для данных (по документации).

## Что такое Kafka Connect

**Kafka Connect** — часть Apache Kafka для потоковой интеграции с внешними системами без написания кода: **коннекторы** перекладывают данные между Kafka и базой, файлами, хранилищем и т. д.

| Вид | Направление | Пример |
|---|---|---|
| **Source** (источник) | внешняя система → Kafka | `FileStreamSourceConnector`: строки файла → записи темы |
| **Sink** (приёмник) | Kafka → внешняя система | `FileStreamSinkConnector`: записи темы → строки файла |

Процессы Connect называются **воркерами** (workers); коннектор делится на **задачи** (tasks), которые выполняют работу параллельно. В режиме distributed воркеры образуют группу, делят между собой задачи и хранят состояние в темах Kafka.

## REST API

Коннекторами управляют через REST API воркера (порт 8083):

```text
GET /                       -> {"version":"4.3.1", ... "kafka_cluster_id": ...}
GET /connector-plugins      -> доступные классы коннекторов
POST /connectors            -> создать коннектор
```

Доступные плагины (в нашем `plugin.path`): `FileStreamSinkConnector`, `FileStreamSourceConnector`, а также `MirrorSourceConnector`, `MirrorCheckpointConnector`, `MirrorHeartbeatConnector` (репликация между кластерами, встроены в Kafka).

## Источник: файл → тема

```python
rest("POST", "/connectors", {"name": "file-src", "config": {
    "connector.class": "org.apache.kafka.connect.file.FileStreamSourceConnector", "tasks.max": "1",
    "file": "/tmp/in.txt", "topic": "connect-test"}})
```

```text
create: 201 file-src source
status: {"name": "file-src", "connector": {"state": "RUNNING", ...}, "tasks": [{"id": 0, "state": "RUNNING", ...}], "type": "source"}
topic: ['"line one"', '"line two"']
after append: ['"line one"', '"line two"', '"line three"']
```

- Коннектор создан (код `201`), он и его задача в состоянии `RUNNING`.
- Строки файла стали записями темы `connect-test` (значения — JSON-строки: `"line one"`).
- Дописанная в файл строка `line three` **появилась в теме** сама: источник следит за файлом.

## Приёмник: тема → файл

```python
rest("POST", "/connectors", {"name": "file-sink", "config": {
    "connector.class": "org.apache.kafka.connect.file.FileStreamSinkConnector", "tasks.max": "1",
    "file": "/tmp/out.txt", "topics": "connect-test"}})
```

```text
sink file: ['line one', 'line two', 'line three']
```

Приёмник записал содержимое темы в файл. Обратите внимание: у источника параметр `topic` (один), у приёмника — `topics` (список).

## Типичные ошибки новичков

- ❌ Писать код ради простой интеграции → ✅ сначала проверьте, есть ли готовый коннектор.
- ❌ Использовать `FileStream*` в рабочей системе → ✅ это примеры; для файлов и баз берут специализированные коннекторы.
- ❌ Путать `topic` у источника и `topics` у приёмника → ✅ разные параметры.
- ❌ Не следить за состоянием → ✅ `GET /connectors/<имя>/status`.
- ❌ Один воркер в рабочей системе → ✅ несколько воркеров и RF=3 у внутренних тем.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — жизненный цикл, преобразования, проверка конфигурации
- Раньше: [07 · Репликация и доступность](../07-replication-and-availability/basics.ru.md)
- Следующий модуль: [09 · Мониторинг и эксплуатация](../../overview.ru.md)

## Источники

- [Apache Kafka: Kafka Connect](https://kafka.apache.org/documentation/#connect)
- [Apache Kafka: Connect REST API](https://kafka.apache.org/documentation/#connect_rest)
- [Apache Kafka: Connect configs](https://kafka.apache.org/documentation/#connectconfigs)
