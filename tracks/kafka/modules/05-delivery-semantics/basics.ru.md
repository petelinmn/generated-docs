---
title: "Семантика доставки: базовый уровень"
lang: ru
track: kafka
kind: module
doc: basics
slug: 05-delivery-semantics
part: 2
tags: [kafka, delivery-semantics, exactly-once, transactions, isolation-level, at-least-once]
versions: "Apache Kafka 4.3.1; Python confluent-kafka 2.16.2 (librdkafka 2.16.0)"
status: draft
updated: 2026-11-24
---

# Семантика доставки: базовый уровень

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [overview](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [02 · Производители](../02-producers/basics.ru.md), [03 · Потребители и группы](../03-consumers-and-groups/basics.ru.md) · **Время:** ~1,5 ч

## Как проверялся код

Код запускался на Apache Kafka 4.3.1 (один брокер, KRaft, `apache/kafka:4.3.1`) в Docker с `confluent-kafka` 2.16.2; выводы скопированы из запусков. Помощники: `fresh(topic)` пересоздаёт тему с одной партицией; `read(topic, isolation, secs=8)` читает тему с начала новой группой с `isolation.level` и возвращает `(смещение, значение)`; `hw(topic)` возвращает `(low, high)`. Читателю нужно несколько секунд на вступление в группу, поэтому окно чтения не короче 5–8 секунд.

## Три семантики доставки

Что гарантирует система при сбоях между отправкой, обработкой и фиксацией позиции:

| Семантика | Смысл | Как получить |
|---|---|---|
| не более одного раза (at-most-once) | запись не дублируется, но может потеряться | зафиксировать позицию **до** обработки |
| минимум один раз (at-least-once) | запись не теряется, но может прийти повторно | зафиксировать позицию **после** обработки |
| ровно один раз (exactly-once) | эффект один, без потерь и дублей | идемпотентный производитель + транзакции |

Минимум один раз вы уже наблюдали в [модуле 03](../03-consumers-and-groups/basics.ru.md#что-будет-без-фиксации): без фиксации записи пришли снова. «Ровно один раз» в Kafka возможна, но **в пределах Kafka**: чтение из Kafka, обработка и запись обратно в Kafka как одна атомарная операция. Внешние действия (письмо, запрос в базу) в неё не входят.

## Идемпотентный производитель — первая ступень

`enable.idempotence=true` защищает от дубликатов при повторах отправки внутри одной партиции ([модуль 02](../02-producers/advanced.ru.md#идемпотентный-производитель)). Но она не делает атомарной запись в **несколько** партиций или тем и не связывает запись с фиксацией позиции потребителя. Для этого нужны транзакции.

## Транзакции производителя

Транзакция группирует записи в несколько партиций/тем: все видны потребителям или ни одна.

```python
tp = Producer({"bootstrap.servers": BS, "transactional.id": "tx-1"})
tp.init_transactions()
tp.begin_transaction(); tp.produce("tx2", value=b"committed-0"); tp.commit_transaction()
tp.begin_transaction(); tp.produce("tx2", value=b"aborted-0");   tp.flush(); tp.abort_transaction()
```

- `transactional.id` — стабильное имя производителя; по нему брокер связывает перезапуски одного логического производителя.
- `init_transactions()` вызывают один раз после создания.
- Дальше циклы `begin_transaction()` → `produce(...)` → `commit_transaction()` или `abort_transaction()`.

```text
watermarks: (0, 8)
read_committed  : [(0, 'committed-0'), (1, 'committed-1'), (6, 'committed-2')]
read_uncommitted: [(0, 'committed-0'), (1, 'committed-1'), (3, 'aborted-0'), (4, 'aborted-1'), (6, 'committed-2')]
```

(Опыт: два сообщения в первой транзакции, два в отменённой, одно в третьей.)

- **`isolation.level=read_committed`** показывает только записи подтверждённых транзакций; отменённые (`aborted-*`) скрыты.
- **`read_uncommitted`** (режим читателя по умолчанию) показывает и отменённые.
- Транзакция занимает смещения: на каждый `commit`/`abort` брокер пишет в партицию служебную **метку** (marker). Здесь записи первой транзакции — смещения 0 и 1, её метка — 2, отменённые записи — 3 и 4, метка отмены — 5, `committed-2` — 6, её метка — 7. Поэтому `high` равен 8: 5 записей и 3 метки.

> `abort_transaction()` **отбрасывает записи, ещё не отправленные брокеру**. В первой версии опыта без `flush()` перед `abort_transaction()` отменённые записи до брокера не дошли: `read_uncommitted` их не показал, а `high` был 5 (нет меток отмены). Для демонстрации отменённых записей нужен `flush()`; в реальном коде он перед отменой не требуется.

## Открытая транзакция

Пока транзакция не завершена, `read_committed` не видит её записей:

```text
open txn, read_committed  : ['committed-1', 'committed-2']
open txn, read_uncommitted: ['committed-2', 'in-flight']
after commit, read_committed: ['committed-2', 'in-flight']
```

Читатель в `read_committed` не пропускает записи дальше **самой старой открытой транзакции**, поэтому долгая транзакция задерживает всех таких читателей (по документации; наблюдалась лишь невидимость).

## Типичные ошибки новичков

- ❌ Считать, что идемпотентность даёт «ровно один раз» везде → ✅ она защищает только повторы одного производителя в партиции.
- ❌ Читать транзакционную тему в `read_uncommitted` и видеть отменённое → ✅ `isolation.level=read_committed`.
- ❌ Ожидать, что «ровно один раз» охватит письмо или запись в БД → ✅ только Kafka→Kafka.
- ❌ Удивляться пропускам в смещениях → ✅ метки транзакций занимают смещения.
- ❌ Не задать стабильный `transactional.id` → ✅ перезапуск не сможет отгородиться от «зомби».

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — чтение-обработка-запись, «зомби»
- Раньше: [03 · Потребители и группы](../03-consumers-and-groups/basics.ru.md), [02 · Производители](../02-producers/basics.ru.md)
- Следующий модуль: [06 · Схемы и сериализация](../06-schemas-and-serialization/overview.ru.md)
- Аналог в другом треке: [RabbitMQ: надёжность и идемпотентность](../../../rabbitmq/modules/05-reliability/advanced.ru.md)

## Источники

- [Apache Kafka: Message delivery semantics](https://kafka.apache.org/documentation/#semantics)
- [Apache Kafka: Transactions](https://kafka.apache.org/documentation/#design_transactions)
- [confluent-kafka-python: Producer transactions](https://docs.confluent.io/platform/current/clients/confluent-kafka-python/html/index.html#confluent_kafka.Producer.init_transactions)
