---
title: "gRPC-сервис: production"
lang: ru
track: csharp
kind: recipe
doc: advanced
slug: grpc-service
part: 2
tags: [grpc, deadline, rpc-exception, metadata, status-codes]
versions: ".NET 11, C# 15; Grpc.AspNetCore 2.85.0, Grpc.Net.Client 2.85.0"
status: draft
updated: 2026-11-22
---

# gRPC-сервис: production

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Минимум](basics.ru.md) · **2. Production** · [3. Масштаб и грани](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [минимальное решение](basics.ru.md). **Время:** ~45 мин.

## Ошибки и статусы

Ошибки gRPC — это `RpcException` со статусом (`StatusCode`), а не HTTP-коды:

```csharp
// server
if (string.IsNullOrWhiteSpace(request.Name))
    throw new RpcException(new Status(StatusCode.InvalidArgument, "name is required"));

// client
try { await client.SayHelloAsync(new HelloRequest { Name = "" }); }
catch (RpcException e) { /* e.StatusCode, e.Status.Detail */ }
```

Проверено: клиент получил `InvalidArgument / name is required`. Недоступный сервер даёт `Unavailable` (проверено), превышение дедлайна — `DeadlineExceeded`.

| Статус | Когда |
|---|---|
| `InvalidArgument` | некорректный ввод |
| `NotFound` | нет сущности |
| `PermissionDenied` / `Unauthenticated` | права / нет аутентификации |
| `DeadlineExceeded` | вышло время |
| `Unavailable` | сервис недоступен (можно ретраить) |

## Дедлайны и отмена

Каждый вызов должен иметь дедлайн; сервер получает отмену через `context.CancellationToken`:

```csharp
await client.SayHelloAsync(req, deadline: DateTime.UtcNow.AddMilliseconds(200));
// server: await Task.Delay(2000, context.CancellationToken);
```

Проверено: медленный вызов (2 с на сервере) при дедлайне 200 мс завершился `DeadlineExceeded`. Дедлайн передаётся серверу и распространяется по цепочке вызовов, если пробрасывать токен дальше.

## Метаданные

Метаданные — аналог HTTP-заголовков (трассировка, токены):

```csharp
var md = new Metadata { { "x-trace", "abc" } };
await client.SayHelloAsync(req, headers: md);
// server: context.RequestHeaders.GetValue("x-trace")
```

Проверено: сервер увидел `x-trace = abc`. Токен аутентификации передают в метаданных `authorization` (см. [JWT-аутентификация](../jwt-auth/overview.ru.md)).

> ⚠️ Проверить: настройку JWT-аутентификации для gRPC (`[Authorize]` на сервисе, `CallCredentials` на клиенте) в этом материале не запускали.

## Стримы

Серверные, клиентские и двунаправленные стримы проверены ([минимум](basics.ru.md#четыре-вида-вызовов)). Для чтения — `await foreach (var x in stream.ReadAllAsync())`, для завершения клиентского стрима — `RequestStream.CompleteAsync()`; без него сервер ждёт новых сообщений бесконечно.

## Перехватчики

Перехватчики (`Interceptor`) — middleware для gRPC: логирование, метрики, аутентификация, преобразование исключений:

```csharp
builder.Services.AddGrpc(o => o.Interceptors.Add<LoggingInterceptor>());
```

> ⚠️ Проверить: API перехватчиков (`Interceptor`, методы `UnaryServerHandler` и др.) в этом материале не запускалось — сверьтесь с документацией.

## Эволюция контракта

| Изменение | Совместимо? |
|---|---|
| добавить новое поле с новым номером | да |
| удалить поле | да, но номер/имя зарезервируйте (`reserved`) |
| изменить номер или тип поля | нет |
| переименовать поле | совместимо на проводе, ломает JSON и код |

## Best practices

| Правило | Почему |
|---|---|
| Дедлайн на каждый вызов | нет «вечных» вызовов |
| Один долгоживущий канал | дорого создавать |
| `RpcException` с правильным `StatusCode` | клиент может реагировать |
| Не менять номера полей | совместимость |
| Метаданные для сквозных данных | трассировка, auth |
| Ретраи только `Unavailable` и идемпотентного | как в [HttpClient](../http-client-resilience/advanced.ru.md#идемпотентность-и-ретраи) |

## Связанные материалы

- Назад: [минимум](basics.ru.md). Дальше: [масштаб и грани](expert.ru.md).
- [Устойчивый HttpClient](../http-client-resilience/overview.ru.md) — ретраи и дедлайны.
- [22 · Наблюдаемость](../../modules/22-observability/overview.ru.md) — трассировка вызовов.
- [JWT-аутентификация](../jwt-auth/overview.ru.md) — токены.

## Источники

- [gRPC error handling](https://learn.microsoft.com/aspnet/core/grpc/error-handling)
- [Deadlines and cancellation](https://learn.microsoft.com/aspnet/core/grpc/deadlines-cancellation)
- [gRPC interceptors on .NET](https://learn.microsoft.com/aspnet/core/grpc/interceptors)
