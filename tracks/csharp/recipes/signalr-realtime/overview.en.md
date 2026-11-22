---
title: "Real-time with SignalR"
lang: en
track: csharp
kind: recipe
doc: overview
slug: signalr-realtime
part: 2
levels: [1, 2, 3, 4]
tags: [signalr, real-time, websockets, hub, notifications, backplane]
versions: ".NET 11, C# 15; ASP.NET Core SignalR 11.0, Microsoft.AspNetCore.SignalR.Client 11.0.0"
status: draft
updated: 2026-11-22
---

# Real-time with SignalR

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

A recipe: send data to clients without their request — notifications, chat, live updates. SignalR keeps a persistent connection (WebSockets with a fallback to SSE and Long Polling), a hub accepts client calls and calls client methods, groups choose the audience, and `IHubContext` lets you send from anywhere. Several servers need a backplane. The JavaScript client, Redis and Azure SignalR Service were not run in this material.

## Key ideas

- `AddSignalR()` + `MapHub<T>()`; clients — `HubConnection`.
- A hub is created per call — it has no state.
- `Clients.All/Group/User`; a group per user solves several tabs.
- `IHubContext<T>` — sending from endpoints and workers.
- Errors are hidden except `HubException`.
- Streams: `IAsyncEnumerable<T>` and `StreamAsync`.
- `WithAutomaticReconnect` and state restoration; delivery is not guaranteed.
- Scale-out: a backplane (Redis) or Azure SignalR Service.

## Minimal example

```csharp
builder.Services.AddSignalR();
app.MapHub<ChatHub>("/chat");

class ChatHub : Hub
{
    public Task Send(string user, string message) => Clients.All.SendAsync("Message", user, message);
}
```

## When to use and when not

- ✅ Notifications, chats, real-time dashboards, collaborative editing.
- ✅ Progress of long operations.
- ✅ Two-way exchange with a client.
- ❌ Guaranteed delivery (you need a queue/history).
- ❌ Rare updates where periodic polling is enough.
- ❌ Large binary transfers (file upload is [another recipe](../file-upload/overview.en.md)).

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Minimal](basics.en.md) | a hub, a client, groups, `IHubContext` |
| [2. Production](advanced.en.md) | lifecycle, errors, streams, transports, reconnecting, authorization |
| [3. Scale and edges](expert.en.md) | a backplane, sticky sessions, gotchas, 10 interview questions |
| [4. Self-check](quiz.en.md) | 10 questions and [4 exercises](exercises.en.md) with solutions |
| 5. Runnable examples | not generated yet — `/learn csharp signalr-realtime level 5` |
| 6. Tests | not generated yet — `/learn csharp signalr-realtime level 6` |

## Related materials

- Prerequisites: [10 · async/await](../../modules/10-async-await/overview.en.md), [14 · ASP.NET Core: Minimal API](../../modules/14-aspnet-core-minimal-api/overview.en.md)
- Nearby: [Background jobs](../background-jobs/overview.en.md), [JWT authentication](../jwt-auth/overview.en.md), [Rate limiting](../rate-limiting/overview.en.md)
- In other tracks: [Python](../../../python/overview.en.md) — WebSockets in asyncio
- Recipes and libraries: [gRPC service](../grpc-service/overview.en.md)
