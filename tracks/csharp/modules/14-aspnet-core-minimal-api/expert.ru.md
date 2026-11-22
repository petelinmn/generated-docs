---
title: "ASP.NET Core: Minimal API: экспертная теория"
lang: ru
track: csharp
kind: module
doc: expert
slug: 14-aspnet-core-minimal-api
part: 2
tags: [aspnet-core, minimal-api, kestrel, endpoint-routing, request-delegate, native-aot, content-negotiation, interview-questions]
versions: ".NET 11, C# 15; ASP.NET Core 11.0"
status: draft
updated: 2026-11-22
---

# ASP.NET Core: Minimal API: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · [2. Продвинутая](advanced.ru.md) · **3. Экспертная** · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [базовая](basics.ru.md) и [продвинутая](advanced.ru.md) теория. **Время:** ~1,5 ч.

## Как это устроено

`WebApplication` объединяет три роли: **хост** ([универсальный хост](../13-di-config-logging/overview.ru.md)), построитель конвейера (`IApplicationBuilder`) и построитель маршрутов (`IEndpointRouteBuilder`). Сервер — **Kestrel**: он принимает соединения, разбирает HTTP и для каждого запроса вызывает конвейер — цепочку делегатов `RequestDelegate` (`HttpContext → Task`). Каждый `app.Use(...)` оборачивает следующий делегат; поэтому порядок регистрации — порядок выполнения «туда», а возврат идёт в обратном порядке.

**Маршрутизация конечных точек.** `MapGet` и подобные регистрируют `Endpoint` — пару «шаблон маршрута + `RequestDelegate` + метаданные». Шаблоны компилируются в автомат для сопоставления; метаданные (ограничения методов, `Consumes`, авторизация, имя) участвуют в выборе. Для Minimal API middleware маршрутизации добавляется неявно, а исполнение выбранной точки встроено в конвейер в конце.

**Из лямбды в `RequestDelegate`.** Ваш обработчик — произвольная сигнатура, поэтому при построении точки `RequestDelegateFactory` анализирует параметры и **генерирует** `RequestDelegate`: код привязки (маршрут, запрос, тело, сервисы), вызов вашего делегата и запись результата. Генерация идёт один раз на старте точки (на деревьях выражений); в Native AOT вместо неё работает генератор исходного кода на этапе компиляции (Request Delegate Generator), чтобы обойтись без динамического кода.

### Правила привязки и коды ответов

Источник параметра определяется порядком проверок: атрибут (`[FromRoute]`, `[FromQuery]`, `[FromBody]`, `[FromServices]`, `[FromHeader]`) → особые типы (`HttpContext`, `CancellationToken` — это `RequestAborted`) → имя в шаблоне маршрута → зарегистрированный сервис → простой тип из строки запроса → сложный тип из тела. Проверено:

| Запрос | Ответ |
|---|---|
| `POST /todos` с `Content-Type: text/plain` | `415 Unsupported Media Type` |
| `POST /todos` с битым JSON | `400` |
| `POST /todos` без тела | `400` |
| `GET /search?page=abc` (параметр `int`) | `400` |
| `GET /items/abc` (ограничение `:int`) | `404` |
| `PUT /` при только `MapGet("/")` | `405` |
| `HEAD` для `MapGet` | `405` (в проверенной версии) |
| путь в другом регистре (`/WHO` вместо `/who`) | находится: сопоставление пути без учёта регистра |
| два одинаковых `MapGet("/a")` | приложение стартует, а запрос к `/a` — `500` (неоднозначное совпадение) |

С `MapFallback` несовпавшие запросы (в том числе `text/plain` вместо JSON и несовпавший метод) попадают в запасной обработчик — проверено: вместо `415` пришёл ответ `fallback`.

**JSON.** Используется `System.Text.Json` с настройками веб-значений: имена в camelCase, регистронезависимый разбор. Анонимные типы, записи и обычные классы сериализуются без атрибутов. Для Native AOT нужен `JsonSerializerContext` (генератор), иначе сериализация через рефлексию недоступна.

## Конвейер и порядок

Рекомендуемый порядок встроенных middleware: обработка исключений → HTTPS-перенаправление/HSTS → статические файлы → маршрутизация → CORS → аутентификация → авторизация → ваши middleware → конечные точки. Правила: обработчик ошибок первым (он должен обернуть всё), аутентификация перед авторизацией, CORS до авторизации. Проверено: с `UseExceptionHandler()` первым и пользовательским middleware корреляции заголовок `X-Request-Id` присутствует в ответе.

Фильтры конечных точек и middleware различаются по уровню: middleware видит `HttpContext` каждого запроса, фильтр — типизированные **аргументы** уже выбранной точки и результат; фильтр может подменить результат и работает на группе/точке.

## Справочник

| Что | API |
|---|---|
| Методы | `MapGet/Post/Put/Delete/Patch`, `MapMethods`, `Map` |
| Группы | `MapGroup`, `WithTags`, `RequireAuthorization`, `AddEndpointFilter` |
| Результаты | `Results.*`, `TypedResults.*`, `Results<T1, T2, …>` |
| Привязка | `[FromRoute/Query/Header/Body/Services/Form]`, `[AsParameters]` |
| Метаданные | `WithName`, `WithSummary`, `Produces<T>`, `Accepts<T>`, `WithOpenApi` |
| Ошибки | `UseExceptionHandler`, `IExceptionHandler`, `AddProblemDetails`, `UseStatusCodePages` |
| Валидация | `AddValidation` (.NET 10+), DataAnnotations, фильтры |
| Документация | `AddOpenApi`, `MapOpenApi` |
| AOT | `CreateSlimBuilder`, `CreateEmptyBuilder`, `JsonSerializerContext` |

## Подвохи

**1. Порядок middleware.** `UseExceptionHandler` после своего middleware не перехватит его исключения; `UseAuthorization` без `UseAuthentication` не имеет пользователя.

**2. Два одинаковых маршрута не ошибка при старте.** Приложение стартует, а запрос падает с `500` (проверено).

**3. `HEAD` не работает на `MapGet` автоматически** (проверено: `405`); для него нужен `MapMethods` или ручная точка.

**4. Фильтры не видят запросы, не дошедшие до точки** (`404`, `405`, `415`) — для них нужны middleware или `UseStatusCodePages`.

**5. Сервис-одиночка с областью.** Параметр-сервис в обработчике берётся из области запроса: `Scoped`-сервисы работают корректно ([13 · DI](../13-di-config-logging/overview.ru.md)); внедрять scoped в singleton-middleware нельзя.

**6. Привязка по умолчанию — не `[FromBody]` для простых типов.** по документации простой тип вроде `string title` ищется в строке запроса, а не в теле; для тела нужен тип-запись или `[FromBody]`.

**7. Возврат сущности EF Core.** Циклы навигационных свойств ломают сериализацию, а лишние поля утекают клиенту — отдавайте DTO.

**8. Блокирующий код в обработчике** занимает поток пула ([10 · Async/await](../10-async-await/overview.ru.md)).

## Компромиссы и альтернативы

| Подход | Плюсы | Минусы |
|---|---|---|
| Minimal API | минимум кода, быстрый старт, AOT | нужна своя структура в больших проектах |
| Контроллеры MVC (`[ApiController]`) | конвенции, фильтры, привычная структура | больше церемоний |
| gRPC | бинарный контракт, стриминг | не для браузеров напрямую |
| FastEndpoints, Carter (внешние) | структура «запрос-ответ» поверх Minimal API | внешняя зависимость |

## Вопросы с собеседований

<details><summary>1. Что такое Minimal API и чем он отличается от контроллеров?</summary>

Способ объявлять конечные точки лямбдами (`MapGet` и т. д.) без классов-контроллеров и атрибутов маршрутов. Это тот же конвейер и маршрутизация, но меньше церемоний и быстрый старт; структуру (группы, файлы) строят самостоятельно.

</details>

<details><summary>2. Что такое middleware и почему важен порядок?</summary>

Звено конвейера `RequestDelegate`, которое может действовать до и после следующего. Выполняются в порядке регистрации; например, обработчик ошибок должен стоять первым, чтобы перехватывать исключения остальных.

</details>

<details><summary>3. Как Minimal API определяет, откуда брать параметр?</summary>

Атрибут → особые типы → имя из шаблона маршрута → сервис из DI → простой тип из строки запроса → сложный тип из тела JSON.

</details>

<details><summary>4. Когда придёт 415, а когда 400?</summary>

`415` — несовпадение `Content-Type` с тем, что точка принимает (например, `text/plain` вместо JSON); `400` — невалидный JSON, отсутствующее тело или значение, не приводимое к типу (проверено).

</details>

<details><summary>5. Чем отличаются `Results` и `TypedResults`?</summary>

`Results.*` возвращают `IResult`; `TypedResults.*` — конкретные типы (`Ok<T>`, `NotFound`) и вместе с `Results<…>` дают компилятору и OpenAPI точный список ответов.

</details>

<details><summary>6. Чем фильтр конечной точки отличается от middleware?</summary>

Middleware обрабатывает каждый запрос, видя `HttpContext`; фильтр работает только для выбранной точки/группы, видит типизированные аргументы и может заменить результат.

</details>

<details><summary>7. Как обрабатывать ошибки единообразно?</summary>

`UseExceptionHandler` + `AddProblemDetails` для `500`, `IExceptionHandler` для доменных исключений (`404`, `409`), валидация возвращает `400` в формате ProblemDetails.

</details>

<details><summary>8. Что такое ProblemDetails?</summary>

Стандартный формат тела ошибки HTTP API (RFC 9457): `type`, `title`, `status`, `detail`, дополнительные поля (`errors`, `traceId`).

</details>

<details><summary>9. Как работает привязка тела и какой тип данных у `CancellationToken` в обработчике?</summary>

Сложный тип десериализуется из JSON `System.Text.Json`; `CancellationToken` — это `HttpContext.RequestAborted`: отменяется при обрыве соединения клиентом.

</details>

<details><summary>10. Как готовят Minimal API к Native AOT?</summary>

`CreateSlimBuilder`, исходные генераторы (Request Delegate Generator и `JsonSerializerContext`), отказ от рефлексии и динамически создаваемого кода.

</details>

<details><summary>11. Вопрос с подвохом: почему запрос `POST` с `text/plain` вернул `404`, а не `415`?</summary>

В проверенном приложении был `MapFallback`: точка не подошла по `Content-Type`, и запрос ушёл в запасной обработчик. Без fallback пришёл бы `415`.

</details>

<details><summary>12. Вопрос с подвохом: почему два одинаковых `MapGet("/a")` не падают при старте?</summary>

Неоднозначность обнаруживается только при сопоставлении запроса: приложение стартует, а запрос к `/a` даёт `500` (проверено).

</details>

<details><summary>13. Вопрос с подвохом: попадёт ли текст исключения в ответ клиенту?</summary>

Нет, при `UseExceptionHandler` + ProblemDetails тело содержит только общий заголовок и `traceId` (проверено: «secret details» отсутствует); подробности — в логе.

</details>

## Связанные материалы

- Назад: [продвинутая теория](advanced.ru.md). Проверка: [квиз](quiz.ru.md) и [упражнения](exercises.ru.md).
- [13 · DI, конфигурация, логирование](../13-di-config-logging/overview.ru.md) — хост и контейнер.
- [10 · Async/await](../10-async-await/overview.ru.md) — `CancellationToken`, блокировки.
- [11 · Современный C#](../11-modern-csharp/overview.ru.md) — записи для DTO, `required`.
- [Python](../../../python/overview.ru.md) — FastAPI/ASGI: аналог конвейера и привязки.

## Источники

- [Конвейер запроса](https://learn.microsoft.com/aspnet/core/fundamentals/middleware/)
- [Маршрутизация конечных точек](https://learn.microsoft.com/aspnet/core/fundamentals/routing)
- [Minimal API и Native AOT](https://learn.microsoft.com/aspnet/core/fundamentals/native-aot)
- [Kestrel](https://learn.microsoft.com/aspnet/core/fundamentals/servers/kestrel)
- [ProblemDetails (RFC 9457)](https://www.rfc-editor.org/rfc/rfc9457)
