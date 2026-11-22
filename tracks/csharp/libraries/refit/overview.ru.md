---
title: "Refit"
lang: ru
track: csharp
kind: library
doc: overview
slug: refit
part: 2
levels: [1, 2, 3, 4]
tags: [refit, http-client, rest, source-generator, json-context]
versions: ".NET 11, C# 15; Refit 17.0.0"
status: draft
updated: 2026-11-22
---

# Refit

🌐 **Русский** · [English](overview.en.md)

## Коротко

Refit превращает интерфейс C# с атрибутами в типизированный HTTP-клиент. Реализацию строит source generator, в версии 17 без рефлексии: JSON идёт через source-generated `JsonSerializerContext`. Ошибки HTTP — `ApiException` или `IApiResponse<T>`; регистрация в DI через `Refit.HttpClientFactory` позволяет навесить обработчики устойчивости. Поведение проверено на локальном сервере.

## Ключевые идеи

- Атрибуты `[Get]`, `[Post]`, `[Body]`, `[Header]`, `[AliasAs]`; параметры вне пути уходят в запрос.
- Refit 17: `RestService.ForGenerated<T>` / `AddRefitGeneratedClient<T>`; `For`/`AddRefitClient` — в `Refit.Reflection`.
- Нужен `RefitSettings.ForJsonContext(...)`; для camelCase API — `JsonSerializerDefaults.Web`, иначе молчаливые нули.
- `ApiException` на 4xx/5xx; `IApiResponse<T>` — без исключений.
- Устойчивость — на `IHttpClientBuilder`, не в Refit.
- Интерфейс удобно подменять в тестах.

## Минимальный пример

```csharp
interface IUsersApi { [Get("/users/{id}")] Task<UserDto> GetUserAsync(int id); }

var api = RestService.ForGenerated<IUsersApi>(
    new HttpClient { BaseAddress = new Uri(baseUrl) }, RefitSettings.ForJsonContext(ApiJson.Default));
```

## Когда применять, а когда нет

- ✅ Много вызовов к REST-API с постоянным контрактом.
- ✅ Нужна типизация и меньше шаблонного кода.
- ✅ Совместимость с trimming/Native AOT (генерируемый клиент).
- ❌ Один-два разовых запроса — хватит `HttpClient`.
- ❌ Нужен полный контроль над каждым запросом/потоком.
- ❌ Контракт задан OpenAPI и лучше сгенерировать клиента из спецификации.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Основы](basics.ru.md) | интерфейс, создание клиента, JSON-контекст, ошибки |
| [2. Продвинутый](advanced.ru.md) | `IApiResponse`, DI, заголовки, сериализация, тесты |
| [3. Эксперт](expert.ru.md) | устройство генератора, подвохи Refit 17, альтернативы, 11 вопросов с собеседований |
| [4. Самопроверка](quiz.ru.md) | 10 вопросов и [4 упражнения](exercises.ru.md) с решениями |
| 5. Запускаемые примеры | не сгенерирован — `/learn csharp refit level 5` |
| 6. Тесты | не сгенерирован — `/learn csharp refit level 6` |

## Связанные материалы

- Пререквизиты: [10 · async/await](../../modules/10-async-await/overview.ru.md), [05 · Интерфейсы, структуры, records](../../modules/05-interfaces-structs-records/overview.ru.md)
- Рядом: [Устойчивый HttpClient](../../recipes/http-client-resilience/overview.ru.md), [Polly](../polly/overview.ru.md), [NSubstitute](../nsubstitute/overview.ru.md)
- В других треках: [Python](../../../python/overview.ru.md) — httpx
