---
title: "Refit: продвинутый уровень"
lang: ru
track: csharp
kind: library
doc: advanced
slug: refit
part: 2
tags: [refit, iapiresponse, dependency-injection, httpclientfactory, resilience]
versions: ".NET 11, C# 15; Refit 17.0.0, Refit.HttpClientFactory 17.0.0"
status: draft
updated: 2026-11-22
---

# Refit: продвинутый уровень

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [основы](basics.ru.md). **Время:** ~45 мин.

## Ответ без исключений

Возвращаемый тип `Task<IApiResponse<T>>` даёт статус, заголовки и содержимое, не бросая исключение на 4xx/5xx:

```csharp
[Get("/users/{id}")] Task<IApiResponse<UserDto>> GetUserResponseAsync(int id);

var resp = await api.GetUserResponseAsync(404);
// IsSuccessStatusCode=False, StatusCode=404, Content=null, Error=ApiException
var ok = await api.GetUserResponseAsync(5);       // ok.Content = UserDto { Id = 5, Name = User5 }
```

Проверено: на `404` — `IsSuccessStatusCode = False`, `StatusCode = 404`, `Content = null`, `Error` — `ApiException`; на успехе `Content` заполнен. Такой подход удобен, когда 404 — штатная ситуация, а не исключение.

## Внедрение зависимостей

```csharp
services.AddRefitGeneratedClient<IUsersApi>(RefitSettings.ForJsonContext(ApiJson.Default))
        .ConfigureHttpClient(c => c.BaseAddress = new Uri(baseUrl));

var api = sp.GetRequiredService<IUsersApi>();
```

Проверено: клиент из DI работает (`UserDto { Id = 9, Name = User9 }`). `AddRefitGeneratedClient` возвращает `IHttpClientBuilder` — к нему подключают всё из мира `IHttpClientFactory`: `ConfigureHttpClient`, `AddHttpMessageHandler`, `AddStandardResilienceHandler()` ([HttpClient](../../recipes/http-client-resilience/overview.ru.md)).

> В версиях до 17 то же делал `AddRefitClient<T>()`; в 17 он находится в пакете `Refit.Reflection`.

> ⚠️ Проверить: цепочку `AddRefitGeneratedClient<T>(...).AddStandardResilienceHandler()` и `DelegatingHandler` для токена в этом материале не запускали.

## Заголовки и авторизация

- Статические: `[Headers("User-Agent: demo")]` на интерфейсе/методе.
- Динамические: `[Header("Authorization")] string auth` (проверено: `Bearer tok` дошёл до сервера).
- Токен «на каждый запрос»: `DelegatingHandler` в `AddHttpMessageHandler` или провайдер авторизации Refit.

## Формы, строка запроса, тело

| Что | Как |
|---|---|
| JSON-тело | `[Body] CreateUser user` (проверено) |
| Форма | `[Body(BodySerializationMethod.UrlEncoded)] Dictionary<string,string>` (проверено: `1|2`) |
| Параметры запроса | все параметры, которых нет в шаблоне пути (проверено: `name=ann&page=2`) |
| Другие имена | `[AliasAs("name")]` |

Загрузку файлов (multipart), стримы и коллекции в запросе см. в документации Refit.

> ⚠️ Проверить: multipart-загрузку, коллекции в строке запроса и потоковые ответы в этом материале не запускали.

## Сериализация

В Refit 17 основной путь — `System.Text.Json` с source-generated контекстом: меньше аллокаций, совместимость с trimming/Native AOT. Настройки передаются в `RefitSettings.ForJsonContext(...)`. Для Newtonsoft.Json и XML есть пакеты `Refit.Newtonsoft.Json`, `Refit.Xml`.

> ⚠️ Проверить: Newtonsoft/XML-сериализаторы и поведение под Native AOT в этом материале не запускали.

## Тестирование

Клиент — интерфейс, поэтому в юнит-тестах его подменяют ([NSubstitute](../nsubstitute/overview.ru.md)). Для проверки самой сборки запросов есть пакет `Refit.Testing` (подмена ответов и проверка вызовов).

> ⚠️ Проверить: `Refit.Testing` в этом материале не запускали.

## Best practices

| Правило | Почему |
|---|---|
| JSON-контекст с `JsonSerializerDefaults.Web` для camelCase API | иначе молчаливые нули |
| `IApiResponse<T>` там, где 4xx — норма | без исключений в потоке управления |
| Клиент через DI и `IHttpClientFactory` | пулы соединений, DNS |
| Устойчивость — обработчиком, не в интерфейсе | единое место политики |
| Один интерфейс — один внешний сервис | ясные границы |
| Мокать интерфейс в тестах | быстрые тесты |

## Связанные материалы

- Назад: [основы](basics.ru.md). Дальше: [эксперт](expert.ru.md).
- [Устойчивый HttpClient](../../recipes/http-client-resilience/advanced.ru.md) — `AddStandardResilienceHandler`.
- [NSubstitute](../nsubstitute/overview.ru.md) — подмена клиента в тестах.
- [13 · DI, конфигурация, логирование](../../modules/13-di-config-logging/overview.ru.md)

## Источники

- [Refit: Dependency injection](https://www.reactiveui.net/documentation/refit/clients/dependency-injection/)
- [Refit: Responses](https://www.reactiveui.net/documentation/refit/results/responses/)
- [Refit: Errors](https://www.reactiveui.net/documentation/refit/results/errors/)
