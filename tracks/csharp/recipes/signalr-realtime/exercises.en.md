---
title: "Exercises: Real-time with SignalR"
lang: en
track: csharp
kind: recipe
doc: exercises
slug: signalr-realtime
part: 2
tags: [signalr, hub, groups, streaming, ihubcontext]
status: draft
updated: 2026-11-22
---

# Exercises: Real-time with SignalR

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. The solutions were verified by running: a Kestrel server and .NET clients `Microsoft.AspNetCore.SignalR.Client` 11.0.0 in one process. There are no runnable versions in `code/` (level 5 does not exist).

## 1. 🟢 A broadcast to everyone

**Task:** a hub with a method `Send(user, message)` broadcasting the event `Message` to everyone. Connect three clients and send a message from the first.

**Expected result:** all three clients received `hello all`.

<details><summary>Hint</summary>

`Clients.All.SendAsync("Message", user, message)`; on the client `conn.On<string, string>("Message", …)`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 1: broadcast to every connected client.
class ChatHub : Hub
{
    public Task Send(string user, string message) => Clients.All.SendAsync("Message", user, message);
}
app.MapHub<ChatHub>("/chat");
```

Verified: `ann`, `bob`, `cid` received `Message from ann: hello all`.

</details>

## 2. 🟡 A room

**Task:** add `Join(room)` and `ToRoom(room, msg)`. `bob` and `cid` join `room1`, `ann` sends a message to the room.

**Expected result:** only `bob` and `cid` received it.

<details><summary>Hint</summary>

`Groups.AddToGroupAsync(Context.ConnectionId, room)` and `Clients.Group(room)`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 2: groups limit the audience.
public Task Join(string room) => Groups.AddToGroupAsync(Context.ConnectionId, room);
public Task ToRoom(string room, string msg) => Clients.Group(room).SendAsync("Room", msg);
```

Verified: `bob got Room: only room`, `cid got Room: only room`; `ann` — nothing.

</details>

## 3. 🟡 A notification from an endpoint

**Task:** on connect add the connection to the group `user:<name>` (the name from the query string). `POST /notify/{user}` must send a notification only to this user.

**Expected result:** `POST /notify/bob` → only `bob` gets the notification.

<details><summary>Hint</summary>

`OnConnectedAsync` + `Context.GetHttpContext()!.Request.Query["user"]`; in the endpoint — `IHubContext<ChatHub>`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 3: push from outside the hub into a per-user group.
public override async Task OnConnectedAsync()
{
    var user = Context.GetHttpContext()!.Request.Query["user"].ToString();
    await Groups.AddToGroupAsync(Context.ConnectionId, "user:" + user);
    await base.OnConnectedAsync();
}

app.MapPost("/notify/{user}", async (string user, IHubContext<ChatHub> hub) =>
{
    await hub.Clients.Group("user:" + user).SendAsync("Notification", "server says hi to " + user);
    return "sent";
});
```

Verified: `server push: sent`, `bob got Notification: server says hi to bob`; the others received nothing.

</details>

## 4. 🔴 Errors and a stream

**Task:** add a method `Friendly` throwing `HubException("Name must not be empty")`, a method `Boom` throwing `InvalidOperationException`, and a method `Count(n)` with `IAsyncEnumerable<int>`. What does the client see?

**Expected result:** `Boom` — a generic message; `Friendly` — with the text; `Count(5)` → `1,2,3,4,5`.

<details><summary>Hint</summary>

`[EnumeratorCancellation] CancellationToken ct` in the streaming method; on the client — `StreamAsync<int>("Count", 5)`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 4: HubException is the only exception whose message reaches the client.
public void Boom() => throw new InvalidOperationException("secret internal detail");
public void Friendly() => throw new HubException("Name must not be empty");
public async IAsyncEnumerable<int> Count(int n, [EnumeratorCancellation] CancellationToken ct)
{
    for (var i = 1; i <= n; i++) { yield return i; await Task.Delay(10, ct); }
}
```

Verified: `plain exception: An unexpected error occurred invoking 'Boom' on the server.`; `HubException: … HubException: Name must not be empty`; `stream: 1,2,3,4,5`.

</details>
