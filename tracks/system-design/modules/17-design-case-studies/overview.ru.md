---
title: "Разбор задач"
lang: ru
track: system-design
kind: module
doc: overview
slug: 17-design-case-studies
part: 3
levels: [1, 2, 3, 4, 5, 6]
tags: [system-design, case-studies, url-shortener, news-feed, notifications, id-generation]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Разбор задач

🌐 **Русский** · [English](overview.en.md)

## Коротко

Три классические задачи в формате интервью: сокращатель ссылок, лента новостей и сервис уведомлений. Здесь всё из предыдущих модулей собирается в один ход рассуждения.

## Ключевые идеи

- Начинают с требований и чисел, а не с технологий.
- Решение объясняют через компромиссы.
- Короткий код — это число в другом основании или случайная строка с проверкой.
- Лента: fan-out при записи против сборки при чтении.
- Уведомления: дедупликация, лимиты, повторы, dead-letter.
- Узкие места и отказы обсуждают явно.
- Простой вариант идёт первым.

## Минимальный пример

```csharp
Console.WriteLine(Base62.Encode(62)); // "10"
```

## Когда применять, а когда нет

- ✅ Подготовка к интервью и разбор своего проекта.
- ✅ Обсуждение дизайна в команде по одной структуре.
- ❌ Копировать чужое решение без своих требований.
- ❌ Заучивать схемы вместо причин.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | план разбора, сокращатель ссылок |
| [2. Продвинутая теория](advanced.ru.md) | идентификаторы, лента, уведомления |
| [3. Экспертная теория](expert.ru.md) | масштаб, подвохи, вопросы |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| [5. Запускаемые примеры](code/Examples/) | base62, генератор id, лента, уведомления |
| [6. Тесты](code/Tests/) | `dotnet test` в папке `code/` |

## Связанные материалы

- Пререквизиты: [процесс проектирования](../01-design-process/overview.ru.md), [кэширование](../03-caching/overview.ru.md), [партиционирование](../06-replication-and-partitioning/overview.ru.md), [сообщения](../08-messaging-and-events/overview.ru.md), [надёжность](../09-reliability-patterns/overview.ru.md)
- Соседний модуль: [несколько регионов](../16-multi-region-and-dr/overview.ru.md)
- В других треках: [rate limiting](../../../csharp/recipes/rate-limiting/overview.ru.md), [фоновые задачи](../../../csharp/recipes/background-jobs/overview.ru.md)
- [Рецепт: сокращатель ссылок](../../recipes/url-shortener/overview.ru.md)
