---
title: "Темы, хранение и компактация: базовый уровень"
lang: ru
track: kafka
kind: module
doc: basics
slug: 04-topics-and-retention
part: 1
tags: [kafka, retention, segment, log-compaction, topic-config, delete-records]
versions: "Apache Kafka 4.3.1; Python confluent-kafka 2.16.2 (librdkafka 2.16.0)"
status: draft
updated: 2026-11-24
---

# Темы, хранение и компактация: базовый уровень

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [overview](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [01 · Основы Kafka](../01-kafka-basics/basics.ru.md), [03 · Потребители и группы](../03-consumers-and-groups/basics.ru.md) · **Время:** ~1,5 ч

## Как проверялся код

Код запускался на Apache Kafka 4.3.1 (один брокер, KRaft, `apache/kafka:4.3.1`) в Docker с `confluent-kafka` 2.16.2; выводы скопированы из запусков. **Чтобы не ждать минуты, брокер запущен с ускоренными проверками:** `log.retention.check.interval.ms=2000` и `log.cleaner.backoff.ms=1000` (по умолчанию 300000 и 15000 мс). Для этого в контейнер до старта скопирован файл `server.properties` с двумя добавленными строками (`docker cp … kafka:/mnt/shared/config/server.properties`); значения по умолчанию в образе другие, а `log.retention.check.interval.ms` нельзя менять динамически (`Cannot update these configs dynamically`). Помощники: `fresh(topic, parts=1, **cfg)` пересоздаёт тему с настройками (`retention_ms=…` превращается в `retention.ms`); `wm(topic)` возвращает `(low, high)`; `read(topic, secs)` читает тему с начала новой группой и возвращает `(смещение, ключ, значение)`. Читателю нужно несколько секунд на вступление в группу: короткое окно даёт пустой результат.

## Срок хранения

Kafka не удаляет записи при чтении ([модуль 01](../01-kafka-basics/basics.ru.md#журнал-вместо-очереди)); их удаляет **политика хранения**. Самая частая — по времени: `retention.ms` (по умолчанию в образе `log.retention.hours=168`, то есть 7 суток). Удаляются не отдельные записи, а целые **сегменты** журнала ([модуль 01](../01-kafka-basics/basics.ru.md#как-это-лежит-на-диске)).

Тема с `retention.ms=4000` и `segment.ms=1500` (сегмент закрывается примерно через 1,5 с):

```python
fresh("ret", retention_ms=4000, segment_ms=1500)
for i in range(5): p.produce("ret", value=f"old{i}".encode())
```

```text
t=0   watermarks: (0, 5)
t=8s  (one active segment, no roll yet) watermarks: (5, 5) readable: []
after roll +3s watermarks: (5, 7) readable: []
after roll +6s watermarks: (7, 7) readable: []
```

- Через 8 секунд **все пять записей удалены**: `low` дошёл до 5, `high` тоже 5 — журнал пуст.
- Новые записи продолжили нумерацию с 5 (смещения **не переиспользуются**): после двух новых записей `high` 7; затем они тоже истекли (`low` 7).

> В подписи «one active segment, no roll yet» я предполагал, что активный сегмент не удаляется. Наблюдение это **опровергло**: сегмент закрылся по `segment.ms` и был удалён целиком. Не рассчитывайте, что последние записи темы переживут срок хранения.

## Удалить записи вручную

`kafka-delete-records.sh` сдвигает нижнюю границу журнала партиции:

```text
Records delete operation completed:
partition: dr-0	low_watermark: 6
dr before: (0, 10)
dr after : (6, 10)
```

Записи со смещениями 0–5 стали недоступны (`low` = 6), а `high` остался 10.

## Настройки темы

Настройки темы задают при создании или меняют позже; описание показывает `kafka-configs.sh --describe --topic …`:

```text
Dynamic configs for topic cmp are:
  cleanup.policy=compact sensitive=false synonyms={DYNAMIC_TOPIC_CONFIG:cleanup.policy=compact, DEFAULT_CONFIG:log.cleanup.policy=delete}
```

Настройка темы (`DYNAMIC_TOPIC_CONFIG`) перекрывает значение по умолчанию (`DEFAULT_CONFIG`) брокера: для `cleanup.policy` значение по умолчанию — `delete`.

### Ограничение на размер сегмента

```text
Invalid value 100000 for configuration segment.bytes: Value must be at least 1048576
```

Сегмент не может быть меньше 1 МиБ. Поэтому в опытах размер сегмента ограничивали временем (`segment.ms`).

## Поиск по времени

У каждой записи есть метка времени; по ней можно найти смещение:

```python
res = c.offsets_for_times([TopicPartition("tsq", 0, mid)], timeout=10)
```

```text
first offset with timestamp >= mid: 1
```

Записи `a` (до `mid`) и `b` (после): первое смещение с меткой не раньше `mid` — 1, то есть запись `b`. Так перематывают чтение на «час назад».

## Типичные ошибки новичков

- ❌ Считать, что тема хранит данные вечно → ✅ действует `retention.ms` (по умолчанию 168 ч).
- ❌ Ожидать, что последняя запись переживёт срок → ✅ сегменты удаляются целиком, активный тоже закрывается по `segment.ms`.
- ❌ Ждать переиспользования смещений → ✅ нумерация не откатывается.
- ❌ Ждать удаления «ровно по сроку» → ✅ проверка идёт периодически (по умолчанию раз в 5 минут).
- ❌ Читать недавно созданной группой с коротким окном → ✅ дайте время на вступление в группу.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — компактация, добавление партиций
- Раньше: [03 · Потребители и группы](../03-consumers-and-groups/basics.ru.md)
- Следующий модуль: [05 · Семантика доставки](../05-delivery-semantics/overview.ru.md)
- Аналог в другом треке: [RabbitMQ: TTL и лимиты длины](../../../rabbitmq/modules/03-queues-and-delivery/advanced.ru.md)

## Источники

- [Apache Kafka: Topic configs](https://kafka.apache.org/documentation/#topicconfigs)
- [Apache Kafka: Log](https://kafka.apache.org/documentation/#log)
- [Apache Kafka: Basic Kafka operations](https://kafka.apache.org/documentation/#basic_ops)
