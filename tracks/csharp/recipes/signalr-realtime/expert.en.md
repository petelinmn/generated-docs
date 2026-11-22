---
title: "Real-time with SignalR: scale and edges"
lang: en
track: csharp
kind: recipe
doc: expert
slug: signalr-realtime
part: 2
tags: [signalr, backplane, redis, scale-out, sticky-sessions, interview-questions]
versions: ".NET 11, C# 15; ASP.NET Core SignalR 11.0"
status: draft
updated: 2026-11-22
---

# Real-time with SignalR: scale and edges

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Minimal](basics.en.md) · [2. Production](advanced.en.md) · **3. Scale and edges** · [4. Quiz](quiz.en.md)

**Prerequisites:** [minimal](basics.en.md) and [production](advanced.en.md). **Time:** ~30 min.

## Scale

- **Several servers.** A connection lives on one server: `Clients.All` on server A will not reach the clients of server B. You need a **backplane** (Redis, Azure SignalR Service): servers exchange messages through a shared layer.
- **Sticky sessions.** With transports other than WebSockets (SSE, Long Polling), and with connection negotiation, a client's requests must land on one server; with WebSockets-only and skipping negotiation (`SkipNegotiation`) the requirement is lifted.
- **A connection limit.** Each connection is a handle and memory; count the limits per node, tune OS and balancer limits.
- **A managed service.** Azure SignalR Service moves holding connections out of your application.
- **A source of events.** Broadcasting from business events (a bus, a DB trigger, a background job) is a separate responsibility ([Background jobs](../background-jobs/overview.en.md)).

> ⚠️ Verify: a Redis backplane, Azure SignalR Service, sticky sessions and `SkipNegotiation` were not run in this material; check the "SignalR scale-out" and "Host and scale" documentation.

## Edges and gotchas

**1. A hub is created anew on every call.** Hub fields do not keep state; shared data belongs in a singleton/DB.

**2. `Clients.All` means everyone on this server.** Without a backplane and with several instances only the current one's clients receive it.

**3. Exceptions are hidden.** Verified: an ordinary exception gives a generic message, `HubException` gets through as text ([production](advanced.en.md#hub-errors)).

**4. After reconnecting — a new connection.** According to the documentation, on automatic reconnect the `ConnectionId` is new, and group membership is tied to the connection and disappears with it; only the firing of `Reconnecting` after a server stop was verified.

> ⚠️ Verify: the `ConnectionId` change and the loss of groups after `Reconnected` were not reproduced in this material.

**5. Messages are not guaranteed to be delivered.** It is not a queue: an offline client misses the message. You need history (a DB) and "catching up" after reconnecting.

**6. Order and parallelism.** Messages of one connection are processed sequentially by default; heavy work in a hub blocks the other calls of that connection — move it to the background.

**7. Transport limits.** A proxy may cut WebSockets; SignalR falls back to SSE/Long Polling (verified: all three transports connect) — but with a different load profile.

**8. A token in the query string.** The token lands in the proxy's access logs; use short-lived tokens.

**9. Group security.** A user must not join others' groups at will: check permissions in `Join`.

## Trade-offs and alternatives

| Approach | Pros | Cons |
|---|---|---|
| SignalR | abstraction, groups, reconnecting | its own server-side connection, scale-out |
| Plain WebSockets | full control | everything by hand: groups, reconnects |
| Server-Sent Events | simple, a one-way stream | server → client only |
| Long polling / short polling | works everywhere | load and latency |
| Push notifications (FCM/APNs) | reach a closed app | an external service |
| A broker (MQTT etc.) | devices/IoT | separate infrastructure |

## Interview questions

<details><summary>1. What is SignalR and why is it needed?</summary>

An ASP.NET Core library for two-way real-time communication: the server can call client methods and clients can call hub methods; the transport is chosen automatically.

</details>

<details><summary>2. Which transports does SignalR support?</summary>

WebSockets, Server-Sent Events, Long Polling; the best available is chosen. Verified: a connection works on each.

</details>

<details><summary>3. How do you send a message to clients from outside a hub?</summary>

Inject `IHubContext<THub>` and use `Clients`/`Groups` (verified: a POST endpoint delivered a notification to the right user).

</details>

<details><summary>4. How do you send a message to a specific user with several tabs?</summary>

Add each connection to the user's group on connect and send to the group (or `Clients.User` with a configured user identifier).

</details>

<details><summary>5. What does a client see if a hub method throws?</summary>

A generic message without details; for visible errors — `HubException` (verified). `EnableDetailedErrors` is for development only.

</details>

<details><summary>6. How do you scale SignalR to several servers?</summary>

A backplane (Redis) or Azure SignalR Service, sticky sessions where connection negotiation requires them.

</details>

<details><summary>7. Does SignalR guarantee delivery?</summary>

No. An offline client loses messages; you need history and catching up after reconnecting.

</details>

<details><summary>8. How does automatic reconnecting work?</summary>

`WithAutomaticReconnect()`: the events `Reconnecting`/`Reconnected`/`Closed`; the `ConnectionId` after reconnecting is new (per the documentation; the firing of `Reconnecting` is verified).

</details>

<details><summary>9. Trick question: a hub field stores a counter of connected clients. What is wrong?</summary>

A hub instance is created per call — the counter resets; you need a singleton service/DB (and a backplane with several servers).

</details>

<details><summary>10. Trick question: `Clients.All` does not reach some users in production although it works locally. The cause?</summary>

Several instances without a backplane: `All` applies to the current server's connections.

</details>

## Related materials

- Back: [production](advanced.en.md). Check yourself: [quiz](quiz.en.md) and [exercises](exercises.en.md).
- [21 · Application architecture](../../modules/21-architecture/overview.en.md) — events and boundaries.
- [Kubernetes](../../../kubernetes/overview.en.md) — ingress and WebSockets, sticky sessions.
- [Background jobs](../background-jobs/overview.en.md) — generating events.

## Sources

- [SignalR scale-out with Redis](https://learn.microsoft.com/aspnet/core/signalr/redis-backplane)
- [Host and scale SignalR](https://learn.microsoft.com/aspnet/core/signalr/scale)
- [Azure SignalR Service](https://learn.microsoft.com/azure/azure-signalr/signalr-overview)
