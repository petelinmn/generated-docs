---
title: "gRPC service: production"
lang: en
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

# gRPC service: production

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Minimal](basics.en.md) · **2. Production** · [3. Scale and edges](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [the minimal solution](basics.en.md). **Time:** ~45 min.

## Errors and statuses

gRPC errors are an `RpcException` with a status (`StatusCode`), not HTTP codes:

```csharp
// server
if (string.IsNullOrWhiteSpace(request.Name))
    throw new RpcException(new Status(StatusCode.InvalidArgument, "name is required"));

// client
try { await client.SayHelloAsync(new HelloRequest { Name = "" }); }
catch (RpcException e) { /* e.StatusCode, e.Status.Detail */ }
```

Verified: the client received `InvalidArgument / name is required`. An unreachable server gives `Unavailable` (verified), an exceeded deadline — `DeadlineExceeded`.

| Status | When |
|---|---|
| `InvalidArgument` | invalid input |
| `NotFound` | no such entity |
| `PermissionDenied` / `Unauthenticated` | permissions / no authentication |
| `DeadlineExceeded` | time ran out |
| `Unavailable` | the service is unavailable (can be retried) |

## Deadlines and cancellation

Every call should have a deadline; the server receives cancellation through `context.CancellationToken`:

```csharp
await client.SayHelloAsync(req, deadline: DateTime.UtcNow.AddMilliseconds(200));
// server: await Task.Delay(2000, context.CancellationToken);
```

Verified: a slow call (2 s on the server) with a 200 ms deadline ended with `DeadlineExceeded`. The deadline is passed to the server and propagates along the call chain if you pass the token on.

## Metadata

Metadata is the analogue of HTTP headers (tracing, tokens):

```csharp
var md = new Metadata { { "x-trace", "abc" } };
await client.SayHelloAsync(req, headers: md);
// server: context.RequestHeaders.GetValue("x-trace")
```

Verified: the server saw `x-trace = abc`. An authentication token is passed in the `authorization` metadata (see [JWT authentication](../jwt-auth/overview.en.md)).

> ⚠️ Verify: the JWT authentication setup for gRPC (`[Authorize]` on the service, `CallCredentials` on the client) was not run in this material.

## Streams

Server, client and bidirectional streams are verified ([minimal](basics.en.md#four-kinds-of-calls)). To read use `await foreach (var x in stream.ReadAllAsync())`, to finish a client stream — `RequestStream.CompleteAsync()`; without it the server waits for new messages forever.

## Interceptors

Interceptors (`Interceptor`) are middleware for gRPC: logging, metrics, authentication, exception translation:

```csharp
builder.Services.AddGrpc(o => o.Interceptors.Add<LoggingInterceptor>());
```

> ⚠️ Verify: the interceptor API (`Interceptor`, the `UnaryServerHandler` methods and others) was not run in this material — check the documentation.

## Contract evolution

| Change | Compatible? |
|---|---|
| add a new field with a new number | yes |
| remove a field | yes, but reserve the number/name (`reserved`) |
| change a field number or type | no |
| rename a field | compatible on the wire, breaks JSON and code |

## Best practices

| Rule | Why |
|---|---|
| A deadline on every call | no "eternal" calls |
| One long-lived channel | it is expensive to create |
| `RpcException` with the right `StatusCode` | the client can react |
| Do not change field numbers | compatibility |
| Metadata for cross-cutting data | tracing, auth |
| Retry only `Unavailable` and idempotent calls | as in [HttpClient](../http-client-resilience/advanced.en.md#idempotency-and-retries) |

## Related materials

- Back: [minimal](basics.en.md). Next: [scale and edges](expert.en.md).
- [Resilient HttpClient](../http-client-resilience/overview.en.md) — retries and deadlines.
- [22 · Observability](../../modules/22-observability/overview.en.md) — call tracing.
- [JWT authentication](../jwt-auth/overview.en.md) — tokens.

## Sources

- [gRPC error handling](https://learn.microsoft.com/aspnet/core/grpc/error-handling)
- [Deadlines and cancellation](https://learn.microsoft.com/aspnet/core/grpc/deadlines-cancellation)
- [gRPC interceptors on .NET](https://learn.microsoft.com/aspnet/core/grpc/interceptors)
