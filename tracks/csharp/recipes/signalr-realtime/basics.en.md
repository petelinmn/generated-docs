---
title: "Real-time with SignalR: the minimal solution"
lang: en
track: csharp
kind: recipe
doc: basics
slug: signalr-realtime
part: 2
tags: [signalr, real-time, websockets, hub, notifications]
versions: ".NET 11, C# 15; ASP.NET Core SignalR 11.0, Microsoft.AspNetCore.SignalR.Client 11.0.0"
status: draft
updated: 2026-11-22
---

# Real-time with SignalR: the minimal solution

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Minimal** · [2. Production](advanced.en.md) · [3. Scale and edges](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [10 · async/await](../../modules/10-async-await/overview.en.md), [14 · ASP.NET Core: Minimal API](../../modules/14-aspnet-core-minimal-api/overview.en.md). **Time:** ~30 min.

## The task

Send data to a client **without a request from its side**: notifications, chat, status updates, live charts. Plain HTTP works as "request — response"; SignalR maintains a persistent connection (WebSockets, with fallback transports when needed) and provides a high-level API: the server calls client methods, the client calls server methods.

## A hub

A hub (`Hub`) is a class whose methods are called by clients; through `Clients` the hub calls client methods:

```csharp
builder.Services.AddSignalR();
app.MapHub<ChatHub>("/chat");

class ChatHub : Hub
{
    public Task Send(string user, string message) => Clients.All.SendAsync("Message", user, message);
    public Task Join(string room) => Groups.AddToGroupAsync(Context.ConnectionId, room);
    public Task ToRoom(string room, string msg) => Clients.Group(room).SendAsync("Room", msg);
    public int Add(int a, int b) => a + b;
}
```

## A client (.NET)

```csharp
var conn = new HubConnectionBuilder().WithUrl(url).Build();
conn.On<string, string>("Message", (u, m) => Console.WriteLine($"{u}: {m}"));
await conn.StartAsync();
await conn.InvokeAsync("Send", "ann", "hello all");
```

For a browser use the JavaScript client `@microsoft/signalr`; the API is similar (`on`, `invoke`, `start`).

> ⚠️ Verify: the JavaScript client was not run in this material; check the client documentation.

Verified by running (three .NET clients `ann`, `bob`, `cid` and the server in one process):

| Action | Who received it |
|---|---|
| `ann` called `Send` (`Clients.All`) | all three |
| `bob` and `cid` joined the group `room1`, `ann` called `ToRoom("room1", …)` | only `bob` and `cid` |
| the hub method `Add(2, 3)` with a return value | the client got `5` |

After `StartAsync` the connection is in the `Connected` state and has a `ConnectionId`.

## Sending from outside a hub

A hub lives only for the duration of a call. To send a message from anywhere (an endpoint, a background service) inject `IHubContext<THub>`:

```csharp
app.MapPost("/notify/{user}", async (string user, IHubContext<ChatHub> hub) =>
{
    await hub.Clients.Group("user:" + user).SendAsync("Notification", "server says hi to " + user);
    return "sent";
});
```

Verified: POST `/notify/bob` → only `bob` received the notification (he is in the group `user:bob`, to which he is added on connect in `OnConnectedAsync`).

## Beginner mistakes

- ❌ Storing state in hub fields → ✅ a hub instance is created per call; state belongs in services/DB.
- ❌ Sending to a user by `ConnectionId` → ✅ a user may have several connections; use groups/`Clients.User`.
- ❌ Polling the server on a timer when you need push → ✅ SignalR.
- ❌ Trusting incoming messages without checks → ✅ validate like any input.
- ❌ Sending secrets in a message → ✅ the client may be someone else's.

## Related materials

- Next: [production](advanced.en.md) — user groups, errors, streams, reconnecting, authorization.
- [14 · ASP.NET Core: Minimal API](../../modules/14-aspnet-core-minimal-api/overview.en.md) — routing and hosting.
- [10 · async/await](../../modules/10-async-await/overview.en.md) — asynchronous hub methods.
- [Background jobs](../background-jobs/overview.en.md) — a source of events for broadcasting.
- [JWT authentication](../jwt-auth/overview.en.md) — authenticating connections.
- [Python](../../../python/overview.en.md) — WebSockets in asyncio.

## Sources

- [Introduction to ASP.NET Core SignalR](https://learn.microsoft.com/aspnet/core/signalr/introduction)
- [Hubs in SignalR](https://learn.microsoft.com/aspnet/core/signalr/hubs)
- [Send messages from outside a hub](https://learn.microsoft.com/aspnet/core/signalr/hubcontext)
- [ASP.NET Core SignalR .NET Client](https://learn.microsoft.com/aspnet/core/signalr/dotnet-client)
