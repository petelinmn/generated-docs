---
title: "Проектирование API"
lang: ru
track: system-design
kind: module
doc: overview
slug: 05-api-design
part: 1
levels: [1, 2, 3, 4, 5, 6]
tags: [system-design, api-design, rest, grpc, idempotency, optimistic-concurrency, keyset-pagination, problem-details]
versions: ".NET 11, C# 15"
status: verified
updated: 2026-11-26
---

# Проектирование API

🌐 **Русский** · [English](overview.en.md)

## Коротко

API — это договор, который трудно изменить после того, как им начали пользоваться. Хороший договор переживает повторы запросов, одновременные изменения, большие списки и новые версии.

## Ключевые идеи

- Стиль (REST, gRPC) выбирают по потребителям.
- Сеть теряет ответы, поэтому клиенты повторяют запросы.
- Idempotency key делает повтор POST безопасным.
- `ETag` и `If-Match` защищают от потерянных обновлений.
- Курсорная пагинация устойчива к вставкам, offset — нет.
- Ошибки должны быть машиночитаемыми.
- Совместимые изменения добавляют, а не переименовывают.

## Минимальный пример

```csharp
if (string.IsNullOrEmpty(key))
    return Results.Problem("Idempotency-Key header is required.", statusCode: 400);
```

## Когда применять, а когда нет

- ✅ Любой интерфейс между независимо выпускаемыми частями.
- ✅ Операции с деньгами, заказами, сообщениями.
- ❌ Вызовы внутри одного процесса: достаточно интерфейса C#.
- ❌ Долгая операция в синхронном ответе: возвращайте `202` и статус.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | стили, ресурсы, методы и коды, идемпотентность |
| [2. Продвинутая теория](advanced.ru.md) | idempotency store, ETag, пагинация, версионирование |
| [3. Экспертная теория](expert.ru.md) | справочник, подвохи сериализации, вопросы |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| [5. Запускаемые примеры](code/Examples/) | idempotency, пагинация, Minimal API |
| [6. Тесты](code/Tests/) | `dotnet test` в папке `code/` |

## Связанные материалы

- Пререквизиты: [процесс проектирования](../01-design-process/overview.ru.md), [ASP.NET Core Minimal API](../../../csharp/modules/14-aspnet-core-minimal-api/overview.ru.md)
- Дальше: [репликация и партиционирование](../06-replication-and-partitioning/overview.ru.md)
- В .NET: [пагинация и фильтрация](../../../csharp/recipes/pagination-filtering/overview.ru.md), [gRPC-сервис](../../../csharp/recipes/grpc-service/overview.ru.md)
- [Сообщения и события](../08-messaging-and-events/overview.ru.md)
- [Паттерны надёжности](../09-reliability-patterns/overview.ru.md)
- [Безопасность архитектуры](../15-security-architecture/overview.ru.md)
- [Формы на клиенте в React](../../../react/modules/05-forms/overview.ru.md)
