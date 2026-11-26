---
title: "Event sourcing и CQRS: базовая теория"
lang: ru
track: system-design
kind: module
doc: basics
slug: 13-event-sourcing-and-cqrs
part: 3
tags: [system-design, event-sourcing, cqrs, events, projection, snapshot, upcasting]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Event sourcing и CQRS: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** сообщения, records в C# · **Время:** ~2 ч

## Зачем это нужно

Обычная таблица хранит баланс 70, но не говорит, как он получился. Поток событий хранит «внесли 100», «сняли 30», и из него можно получить любое прошлое состояние.

## Агрегат

```csharp
// Apply states facts: it never rejects an event that already happened.
public Account Apply(AccountEvent e) => e switch
{
    Deposited d => new(Balance + d.Amount, Version + 1),
    Withdrawn w => new(Balance - w.Amount, Version + 1),
    _ => throw new InvalidOperationException($"Unknown event {e.GetType().Name}"),
};
```

Правила проверяются при принятии решения (команда), а не при применении события. Событие называют в прошедшем времени.

## Event store и версия

```csharp
public bool TryAppend(string stream, int expectedVersion, params AccountEvent[] events)
{
    if (Read(stream).Count != expectedVersion) return false; // someone else wrote first
```

Это оптимистичная конкурентность: двое прочитали версию 1, второй получает отказ и должен перечитать. Файл: [Ex01_EventStore.cs](code/Examples/Ex01_EventStore.cs). Пример хранит всё в памяти и не потокобезопасен.

## CQRS

Команды изменяют состояние через агрегат, запросы читают отдельную модель, удобную для экрана. CQRS возможен и без event sourcing, даже в одной базе.

## Ошибки новичка

- ❌ Событие `AccountUpdated` со всем состоянием → ✅ событие с намерением.
- ❌ Проверка правил в `Apply` → ✅ при решении.
- ❌ Исправлять или удалять старые события → ✅ компенсирующее событие.
- ❌ Ждать, что модель чтения обновится мгновенно → ✅ учитывать задержку.

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [CQRS внутри приложения](../../../csharp/modules/21-architecture/overview.ru.md)

## Источники

- [Паттерн Event Sourcing](https://learn.microsoft.com/azure/architecture/patterns/event-sourcing)
- [Паттерн CQRS](https://learn.microsoft.com/azure/architecture/patterns/cqrs)
