---
title: "Темы, хранение и компактация: экспертный уровень"
lang: ru
track: kafka
kind: module
doc: expert
slug: 04-topics-and-retention
part: 1
tags: [kafka, retention, log-compaction, topic-config, segment, tombstone, delete-records]
versions: "Apache Kafka 4.3.1; Python confluent-kafka 2.16.2 (librdkafka 2.16.0)"
status: draft
updated: 2026-11-24
---

# Темы, хранение и компактация: экспертный уровень

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · [2. Продвинутый](advanced.ru.md) · **3. Эксперт** · [4. Квиз](quiz.ru.md)

Опирается на [базовую](basics.ru.md) и [продвинутую](advanced.ru.md) теорию. «Проверено» означает наблюдалось на одном брокере Apache Kafka 4.3.1 с ускоренными проверками (`log.retention.check.interval.ms=2000`, `log.cleaner.backoff.ms=1000`).

## Справочник

### Настройки темы

| Настройка | Назначение | Проверено |
|---|---|---|
| `retention.ms` | срок хранения закрытых сегментов | да |
| `segment.ms` | через сколько закрывать сегмент | да |
| `segment.bytes` | размер сегмента; минимум 1048576 | да (отказ при 100000) |
| `cleanup.policy` (`delete`, `compact`, `compact,delete`) | политика очистки | `delete` (по умолчанию), `compact` — да |
| `min.cleanable.dirty.ratio` | доля «грязной» части для запуска компактации | да (0,01) |
| `delete.retention.ms` | срок хранения tombstone | задавался (3000), удаление не подтверждено |
| `min.compaction.lag.ms` | минимальный возраст записи до компактации | задавался (0) |
| `retention.bytes`, `max.compaction.lag.ms`, `min.insync.replicas`, `max.message.bytes`, `compression.type`, `message.timestamp.type`, `flush.*` | другие | не проверялись |

### Настройки брокера и значения образа

| Параметр | Значение в образе | Заметка |
|---|---|---|
| `log.retention.hours` | 168 | срок по умолчанию |
| `log.segment.bytes` | 1073741824 | |
| `log.retention.check.interval.ms` | 300000 (в опытах 2000) | **не** меняется динамически: `Cannot update these configs dynamically` |
| `log.cleaner.backoff.ms` | 15000 (в опытах 1000) | интервал очистителя |
| `log.cleanup.policy` | `delete` | значение по умолчанию для `cleanup.policy` |

### Инструменты (проверено)

| Команда | Результат |
|---|---|
| `kafka-configs.sh --describe --topic T` | настройки темы и их синонимы |
| `kafka-delete-records.sh --offset-json-file F` | `low_watermark` сдвинут до 6 |
| `Consumer.offsets_for_times([TopicPartition(t, p, ts)])` | первое смещение с меткой ≥ `ts` |
| `Consumer.get_watermark_offsets` | `(low, high)` |
| `AdminClient.create_partitions([NewPartitions(t, n)])` | увеличивает число партиций |

## Внутреннее устройство

- **Сегменты — единица удаления.** Журнал партиции — последовательность сегментов; удаляется сегмент целиком, когда все его записи старше `retention.ms`.
- **Закрытие сегмента по времени** (`segment.ms`) приводит к тому, что и «активный» сегмент со временем попадает под удаление (в опыте журнал опустел целиком: `low = high = 5`).
- **Нумерация не откатывается:** новые записи продолжали с 5 и 6 (проверено).
- **Компактация сохраняет смещения** оставшихся записей (9, 10, 11 — проверено), поэтому в нумерации возникают пропуски.
- **Tombstone — запись с пустым значением** и тем же ключом; очиститель удаляет прежние значения ключа (проверено).
- **Очистка периодична,** поэтому удаление происходит не «ровно по сроку».

> ⚠️ Проверить: алгоритм очистителя (грязные/чистые участки, карта смещений), точная логика выбора сегментов к удалению и срок жизни tombstone здесь не проверялись.

## Подвохи

### 1. Последняя запись тоже удаляется

После срока `retention.ms` и закрытия сегмента журнал мог опустеть целиком (проверено).

### 2. Смещения не переиспользуются

После очистки `low = high`, а новые записи получают следующие номера (проверено).

### 3. Минимум размера сегмента

`segment.bytes` меньше 1 МиБ отвергается (проверено).

### 4. Компактация не мгновенна и не трогает активный сегмент

Две записи одного ключа остались (проверено).

### 5. Tombstone нужно обрабатывать

Значение `None` — удаление ключа.

### 6. Добавление партиций ломает порядок ключей

Новый производитель отправлял четыре ключа из восьми в другие партиции (проверено).

### 7. Старый производитель не видит новые партиции

Он продолжил писать по прежней схеме (проверено).

## Компромиссы и альтернативы

| Выбор | Плюсы | Минусы |
|---|---|---|
| `delete` по сроку | простая история событий | потеря старого |
| `compact` | состояние по ключу, ограниченный размер | нет истории, нужны tombstone |
| `compact,delete` | состояние и ограничение срока | сложнее рассуждать |
| хранить дольше | можно перечитывать | больше диска |
| вынести историю в хранилище | неограниченная история | отдельный компонент |

## Вопросы с собеседований

<details><summary>1. Что удаляет записи из темы Kafka?</summary>

Политика хранения (по времени/размеру) или компактация; чтение их не удаляет.

→ [Основы](basics.ru.md#срок-хранения)

</details>

<details><summary>2. Подвох: записи удаляются по одной по истечении срока?</summary>

Нет, целыми сегментами.

→ [Основы](basics.ru.md#срок-хранения)

</details>

<details><summary>3. Что происходит со смещениями после очистки?</summary>

Они не переиспользуются: новые записи продолжают нумерацию.

→ [Подвох 2](#2-смещения-не-переиспользуются)

</details>

<details><summary>4. Чем компактация отличается от `delete`?</summary>

Хранит последнее значение каждого ключа, а не записи за период; смещения сохраняются.

→ [Продвинутая теория](advanced.ru.md#компактация-журнала)

</details>

<details><summary>5. Как удалить ключ в compact-теме?</summary>

Опубликовать запись с этим ключом и значением `None` (tombstone).

→ [Продвинутая теория](advanced.ru.md#могильные-записи-tombstones)

</details>

<details><summary>6. Подвох: после добавления партиций порядок записей ключа сохраняется?</summary>

Нет: часть ключей переходит в другие партиции.

→ [Подвох 6](#6-добавление-партиций-ломает-порядок-ключей)

</details>

<details><summary>7. Как вручную сдвинуть начало журнала?</summary>

`kafka-delete-records.sh` с JSON (`low_watermark` вырос до 6).

→ [Основы](basics.ru.md#удалить-записи-вручную)

</details>

<details><summary>8. Как найти смещение по времени?</summary>

`offsets_for_times`.

→ [Основы](basics.ru.md#поиск-по-времени)

</details>

<details><summary>9. Подвох: можно ли задать `segment.bytes=100000`?</summary>

Нет: минимум 1048576.

→ [Подвох 3](#3-минимум-размера-сегмента)

</details>

<details><summary>10. Почему компактация сохранила две записи `k9`?</summary>

Вероятно, они лежали в активном сегменте, который не очищается.

→ [Подвох 4](#4-компактация-не-мгновенна-и-не-трогает-активный-сегмент)

</details>

<details><summary>11. Почему старый производитель продолжил писать в старые партиции?</summary>

Он не обновил метаданные темы (интервал обновления по умолчанию 5 минут по документации).

→ [Подвох 7](#7-старый-производитель-не-видит-новые-партиции)

</details>

<details><summary>12. Когда `compact`, а когда `delete`?</summary>

`compact` — для состояния по ключу, `delete` — для истории событий за период.

→ [Продвинутая теория](advanced.ru.md#компактация-или-срок-хранения)

</details>

## Шпаргалка

```text
retention.ms / segment.ms / segment.bytes(>=1 MiB) ; cleanup.policy = delete | compact | compact,delete
delete-records: low watermark moves, offsets never reused ; compaction keeps offsets (gaps), tombstone = key + None
adding partitions remaps keys ; producers refresh metadata lazily
kafka-configs.sh --describe --topic T ; offsets_for_times ; kafka-delete-records.sh
```

- Сегменты удаляются целиком; последняя запись тоже.
- `compact` — состояние, `delete` — история.
- Число партиций планируйте заранее.

## Связанные материалы

- Предыдущий уровень: [продвинутая теория](advanced.ru.md)
- Раньше: [03 · Потребители и группы](../03-consumers-and-groups/expert.ru.md)
- Следующий модуль: [05 · Семантика доставки](../05-delivery-semantics/overview.ru.md)

## Источники

- [Apache Kafka: Topic configs](https://kafka.apache.org/documentation/#topicconfigs)
- [Apache Kafka: Log compaction](https://kafka.apache.org/documentation/#compaction)
- [Apache Kafka: Broker configs](https://kafka.apache.org/documentation/#brokerconfigs)
