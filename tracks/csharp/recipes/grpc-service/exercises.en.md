---
title: "Exercises: gRPC service"
lang: en
track: csharp
kind: recipe
doc: exercises
slug: grpc-service
part: 2
tags: [grpc, protobuf, streaming, deadline, metadata]
status: draft
updated: 2026-11-22
---

# Exercises: gRPC service

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. The solutions were verified by running (`Grpc.AspNetCore` 2.85.0, `Grpc.Net.Client` 2.85.0; server and client in one process, HTTP/2 without TLS on loopback). There are no runnable versions in `code/` (level 5 does not exist). The contract is from the [minimal level](basics.en.md#the-proto-contract).

## 1. 🟢 A unary call

**Task:** implement `SayHello` and call it with `Ann`.

**Expected result:** `Hello Ann`.

<details><summary>Hint</summary>

Inherit `Greeter.GreeterBase`, `app.MapGrpcService<…>()`, the client `Greeter.GreeterClient(channel)`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 1: unary RPC.
class GreeterService : Greeter.GreeterBase
{
    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        => Task.FromResult(new HelloReply { Message = "Hello " + request.Name });
}
```

Verified: `unary: Hello Ann`.

</details>

## 2. 🟡 An error with the right status

**Task:** for an empty name the server must return `InvalidArgument` with the message `name is required`.

**Expected result:** the client catches an `RpcException` with the status `InvalidArgument`.

<details><summary>Hint</summary>

`throw new RpcException(new Status(StatusCode.InvalidArgument, "…"))`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 2: signal a client error with a gRPC status.
if (string.IsNullOrWhiteSpace(request.Name))
    throw new RpcException(new Status(StatusCode.InvalidArgument, "name is required"));
```

Verified: `error: InvalidArgument / name is required`.

</details>

## 3. 🟡 Streams

**Task:** implement `Countdown` (server streaming), `Sum` (client streaming) and `Chat` (bidirectional echo).

**Expected result:** `3,2,1`; `10`; `echo:a,echo:b`.

<details><summary>Hint</summary>

`stream.WriteAsync`, `stream.ReadAllAsync()`, on the client `RequestStream.CompleteAsync()`.

</details>

<details><summary>Solution</summary>

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

Verified: `server stream: 3,2,1`, `client stream sum: 10`, `bidi: echo:a,echo:b`.

</details>

## 4. 🔴 A deadline and metadata

**Task:** a call with the name `slow` takes 2 s; set a 200 ms deadline on the client. Then pass the metadata `x-trace: abc` and read it on the server.

**Expected result:** `DeadlineExceeded`; the reply `trace=abc`.

<details><summary>Hint</summary>

`deadline: DateTime.UtcNow.AddMilliseconds(200)`; `headers: new Metadata { … }`; `context.RequestHeaders.GetValue("x-trace")`; on the server pass `context.CancellationToken` to `Task.Delay`.

</details>

<details><summary>Solution</summary>

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

Verified: `deadline: DeadlineExceeded`, `metadata echoed: trace=abc`.

</details>
