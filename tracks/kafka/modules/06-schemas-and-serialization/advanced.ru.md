---
title: "Схемы и сериализация: продвинутый уровень"
lang: ru
track: kafka
kind: module
doc: advanced
slug: 06-schemas-and-serialization
part: 2
tags: [kafka, schema-registry, schema-evolution, compatibility, backward-compatibility, avro]
versions: "Apache Kafka 4.3.1; Confluent Schema Registry 8.4.1; Python confluent-kafka 2.16.2, fastavro 1.13.1"
status: draft
updated: 2026-11-24
---

# Схемы и сериализация: продвинутый уровень

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

Опирается на [базовую теорию](basics.ru.md) (тот же стенд). Схема `V1` — `Order(id, customer, amount)`; `V2_OK` добавляет поле `currency` со значением по умолчанию `"USD"`; `V2_BAD` добавляет `currency` **без** значения по умолчанию.

## Эволюция схем и совместимость

Схемы со временем меняются, а старые записи остаются в теме, поэтому нужны правила, при каких изменениях старые и новые читатели и писатели не ломаются. Реестр проверяет **совместимость** при регистрации новой версии. Уровень по умолчанию:

```text
global compatibility: BACKWARD
```

**BACKWARD** означает: читатель с **новой** схемой может прочитать данные, записанные по **предыдущей**. Допустимые изменения: добавить поле **со значением по умолчанию**, удалить поле.

```python
sr.test_compatibility("orders-av-value", Schema(V2_OK, "AVRO"))
sr.test_compatibility("orders-av-value", Schema(V2_BAD, "AVRO"))
```

```text
v2 with default field compatible: True
v2 without default compatible   : False
register incompatible -> 409 40901 Schema being registered is incompatible with an earlier schema for subject "orders-av-valu…
registered v2 id: 2 | versions: [1, 2]
```

- Поле с умолчанием проходит проверку, без умолчания — нет: новый читатель не знает, что подставить для старых записей.
- Попытка зарегистрировать несовместимую схему заканчивается ответом `409` (код `40901`).
- Совместимая версия получила идентификатор 2 и стала версией 2 субъекта.

## Что видят читатели с разными схемами

Запись записана по `V2_OK` (с `currency="EUR"`), а `V1` — старая схема:

```text
reader with writer's schema sees: {'id': 1001, 'customer': 'alice', 'amount': 19.99, 'currency': 'EUR'}
reader pinned to V1 sees       : {'id': 1001, 'customer': 'alice', 'amount': 19.99}
reader pinned to V2 reads old  : {'id': 1001, 'customer': 'alice', 'amount': 19.99, 'currency': 'USD'}
```

- По умолчанию десериализатор использует схему **писателя** (взятую из реестра по номеру в записи).
- Читатель, зафиксированный на `V1`, прочитал новую запись, **проигнорировав** новое поле.
- Читатель на `V2` прочитал **старую** запись (написанную по `V1`) и подставил `currency` = `USD` по умолчанию — именно это и означает «обратная совместимость».

## Режимы совместимости

| Режим | Что гарантирует |
|---|---|
| `BACKWARD` (по умолчанию) | новый читатель читает старые данные |
| `FORWARD` | старый читатель читает новые данные |
| `FULL` | и то и другое |
| `NONE` | проверка отключена |
| режимы `*_TRANSITIVE` | проверка против всех прошлых версий, а не только предыдущей |

> ⚠️ Проверить: в опыте проверялись только `BACKWARD` и `NONE`; поведение `FORWARD`, `FULL` и `*_TRANSITIVE` описано по документации.

Режим можно задать для субъекта отдельно от глобального:

```python
sr.set_compatibility("orders-av-value", "NONE")
```

```text
after NONE, bad v3 compatible: True
global stays: BACKWARD
```

После `NONE` у субъекта несовместимая схема проходит проверку, а глобальный режим не изменился. Так легко «отключить защиту» для одной темы: делайте это осознанно.

## Автоматическая регистрация схем

По умолчанию сериализатор **сам регистрирует** схему (`auto.register.schemas`). В рабочих системах это отключают, чтобы схемы попадали в реестр только через проверенный процесс (CI):

```python
AvroSerializer(sr, schema, conf={"auto.register.schemas": False})
```

```text
auto.register=False, unknown schema -> SchemaRegistryError Subject 'other-topic-value' not found. (HTTP status code 404, SR code 40401)
```

Если схемы нет в реестре, сериализация падает с ошибкой — запись с неизвестным форматом не уйдёт в тему.

## Лучшие практики

- Выбирайте режим совместимости осознанно (по умолчанию `BACKWARD`) и проверяйте новые схемы в CI (`test_compatibility`) до развёртывания.
- Новые поля — со значениями по умолчанию; не переименовывайте и не меняйте тип поля, а добавляйте новое.
- В рабочей системе отключите автоматическую регистрацию.
- Не отключайте проверку совместимости (`NONE`) без необходимости.
- Порядок развёртывания при `BACKWARD`: сначала читатели (с новой схемой), затем писатели.

## Частые ошибки

- ❌ Добавить обязательное поле без умолчания → ✅ `409` при регистрации.
- ❌ Переименовать поле → ✅ новое поле и постепенный отказ от старого.
- ❌ Оставить `auto.register.schemas=true` в рабочей системе → ✅ регистрация через CI.
- ❌ Сменить режим на `NONE` «временно» → ✅ несовместимые данные останутся в теме.

## Связанные материалы

- Раньше: [основы](basics.ru.md)
- Дальше: [экспертная теория](expert.ru.md)
- [04 · Темы, хранение и компактация](../04-topics-and-retention/advanced.ru.md) — старые записи остаются в теме
- Следующий модуль: [07 · Репликация и доступность](../07-replication-and-availability/overview.ru.md)

## Источники

- [Confluent Schema Registry: schema evolution and compatibility](https://docs.confluent.io/platform/current/schema-registry/fundamentals/schema-evolution.html)
- [Apache Avro: schema resolution](https://avro.apache.org/docs/current/specification/#schema-resolution)
- [confluent-kafka-python: schema registry](https://docs.confluent.io/platform/current/clients/confluent-kafka-python/html/index.html#schemaregistry-client)
