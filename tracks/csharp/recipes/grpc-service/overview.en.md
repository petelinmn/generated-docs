---
title: "gRPC service"
lang: en
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

# gRPC service

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

A recipe: a service and a client over gRPC in ASP.NET Core. The contract is described in `.proto`, from which the server base and the client are generated; the exchange is binary over HTTP/2. There are four kinds of calls (unary and three streaming), errors go through `RpcException` and statuses, and every call should have a deadline. All kinds of calls, errors, deadlines and metadata were verified by running; gRPC-Web, balancing and interceptors were not.

## Key ideas

- `.proto` is the contract; field numbers are part of the contract.
- `AddGrpc()` + `MapGrpcService<T>()`; the client — `GrpcChannel` + a generated client.
- Unary, server streaming, client streaming, bidirectional.
- Errors — `RpcException` + `StatusCode`; deadlines and `CancellationToken`.
- Metadata — the analogue of headers.
- A long-lived channel; L7/client-side balancing.
- A browser needs gRPC-Web or a REST facade.

## Minimal example

```csharp
builder.Services.AddGrpc();
app.MapGrpcService<GreeterService>();

class GreeterService : Greeter.GreeterBase
{
    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        => Task.FromResult(new HelloReply { Message = "Hello " + request.Name });
}
```

## When to use and when not

- ✅ Microservice interaction, internal APIs.
- ✅ Data streaming, low latency, a strict contract.
- ✅ Polyglot environments (code generation for many languages).
- ❌ A public API for browsers out of the box (needs gRPC-Web).
- ❌ Simple CRUD APIs where REST/JSON is enough.
- ❌ Environments without HTTP/2.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Minimal](basics.en.md) | `.proto`, the server, the client, four kinds of calls |
| [2. Production](advanced.en.md) | statuses, deadlines, metadata, streams, contract evolution |
| [3. Scale and edges](expert.en.md) | balancing, gRPC-Web, gotchas, 10 interview questions |
| [4. Self-check](quiz.en.md) | 10 questions and [4 exercises](exercises.en.md) with solutions |
| 5. Runnable examples | not generated yet — `/learn csharp grpc-service level 5` |
| 6. Tests | not generated yet — `/learn csharp grpc-service level 6` |

## Related materials

- Prerequisites: [10 · async/await](../../modules/10-async-await/overview.en.md), [14 · ASP.NET Core: Minimal API](../../modules/14-aspnet-core-minimal-api/overview.en.md)
- Nearby: [Resilient HttpClient](../http-client-resilience/overview.en.md), [Real-time with SignalR](../signalr-realtime/overview.en.md), [JWT authentication](../jwt-auth/overview.en.md)
- In other tracks: [Python](../../../python/overview.en.md) — gRPC in Python
- Recipes and libraries: [Refit](../../libraries/refit/overview.en.md)
