---
title: "Упражнения: gRPC-сервис"
lang: ru
track: csharp
kind: recipe
doc: exercises
slug: grpc-service
part: 2
tags: [grpc, protobuf, streaming, deadline, metadata]
status: draft
updated: 2026-11-22
---

# Упражнения: gRPC-сервис

🌐 **Русский** · [English](exercises.en.md)

> Решайте сами, потом открывайте подсказки. Решения проверены запуском (`Grpc.AspNetCore` 2.85.0, `Grpc.Net.Client` 2.85.0; сервер и клиент в одном процессе, HTTP/2 без TLS на loopback). Запускаемых версий в `code/` нет (уровня 5 нет). Контракт — из [минимума](basics.ru.md#контракт-proto).

## 1. 🟢 Унарный вызов

**Задача:** реализуйте `SayHello` и вызовите его с `Ann`.

**Ожидаемый результат:** `Hello Ann`.

<details><summary>Подсказка</summary>

Наследуйте `Greeter.GreeterBase`, `app.MapGrpcService<…>()`, клиент `Greeter.GreeterClient(channel)`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 1: unary RPC.
class GreeterService : Greeter.GreeterBase
{
    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        => Task.FromResult(new HelloReply { Message = "Hello " + request.Name });
}
```

Проверено: `unary: Hello Ann`.

</details>

## 2. 🟡 Ошибка с правильным статусом

**Задача:** на пустое имя сервер должен вернуть `InvalidArgument` с сообщением `name is required`.

**Ожидаемый результат:** клиент ловит `RpcException` со статусом `InvalidArgument`.

<details><summary>Подсказка</summary>

`throw new RpcException(new Status(StatusCode.InvalidArgument, "…"))`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 2: signal a client error with a gRPC status.
if (string.IsNullOrWhiteSpace(request.Name))
    throw new RpcException(new Status(StatusCode.InvalidArgument, "name is required"));
```

Проверено: `error: InvalidArgument / name is required`.

</details>

## 3. 🟡 Стримы

**Задача:** реализуйте `Countdown` (серверный стрим), `Sum` (клиентский) и `Chat` (двунаправленный эхо).

**Ожидаемый результат:** `3,2,1`; `10`; `echo:a,echo:b`.

<details><summary>Подсказка</summary>

`stream.WriteAsync`, `stream.ReadAllAsync()`, на клиенте `RequestStream.CompleteAsync()`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 3: streaming RPCs.
public override async Task Countdown(CountRequest request, IServerStreamWriter<CountReply> stream, ServerCallContext context)
{
    for (var i = request.From; i >= 1; i--) await stream.WriteAsync(new CountReply { Value = i });
}
public override async Task<Total> Sum(IAsyncStreamReader<Number> stream, ServerCallContext context)
{
    var s = 0; await foreach (var n in stream.ReadAllAsync()) s += n.Value; return new Total { Sum = s };
}
public override async Task Chat(IAsyncStreamReader<Msg> req, IServerStreamWriter<Msg> resp, ServerCallContext context)
{
    await foreach (var m in req.ReadAllAsync()) await resp.WriteAsync(new Msg { Text = "echo:" + m.Text });
}
```

Проверено: `server stream: 3,2,1`, `client stream sum: 10`, `bidi: echo:a,echo:b`.

</details>

## 4. 🔴 Дедлайн и метаданные

**Задача:** вызов с именем `slow` занимает 2 с; задайте клиенту дедлайн 200 мс. Затем передайте метаданные `x-trace: abc` и прочитайте их на сервере.

**Ожидаемый результат:** `DeadlineExceeded`; ответ `trace=abc`.

<details><summary>Подсказка</summary>

`deadline: DateTime.UtcNow.AddMilliseconds(200)`; `headers: new Metadata { … }`; `context.RequestHeaders.GetValue("x-trace")`; на сервере передайте `context.CancellationToken` в `Task.Delay`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 4: deadlines and metadata.
try { await cl.SayHelloAsync(new HelloRequest { Name = "slow" }, deadline: DateTime.UtcNow.AddMilliseconds(200)); }
catch (RpcException e) { Console.WriteLine("deadline: " + e.StatusCode); }

var md = new Metadata { { "x-trace", "abc" } };
var reply = await cl.SayHelloAsync(new HelloRequest { Name = "meta" }, headers: md);

// server
if (request.Name == "slow") await Task.Delay(2000, context.CancellationToken);
var trace = context.RequestHeaders.GetValue("x-trace");
```

Проверено: `deadline: DeadlineExceeded`, `metadata echoed: trace=abc`.

</details>
