---
title: "Real-time with SignalR: production"
lang: en
track: csharp
kind: recipe
doc: advanced
slug: signalr-realtime
part: 2
tags: [signalr, hub-exception, streaming, reconnect, authorization, transports]
versions: ".NET 11, C# 15; ASP.NET Core SignalR 11.0, Microsoft.AspNetCore.SignalR.Client 11.0.0"
status: draft
updated: 2026-11-22
---

# Real-time with SignalR: production

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Minimal](basics.en.md) · **2. Production** · [3. Scale and edges](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [the minimal solution](basics.en.md). **Time:** ~45 min.

## User groups and the connection lifecycle

One user may have several connections (tabs, devices). A practical trick — on connect add the connection to the user's group:

```csharp
public override async Task OnConnectedAsync()
{
    var user = Context.GetHttpContext()!.Request.Query["user"].ToString();
    await Groups.AddToGroupAsync(Context.ConnectionId, "user:" + user);
    await base.OnConnectedAsync();
}
public override Task OnDisconnectedAsync(Exception? e) => base.OnDisconnectedAsync(e);
```

Verified: the event log `connected ann | connected bob | connected cid | disconnected cid (clean)` — after the client's `StopAsync` `OnDisconnectedAsync` was called with `null` (a clean disconnect). Groups in SignalR need not be cleaned up by hand: when a connection drops it leaves its groups by itself.

> In a real application the user name comes not from the query string but from the authenticated `Context.User` (see [authorization](#authorization)): in the example the query string is a simplification for the test.

## Hub errors

An exception inside a hub method reaches the client **without details** by default. Verified:

| What the method throws | What the client sees |
|---|---|
| `InvalidOperationException("secret internal detail")` | `An unexpected error occurred invoking 'Boom' on the server.` — details hidden |
| `HubException("Name must not be empty")` | `…invoking 'Friendly' on the server. HubException: Name must not be empty` — the message gets through |

Throw `HubException` for errors the client should see (validation) and log the rest on the server. `EnableDetailedErrors = true` shows internal messages — for development only.

## Streams from the server

A hub method can return `IAsyncEnumerable<T>` — the client reads the stream as data appears:

```csharp
public async IAsyncEnumerable<int> Count(int n, [EnumeratorCancellation] CancellationToken ct)
{
    for (var i = 1; i <= n; i++) { yield return i; await Task.Delay(10, ct); }
}
// client
await foreach (var x in conn.StreamAsync<int>("Count", 5)) { /* 1,2,3,4,5 */ }
```

Verified: the client received `1,2,3,4,5`.

## Transports

SignalR picks the transport itself: WebSockets → Server-Sent Events → Long Polling. Verified that a connection is established on each of the three (WebSockets, ServerSentEvents, LongPolling) when `o.Transports` forces it. Behind a proxy make sure it lets WebSockets through.

## Reconnecting

```csharp
var conn = new HubConnectionBuilder().WithUrl(url).WithAutomaticReconnect().Build();
conn.Reconnecting += e => { /* show "reconnecting…" */ return Task.CompletedTask; };
conn.Reconnected += id => { /* re-subscribe, refresh state */ return Task.CompletedTask; };
conn.Closed += e => { /* give up or restart manually */ return Task.CompletedTask; };
```

Verified: after the server was stopped a client with `WithAutomaticReconnect` raised the `Reconnecting` event, the state was `Reconnecting`.

> ⚠️ Verify: the final move to `Closed` after attempts run out and reconnecting to a returned server were not checked; also remember that after reconnecting the `ConnectionId` is new and server groups must be restored (for example in `OnConnectedAsync`).

## Authorization

`[Authorize]` on a hub or a method requires an authenticated user; for browser WebSockets the token is usually passed in the query string (`access_token`), because WebSocket headers are not available from JS. For token details see [JWT authentication](../jwt-auth/overview.en.md).

> ⚠️ Verify: the JWT configuration for SignalR (reading `access_token` from the query in `OnMessageReceived`) was not run in this material.

## Best practices

| Rule | Why |
|---|---|
| A thin hub, logic in services | a hub is created per call |
| `IHubContext` for sending from outside | from endpoints and workers |
| A group per user | several connections |
| `HubException` for client errors | the rest is hidden |
| Reconnecting and state restoration | networks are unstable |
| Small messages | do not push big data through a socket |
| Limit sizes and frequency | protection from abuse ([Rate limiting](../rate-limiting/overview.en.md)) |

## Related materials

- Back: [minimal](basics.en.md). Next: [scale and edges](expert.en.md).
- [JWT authentication](../jwt-auth/overview.en.md) — tokens for connections.
- [22 · Observability](../../modules/22-observability/overview.en.md) — connection counts and messages.
- [Rate limiting](../rate-limiting/overview.en.md) — limiting frequency.

## Sources

- [SignalR hubs: error handling](https://learn.microsoft.com/aspnet/core/signalr/hubs#handle-errors)
- [Streaming in SignalR](https://learn.microsoft.com/aspnet/core/signalr/streaming)
- [SignalR configuration: transports](https://learn.microsoft.com/aspnet/core/signalr/configuration)
- [SignalR authentication and authorization](https://learn.microsoft.com/aspnet/core/signalr/authn-and-authz)
