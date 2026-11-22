---
title: "Real-time через SignalR: минимальное решение"
lang: ru
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

# Real-time через SignalR: минимальное решение

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Минимум** · [2. Production](advanced.ru.md) · [3. Масштаб и грани](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [10 · async/await](../../modules/10-async-await/overview.ru.md), [14 · ASP.NET Core: Minimal API](../../modules/14-aspnet-core-minimal-api/overview.ru.md). **Время:** ~30 мин.

## Задача

Отправлять данные клиенту **без запроса с его стороны**: уведомления, чат, обновление статуса, живые графики. Обычный HTTP работает по схеме «запрос — ответ»; SignalR поддерживает постоянное соединение (WebSockets, при необходимости — запасные транспорты) и даёт высокоуровневый API: сервер вызывает методы клиента, клиент — методы сервера.

## Хаб

Хаб (`Hub`) — класс, методы которого вызывают клиенты; через `Clients` хаб вызывает методы клиентов:

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

## Клиент (.NET)

```csharp
var conn = new HubConnectionBuilder().WithUrl(url).Build();
conn.On<string, string>("Message", (u, m) => Console.WriteLine($"{u}: {m}"));
await conn.StartAsync();
await conn.InvokeAsync("Send", "ann", "hello all");
```

Для браузера используют JavaScript-клиент `@microsoft/signalr`; API аналогичен (`on`, `invoke`, `start`).

> ⚠️ Проверить: JavaScript-клиент в этом материале не запускался; сверьтесь с документацией клиента.

Проверено запуском (три .NET-клиента `ann`, `bob`, `cid` и сервер в одном процессе):

| Действие | Кто получил |
|---|---|
| `ann` вызвала `Send` (`Clients.All`) | все трое |
| `bob` и `cid` вошли в группу `room1`, `ann` вызвала `ToRoom("room1", …)` | только `bob` и `cid` |
| хаб-метод `Add(2, 3)` с возвращаемым значением | клиент получил `5` |

Соединение после `StartAsync` в состоянии `Connected`, у него есть `ConnectionId`.

## Отправка не из хаба

Хаб живёт только на время вызова. Чтобы отправить сообщение из любого места (эндпоинта, фонового сервиса), внедряют `IHubContext<THub>`:

```csharp
app.MapPost("/notify/{user}", async (string user, IHubContext<ChatHub> hub) =>
{
    await hub.Clients.Group("user:" + user).SendAsync("Notification", "server says hi to " + user);
    return "sent";
});
```

Проверено: POST `/notify/bob` → уведомление получил только `bob` (он состоит в группе `user:bob`, куда добавляется при подключении в `OnConnectedAsync`).

## Частые ошибки новичка

- ❌ Хранить состояние в полях хаба → ✅ экземпляр хаба создаётся на каждый вызов; состояние — в сервисах/БД.
- ❌ Слать пользователю по `ConnectionId` → ✅ у пользователя может быть несколько соединений; используйте группы/`Clients.User`.
- ❌ Опрашивать сервер таймером, когда нужен push → ✅ SignalR.
- ❌ Доверять входящим сообщениям без проверки → ✅ валидировать, как любой ввод.
- ❌ Отправлять секреты в сообщении → ✅ клиент может быть чужим.

## Связанные материалы

- Дальше: [production](advanced.ru.md) — группы пользователей, ошибки, потоки, переподключение, авторизация.
- [14 · ASP.NET Core: Minimal API](../../modules/14-aspnet-core-minimal-api/overview.ru.md) — маршрутизация и хостинг.
- [10 · async/await](../../modules/10-async-await/overview.ru.md) — асинхронные методы хаба.
- [Фоновые задачи](../background-jobs/overview.ru.md) — источник событий для рассылки.
- [JWT-аутентификация](../jwt-auth/overview.ru.md) — аутентификация подключений.
- [Python](../../../python/overview.ru.md) — WebSockets в asyncio.

## Источники

- [Introduction to ASP.NET Core SignalR](https://learn.microsoft.com/aspnet/core/signalr/introduction)
- [Hubs in SignalR](https://learn.microsoft.com/aspnet/core/signalr/hubs)
- [Send messages from outside a hub](https://learn.microsoft.com/aspnet/core/signalr/hubcontext)
- [ASP.NET Core SignalR .NET Client](https://learn.microsoft.com/aspnet/core/signalr/dotnet-client)
