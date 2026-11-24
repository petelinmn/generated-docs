---
title: "Потребители и группы: базовый уровень"
lang: ru
track: kafka
kind: module
doc: basics
slug: 03-consumers-and-groups
part: 1
tags: [kafka, consumer, consumer-group, offset-commit, rebalance, at-least-once]
versions: "Apache Kafka 4.3.1; Python confluent-kafka 2.16.2 (librdkafka 2.16.0)"
status: draft
updated: 2026-11-24
---

# Потребители и группы: базовый уровень

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [overview](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [01 · Основы Kafka](../01-kafka-basics/basics.ru.md), [02 · Производители](../02-producers/basics.ru.md) · **Время:** ~1,5 ч

## Как проверялся код

Код запускался на Apache Kafka 4.3.1 (один брокер, KRaft, образ `apache/kafka:4.3.1`) в Docker с `confluent-kafka` 2.16.2; выводы скопированы из запусков. Помощники: `fresh(topic, parts=3)` пересоздаёт тему; `fill(topic, n)` записывает `n` записей поровну в три партиции; `mk(group, **kw)` создаёт `Consumer` с `auto.offset.reset=earliest` и **выключенной** автофиксацией (`enable.auto.commit=False`); `pump(consumers, secs)` в течение `secs` секунд вызывает `poll` у каждого; `asg(c)` возвращает отсортированные номера назначенных партиций. Группы создавались со случайными именами (`grp-xxxxx`).

## Группа потребителей

**Группа потребителей** (consumer group) — набор процессов с одним `group.id`, которые совместно читают тему. Правило: **каждая партиция назначается ровно одному члену группы**, поэтому внутри группы запись обрабатывается одним потребителем, а партиции делятся между членами.

```python
a = mk(g); a.subscribe(["g1"]); pump([a], 6)
print("one member:", asg(a))
b = mk(g); b.subscribe(["g1"]); pump([a, b], 10)
print("two members:", asg(a), asg(b))
```

```text
one member: [0, 1, 2]
two members: [0, 1] [2]
```

Один член получил все три партиции, после подключения второго партиции разделились. Это и есть **масштабирование чтения**: добавили потребителя — нагрузка разделилась.

### Потребителей больше, чем партиций

```python
c3 = mk(g); c3.subscribe(["g1"]); d = mk(g); d.subscribe(["g1"]); pump([a, b, c3, d], 12)
print("four members on 3 partitions:", [asg(x) for x in (a, b, c3, d)])
```

```text
four members on 3 partitions: [[1], [2], [0], []]
```

Четвёртый член остался **без партиций**: партиция не делится между потребителями группы, поэтому число активных читателей не больше числа партиций. Лишний потребитель — резерв на случай отказа соседа.

### Выход члена группы

Когда два члена закрылись, оставшиеся вновь поделили партиции:

```text
after two leave: [0, 1] [2]
```

Такое перераспределение партиций называется **ребалансировкой** (rebalance); подробности — [продвинутая теория](advanced.ru.md#ребалансировка).

## Фиксация смещений (commit)

Чтобы после перезапуска продолжить с места остановки, группа **фиксирует** (commit) позицию. Пока фиксации не было, сохранённой позиции нет:

```python
print("committed before commit:", [(tp.partition, tp.offset) for tp in c.committed([...])])
c.commit(asynchronous=False)
print("committed after commit :", [(tp.partition, tp.offset) for tp in c.committed([...])])
```

```text
read: 9
committed before commit: [(0, -1001), (1, -1001), (2, -1001)]
committed after commit : [(0, 3), (1, 3), (2, 3)]
```

`-1001` — «позиция не задана» (`OFFSET_INVALID`). После `commit` у каждой партиции зафиксировано смещение 3: прочитано по три записи (0, 1, 2), а зафиксирован номер **следующей** записи к чтению.

### Зафиксированное смещение — следующее к чтению

```text
read offset 0 partition 2 -> committed [(2, 1)]
```

Запись со смещением 0 обработана — зафиксировано смещение 1. Если фиксировать вручную по сообщению, фиксируйте `offset + 1`; метод `commit(message=m)` делает это сам.

## Что будет без фиксации

Потребитель прочитал шесть записей, закрылся **без** `commit` (автофиксация выключена) и был запущен снова в той же группе:

```text
processed without commit, restarted -> redelivered: 6 6 True
```

Те же шесть записей прочитаны повторно. Так работает доставка «минимум один раз»: если обработали, но не успели зафиксировать, после сбоя запись придёт снова ([модуль 05](../05-delivery-semantics/overview.ru.md)). Фиксируйте **после** обработки и делайте обработчик идемпотентным.

## Чтение с заданной позиции

Можно не использовать группу и позицию брокера, а назначить партицию и смещение самому:

```python
c.assign([TopicPartition("g6", 0, 2)])     # start at offset 2 of partition 0
c.seek(TopicPartition("g6", 0, 0))          # go back to offset 0
```

```text
assign at offset 2: [(2, 'm6')]
after seek(0): 0 m0
```

В партиции 0 лежали записи `m0`, `m3`, `m6` со смещениями 0, 1, 2, поэтому смещение 2 — запись `m6`, а после `seek(0)` читается `m0`. Так перечитывают данные или пропускают проблемный участок.

## Типичные ошибки новичков

- ❌ Ждать, что два потребителя одной группы получат одну запись → ✅ внутри группы запись получает один.
- ❌ Больше потребителей, чем партиций, ради скорости → ✅ лишние простаивают.
- ❌ Фиксировать позицию до обработки → ✅ после; иначе запись теряется при сбое.
- ❌ Забыть, что зафиксированное смещение — следующее к чтению → ✅ `offset + 1`.
- ❌ Разные приложения с одним `group.id` → ✅ они начнут делить партиции.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — ребалансировка, `max.poll.interval.ms`, стратегии
- Раньше: [02 · Производители](../02-producers/basics.ru.md)
- Следующий модуль: [04 · Темы, хранение и компактация](../04-topics-and-retention/overview.ru.md)
- Аналог в другом треке: [RabbitMQ: подтверждения и prefetch](../../../rabbitmq/modules/03-queues-and-delivery/basics.ru.md)

## Источники

- [Apache Kafka: Consumer configs](https://kafka.apache.org/documentation/#consumerconfigs)
- [Apache Kafka: Design: the consumer](https://kafka.apache.org/documentation/#design_consumer)
- [confluent-kafka-python: Consumer](https://docs.confluent.io/platform/current/clients/confluent-kafka-python/html/index.html#pythonclient-consumer)
