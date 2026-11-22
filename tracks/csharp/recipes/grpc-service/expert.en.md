---
title: "gRPC service: scale and edges"
lang: en
track: csharp
kind: recipe
doc: expert
slug: grpc-service
part: 2
tags: [grpc, grpc-web, load-balancing, http2, protobuf, interview-questions]
versions: ".NET 11, C# 15; Grpc.AspNetCore 2.85.0, Grpc.Net.Client 2.85.0"
status: draft
updated: 2026-11-22
---

# gRPC service: scale and edges

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Minimal](basics.en.md) · [2. Production](advanced.en.md) · **3. Scale and edges** · [4. Quiz](quiz.en.md)

**Prerequisites:** [minimal](basics.en.md) and [production](advanced.en.md). **Time:** ~30 min.

## Scale

- **Load balancing.** gRPC holds a long HTTP/2 connection and multiplexes calls: an L4 balancer "sticks" all of a client's calls to one server. You need L7 balancing (a proxy that understands HTTP/2) or client-side balancing (several addresses, round-robin).
- **Streams and resources.** A long stream holds a connection and memory; limit the number of concurrent streams and idle timeouts.
- **Message size.** There are default message size limits; send large data as a stream in parts.
- **API versioning.** Packages and services with a version in the name (`package demo.v1`), backward compatibility of the contract.
- **Monitoring.** Metrics per method and status, tracing through metadata ([22](../../modules/22-observability/overview.en.md)).

> ⚠️ Verify: client-side balancing, message size limit settings and gRPC-Web were not run in this material.

## Edges and gotchas

**1. HTTP/2 is mandatory.** Without TLS the server must listen specifically for HTTP/2 (in the check — `HttpProtocols.Http2` on loopback); an HTTP/1.1 port will not serve gRPC.

**2. A browser does not call gRPC directly.** You need gRPC-Web (a proxy/middleware) or a REST facade (gRPC JSON transcoding).

**3. A dynamic port and `localhost`.** For port `0` Kestrel does not allow `ListenLocalhost(0, …)` — you need `Listen(IPAddress.Loopback, 0, …)` (verified: `InvalidOperationException`).

**4. A server exception → `Unknown`.** The client sees an unhandled exception as a generic status; use `RpcException` with the right `StatusCode` (verified with `InvalidArgument`).

**5. An unfinished client stream.** Without `CompleteAsync()` the server waits for new messages.

**6. A deadline is not a socket timeout.** A deadline is a property of the call and propagates to the server; verified: `DeadlineExceeded` at 200 ms.

**7. Field numbers.** Changing a number/type silently breaks old clients — the data is read as other fields.

**8. Defaults in proto3.** An absent field and a field with the default value (0, "") are indistinguishable; for "no value" use `optional`/wrappers (`google.protobuf.*Value`).

**9. One channel — many calls.** Creating a channel per call means extra connections; a channel is thread-safe and long-lived.

## Trade-offs and alternatives

| Approach | Pros | Cons |
|---|---|---|
| gRPC | contract, binary format, streams, code generation | not for browsers out of the box, harder to debug |
| REST/JSON | universal, easy to debug | no strict contract out of the box, heavier |
| GraphQL | flexible client queries | server complexity |
| SignalR | two-way real-time for browsers | a .NET-centric protocol |
| A message broker | asynchrony, reliability | not a synchronous RPC |

## Interview questions

<details><summary>1. What is gRPC and how does it differ from REST?</summary>

An RPC framework over HTTP/2 with a contract in Protocol Buffers: binary serialization, code generation, streams; REST is resources and JSON over HTTP.

</details>

<details><summary>2. Which kinds of calls does gRPC have?</summary>

Unary, server streaming, client streaming, bidirectional. All four are verified in the example.

</details>

<details><summary>3. Why can't you change field numbers in `.proto`?</summary>

Numbers, not names, go over the wire; changing a number or type silently breaks compatibility.

</details>

<details><summary>4. How do you report an error to the client?</summary>

`RpcException` with a `StatusCode` and a description (verified: `InvalidArgument`).

</details>

<details><summary>5. Why a deadline?</summary>

It bounds the call time, propagates to the server and frees resources; verified: `DeadlineExceeded`.

</details>

<details><summary>6. How do you pass a trace id or a token?</summary>

In metadata (`Metadata`), on the server — `context.RequestHeaders` (verified).

</details>

<details><summary>7. Why is an L4 balancer poor for gRPC?</summary>

A long multiplexed HTTP/2 connection pins all of a client's calls to one server; you need L7 or client-side balancing.

</details>

<details><summary>8. How do you call gRPC from a browser?</summary>

Through gRPC-Web or JSON transcoding; a browser does not control HTTP/2 frames directly.

</details>

<details><summary>9. Trick question: a client stream "hangs" on the server. The cause?</summary>

The client did not call `RequestStream.CompleteAsync()` — the server waits for further messages.

</details>

<details><summary>10. Trick question: the field `int32 age` arrived as 0 — did the user enter 0 or leave it unset?</summary>

In proto3 it is indistinguishable; use `optional int32 age` or the `Int32Value` wrapper.

</details>

## Related materials

- Back: [production](advanced.en.md). Check yourself: [quiz](quiz.en.md) and [exercises](exercises.en.md).
- [21 · Application architecture](../../modules/21-architecture/overview.en.md) — service boundaries.
- [Kubernetes](../../../kubernetes/overview.en.md) — L7 balancing and ingress for gRPC.
- [Real-time with SignalR](../signalr-realtime/overview.en.md) — an alternative for browsers.

## Sources

- [gRPC on .NET: load balancing](https://learn.microsoft.com/aspnet/core/grpc/loadbalancing)
- [gRPC-Web in ASP.NET Core](https://learn.microsoft.com/aspnet/core/grpc/grpcweb)
- [Protocol Buffers: field presence](https://protobuf.dev/programming-guides/field_presence/)
