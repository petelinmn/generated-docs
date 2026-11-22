---
title: "Упражнения: Real-time через SignalR"
lang: ru
track: csharp
kind: recipe
doc: exercises
slug: signalr-realtime
part: 2
tags: [signalr, hub, groups, streaming, ihubcontext]
status: draft
updated: 2026-11-22
---

# Упражнения: Real-time через SignalR

🌐 **Русский** · [English](exercises.en.md)

> Решайте сами, потом открывайте подсказки. Решения проверены запуском: сервер Kestrel и .NET-клиенты `Microsoft.AspNetCore.SignalR.Client` 11.0.0 в одном процессе. Запускаемых версий в `code/` нет (уровня 5 нет).

## 1. 🟢 Рассылка всем

**Задача:** хаб с методом `Send(user, message)`, рассылающий событие `Message` всем. Подключите трёх клиентов и отправьте сообщение от первого.

**Ожидаемый результат:** все три клиента получили `hello all`.

<details><summary>Подсказка</summary>

`Clients.All.SendAsync("Message", user, message)`; на клиенте `conn.On<string, string>("Message", …)`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 1: broadcast to every connected client.
class ChatHub : Hub
{
    public Task Send(string user, string message) => Clients.All.SendAsync("Message", user, message);
}
app.MapHub<ChatHub>("/chat");
```

Проверено: `ann`, `bob`, `cid` получили `Message from ann: hello all`.

</details>

## 2. 🟡 Комната

**Задача:** добавьте `Join(room)` и `ToRoom(room, msg)`. `bob` и `cid` входят в `room1`, `ann` отправляет сообщение в комнату.

**Ожидаемый результат:** получили только `bob` и `cid`.

<details><summary>Подсказка</summary>

`Groups.AddToGroupAsync(Context.ConnectionId, room)` и `Clients.Group(room)`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 2: groups limit the audience.
public Task Join(string room) => Groups.AddToGroupAsync(Context.ConnectionId, room);
public Task ToRoom(string room, string msg) => Clients.Group(room).SendAsync("Room", msg);
```

Проверено: `bob got Room: only room`, `cid got Room: only room`; `ann` — ничего.

</details>

## 3. 🟡 Уведомление из эндпоинта

**Задача:** при подключении добавляйте соединение в группу `user:<имя>` (имя из строки запроса). `POST /notify/{user}` должен отправлять уведомление только этому пользователю.

**Ожидаемый результат:** `POST /notify/bob` → уведомление получает только `bob`.

<details><summary>Подсказка</summary>

`OnConnectedAsync` + `Context.GetHttpContext()!.Request.Query["user"]`; в эндпоинте — `IHubContext<ChatHub>`.

</details>

<details><summary>Решение</summary>

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

Проверено: `server push: sent`, `bob got Notification: server says hi to bob`; остальные ничего не получили.

</details>

## 4. 🔴 Ошибки и поток

**Задача:** добавьте метод `Friendly`, бросающий `HubException("Name must not be empty")`, метод `Boom`, бросающий `InvalidOperationException`, и метод `Count(n)` с `IAsyncEnumerable<int>`. Что увидит клиент?

**Ожидаемый результат:** `Boom` — общее сообщение; `Friendly` — с текстом; `Count(5)` → `1,2,3,4,5`.

<details><summary>Подсказка</summary>

`[EnumeratorCancellation] CancellationToken ct` в потоковом методе; на клиенте — `StreamAsync<int>("Count", 5)`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 4: HubException is the only exception whose message reaches the client.
public void Boom() => throw new InvalidOperationException("secret internal detail");
public void Friendly() => throw new HubException("Name must not be empty");
public async IAsyncEnumerable<int> Count(int n, [EnumeratorCancellation] CancellationToken ct)
{
    for (var i = 1; i <= n; i++) { yield return i; await Task.Delay(10, ct); }
}
```

Проверено: `plain exception: An unexpected error occurred invoking 'Boom' on the server.`; `HubException: … HubException: Name must not be empty`; `stream: 1,2,3,4,5`.

</details>
