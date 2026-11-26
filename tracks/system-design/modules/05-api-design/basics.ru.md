---
title: "Проектирование API: базовая теория"
lang: ru
track: system-design
kind: module
doc: basics
slug: 05-api-design
part: 1
tags: [system-design, api-design, rest, grpc, idempotency, optimistic-concurrency, keyset-pagination, problem-details]
versions: ".NET 11, C# 15"
status: verified
updated: 2026-11-26
---

# Проектирование API: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** HTTP, Minimal API · **Время:** ~2 ч

## Зачем это нужно

Клиентов нельзя обновить одновременно с сервером. Всё, что вы опубликовали, придётся поддерживать, а всё, что не продумали (повторы, большие списки), всплывёт под нагрузкой.

## Стили

| | REST поверх HTTP | gRPC |
|---|---|---|
| Контракт | OpenAPI (по желанию) | `.proto` (обязателен) |
| Формат | обычно JSON | Protocol Buffers |
| Потоки | ограниченно | клиентские, серверные, двунаправленные |
| Из браузера | напрямую | нужен gRPC-Web или шлюз |
| Обычно | публичные API | вызовы между сервисами |

## Ресурсы, методы и коды

Ресурсы называют существительными (`/orders/1`), действие задаёт метод. По RFC 9110 безопасны GET, HEAD, OPTIONS и TRACE; идемпотентны они, а также PUT и DELETE. POST и PATCH такими не объявлены.

| Код | Когда |
|---|---|
| 201 | создан ресурс, `Location` указывает на него |
| 202 | принято, но ещё не выполнено |
| 400 | некорректный запрос |
| 404 | нет такого ресурса |
| 409 | конфликт с текущим состоянием |
| 412 / 428 | условие не выполнено / условие обязательно |
| 429 | слишком много запросов |

## Идемпотентность

Клиент отправил POST, ответ потерялся. Повторить — значит рискнуть вторым заказом. Решение: клиент присылает уникальный ключ в `Idempotency-Key`, сервер запоминает результат и при повторе возвращает его.

```csharp
IdempotencyOutcome.KeyReused => Results.Problem("The key was used with another payload.", statusCode: 422),
IdempotencyOutcome.InProgress => Results.Problem("The original request is still running.", statusCode: 409),
_ => Results.Created($"/orders/{order!.Id}", order),
```

Файл: [Ex03_OrdersApi.cs](code/Examples/Ex03_OrdersApi.cs). Коды соответствуют проекту стандарта IETF для этого заголовка; на ноябрь 2026 года это черновик, а не RFC.

## Ошибки

`Results.Problem` отдаёт `application/problem+json` по RFC 9457: поля `type`, `title`, `status`, `detail`, `instance`. Клиент разбирает ошибки одинаково, а не угадывает по тексту.

## Ошибки новичка

- ❌ `200` с `{"error": ...}` → ✅ подходящий код и problem details.
- ❌ GET, меняющий данные → ✅ GET только читает.
- ❌ Список без ограничения → ✅ пагинация с максимальным размером.
- ❌ Отдавать сущности базы как есть → ✅ отдельные DTO.

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [ASP.NET Core Minimal API](../../../csharp/modules/14-aspnet-core-minimal-api/overview.ru.md)

## Источники

- [RFC 9110: HTTP Semantics](https://www.rfc-editor.org/rfc/rfc9110)
- [RFC 9457: Problem Details](https://www.rfc-editor.org/rfc/rfc9457)
- [Проект IETF: Idempotency-Key](https://datatracker.ietf.org/doc/draft-ietf-httpapi-idempotency-key-header/)
