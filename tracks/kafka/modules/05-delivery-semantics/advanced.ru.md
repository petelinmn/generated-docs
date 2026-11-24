---
title: "Семантика доставки: продвинутый уровень"
lang: ru
track: kafka
kind: module
doc: advanced
slug: 05-delivery-semantics
part: 2
tags: [kafka, exactly-once, consume-process-produce, send-offsets-to-transaction, zombie-fencing, transactional-id]
versions: "Apache Kafka 4.3.1; Python confluent-kafka 2.16.2 (librdkafka 2.16.0)"
status: draft
updated: 2026-11-24
---

# Семантика доставки: продвинутый уровень

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

Опирается на [базовую теорию](basics.ru.md). Код запускался на Apache Kafka 4.3.1 (один брокер) с `confluent-kafka` 2.16.2.

## Чтение — обработка — запись «ровно один раз»

Типовая задача: прочитать запись из темы `in`, преобразовать и записать результат в `out`. «Ровно один раз» требует, чтобы **запись результата и фиксация позиции чтения** произошли атомарно: в одной транзакции. Для этого `send_offsets_to_transaction` добавляет в транзакцию смещения потребителя:

```python
def run_once(crash_after=False, txid="eos-app"):
    c = Consumer({..., "group.id": group, "enable.auto.commit": False, "isolation.level": "read_committed"})
    c.subscribe(["in"])
    tpp = Producer({"bootstrap.servers": BS, "transactional.id": txid}); tpp.init_transactions()
    got = poll_up_to_3_messages(c)
    tpp.begin_transaction()
    for m in got: tpp.produce("out", value=m.value().upper())
    offs = [TopicPartition("in", got[-1].partition(), got[-1].offset() + 1)]
    tpp.send_offsets_to_transaction(offs, c.consumer_group_metadata())
    if crash_after: tpp.abort_transaction(); return -len(got)      # simulated crash
    tpp.commit_transaction(); return len(got)
```

Потребитель с `enable.auto.commit=False` и `isolation.level=read_committed`, смещение для фиксации — **следующее** к чтению (`offset + 1`, [модуль 03](../03-consumers-and-groups/basics.ru.md#зафиксированное-смещение--следующее-к-чтению)). Тема `in` содержит шесть записей `m0…m5`:

```text
batch 1 aborted (simulated crash): -3
output visible after the abort   : []
batch 1 retried                  : 3
batch 2                          : 3
batch 3 (nothing left)           : 0
output (read_committed): ['M0', 'M1', 'M2', 'M3', 'M4', 'M5']
```

- Первая пачка (3 записи) «упала» — транзакция отменена: в `out` ничего не видно, а позиция чтения не сдвинулась.
- Повтор той же пачки сработал заново, затем вторая, затем читать нечего.
- В итоге выход — шесть записей `M0…M5`, **каждая один раз**: отменённый результат не виден, а входная позиция и результат подтверждаются вместе.

Без транзакции при сбое между записью результата и фиксацией позиции результат записан, а позиция нет: после перезапуска запись обработана повторно, и в `out` дубликат.

## Зомби и ограждение (fencing)

Если процесс завис, а оркестратор запустил его замену с тем же `transactional.id`, старый процесс — «зомби» — может очнуться и записать лишнее. Брокер защищает от этого: новый `init_transactions()` **отгораживает** старого производителя.

```python
a = Producer({..., "transactional.id": "same-id"}); a.init_transactions()
b = Producer({..., "transactional.id": "same-id"}); b.init_transactions()   # fences a
a.begin_transaction(); a.produce("fence", value=b"zombie"); a.commit_transaction()
```

```text
old producer fenced: _FENCED fatal: True | txn_requires_abort: False
```

Старый производитель получил фатальную ошибку `_FENCED`: продолжать он не может, процесс надо перезапустить. Поэтому `transactional.id` должен быть **стабильным для логического производителя** (например, привязан к партиции входной темы), но уникальным среди одновременно работающих.

## Порядок вызовов

Нарушение порядка даёт ошибку состояния:

```text
produce without init_transactions -> _STATE KafkaError{code=_STATE,val=-172,str="Unable to produce message: Local: Erroneous state"}
```

Правильный порядок: `init_transactions()` → (`begin_transaction()` → `produce`/`send_offsets_to_transaction` → `commit_transaction()`/`abort_transaction()`)*.

## Границы «ровно один раз»

- Гарантия действует для цепочки **Kafka → обработка → Kafka**. Если обработка пишет во внешнюю систему (БД, письмо, HTTP), атомарности с Kafka нет: нужны идемпотентный приёмник или схема outbox ([рецепт в дорожной карте](../../overview.ru.md)).
- Потребители результата должны читать с `isolation.level=read_committed`, иначе увидят отменённые записи ([основы](basics.ru.md#транзакции-производителя)).
- Транзакции добавляют задержку и нагрузку: подтверждение только после завершения транзакции.

> ⚠️ Проверить: влияние транзакций на задержку/пропускную способность и параметры `transaction.timeout.ms`, `transactional.id.expiration.ms` здесь не измерялись.

## Лучшие практики

- Для конвейеров Kafka→Kafka — транзакции с `send_offsets_to_transaction` и `read_committed` у читателей.
- `transactional.id` — стабильный на каждый логический экземпляр.
- Фатальные ошибки (`_FENCED`) — перезапуск процесса, не повтор.
- Внешние эффекты — идемпотентны или через outbox.
- Короткие транзакции: открытая транзакция задерживает `read_committed`-читателей.

## Частые ошибки

- ❌ Фиксировать позицию потребителя отдельно от транзакции → ✅ `send_offsets_to_transaction`.
- ❌ Один `transactional.id` у разных одновременных экземпляров → ✅ они ограждают друг друга.
- ❌ Читать результат в `read_uncommitted` → ✅ `read_committed`.
- ❌ Ждать «ровно один раз» для письма → ✅ идемпотентность на стороне приёмника.

## Связанные материалы

- Раньше: [основы](basics.ru.md)
- Дальше: [экспертная теория](expert.ru.md)
- [03 · Потребители и группы](../03-consumers-and-groups/advanced.ru.md) — стратегии фиксации
- Следующий модуль: [06 · Схемы и сериализация](../06-schemas-and-serialization/overview.ru.md)

## Источники

- [Apache Kafka: Transactions](https://kafka.apache.org/documentation/#design_transactions)
- [KIP-98: Exactly Once Delivery and Transactional Messaging](https://cwiki.apache.org/confluence/display/KAFKA/KIP-98+-+Exactly+Once+Delivery+and+Transactional+Messaging)
- [confluent-kafka-python: send_offsets_to_transaction](https://docs.confluent.io/platform/current/clients/confluent-kafka-python/html/index.html#confluent_kafka.Producer.send_offsets_to_transaction)
