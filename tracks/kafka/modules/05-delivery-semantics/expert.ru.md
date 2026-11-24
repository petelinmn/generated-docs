---
title: "Семантика доставки: экспертный уровень"
lang: ru
track: kafka
kind: module
doc: expert
slug: 05-delivery-semantics
part: 2
tags: [kafka, exactly-once, transactions, isolation-level, idempotence, fencing, transactional-id]
versions: "Apache Kafka 4.3.1; Python confluent-kafka 2.16.2 (librdkafka 2.16.0)"
status: draft
updated: 2026-11-24
---

# Семантика доставки: экспертный уровень

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · [2. Продвинутый](advanced.ru.md) · **3. Эксперт** · [4. Квиз](quiz.ru.md)

Опирается на [базовую](basics.ru.md) и [продвинутую](advanced.ru.md) теорию. «Проверено» означает наблюдалось на одном брокере Apache Kafka 4.3.1 с `confluent-kafka` 2.16.2.

## Справочник

### API транзакций производителя

| Метод | Назначение | Проверено |
|---|---|---|
| `Producer({"transactional.id": id, ...})` | создаёт транзакционного производителя | да |
| `init_transactions()` | регистрация `transactional.id`, ограждение предыдущих | да |
| `begin_transaction()` | начало транзакции | да |
| `produce(...)` | запись в транзакции | да |
| `send_offsets_to_transaction(offsets, group_metadata)` | включает смещения потребителя в транзакцию | да |
| `commit_transaction()` / `abort_transaction()` | завершение | да |
| `consumer.consumer_group_metadata()` | метаданные группы для `send_offsets_to_transaction` | да |

### Параметры

| Параметр | Значение | Проверено |
|---|---|---|
| `transactional.id` | стабильное имя производителя | да |
| `isolation.level` читателя | `read_uncommitted` / `read_committed` | да |
| `enable.idempotence` | идемпотентность (включается транзакциями) | идемпотентность — [модуль 02](../02-producers/advanced.ru.md#идемпотентный-производитель) |
| `transaction.timeout.ms`, `transactional.id.expiration.ms`, `max.in.flight.requests.per.connection` | таймауты и порядок | не проверялись |

### Ошибки (проверено)

| Ситуация | Ошибка |
|---|---|
| `produce` без `init_transactions()` | `_STATE` («Local: Erroneous state») |
| производитель отгорожен новым с тем же `transactional.id` | `_FENCED`, `fatal() == True` |

### Что видит читатель (проверено)

| Тип записи | `read_uncommitted` | `read_committed` |
|---|---|---|
| подтверждённая | да | да |
| отменённая (отправленная брокеру) | да | нет |
| открытая | да | нет |
| метки транзакций | не отдаются приложению, но занимают смещения | то же |

## Внутреннее устройство

- **Метки (control records)** в журнале партиции отмечают завершение транзакции; приложению они не отдаются, но занимают смещения (проверено: `high` = 8 при пяти записях).
- **Читатель `read_committed`** видит только записи завершённых транзакций (проверено: открытая невидима).
- **Ограждение:** новый `init_transactions()` с тем же `transactional.id` повышает «эпоху» и делает прежнего производителя недействительным (`_FENCED`).
- **Атомарность чтения-записи** достигается тем, что смещения потребителя пишутся в службу `__consumer_offsets` в той же транзакции.
- **`abort_transaction()` отбрасывает ещё не отправленные записи** (проверено: без `flush()` до отмены записи до брокера не дошли).

> ⚠️ Проверить: координатор транзакций и его журнал `__transaction_state`, механизм LSO (last stable offset) и таймаут зависших транзакций описаны по документации и здесь не проверялись.

## Подвохи

### 1. Идемпотентность — не транзакции

Она защищает от повторов отправки одним производителем в одну партицию.

### 2. `read_uncommitted` по умолчанию

Читатель без `isolation.level=read_committed` увидит отменённые записи (проверено).

### 3. Метки занимают смещения

Смещения в транзакционной теме идут с пропусками (проверено).

### 4. `abort` без `flush`

Не отправленные записи отбрасываются; отменённая транзакция тогда «пустая» (проверено).

### 5. `_FENCED` фатальна

Повторять нельзя: перезапуск процесса (проверено).

### 6. Внешние эффекты

«Ровно один раз» не распространяется на БД и письма.

### 7. Долгая открытая транзакция

Задерживает `read_committed`-читателей (по документации).

## Компромиссы и альтернативы

| Выбор | Плюсы | Минусы |
|---|---|---|
| at-least-once + идемпотентный обработчик | просто, без транзакций | дубликаты нужно терпеть |
| транзакции (exactly-once) | атомарность Kafka→Kafka | сложнее, задержка, только внутри Kafka |
| at-most-once | без дублей | потери |
| outbox | согласованность с БД | дополнительная таблица и процесс |

## Вопросы с собеседований

<details><summary>1. Чем at-least-once отличается от exactly-once?</summary>

При at-least-once запись может прийти повторно; exactly-once добавляет транзакции, чтобы эффект был один.

→ [Основы](basics.ru.md#три-семантики-доставки)

</details>

<details><summary>2. Что даёт идемпотентный производитель?</summary>

Защиту от дубликатов повторов отправки внутри партиции.

→ [Основы](basics.ru.md#идемпотентный-производитель--первая-ступень)

</details>

<details><summary>3. Что такое `read_committed`?</summary>

Режим читателя, скрывающий записи отменённых и открытых транзакций.

→ [Основы](basics.ru.md#транзакции-производителя)

</details>

<details><summary>4. Подвох: почему в транзакционной теме смещения идут с пропусками?</summary>

Метки завершения транзакций занимают смещения.

→ [Подвох 3](#3-метки-занимают-смещения)

</details>

<details><summary>5. Как атомарно записать результат и зафиксировать позицию чтения?</summary>

`send_offsets_to_transaction` в той же транзакции.

→ [Продвинутая теория](advanced.ru.md#чтение--обработка--запись-ровно-один-раз)

</details>

<details><summary>6. Что такое «зомби» и как от него защищаются?</summary>

Старый зависший процесс; новый `init_transactions()` с тем же `transactional.id` ограждает его (`_FENCED`).

→ [Продвинутая теория](advanced.ru.md#зомби-и-ограждение-fencing)

</details>

<details><summary>7. Подвох: охватывает ли «ровно один раз» отправку письма?</summary>

Нет: только Kafka→Kafka.

→ [Подвох 6](#6-внешние-эффекты)

</details>

<details><summary>8. Что произойдёт при `produce` без `init_transactions()`?</summary>

Ошибка `_STATE`.

→ [Продвинутая теория](advanced.ru.md#порядок-вызовов)

</details>

<details><summary>9. Подвох: что значит `_FENCED` с `fatal=True`?</summary>

Производитель недействителен; перезапустить процесс, не повторять.

→ [Подвох 5](#5-_fenced-фатальна)

</details>

<details><summary>10. Почему `abort_transaction()` без `flush()` не оставил отменённых записей?</summary>

Он отбрасывает не отправленные записи.

→ [Подвох 4](#4-abort-без-flush)

</details>

<details><summary>11. Когда лучше at-least-once с идемпотентным обработчиком?</summary>

Когда нужна простота или результат уходит во внешнюю систему.

→ [Компромиссы](#компромиссы-и-альтернативы)

</details>

<details><summary>12. Зачем стабильный `transactional.id`?</summary>

Чтобы перезапуск мог отгородить прежний экземпляр и продолжить с последнего состояния.

→ [Продвинутая теория](advanced.ru.md#зомби-и-ограждение-fencing)

</details>

## Шпаргалка

```text
Producer({"transactional.id": stable_id}) ; init_transactions() ; begin_transaction() ; produce() ; send_offsets_to_transaction(offs, c.consumer_group_metadata()) ; commit_transaction() | abort_transaction()
reader: isolation.level=read_committed (default is read_uncommitted)
errors: _STATE (wrong order), _FENCED (fatal: restart) ; markers take offsets ; EOS = Kafka -> Kafka only
```

- Транзакции: атомарность записей и позиции.
- `read_committed` у читателей результата.
- Внешние эффекты — идемпотентность или outbox.

## Связанные материалы

- Предыдущий уровень: [продвинутая теория](advanced.ru.md)
- Раньше: [04 · Темы, хранение и компактация](../04-topics-and-retention/expert.ru.md)
- Следующий модуль: [06 · Схемы и сериализация](../../overview.ru.md)

## Источники

- [Apache Kafka: Message delivery semantics](https://kafka.apache.org/documentation/#semantics)
- [Apache Kafka: Transactions](https://kafka.apache.org/documentation/#design_transactions)
- [KIP-98](https://cwiki.apache.org/confluence/display/KAFKA/KIP-98+-+Exactly+Once+Delivery+and+Transactional+Messaging)
