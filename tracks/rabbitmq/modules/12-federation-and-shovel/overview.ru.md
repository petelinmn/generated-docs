---
title: "Federation и Shovel"
lang: ru
track: rabbitmq
kind: module
doc: overview
slug: 12-federation-and-shovel
part: 3
levels: [1, 2, 3, 4]
tags: [rabbitmq, shovel, federation, upstream, multi-datacenter]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Federation и Shovel

🌐 **Русский** · [English](overview.en.md)

## Коротко

Когда брокеры находятся в разных дата-центрах или принадлежат разным системам, их не объединяют в кластер, а связывают плагинами. **Shovel** перекладывает (перемещает) сообщения из источника на одном брокере в приёмник на другом. **Federation** распространяет сообщения, опубликованные в обменнике вышестоящего брокера, на нижестоящий.

## Ключевые идеи

- **Брокеры остаются независимыми:** у каждого свои пользователи, vhost-ы и очереди.
- **Shovel перемещает:** из источника сообщения удаляются; порядок сохраняется (проверено).
- **Динамический shovel — параметр:** `rabbitmqctl set_parameter shovel …`; состояние — `shovel_status`.
- **Отказ источника:** shovel `terminated` («failed to connect to source»), после возвращения источника восстановился.
- **Federation:** параметр `federation-upstream` и политика с `federation-upstream-set`; состояние — `federation_status`.
- **Federation по умолчанию однонаправленная** (от вышестоящего к нижестоящему).
- **Для связи — отдельный пользователь и TLS;** `guest` лишь в учебной среде.
- **Дубликаты возможны** — приёмник идемпотентен.

## Минимальный пример

```text
rabbitmqctl set_parameter shovel s1 '{"src-protocol":"amqp091","src-uri":"amqp://U:P@rmqa","src-queue":"q.src",
  "dest-protocol":"amqp091","dest-uri":"amqp://U:P@rmqb","dest-queue":"q.dst"}'
```

Всё запускалось на двух независимых брокерах RabbitMQ 4.4.1 в Docker (см. [базовую теорию](basics.ru.md)).

## Когда применять и когда нет

| Нужно | Решение |
|---|---|
| Перенести сообщения из очереди в очередь на другом брокере | Shovel |
| Общий обменник между брокерами | Federation |
| Один дата-центр, единая система | кластер ([модуль 08](../08-clustering/overview.ru.md)) |

- ❌ Не растягивайте кластер между дата-центрами.
- ❌ Не используйте `guest` для связи в рабочей системе.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | зачем связывать брокеры, shovel, federation, выбор |
| [2. Продвинутая теория](advanced.ru.md) | сбой источника, управление, состояние федерации |
| [3. Экспертная теория](expert.ru.md) | справочник параметров, подвохи, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| 5. Запускаемые примеры | пока не создан — `/learn rabbitmq 12-federation-and-shovel level 5` |
| 6. Тесты | пока не созданы — `/learn rabbitmq 12-federation-and-shovel level 6` |

## Связанные материалы

- Пререквизиты: [08 · Кластеризация](../08-clustering/overview.ru.md), [11 · Безопасность](../11-security/overview.ru.md)
- Дальше: [13 · Внутреннее устройство](../../overview.ru.md)
- Аналоги в других треках: [Kubernetes](../../../kubernetes/overview.ru.md)
