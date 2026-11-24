---
title: "Производители: продвинутый уровень"
lang: ru
track: kafka
kind: module
doc: advanced
slug: 02-producers
part: 1
tags: [kafka, producer, idempotence, batching, linger-ms, compression, message-timeout]
versions: "Apache Kafka 4.3.1; Python confluent-kafka 2.16.2 (librdkafka 2.16.0)"
status: draft
updated: 2026-11-24
---

# Производители: продвинутый уровень

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

Опирается на [базовую теорию](basics.ru.md). Код запускался на Apache Kafka 4.3.1 (один брокер) с `confluent-kafka` 2.16.2.

## Идемпотентный производитель

При сбое сети клиент может **повторить** отправку, и запись окажется в партиции дважды. Идемпотентный производитель (`enable.idempotence=true`) нумерует записи, чтобы брокер отбрасывал дубликаты повторов внутри одной партиции.

```python
p = Producer({"bootstrap.servers": BS, "enable.idempotence": True})
r = send(p, "ak", [b"idem"]); print("idempotent producer ok:", r[0][0] is None)
```

```text
idempotent producer ok: True
```

Идемпотентность требует `acks=all`; несовместимую пару клиент отвергает при создании:

```text
idempotence + acks=1 -> KafkaException KafkaError{code=_INVALID_ARG,val=-186,str="Failed to create producer: `acks` must be set to `all` when `enable.idempoten…
```

> ⚠️ Проверить: значение `enable.idempotence` по умолчанию в `confluent-kafka` 2.16.2 и защита от дубликатов при настоящих повторах (разрыв связи между записью и подтверждением) здесь не проверялись — на одном стенде повторов не было. Явно включайте идемпотентность для важных записей.

## Пакеты и задержка: `linger.ms`

Клиент собирает записи в пакеты. `linger.ms` — сколько он ждёт, чтобы пакет наполнился, прежде чем отправить:

```text
linger.ms=0: delivery latency of one message ms: [1, 1, 0, 0, 1]
linger.ms=100: delivery latency of one message ms: [108, 113, 106, 115, 115]
```

При `linger.ms=0` одиночная запись доходит за миллисекунду, при `100` — примерно за 100 мс: клиент ждал, не придут ли ещё записи. Большая задержка повышает долю полных пакетов (и экономит запросы), малая — снижает задержку доставки. Что выгоднее, зависит от нагрузки; мерьте.

> ⚠️ Проверить: влияние `linger.ms` и `batch.size` на пропускную способность. В нашем опыте с 50 000 записей `linger.ms=0` дал 275 тыс. записей/с против 145 тыс. при 50 мс: вывод «задержка замедляет» нельзя обобщать, потому что опыт был коротким и на одной машине.

## Сжатие

`compression.type` сжимает пакеты на клиенте; брокер хранит их сжатыми. 20 000 повторяющихся записей по 1850 байт (37 МБ) в одну партицию:

```text
none    payload 1850B x 20000 = 37 MB raw -> on disk 37.2 MB (0.61s)
gzip    payload 1850B x 20000 = 37 MB raw -> on disk 0.2 MB (0.16s)
snappy  payload 1850B x 20000 = 37 MB raw -> on disk 1.9 MB (0.05s)
lz4     payload 1850B x 20000 = 37 MB raw -> on disk 0.3 MB (0.04s)
zstd    payload 1850B x 20000 = 37 MB raw -> on disk 0.1 MB (0.09s)
```

Размер — сумма `.log`-файлов партиции. Данные здесь **крайне повторяющиеся**, поэтому степень сжатия нереалистична: реальные данные сжимаются в разы, а не в сотни раз. Вывод: сжатие экономит диск и сеть почти даром; сравнивайте кодеки на своих данных.

## Таймауты и повторы

Запись, не доставленная за `message.timeout.ms`, завершается ошибкой `_MSG_TIMED_OUT` ([основы](basics.ru.md#ошибки)). Клиент сам повторяет отправку при временных ошибках в пределах этого срока.

> ⚠️ Проверить: взаимодействие `retries`, `retry.backoff.ms`, `delivery.timeout.ms` и порядка записей при повторах без идемпотентности здесь не проверялось.

## Лучшие практики

- Для важных данных: `acks=all` и `enable.idempotence=true`.
- Всегда проверяйте `err` в обратном вызове и вызывайте `flush()` при остановке.
- Ключ — по бизнес-сущности, порядок которой нужен (клиент, заказ).
- `compression.type=lz4` или `zstd` как разумный выбор; проверить на своих данных.
- `linger.ms` — по требованиям к задержке; для интерактивных потоков — малое значение.

## Частые ошибки

- ❌ `acks=1` с идемпотентностью → ✅ клиент отвергнет: нужен `all`.
- ❌ Огромный `linger.ms` в интерактивной системе → ✅ рост задержки.
- ❌ Оценивать сжатие на повторяющихся тестовых данных → ✅ мерить на реальных.
- ❌ Игнорировать `_MSG_TIMED_OUT` → ✅ повторять/сигнализировать.

## Связанные материалы

- Раньше: [основы](basics.ru.md)
- Дальше: [экспертная теория](expert.ru.md)
- [05 · Семантика доставки](../05-delivery-semantics/overview.ru.md) — идемпотентность и транзакции
- Следующий модуль: [03 · Потребители и группы](../03-consumers-and-groups/overview.ru.md)

## Источники

- [Apache Kafka: Producer configs](https://kafka.apache.org/documentation/#producerconfigs)
- [Apache Kafka: Idempotent producer](https://kafka.apache.org/documentation/#producerconfigs_enable.idempotence)
- [librdkafka: Configuration](https://github.com/confluentinc/librdkafka/blob/master/CONFIGURATION.md)
