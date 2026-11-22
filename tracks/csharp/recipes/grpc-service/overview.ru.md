---
title: "gRPC-сервис"
lang: ru
track: csharp
kind: recipe
doc: overview
slug: grpc-service
part: 2
levels: [1, 2, 3, 4]
tags: [grpc, protobuf, rpc, http2, streaming, deadline]
versions: ".NET 11, C# 15; Grpc.AspNetCore 2.85.0, Grpc.Net.Client 2.85.0"
status: draft
updated: 2026-11-22
---

# gRPC-сервис

🌐 **Русский** · [English](overview.en.md)

## Коротко

Рецепт: сервис и клиент на gRPC в ASP.NET Core. Контракт описывается в `.proto`, из него генерируются серверная база и клиент; обмен бинарный поверх HTTP/2. Есть четыре вида вызовов (унарный и три стриминговых), ошибки — через `RpcException` и статусы, у каждого вызова должен быть дедлайн. Все виды вызовов, ошибки, дедлайны и метаданные проверены запуском; gRPC-Web, балансировка и перехватчики — нет.

## Ключевые идеи

- `.proto` — контракт; номера полей — часть контракта.
- `AddGrpc()` + `MapGrpcService<T>()`; клиент — `GrpcChannel` + сгенерированный клиент.
- Унарный, серверный стрим, клиентский стрим, двунаправленный.
- Ошибки — `RpcException` + `StatusCode`; дедлайны и `CancellationToken`.
- Метаданные — аналог заголовков.
- Канал долгоживущий; балансировка L7/клиентская.
- Браузеру нужен gRPC-Web или REST-фасад.

## Минимальный пример

```csharp
builder.Services.AddGrpc();
app.MapGrpcService<GreeterService>();

class GreeterService : Greeter.GreeterBase
{
    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        => Task.FromResult(new HelloReply { Message = "Hello " + request.Name });
}
```

## Когда применять, а когда нет

- ✅ Взаимодействие микросервисов, внутренние API.
- ✅ Стриминг данных, низкая задержка, строгий контракт.
- ✅ Полиглот-среды (кодогенерация для многих языков).
- ❌ Публичный API для браузеров «из коробки» (нужен gRPC-Web).
- ❌ Простые CRUD-API, где достаточно REST/JSON.
- ❌ Среды без HTTP/2.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Минимум](basics.ru.md) | `.proto`, сервер, клиент, четыре вида вызовов |
| [2. Production](advanced.ru.md) | статусы, дедлайны, метаданные, стримы, эволюция контракта |
| [3. Масштаб и грани](expert.ru.md) | балансировка, gRPC-Web, подвохи, 10 вопросов с собеседований |
| [4. Самопроверка](quiz.ru.md) | 10 вопросов и [4 упражнения](exercises.ru.md) с решениями |
| 5. Запускаемые примеры | не сгенерирован — `/learn csharp grpc-service level 5` |
| 6. Тесты | не сгенерирован — `/learn csharp grpc-service level 6` |

## Связанные материалы

- Пререквизиты: [10 · async/await](../../modules/10-async-await/overview.ru.md), [14 · ASP.NET Core: Minimal API](../../modules/14-aspnet-core-minimal-api/overview.ru.md)
- Рядом: [Устойчивый HttpClient](../http-client-resilience/overview.ru.md), [Real-time через SignalR](../signalr-realtime/overview.ru.md), [JWT-аутентификация](../jwt-auth/overview.ru.md)
- В других треках: [Python](../../../python/overview.ru.md) — gRPC в Python
- Рецепты и библиотеки: [Refit](../../libraries/refit/overview.ru.md)
