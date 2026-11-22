---
title: "Refit: экспертный уровень"
lang: ru
track: csharp
kind: library
doc: expert
slug: refit
part: 2
tags: [refit, source-generator, native-aot, json-serialization, interview-questions]
versions: ".NET 11, C# 15; Refit 17.0.0"
status: draft
updated: 2026-11-22
---

# Refit: экспертный уровень

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Основы](basics.ru.md) · [2. Продвинутый](advanced.ru.md) · **3. Эксперт** · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [основы](basics.ru.md) и [продвинутый](advanced.ru.md). **Время:** ~30 мин.

## Внутреннее устройство

В Refit 17 реализация интерфейса **генерируется на этапе компиляции** (Roslyn source generator, нужен Roslyn 4.8+ и `PackageReference`): для каждого метода генератор создаёт код сборки `HttpRequestMessage` и отправки через общий исполнитель (`RequestExecutionHelpers`). Фабрики сгенерированных клиентов регистрируются в `RestService`; `RestService.ForGenerated<T>(HttpClient, RefitSettings)` находит фабрику и создаёт клиента без рефлексии. Благодаря этому клиент совместим с trimming и Native AOT, а ошибки в интерфейсе (неверный путь, дублирующие атрибуты) ловят анализаторы на этапе сборки.

Старый путь (`RestService.For<T>` и построение запросов через рефлексию) вынесен в пакет `Refit.Reflection` — для тех, кому нужна совместимость со старым поведением.

> ⚠️ Проверить: детали работы генератора, анализаторов и пакета `Refit.Reflection` взяты из README пакета и не воспроизводились запуском (кроме отсутствия `RestService.For` в одном `Refit` — проверено CS0117).

## Грани и подвохи

**1. JSON без контекста не работает.** Проверено: `NotSupportedException` («no type metadata») внутри `ApiException`.

**2. Регистр по умолчанию.** Проверено: контекст без `Web`-настроек вернул нули при camelCase-ответе — без ошибки.

**3. Исключения на 4xx/5xx.** Проверено: `ApiException` с `StatusCode` и `Content`; `IApiResponse<T>` — альтернатива без исключений.

**4. Параметры не из пути — в запросе.** Проверено: `name=ann&page=2`; для POST не путайте их с телом — тело задаёт `[Body]`.

**5. Версия 17 ломает старые инструкции.** `RestService.For<T>` и `AddRefitClient<T>` из старых статей в одном `Refit` не доступны (CS0117/CS1061, проверено) — используйте `ForGenerated`/`AddRefitGeneratedClient` или подключите `Refit.Reflection`.

**5a. Таймауты и повторные запросы.** Сам Refit повторы не делает: для ретраев используйте обработчик `IHttpClientFactory` ([Polly](../polly/overview.ru.md)); тела запросов должны быть повторяемыми.

**6. Один клиент — один базовый адрес.** `BaseAddress` задаётся на `HttpClient`; для нескольких окружений — несколько регистраций.

**7. Интерфейсы и generic-методы.** Ограничения на дженерики и сложные сигнатуры диагностируются анализатором — читайте сообщения компилятора.

**8. Токены в заголовках.** Статический `[Header]` попадёт в логи при неосторожном логировании запросов — маскируйте.

**9. Тестируемость.** Интерфейс легко подменить, но это не проверяет сборку запросов — для этого нужны интеграционные тесты с локальным сервером (как в примерах) или `Refit.Testing`.

## Компромиссы и альтернативы

| Подход | Плюсы | Минусы |
|---|---|---|
| Refit | меньше шаблонного кода, типизированный контракт | зависимость, свои правила (JSON-контекст) |
| `HttpClient` + `System.Net.Http.Json` | никаких зависимостей | вручную пути, параметры, ошибки |
| Типизированный клиент вручную | полный контроль | много кода на каждый метод |
| Сгенерированный клиент из OpenAPI (NSwag, Kiota) | контракт из спецификации | генерация, большие модели |
| gRPC | строгий контракт, стримы | другой протокол ([gRPC](../../recipes/grpc-service/overview.ru.md)) |

## Вопросы с собеседований

<details><summary>1. Что такое Refit?</summary>

Библиотека, превращающая интерфейс с атрибутами в типизированный HTTP-клиент; реализацию строит source generator.

</details>

<details><summary>2. Как Refit отправляет параметры, которых нет в пути?</summary>

В строку запроса (проверено: `name=ann&page=2`); тело — только параметр с `[Body]`.

</details>

<details><summary>3. Как обработать 404 без исключения?</summary>

Вернуть `Task<IApiResponse<T>>` и смотреть `IsSuccessStatusCode`/`StatusCode`/`Error` (проверено).

</details>

<details><summary>4. Что бросает Refit на 4xx/5xx по умолчанию?</summary>

`ApiException` с `StatusCode` и `Content` (проверено).

</details>

<details><summary>5. Как зарегистрировать клиент в DI?</summary>

`AddRefitGeneratedClient<T>(settings)` (Refit.HttpClientFactory) и настроить `IHttpClientBuilder` (проверено); в старых версиях — `AddRefitClient<T>`.

</details>

<details><summary>6. Почему в Refit 17 нужен `JsonSerializerContext`?</summary>

Клиент не использует рефлексию: сериализация идёт через source-generated метаданные; без них — `NotSupportedException` (проверено). Это даёт совместимость с AOT.

</details>

<details><summary>7. Почему поля десериализовались нулями, хотя ошибки нет?</summary>

Контекст по умолчанию регистрозависим, а сервер отдаёт camelCase; нужен `JsonSerializerDefaults.Web` (проверено).

</details>

<details><summary>8. Как добавить авторизацию?</summary>

`[Header("Authorization")]` в параметре (проверено), либо `DelegatingHandler`/провайдер авторизации.

</details>

<details><summary>9. Подвох: пример из статьи с `RestService.For<IApi>()` не компилируется в Refit 17. Почему?</summary>

Рефлексивный путь вынесен в `Refit.Reflection`; в основном пакете — `ForGenerated` (проверено CS0117).

</details>

<details><summary>10. Подвох: добавили Refit-клиент, а ретраев нет. Откуда им взяться?</summary>

Refit их не делает; добавьте обработчик устойчивости на `IHttpClientBuilder` ([Устойчивый HttpClient](../../recipes/http-client-resilience/overview.ru.md)).

</details>

<details><summary>11. Как тестировать код, зависящий от Refit-клиента?</summary>

Подменить интерфейс (NSubstitute) для юнит-тестов; сборку запросов проверять интеграционно или `Refit.Testing`.

</details>

## Связанные материалы

- Назад: [продвинутый](advanced.ru.md). Проверьте себя: [квиз](quiz.ru.md) и [упражнения](exercises.ru.md).
- [20 · Reflection и source generators](../../modules/20-reflection-and-source-generators/overview.ru.md)
- [Устойчивый HttpClient](../../recipes/http-client-resilience/overview.ru.md)
- [gRPC-сервис](../../recipes/grpc-service/overview.ru.md) — альтернатива для межсервисного обмена.

## Источники

- [Refit README](https://github.com/reactiveui/refit)
- [Refit: Client creation](https://www.reactiveui.net/documentation/refit/clients/creation/)
- [Refit: AOT](https://www.reactiveui.net/documentation/refit/aot/)
