---
title: "Real-time через SignalR"
lang: ru
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

# Real-time через SignalR

🌐 **Русский** · [English](overview.en.md)

## Коротко

Рецепт: отправлять данные клиентам без их запроса — уведомления, чат, живые обновления. SignalR держит постоянное соединение (WebSockets с откатом на SSE и Long Polling), хаб принимает вызовы клиентов и вызывает методы клиентов, группы выбирают аудиторию, `IHubContext` позволяет слать из любого места. Для нескольких серверов нужен backplane. JavaScript-клиент, Redis и Azure SignalR Service в этом материале не запускались.

## Ключевые идеи

- `AddSignalR()` + `MapHub<T>()`; клиенты — `HubConnection`.
- Хаб создаётся на каждый вызов — состояния в нём нет.
- `Clients.All/Group/User`; группа на пользователя — решает несколько вкладок.
- `IHubContext<T>` — отправка из эндпоинтов и воркеров.
- Ошибки скрыты, кроме `HubException`.
- Потоки: `IAsyncEnumerable<T>` и `StreamAsync`.
- `WithAutomaticReconnect` и восстановление состояния; доставка не гарантируется.
- Scale-out: backplane (Redis) или Azure SignalR Service.

## Минимальный пример

```csharp
builder.Services.AddSignalR();
app.MapHub<ChatHub>("/chat");

class ChatHub : Hub
{
    public Task Send(string user, string message) => Clients.All.SendAsync("Message", user, message);
}
```

## Когда применять, а когда нет

- ✅ Уведомления, чаты, дашборды в реальном времени, совместное редактирование.
- ✅ Прогресс долгих операций.
- ✅ Двусторонний обмен с клиентом.
- ❌ Гарантированная доставка (нужна очередь/история).
- ❌ Редкие обновления, где хватает периодического опроса.
- ❌ Большие бинарные передачи (загрузка файлов — [другой рецепт](../file-upload/overview.ru.md)).

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Минимум](basics.ru.md) | хаб, клиент, группы, `IHubContext` |
| [2. Production](advanced.ru.md) | жизненный цикл, ошибки, потоки, транспорты, переподключение, авторизация |
| [3. Масштаб и грани](expert.ru.md) | backplane, sticky sessions, подвохи, 10 вопросов с собеседований |
| [4. Самопроверка](quiz.ru.md) | 10 вопросов и [4 упражнения](exercises.ru.md) с решениями |
| 5. Запускаемые примеры | не сгенерирован — `/learn csharp signalr-realtime level 5` |
| 6. Тесты | не сгенерирован — `/learn csharp signalr-realtime level 6` |

## Связанные материалы

- Пререквизиты: [10 · async/await](../../modules/10-async-await/overview.ru.md), [14 · ASP.NET Core: Minimal API](../../modules/14-aspnet-core-minimal-api/overview.ru.md)
- Рядом: [Фоновые задачи](../background-jobs/overview.ru.md), [JWT-аутентификация](../jwt-auth/overview.ru.md), [Rate limiting](../rate-limiting/overview.ru.md)
- В других треках: [Python](../../../python/overview.ru.md) — WebSockets в asyncio
- Рецепты и библиотеки: [gRPC-сервис](../grpc-service/overview.ru.md)
