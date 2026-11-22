---
title: "ASP.NET Core: Minimal API: продвинутая теория"
lang: ru
track: csharp
kind: module
doc: advanced
slug: 14-aspnet-core-minimal-api
part: 2
tags: [aspnet-core, minimal-api, typed-results, endpoint-filters, validation, problem-details, openapi, exception-handling]
versions: ".NET 11, C# 15; ASP.NET Core 11.0; Microsoft.AspNetCore.OpenApi 11.0.0"
status: draft
updated: 2026-11-22
---

# ASP.NET Core: Minimal API: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [базовая теория](basics.ru.md). **Время:** ~1,5 ч.

## TypedResults и описание ответов

`Results.Ok(...)` возвращает `IResult` — тип не говорит, какие коды возможны. `TypedResults.Ok(...)` возвращает конкретный тип (`Ok<T>`), а объединение `Results<…>` перечисляет **все** варианты ответа — компилятор проверяет, что возвращаются только они, а OpenAPI получает точные коды:

```csharp
app.MapGet("/todos/{id:int}", Results<Ok<Todo>, NotFound> (int id, ITodoStore s) =>
    s.Find(id) is { } t ? TypedResults.Ok(t) : TypedResults.NotFound());
```

(`using Microsoft.AspNetCore.Http.HttpResults;`.) Проверено: ответы `200 {"id":1,...}` и `404`; в сгенерированном OpenAPI-документе для маршрута описаны `200` со схемой `Todo` и `404`. Для `Results.Ok(...)` схема ответа не выводится так точно — используйте `TypedResults`.

### `[AsParameters]`

Группу параметров запроса можно собрать в один тип:

```csharp
record Paging(int Page = 1, int Size = 20);
app.MapGet("/params", ([AsParameters] Paging p) => $"{p.Page}/{p.Size}");   // /params?page=2&size=10 → 2/10
```

(Имена в запросе сопоставляются без учёта регистра: `page` → `Page`; проверено.)

## Фильтры конечных точек

Фильтр оборачивает обработчик конкретной точки или группы и видит его аргументы: годится для валидации и общей логики:

```csharp
app.MapPost("/filtered", (string v) => v).AddEndpointFilter(async (ctx, next) =>
{
    if (ctx.GetArgument<string>(0) == "bad") return Results.BadRequest("filtered out");
    return await next(ctx);
});
```

Проверено: `?v=ok` → `200 ok`, `?v=bad` → `400 "filtered out"`. Отличие от middleware: фильтр работает **после** выбора маршрута и знает типы аргументов; middleware обрабатывает все запросы подряд. Фильтр добавляют и на группу: `group.AddEndpointFilter(...)` — он применится ко всем её точкам.

## Валидация и ProblemDetails

Начиная с .NET 10 Minimal API умеет проверять данные атрибутами `DataAnnotations` автоматически: достаточно включить службу.

```csharp
builder.Services.AddValidation();
builder.Services.AddProblemDetails();

record CreateTodo([property: Required, MinLength(1)] string Title);
```

Проверено: `POST /todos` с `{"title":""}` → `400 application/problem+json` с телом `{"type":"…","title":"One or more validation errors occurred.","status":400,"errors":{"Title":["The Title field is required."]},"traceId":"…"}`. **ProblemDetails** — стандартный формат ошибок HTTP API (RFC 9457): поля `type`, `title`, `status`, `detail`, расширение `errors`. Для сложных правил — фильтр конечной точки или библиотека FluentValidation (ещё не создана).

## Обработка ошибок

`app.UseExceptionHandler()` перехватывает необработанное исключение и возвращает ответ `500` в формате ProblemDetails, **не раскрывая** детали:

```csharp
app.MapGet("/crash", () => { throw new InvalidOperationException("secret details"); return "x"; });
// → 500, the response body does not contain "secret details"
```

Проверено: код `500`, текст исключения в теле отсутствует (`contains secret=False`). Для собственных исключений реализуют `IExceptionHandler`, который превращает доменную ошибку в нужный ответ:

```csharp
builder.Services.AddExceptionHandler<NotFoundHandler>();

class NotFoundHandler(IProblemDetailsService pds) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext ctx, Exception ex, CancellationToken ct)
    {
        if (ex is not NotFoundException) return false;        // not ours: let the next handler try
        ctx.Response.StatusCode = 404;
        return await pds.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = ctx,
            ProblemDetails = new ProblemDetails { Status = 404, Title = "Not found", Detail = ex.Message }
        });
    }
}
```

Проверено: `GET /orders/7` → `404 application/problem+json` с `"detail":"Order 7 not found"`. Вызов `UseExceptionHandler()` ставят **первым** в конвейере, чтобы он покрывал всё остальное.

## Собственные middleware

Middleware можно писать лямбдой, классом или методом расширения. Типичная задача — идентификатор корреляции: взять из заголовка или создать и вернуть в ответе:

```csharp
app.Use(async (ctx, next) =>
{
    var id = ctx.Request.Headers["X-Request-Id"].FirstOrDefault() ?? Guid.NewGuid().ToString("N")[..8];
    ctx.Response.Headers["X-Request-Id"] = id;
    await next();
});
app.Map("/health", () => Results.Text("ok"));   // an endpoint for liveness probes
```

Проверено: заголовок `X-Request-Id` есть в ответе, а переданный клиентом `abc123` вернулся без изменений. Для проверок живости используют конечную точку вроде `/health` (в продакшене — `AddHealthChecks`, [22 · Наблюдаемость](../22-observability/overview.ru.md)).

## OpenAPI

Пакет `Microsoft.AspNetCore.OpenApi` строит описание API из конечных точек:

```csharp
builder.Services.AddOpenApi();
app.MapOpenApi();          // GET /openapi/v1.json
```

Проверено: `GET /openapi/v1.json` → `200 application/json`; документ версии `3.2.0` (значение зависит от версии пакета) содержит все маршруты, параметры пути и запроса (`id` — `integer/int32`, `q` — `string`, `page` — со значением по умолчанию `1`), тело `CreateTodo` с `minLength: 1` и ответы `Todo`. Интерфейс для просмотра (Scalar, Swagger UI) подключают отдельными пакетами.

## Структура большого приложения

| Приём | Зачем |
|---|---|
| `MapGroup("/todos")` | общий префикс, фильтры и политики |
| Статический класс с `MapXxxEndpoints(this IEndpointRouteBuilder)` | разбиение по файлам |
| Отдельные классы-обработчики и сервисы | тестируемость |
| Папка `Endpoints/` + регистрация в `Program.cs` | читаемость |

## Лучшие практики

| Правило | Почему |
|---|---|
| `TypedResults` и `Results<…>` | точный контракт и OpenAPI |
| Валидация на границе (`AddValidation`) | `400` вместо ошибок глубже |
| Единый формат ошибок — ProblemDetails | клиенту проще разбирать |
| `UseExceptionHandler` первым | покрывает весь конвейер |
| `CancellationToken` в обработчиках | отмена при обрыве соединения |
| Не возвращайте сущности БД напрямую | отдельные DTO ([15 · EF Core](../15-ef-core/overview.ru.md)) |

## Связанные материалы

- Назад: [базовая теория](basics.ru.md). Дальше: [экспертная теория](expert.ru.md) — конвейер, привязка, AOT.
- [13 · DI, конфигурация, логирование](../13-di-config-logging/overview.ru.md) — `ValidateOnStart`, сервисы.
- [10 · Async/await](../10-async-await/overview.ru.md) — асинхронные обработчики.
- [08 · Исключения](../08-exceptions/overview.ru.md) — какие ошибки превращать в `4xx`.
- [17 · Тестирование: интеграционные тесты](../17-testing-integration/overview.ru.md) — `WebApplicationFactory`.

## Источники

- [TypedResults](https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/responses#typedresults-vs-results)
- [Фильтры конечных точек](https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/min-api-filters)
- [Валидация в Minimal API](https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/parameter-binding#validation-support-in-minimal-apis)
- [Обработка ошибок](https://learn.microsoft.com/aspnet/core/fundamentals/error-handling)
- [OpenAPI в ASP.NET Core](https://learn.microsoft.com/aspnet/core/fundamentals/openapi/overview)
