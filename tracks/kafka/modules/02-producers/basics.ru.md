---
title: "Производители: базовый уровень"
lang: ru
track: kafka
kind: module
doc: basics
slug: 02-producers
part: 1
tags: [kafka, producer, partitioning, message-key, acks, delivery-report]
versions: "Apache Kafka 4.3.1; Python confluent-kafka 2.16.2 (librdkafka 2.16.0)"
status: draft
updated: 2026-11-24
---

# Производители: базовый уровень

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [overview](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [01 · Основы Kafka](../01-kafka-basics/basics.ru.md) · **Время:** ~1,5 ч

## Как проверялся код

Код запускался на настоящем Apache Kafka 4.3.1 (`apache/kafka:4.3.1`, один брокер, KRaft) в Docker с `confluent-kafka` 2.16.2; выводы скопированы из запусков. Помощники: `fresh(topic, parts=3)` удаляет и создаёт тему; `send(p, topic, items, **kw)` публикует записи (элементы — значения или пары «ключ, значение»), вызывает `flush` и возвращает список `(ошибка, партиция, смещение)` из обратных вызовов доставки; `BS = "localhost:9092"`. Скорость и размеры зависят от машины и данных: смотрите соотношения.

## Что делает производитель

Производитель (producer) — клиент, который пишет записи в темы. Для каждой записи он выбирает **партицию**, собирает записи в **пакеты** (batches), отправляет их брокеру-лидеру партиции и сообщает результат в обратном вызове.

```python
p = Producer({"bootstrap.servers": BS})
p.produce("orders", key=b"user1", value=b"order-1", on_delivery=cb)
p.flush()
```

`produce` лишь ставит запись в очередь клиента; отправку выполняет фоновый поток, а `flush()` дожидается её и запускает обратные вызовы ([модуль 01](../01-kafka-basics/basics.ru.md#запись)).

## Как выбирается партиция

| Условие | Партиция |
|---|---|
| задан `partition=` | указанная |
| есть ключ | определяется хешем ключа: один ключ — одна партиция |
| ключа нет | распределяется клиентом между партициями |

### Явная партиция

```python
r = send(p, "ex", [(b"same-key", b"v")] * 3, partition=2)
print("explicit partition=2:", [x[1] for x in r])
```

```text
explicit partition=2: [2, 2, 2]
```

### Записи без ключа

```text
keyless 300 in one batch run: {1: 108, 2: 100, 0: 92}
keyless 30 flushed one by one: {2: 12, 0: 18}
```

Без ключа записи расходятся по партициям: 300 записей одним потоком — по всем трём, 30 записей, отправленных по одной с `flush()` после каждой, — по двум из трёх (в этом запуске). Распределение без ключа **не равномерное и не предсказуемое**: на него не полагаются. Нужен порядок записей — задавайте ключ.

## Обратный вызов доставки

Результат каждой записи приходит в `on_delivery(err, msg)`: при успехе `err is None`, а `msg` содержит партицию и смещение; при ошибке — `err` описывает причину. Обратные вызовы выполняются внутри `flush()` или `poll()`.

## Подтверждения брокера: `acks`

`acks` определяет, когда брокер считает запись принятой:

| Значение | Смысл |
|---|---|
| `0` | не ждать подтверждения |
| `1` | ждать записи лидером |
| `all` | ждать записи лидером и всеми синхронными репликами |

20 000 записей по 100 байт на одном брокере:

```text
{'acks': '0'} 20000 msgs: 0.15 s; errors: 0 ; offsets reported: {None}
{'acks': '1'} 20000 msgs: 0.16 s; errors: 0 ; offsets reported: {0, 1, 2}
{'acks': 'all'} 20000 msgs: 0.13 s; errors: 0 ; offsets reported: {0, 1, 2}
```

- При `acks=0` смещение в отчёте **неизвестно** (`None`): брокер ничего не ответил.
- На одном брокере разницы в скорости нет: у `all` нет других реплик, которых надо ждать. В кластере `all` медленнее, но надёжнее ([модуль 07](../../overview.ru.md)).

## Ошибки

Слишком большая запись отвергается сразу, при `produce`:

```text
2 MB: KafkaError{code=MSG_SIZE_TOO_LARGE,val=10,str="Unable to produce message: Broker: Message size too large"}
```

Если брокер недоступен, запись не будет потеряна молча: по истечении `message.timeout.ms` обратный вызов получит ошибку:

```text
unreachable broker: [('_MSG_TIMED_OUT', 'Local: Message timed out')]
```

(опыт с `message.timeout.ms=3000` и несуществующим адресом `localhost:1`). Всегда проверяйте `err` в обратном вызове.

## Типичные ошибки новичков

- ❌ Не вызывать `flush()` перед выходом → ✅ записи останутся в буфере.
- ❌ Не проверять `err` в обратном вызове → ✅ потери остаются незамеченными.
- ❌ Ждать равномерного распределения без ключа → ✅ задавайте ключ или партицию.
- ❌ Использовать `acks=0` для важных данных → ✅ `all` (см. продвинутую теорию).
- ❌ Публиковать записи крупнее лимита → ✅ ограничение размера записи (около 1 МБ) или хранение вне Kafka.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — идемпотентность, пакеты, сжатие
- Раньше: [01 · Основы Kafka](../01-kafka-basics/basics.ru.md)
- Следующий модуль: [03 · Потребители и группы](../03-consumers-and-groups/overview.ru.md)

## Источники

- [Apache Kafka: Producer configs](https://kafka.apache.org/documentation/#producerconfigs)
- [Apache Kafka: Design: the producer](https://kafka.apache.org/documentation/#design_producer)
- [librdkafka: Configuration](https://github.com/confluentinc/librdkafka/blob/master/CONFIGURATION.md)
