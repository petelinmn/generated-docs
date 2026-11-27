---
title: "Сокращатель ссылок"
lang: ru
track: system-design
kind: recipe
doc: overview
slug: url-shortener
part: 2
levels: [1, 2, 3, 4, 5, 6]
tags: [url-shortener, caching, redirect, rate-limiting, minimal-api]
versions: ".NET 11, C# 15; Microsoft.Extensions.Caching.Hybrid 11.0.1; Microsoft.Data.Sqlite 11.0.0"
status: draft
updated: 2026-11-27
---

# Сокращатель ссылок

🌐 **Русский** · [English](overview.en.md)

## Коротко

Сокращатель ссылок хранит пару «короткий код → длинный адрес» и перенаправляет. Рецепт строит его на ASP.NET Core по шагам: версия в памяти, затем проверка адресов, база данных, кэш, счётчик переходов и ограничение на создание. Проектные рассуждения и оценки нагрузки — в [17 · Разбор проектных задач](../../modules/17-design-case-studies/overview.ru.md); здесь — реализация.

## Ключевые идеи

- Случайные коды, а коллизии разрешает первичный ключ базы.
- Принимать только `http` и `https`, без учётных данных и не ведущие обратно.
- Чтений гораздо больше, поэтому их кэшируют, включая «не найдено».
- Срок жизни проверяют при каждом запросе, даже для ссылки из кэша.
- Переходы считают вне пути запроса.
- При нескольких экземплярах локальные кэши и лимиты — у каждого свои.

## Минимальный пример

```csharp
app.MapPost("/links", (CreateLink request) =>
{
    string code = ShortCode.Random();
    while (!links.TryAdd(code, request.Url))
        code = ShortCode.Random();
    return Results.Created($"/{code}", new { code });
});

app.MapGet("/{code}", (string code) =>
    links.TryGetValue(code, out string? url) ? Results.Redirect(url) : Results.NotFound());
```

## Когда применять и когда нет

- ✅ Нужны свой домен, свои правила, аналитика или закрытые ссылки.
- ❌ Несколько ссылок для презентации: хватит готового сервиса.
- ❌ Публичный сервис без модерации: сокращатели притягивают фишинг.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | минимальный рабочий сервис |
| [2. Продвинутая теория](advanced.ru.md) | production-ready: проверка, хранение, кэш, переходы, лимиты |
| [3. Экспертная теория](expert.ru.md) | несколько экземпляров, масштаб, подвохи, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| [5. Запускаемые примеры](code/Examples/Ex06_Service.cs) | папка `code/` |
| [6. Тесты](code/Tests/ExamplesTests.cs) | `dotnet test` в `code/` |

## Связанные материалы

- Проектирование: [17 · Разбор проектных задач](../../modules/17-design-case-studies/overview.ru.md)
- [03 · Кэширование](../../modules/03-caching/overview.ru.md), [09 · Паттерны надёжности](../../modules/09-reliability-patterns/overview.ru.md)
- В C#: [кэширование](../../../csharp/recipes/caching/overview.ru.md), [ограничение частоты запросов](../../../csharp/recipes/rate-limiting/overview.ru.md), [фоновые задачи](../../../csharp/recipes/background-jobs/overview.ru.md)
