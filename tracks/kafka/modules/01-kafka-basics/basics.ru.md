---
title: "Основы Kafka: базовый уровень"
lang: ru
track: kafka
kind: module
doc: basics
slug: 01-kafka-basics
part: 1
tags: [kafka, topic, partition, offset, producer, consumer, log, kraft]
versions: "Apache Kafka 4.3.1; Python confluent-kafka 2.16.2 (librdkafka 2.16.0)"
status: draft
updated: 2026-11-24
---

# Основы Kafka: базовый уровень

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [overview](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [трек Python](../../../python/overview.ru.md) (достаточно базового синтаксиса) · **Время:** ~1,5 ч

## Как проверялся код

Весь код запускался на настоящем одиночном брокере Apache Kafka 4.3.1 (образ `apache/kafka:4.3.1`, режим KRaft) в Docker с `confluent-kafka` 2.16.2 для Python; выводы скопированы из запусков. Помощники: `BS = "localhost:9092"`; `admin = AdminClient({"bootstrap.servers": BS})`; `read_all(group, reset="earliest", n=6, topic="orders")` читает до `n` записей и возвращает отсортированный список `(партиция, смещение, ключ, значение)`; `kcli(*cmd)` выполняет команду внутри контейнера (`docker exec kafka …`). Скрипты Kafka лежат в `/opt/kafka/bin/`. Брокер запускался так:

```bash
docker run -d --name kafka -p 9092:9092 apache/kafka:4.3.1
```

> В Git Bash на Windows перед `docker exec … /opt/kafka/bin/…` задайте `MSYS_NO_PATHCONV=1`: иначе путь превращается в `C:/Program Files/Git/…`.

## Журнал вместо очереди

В основе Kafka — **журнал** (log): файл, в который записи только дописываются в конец. Каждая запись получает порядковый номер — **смещение** (offset). Читатель запоминает, до какого смещения дочитал, и продолжает с него.

| | Очередь (например, [RabbitMQ](../../../rabbitmq/overview.ru.md)) | Журнал Kafka |
|---|---|---|
| прочитанное сообщение | удаляется после подтверждения | остаётся, пока не истечёт срок хранения |
| повторное чтение | нужна повторная публикация | читатель просто возвращается к нужному смещению |
| несколько независимых читателей | нужны отдельные очереди | каждый читает тот же журнал со своей позицией |
| позиция чтения | у брокера | у читателя (хранится в Kafka как смещение группы) |

## Тема и партиции

**Тема** (topic) — именованный поток записей. Тема делится на **партиции** (partitions): каждая — отдельный упорядоченный журнал. Партиции позволяют масштабировать запись и чтение, потому что разные партиции обрабатываются параллельно.

```python
fs = admin.create_topics([NewTopic("orders", num_partitions=3, replication_factor=1)])
print("create:", {t: f.result() for t, f in fs.items()})
md = admin.list_topics(timeout=10)
print("partitions of orders:", sorted(md.topics["orders"].partitions))
```

```text
create: {'orders': None}
partitions of orders: [0, 1, 2]
```

`None` в результате `create_topics` означает успех. `replication_factor=1` допустим только потому, что брокер один; про репликацию — [модуль 07](../../overview.ru.md).

## Запись

```python
p = Producer({"bootstrap.servers": BS})
def on_delivery(err, msg):
    reports.append((msg.topic(), msg.partition(), msg.offset(), msg.key(), msg.value()))
for i in range(6):
    p.produce("orders", key=f"user{i % 2}".encode(), value=f"order-{i}".encode(), on_delivery=on_delivery)
p.flush()
```

```text
delivered: ('orders', 1, 0, b'user1', b'order-1')
delivered: ('orders', 1, 1, b'user1', b'order-3')
delivered: ('orders', 1, 2, b'user1', b'order-5')
delivered: ('orders', 2, 0, b'user0', b'order-0')
delivered: ('orders', 2, 1, b'user0', b'order-2')
delivered: ('orders', 2, 2, b'user0', b'order-4')
```

- `produce` **асинхронна**: она ставит запись в буфер клиента; результат приходит в обратном вызове `on_delivery`, который срабатывает внутри `flush()` или `poll()`.
- Отчёт содержит партицию и смещение записи.
- Записи с одним **ключом** попали в одну партицию: `user1` — в партицию 1, `user0` — в партицию 2. Внутри партиции смещения идут 0, 1, 2.

Партиция 0 осталась пустой: с двумя ключами хватило двух партиций. Подробнее о ключах — [модуль 02](../02-producers/overview.ru.md).

## Чтение и смещения

```python
a = read_all("g-1")
b = read_all("g-2")
print("reader A:", a)
print("reader B sees the same:", a == b)
```

```text
reader A: [(1, 0, 'user1', 'order-1'), (1, 1, 'user1', 'order-3'), (1, 2, 'user1', 'order-5'), (2, 0, 'user0', 'order-0'), (2, 1, 'user0', 'order-2'), (2, 2, 'user0', 'order-4')]
reader B sees the same: True
```

Второй читатель (другая группа) получил **те же** записи: чтение не удаляет. В Kafka читатели не «забирают» сообщения, а читают журнал.

Границы журнала партиции — **low** (самое раннее доступное смещение) и **high** (смещение, которое получит следующая запись):

```text
partition 0: low=0 high=0
partition 1: low=0 high=3
partition 2: low=0 high=3
```

## Откуда читать, если у группы нет сохранённой позиции

Параметр `auto.offset.reset` решает, что делать новой группе: `earliest` — с начала журнала, `latest` — только новые записи.

```text
latest, existing data: []
latest, after a new write: [(2, 3, 'new-1')]
```

Группа с `latest` не увидела старые записи, а новую (`new-1`, партиция 2, смещение 3) увидела. Когда группа зафиксировала позицию (commit), повторный запуск начинается с неё; об этом — [модуль 03](../03-consumers-and-groups/overview.ru.md).

## Как это лежит на диске

Каждая партиция — каталог `<тема>-<номер>` в `log.dirs`; в нём файлы сегмента:

```text
00000000000000000000.index
00000000000000000000.log
00000000000000000000.timeindex
leader-epoch-checkpoint
partition.metadata
```

Имя файла — смещение первой записи сегмента (здесь 0). `.log` хранит записи, `.index` и `.timeindex` ускоряют поиск по смещению и времени. Подробности — [экспертная теория](expert.ru.md).

## Типичные ошибки новичков

- ❌ Думать, что прочитанное сообщение исчезает → ✅ журнал хранится по сроку хранения.
- ❌ Ожидать порядок по всей теме → ✅ порядок гарантирован только внутри партиции.
- ❌ Читать с `latest` и удивляться пустому результату → ✅ у новой группы выбрать `earliest`.
- ❌ Не вызывать `flush()` в короткой программе → ✅ записи могут остаться в буфере клиента.
- ❌ Создавать тему с `replication_factor` больше числа брокеров → ✅ он не может превышать число брокеров.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — брокер, KRaft, конфигурация, позиции групп
- Следующий модуль: [02 · Производители](../02-producers/overview.ru.md)
- Аналог в другом треке: [RabbitMQ: основы AMQP](../../../rabbitmq/modules/01-messaging-basics/overview.ru.md)

## Источники

- [Apache Kafka: Introduction](https://kafka.apache.org/intro)
- [Apache Kafka: Design](https://kafka.apache.org/documentation/#design)
- [confluent-kafka-python: API](https://docs.confluent.io/platform/current/clients/confluent-kafka-python/html/index.html)
