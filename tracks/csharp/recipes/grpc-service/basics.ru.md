---
title: "gRPC-сервис: минимальное решение"
lang: ru
track: csharp
kind: recipe
doc: basics
slug: grpc-service
part: 2
tags: [grpc, protobuf, rpc, http2, streaming]
versions: ".NET 11, C# 15; Grpc.AspNetCore 2.85.0, Grpc.Net.Client 2.85.0"
status: draft
updated: 2026-11-22
---

# gRPC-сервис: минимальное решение

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Минимум** · [2. Production](advanced.ru.md) · [3. Масштаб и грани](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [10 · async/await](../../modules/10-async-await/overview.ru.md), [14 · ASP.NET Core: Minimal API](../../modules/14-aspnet-core-minimal-api/overview.ru.md). **Время:** ~30 мин.

## Задача

Построить сервис с чётким контрактом, быстрым бинарным обменом и стримингом, а к нему — типизированный клиент. gRPC описывает API в файле `.proto` (Protocol Buffers), из которого генерируется серверная база и клиент; работает поверх HTTP/2.

## Контракт `.proto`

```protobuf
syntax = "proto3";
option csharp_namespace = "Demo";
package demo;

service Greeter {
  rpc SayHello (HelloRequest) returns (HelloReply);
  rpc Countdown (CountRequest) returns (stream CountReply);
  rpc Sum (stream Number) returns (Total);
  rpc Chat (stream Msg) returns (stream Msg);
}
message HelloRequest { string name = 1; }
message HelloReply { string message = 1; }
message CountRequest { int32 from = 1; }
message CountReply { int32 value = 1; }
message Number { int32 value = 1; }
message Total { int32 sum = 1; }
message Msg { string text = 1; }
```

Подключение в `.csproj` (пакеты `Grpc.AspNetCore`, `Grpc.Net.Client` — для клиента):

```xml
<ItemGroup>
  <Protobuf Include="greeter.proto" GrpcServices="Both" />
  <PackageReference Include="Grpc.AspNetCore" Version="2.85.0" />
  <PackageReference Include="Grpc.Net.Client" Version="2.85.0" />
</ItemGroup>
```

Номера полей (`= 1`) — часть контракта: они, а не имена, идут по сети.

## Сервер

```csharp
builder.Services.AddGrpc();
app.MapGrpcService<GreeterService>();

class GreeterService : Greeter.GreeterBase
{
    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        => Task.FromResult(new HelloReply { Message = "Hello " + request.Name });
}
```

## Клиент

```csharp
using var channel = GrpcChannel.ForAddress(address);
var client = new Greeter.GreeterClient(channel);
var reply = await client.SayHelloAsync(new HelloRequest { Name = "Ann" });
```

Проверено запуском (сервер Kestrel на HTTP/2 без TLS и клиент в одном процессе): унарный вызов вернул `Hello Ann`.

> В примере используется HTTP/2 без TLS (`HttpProtocols.Http2` на loopback) — только для локальной проверки. В продакшене gRPC работает по HTTPS.

## Четыре вида вызовов

| Вид | Сигнатура в `.proto` | Проверенный результат |
|---|---|---|
| Унарный | `rpc SayHello (Req) returns (Res)` | `Hello Ann` |
| Серверный стрим | `returns (stream CountReply)` | `Countdown(3)` → `3,2,1` |
| Клиентский стрим | `rpc Sum (stream Number) returns (Total)` | `1,2,3,4` → `sum = 10` |
| Двунаправленный | `rpc Chat (stream Msg) returns (stream Msg)` | `a,b` → `echo:a,echo:b` |

## Частые ошибки новичка

- ❌ Менять номера полей в `.proto` → ✅ номера нельзя переиспользовать/менять — это ломает совместимость.
- ❌ Новый `GrpcChannel` на каждый вызов → ✅ канал долгоживущий, переиспользуется.
- ❌ Бросать обычные исключения для ошибок клиента → ✅ `RpcException` со `StatusCode` ([production](advanced.ru.md#ошибки-и-статусы)).
- ❌ Вызовы без дедлайна → ✅ задавать `deadline`.
- ❌ Вызывать gRPC из браузера напрямую → ✅ нужен gRPC-Web ([грани](expert.ru.md#грани-и-подвохи)).

## Связанные материалы

- Дальше: [production](advanced.ru.md) — ошибки, дедлайны, метаданные, перехватчики.
- [14 · ASP.NET Core: Minimal API](../../modules/14-aspnet-core-minimal-api/overview.ru.md) — хостинг.
- [10 · async/await](../../modules/10-async-await/overview.ru.md) — потоки `IAsyncEnumerable`.
- [Устойчивый HttpClient](../http-client-resilience/overview.ru.md) — ретраи и таймауты для HTTP-клиентов.
- [Real-time через SignalR](../signalr-realtime/overview.ru.md) — другой подход к потокам.
- [Python](../../../python/overview.ru.md) — gRPC в Python.

## Источники

- [gRPC services with ASP.NET Core](https://learn.microsoft.com/aspnet/core/grpc/aspnetcore)
- [Create a .NET Core gRPC client](https://learn.microsoft.com/aspnet/core/grpc/client)
- [Protocol Buffers language guide](https://protobuf.dev/programming-guides/proto3/)
