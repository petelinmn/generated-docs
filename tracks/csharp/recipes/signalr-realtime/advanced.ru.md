---
title: "Real-time через SignalR: production"
lang: ru
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

# Real-time через SignalR: production

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Минимум](basics.ru.md) · **2. Production** · [3. Масштаб и грани](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [минимальное решение](basics.ru.md). **Время:** ~45 мин.

## Группы пользователей и жизненный цикл соединения

Один пользователь может иметь несколько соединений (вкладки, устройства). Практичный приём — при подключении добавлять соединение в группу пользователя:

```csharp
public override async Task OnConnectedAsync()
{
    var user = Context.GetHttpContext()!.Request.Query["user"].ToString();
    await Groups.AddToGroupAsync(Context.ConnectionId, "user:" + user);
    await base.OnConnectedAsync();
}
public override Task OnDisconnectedAsync(Exception? e) => base.OnDisconnectedAsync(e);
```

Проверено: журнал событий `connected ann | connected bob | connected cid | disconnected cid (clean)` — после `StopAsync` клиента `OnDisconnectedAsync` вызван с `null` (чистое отключение). Группы в `SignalR` не нужно удалять вручную: при разрыве соединение исчезает из групп само.

> В реальном приложении имя пользователя берут не из строки запроса, а из аутентифицированного `Context.User` (см. [авторизация](#авторизация)): в примере строка запроса — упрощение для теста.

## Ошибки хаба

Исключение внутри метода хаба клиент по умолчанию получает **без подробностей**. Проверено:

| Что бросает метод | Что видит клиент |
|---|---|
| `InvalidOperationException("secret internal detail")` | `An unexpected error occurred invoking 'Boom' on the server.` — детали скрыты |
| `HubException("Name must not be empty")` | `…invoking 'Friendly' on the server. HubException: Name must not be empty` — сообщение доходит |

Бросайте `HubException` для ошибок, которые клиент должен увидеть (валидация), а остальное логируйте на сервере. `EnableDetailedErrors = true` показывает внутренние сообщения — только для разработки.

## Потоки с сервера

Хаб-метод может возвращать `IAsyncEnumerable<T>` — клиент читает поток по мере появления данных:

```csharp
public async IAsyncEnumerable<int> Count(int n, [EnumeratorCancellation] CancellationToken ct)
{
    for (var i = 1; i <= n; i++) { yield return i; await Task.Delay(10, ct); }
}
// client
await foreach (var x in conn.StreamAsync<int>("Count", 5)) { /* 1,2,3,4,5 */ }
```

Проверено: клиент получил `1,2,3,4,5`.

## Транспорты

SignalR выбирает транспорт сам: WebSockets → Server-Sent Events → Long Polling. Проверено, что соединение устанавливается на каждом из трёх (WebSockets, ServerSentEvents, LongPolling), если принудительно указать `o.Transports`. За прокси убедитесь, что он пропускает WebSockets.

## Переподключение

```csharp
var conn = new HubConnectionBuilder().WithUrl(url).WithAutomaticReconnect().Build();
conn.Reconnecting += e => { /* show "reconnecting…" */ return Task.CompletedTask; };
conn.Reconnected += id => { /* re-subscribe, refresh state */ return Task.CompletedTask; };
conn.Closed += e => { /* give up or restart manually */ return Task.CompletedTask; };
```

Проверено: после остановки сервера у клиента с `WithAutomaticReconnect` сработало событие `Reconnecting`, состояние — `Reconnecting`.

> ⚠️ Проверить: итоговый переход в `Closed` после исчерпания попыток и повторное подключение к вернувшемуся серверу не проверялись; также помните, что после переподключения `ConnectionId` новый и серверные группы нужно восстанавливать (например, в `OnConnectedAsync`).

## Авторизация

`[Authorize]` на хабе или методе требует аутентифицированного пользователя; для браузерных WebSocket токен обычно передают в строке запроса (`access_token`), потому что заголовки WebSocket недоступны из JS. Подробности токенов — [JWT-аутентификация](../jwt-auth/overview.ru.md).

> ⚠️ Проверить: конфигурация JWT для SignalR (чтение `access_token` из запроса в `OnMessageReceived`) в этом материале не запускалась.

## Best practices

| Правило | Почему |
|---|---|
| Хаб тонкий, логика в сервисах | хаб создаётся на каждый вызов |
| `IHubContext` для отправки извне | из эндпоинтов и воркеров |
| Группа на пользователя | несколько соединений |
| `HubException` для ошибок клиента | остальное скрыто |
| Переподключение и восстановление состояния | сети нестабильны |
| Небольшие сообщения | не гонять большие данные по сокету |
| Ограничить размеры и частоту | защита от злоупотреблений ([Rate limiting](../rate-limiting/overview.ru.md)) |

## Связанные материалы

- Назад: [минимум](basics.ru.md). Дальше: [масштаб и грани](expert.ru.md).
- [JWT-аутентификация](../jwt-auth/overview.ru.md) — токены для подключений.
- [22 · Наблюдаемость](../../modules/22-observability/overview.ru.md) — число соединений и сообщения.
- [Rate limiting](../rate-limiting/overview.ru.md) — ограничение частоты.

## Источники

- [SignalR hubs: error handling](https://learn.microsoft.com/aspnet/core/signalr/hubs#handle-errors)
- [Streaming in SignalR](https://learn.microsoft.com/aspnet/core/signalr/streaming)
- [SignalR configuration: transports](https://learn.microsoft.com/aspnet/core/signalr/configuration)
- [SignalR authentication and authorization](https://learn.microsoft.com/aspnet/core/signalr/authn-and-authz)
