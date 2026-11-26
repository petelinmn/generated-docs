---
title: "Разбор задач: продвинутая теория"
lang: ru
track: system-design
kind: module
doc: advanced
slug: 17-design-case-studies
part: 3
tags: [system-design, case-studies, url-shortener, news-feed, notifications, id-generation]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Разбор задач: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Генерация идентификаторов

| Способ | Плюс | Минус |
|---|---|---|
| Счётчик базы | просто | центральная точка, коды предсказуемы |
| Диапазоны, выданные экземплярам | мало обращений | дыры, предсказуемость |
| Время + узел + номер | без координации | зависит от часов и уникальности узла |
| Случайная строка | непредсказуемо | проверка уникальности |

```csharp
// Refusing is safer than issuing an id that may already exist.
if (ms < _lastMs) throw new InvalidOperationException("The clock moved backwards.");
```

Коллекция битов: 41 на миллисекунды, 10 на узел, 12 на номер. Защита от скачка часов назад действует только в пределах жизни процесса.

## Лента новостей

**Fan-out при записи:** пост кладут в ящик каждого подписчика — чтение быстрое. **При чтении:** ленту собирают из постов авторов — запись дешёвая. Гибрид:

```csharp
if (followers.Count >= popularThreshold)
{
    Bucket(_popular, author).Add(postId);
    return;
}
```

У автора с миллионами подписчиков копировать пост дорого, поэтому его посты подмешивают при чтении. Файл: [Ex02_NewsFeed.cs](code/Examples/Ex02_NewsFeed.cs).

## Сервис уведомлений

```csharp
if (_accepted.Contains(n.Id)) return SendResult.Duplicate;
if (_sentToUser.GetValueOrDefault(n.UserId) >= maxPerUser) return SendResult.RateLimited;
_accepted.Add(n.Id);
```

Затем попытки отправки и dead-letter. Файл: [Ex03_Notifications.cs](code/Examples/Ex03_Notifications.cs). Это модель порядка проверок: в системе нужны очередь, долговечное хранилище, задержки между повторами и лимит по окну времени.

## Лучшие практики

- Называйте допущения вслух.
- Покажите простое решение, затем место, где оно перестанет работать.
- Для каждого компонента опишите поведение при отказе.
- Метрики назовите вместе с решением.

## Частые ошибки

- ❌ Одна стратегия ленты для всех авторов → ✅ гибрид.
- ❌ Повторять отправку без идемпотентности у провайдера → ✅ ключ идемпотентности, иначе дубли.
- ❌ Забыть удаление и приватность → ✅ учесть сразу.
- ❌ Горячий ключ у популярной ссылки → ✅ кэш на нескольких уровнях.

## Сравнение с другими треками

- Ограничение частоты в ASP.NET Core — [рецепт](../../../csharp/recipes/rate-limiting/overview.ru.md).
- Обработка очереди — [фоновые задачи](../../../csharp/recipes/background-jobs/overview.ru.md).

## Связанные материалы

- [Базовая теория](basics.ru.md)
- [Экспертная теория](expert.ru.md)

## Источники

- [RFC 9110: коды перенаправления](https://www.rfc-editor.org/rfc/rfc9110)
