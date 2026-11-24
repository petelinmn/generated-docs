---
title: "Потребители и группы"
lang: ru
track: kafka
kind: module
doc: overview
slug: 03-consumers-and-groups
part: 1
levels: [1, 2, 3, 4]
tags: [kafka, consumer, consumer-group, rebalance, offset-commit, at-least-once]
versions: "Apache Kafka 4.3.1; Python confluent-kafka 2.16.2 (librdkafka 2.16.0)"
status: draft
updated: 2026-11-24
---

# Потребители и группы

🌐 **Русский** · [English](overview.en.md)

## Коротко

Потребители объединяются в **группы** с общим `group.id`. Партиции темы делятся между членами группы: каждая — ровно одному, поэтому группа читает тему параллельно, а число активных членов не больше числа партиций. Группа **фиксирует** (commit) позицию в Kafka; после перезапуска чтение продолжается с зафиксированного смещения.

## Ключевые идеи

- **Одна партиция — один член группы;** лишние потребители простаивают.
- **Ребалансировка** перераспределяет партиции при смене состава; eager отзывает все партиции, `cooperative-sticky` и новый протокол (`group.protocol=consumer`) — только переезжающие.
- **Зафиксированное смещение — следующее к чтению** (`offset + 1`); `-1001` — позиции нет.
- **Нет фиксации — повтор** (минимум один раз): фиксируйте после обработки, обработчик идемпотентен.
- **`max.poll.interval.ms`:** слишком долгая пауза между `poll` даёт `_MAX_POLL_EXCEEDED`.
- **Без группы:** `assign` и `seek` задают позицию вручную.

## Минимальный пример

```python
c = Consumer({"bootstrap.servers": "localhost:9092", "group.id": "g", "auto.offset.reset": "earliest", "enable.auto.commit": False})
c.subscribe(["orders"])
m = c.poll(1.0)
if m and not m.error():
    handle(m); c.commit(message=m)
```

Всё запускалось на настоящем Apache Kafka 4.3.1 в Docker (см. [базовую теорию](basics.ru.md)).

## Когда применять и когда нет

| Задача | Решение |
|---|---|
| Масштабировать обработку | группа, число партиций с запасом |
| Независимые обработчики одних данных | разные группы |
| Не терять записи при сбое | ручная фиксация после обработки |
| Перечитать данные | `seek`/`assign` или новая группа с `earliest` |

- ❌ Не обрабатывайте дольше `max.poll.interval.ms` без выхода в `poll`.
- ❌ Не фиксируйте до обработки.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | группа, деление партиций, фиксация, повтор, чтение с позиции |
| [2. Продвинутая теория](advanced.ru.md) | ребалансировка, стратегии, новый протокол, `max.poll.interval.ms`, фиксация |
| [3. Экспертная теория](expert.ru.md) | справочник параметров и API, подвохи, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| 5. Запускаемые примеры | пока не создан — `/learn kafka 03-consumers-and-groups level 5` |
| 6. Тесты | пока не созданы — `/learn kafka 03-consumers-and-groups level 6` |

## Связанные материалы

- Пререквизиты: [01 · Основы Kafka](../01-kafka-basics/overview.ru.md), [02 · Производители](../02-producers/overview.ru.md)
- Дальше: [04 · Темы, хранение и компактация](../04-topics-and-retention/overview.ru.md)
- Аналоги в других треках: [RabbitMQ: очереди и доставка](../../../rabbitmq/modules/03-queues-and-delivery/overview.ru.md)
