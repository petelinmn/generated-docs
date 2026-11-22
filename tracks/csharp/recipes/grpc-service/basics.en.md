---
title: "gRPC service: the minimal solution"
lang: en
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

# gRPC service: the minimal solution

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Minimal** · [2. Production](advanced.en.md) · [3. Scale and edges](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [10 · async/await](../../modules/10-async-await/overview.en.md), [14 · ASP.NET Core: Minimal API](../../modules/14-aspnet-core-minimal-api/overview.en.md). **Time:** ~30 min.

## The task

Build a service with a clear contract, fast binary exchange and streaming, plus a typed client for it. gRPC describes the API in a `.proto` file (Protocol Buffers) from which the server base and the client are generated; it works over HTTP/2.

## The `.proto` contract

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

Wiring in the `.csproj` (the packages `Grpc.AspNetCore`, and `Grpc.Net.Client` for the client):

```xml
<ItemGroup>
  <Protobuf Include="greeter.proto" GrpcServices="Both" />
  <PackageReference Include="Grpc.AspNetCore" Version="2.85.0" />
  <PackageReference Include="Grpc.Net.Client" Version="2.85.0" />
</ItemGroup>
```

Field numbers (`= 1`) are part of the contract: they, not the names, go over the wire.

## The server

```csharp
builder.Services.AddGrpc();
app.MapGrpcService<GreeterService>();

class GreeterService : Greeter.GreeterBase
{
    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        => Task.FromResult(new HelloReply { Message = "Hello " + request.Name });
}
```

## The client

```csharp
using var channel = GrpcChannel.ForAddress(address);
var client = new Greeter.GreeterClient(channel);
var reply = await client.SayHelloAsync(new HelloRequest { Name = "Ann" });
```

Verified by running (a Kestrel server on HTTP/2 without TLS and a client in one process): the unary call returned `Hello Ann`.

> The example uses HTTP/2 without TLS (`HttpProtocols.Http2` on loopback) — only for a local check. In production gRPC runs over HTTPS.

## Four kinds of calls

| Kind | Signature in `.proto` | Verified result |
|---|---|---|
| Unary | `rpc SayHello (Req) returns (Res)` | `Hello Ann` |
| Server streaming | `returns (stream CountReply)` | `Countdown(3)` → `3,2,1` |
| Client streaming | `rpc Sum (stream Number) returns (Total)` | `1,2,3,4` → `sum = 10` |
| Bidirectional | `rpc Chat (stream Msg) returns (stream Msg)` | `a,b` → `echo:a,echo:b` |

## Beginner mistakes

- ❌ Changing field numbers in `.proto` → ✅ numbers must not be reused/changed — it breaks compatibility.
- ❌ A new `GrpcChannel` per call → ✅ a channel is long-lived and reused.
- ❌ Throwing ordinary exceptions for client errors → ✅ `RpcException` with a `StatusCode` ([production](advanced.en.md#errors-and-statuses)).
- ❌ Calls without a deadline → ✅ set a `deadline`.
- ❌ Calling gRPC from a browser directly → ✅ you need gRPC-Web ([edges](expert.en.md#edges-and-gotchas)).

## Related materials

- Next: [production](advanced.en.md) — errors, deadlines, metadata, interceptors.
- [14 · ASP.NET Core: Minimal API](../../modules/14-aspnet-core-minimal-api/overview.en.md) — hosting.
- [10 · async/await](../../modules/10-async-await/overview.en.md) — `IAsyncEnumerable` streams.
- [Resilient HttpClient](../http-client-resilience/overview.en.md) — retries and timeouts for HTTP clients.
- [Real-time with SignalR](../signalr-realtime/overview.en.md) — another approach to streams.
- [Python](../../../python/overview.en.md) — gRPC in Python.

## Sources

- [gRPC services with ASP.NET Core](https://learn.microsoft.com/aspnet/core/grpc/aspnetcore)
- [Create a .NET Core gRPC client](https://learn.microsoft.com/aspnet/core/grpc/client)
- [Protocol Buffers language guide](https://protobuf.dev/programming-guides/proto3/)
