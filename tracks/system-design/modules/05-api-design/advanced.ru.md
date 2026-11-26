---
title: "Проектирование API: продвинутая теория"
lang: ru
track: system-design
kind: module
doc: advanced
slug: 05-api-design
part: 1
tags: [system-design, api-design, rest, grpc, idempotency, optimistic-concurrency, keyset-pagination, problem-details]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Проектирование API: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Хранилище ключей идемпотентности

Захват ключа должен быть атомарным: «проверить, потом записать» пропустит два одновременных запроса.

```csharp
// TryAdd is the atomic claim: only one caller gets to run the action.
while (!_entries.TryAdd(key, pending))
{
    if (!_entries.TryGetValue(key, out Entry? existing)) continue;
    if (existing.Fingerprint != fingerprint) return (IdempotencyOutcome.KeyReused, default);
    return existing.Done
        ? (IdempotencyOutcome.Replayed, existing.Result)
        : (IdempotencyOutcome.InProgress, default);
}
```

Файл: [Ex01_Idempotency.cs](code/Examples/Ex01_Idempotency.cs). Это учебный вариант. В production запись хранят в базе с уникальным ограничением, пишут в одной транзакции с эффектом, ограничивают срок и привязывают ключ к клиенту.

## Оптимистичная конкурентность

Двое прочитали заказ версии 1 и оба изменили. Без защиты второе обновление молча затрёт первое. Сервер отдаёт `ETag`, клиент присылает его в `If-Match`.

```csharp
// TryUpdate makes check-and-write one atomic step.
if (ifMatch != ETag(current) || !orders.TryUpdate(id, updated, current))
    return Results.Problem("The order has changed.", statusCode: 412);
```

Без `If-Match` пример отвечает 428 (RFC 6585).

## Пагинация

Offset пропускает N строк. Если между страницами вставили запись, строки сдвигаются и повторяются: первая страница `4 3`, после вставки вторая `3 2`. Курсор (keyset) запоминает последнюю строку и берёт следующие после неё.

```csharp
// One extra row tells whether there is a next page without a COUNT.
List<Post> page = [.. posts.Where(p => p.Id < before).OrderByDescending(p => p.Id).Take(size + 1)];
```

Сортировка должна быть однозначной: при равных значениях добавляют ключ. Курсор делают непрозрачным и проверяют на входе. Файл: [Ex02_Pagination.cs](code/Examples/Ex02_Pagination.cs).

## Версионирование и совместимость

| Изменение в protobuf | Совместимо? |
|---|---|
| новое поле, новый метод, новый сервис | да |
| новое значение enum | да, если старые клиенты его переживут |
| удаление поля | ломает код клиента при обновлении контракта; номер и имя резервируют |
| изменение номера поля или типа на несовместимый | нет |
| переименование метода, сервиса, пакета | нет |
| переименование поля | для protobuf не ломает, для JSON ломает |

Для JSON действует то же правило: добавлять необязательное можно, переименовывать и менять смысл нельзя. Версию (`/v2`, заголовок) вводят, когда совместимо сделать не получается.

## Лучшие практики

- Задавайте максимальный размер страницы и тела запроса.
- Долгие операции: `202` и ресурс статуса.
- Публикуйте контракт и проверяйте совместимость в CI.
- Документируйте, какие ошибки можно повторять.

## Частые ошибки

- ❌ Новый idempotency key на каждую повторную попытку → ✅ один ключ на одно намерение.
- ❌ Новое обязательное поле запроса → ✅ необязательное со значением по умолчанию.
- ❌ Клиент разбирает курсор → ✅ курсор непрозрачен.
- ❌ Повторы без паузы → ✅ экспоненциальная задержка и `Retry-After`.

## Сравнение с другими треками

- Повторы на стороне клиента — [устойчивый HttpClient](../../../csharp/recipes/http-client-resilience/overview.ru.md).
- Keyset в EF Core — [пагинация и фильтрация](../../../csharp/recipes/pagination-filtering/overview.ru.md).
- Контракты `.proto` — [gRPC-сервис](../../../csharp/recipes/grpc-service/overview.ru.md).

## Связанные материалы

- [Базовая теория](basics.ru.md)
- [Экспертная теория](expert.ru.md)

## Источники

- [ASP.NET Core: версионирование gRPC](https://learn.microsoft.com/aspnet/core/grpc/versioning)
- [RFC 6585: дополнительные коды](https://www.rfc-editor.org/rfc/rfc6585)
