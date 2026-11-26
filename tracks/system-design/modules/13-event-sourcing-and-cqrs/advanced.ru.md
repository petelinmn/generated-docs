---
title: "Event sourcing и CQRS: продвинутая теория"
lang: ru
track: system-design
kind: module
doc: advanced
slug: 13-event-sourcing-and-cqrs
part: 3
tags: [system-design, event-sourcing, cqrs, events, projection, snapshot, upcasting]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Event sourcing и CQRS: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Проекции

```csharp
if (stored.Position <= Checkpoint) return; // redelivery
```

Проекция читает события по порядку и помнит позицию. Повторы пропускаются, а удалённую модель можно построить заново. Позицию и модель обновляют в одной транзакции. Файл: [Ex02_Projection.cs](code/Examples/Ex02_Projection.cs).

## Снимки

Снимок — сохранённое состояние на некоторой версии: загружают его и применяют только более поздние события. Это оптимизация; поток остаётся источником истины, и снимки можно пересоздать.

## Версии событий

| Приём | Идея |
|---|---|
| Терпимое чтение | игнорировать лишнее, задавать значения по умолчанию |
| Версия в конверте или имени типа | различать формы |
| Upcasting | преобразовать при чтении, не меняя хранимое |
| Миграция хранилища | крайняя мера |

```csharp
// Each step converts one version to the next, so steps can be chained.
if (version == 1) payload["currency"] = "USD"; // v1 was recorded when only USD existed
```

Файл: [Ex03_Upcasting.cs](code/Examples/Ex03_Upcasting.cs).

## Персональные данные

Неизменяемый журнал плохо сочетается с удалением данных. Варианты: хранить персональные данные отдельно и ссылаться по идентификатору, или шифровать ключом субъекта и удалять ключ (crypto-shredding).

## Лучшие практики

- Проектируйте события как часть контракта.
- Потребителей делайте идемпотентными: доставка может повторяться.
- Измеряйте отставание проекций.
- Применяйте паттерн к части системы, а не ко всей.

## Частые ошибки

- ❌ Побочные эффекты при перестроении проекции (повторные письма) → ✅ отделить проекции от реакций.
- ❌ Публиковать внутренние события наружу как есть → ✅ отдельные интеграционные события.
- ❌ Один огромный поток → ✅ поток на агрегат.
- ❌ Инвариант между потоками «в проекции» → ✅ пересмотреть границы агрегата.

## Сравнение с другими треками

- Хранение и перечитывание журнала — [Kafka](../../../kafka/modules/04-topics-and-retention/overview.ru.md).
- CQRS и vertical slices — [C#](../../../csharp/modules/21-architecture/overview.ru.md).

## Связанные материалы

- [Базовая теория](basics.ru.md)
- [Экспертная теория](expert.ru.md)

## Источники

- [Паттерн Event Sourcing](https://learn.microsoft.com/azure/architecture/patterns/event-sourcing)
