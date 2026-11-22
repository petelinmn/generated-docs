---
title: "Устойчивый HttpClient: минимальное решение"
lang: ru
track: csharp
kind: recipe
doc: basics
slug: http-client-resilience
part: 2
tags: [httpclient, ihttpclientfactory, retry, resilience, polly, timeout]
versions: ".NET 11, C# 15; Microsoft.Extensions.Http.Resilience 11.0.1"
status: draft
updated: 2026-11-22
---

# Устойчивый HttpClient: минимальное решение

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Минимум** · [2. Production](advanced.ru.md) · [3. Масштаб и грани](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [10 · async/await](../../modules/10-async-await/overview.ru.md), [13 · DI, конфигурация, логирование](../../modules/13-di-config-logging/overview.ru.md). **Время:** ~30 мин.

## Задача

Приложение вызывает внешний HTTP-сервис. Сеть ненадёжна: сервис отвечает 503, зависает, временно падает. Нужно вызывать его так, чтобы разовые сбои не ломали наш запрос, а долгие сбои не клали всё приложение. В .NET для этого есть `IHttpClientFactory` и пакет `Microsoft.Extensions.Http.Resilience` (построен на Polly v8).

## Минимальное решение

```bash
dotnet add package Microsoft.Extensions.Http.Resilience
```

```csharp
builder.Services.AddHttpClient("api", c => c.BaseAddress = new Uri("https://api.example.com"))
    .AddStandardResilienceHandler();

// usage
var client = factory.CreateClient("api");
var response = await client.GetAsync("/items");
```

`AddStandardResilienceHandler()` вставляет в цепочку готовый конвейер: общий таймаут, ретраи, circuit breaker, таймаут попытки (и rate limiter на исходящую очередь). Проверено на сервере, который отвечает 503 два раза и затем 200 (задержка ретрая уменьшена до 50 мс, чтобы пример шёл быстро):

| Клиент | Результат |
|---|---|
| обычный `AddHttpClient` | `503` за 1 вызов |
| `AddStandardResilienceHandler` | `200` за 3 вызова (2 ретрая) |

## Значения по умолчанию

Проверено чтением `HttpStandardResilienceOptions`:

| Стратегия | Значения по умолчанию |
|---|---|
| Retry | `MaxRetryAttempts = 3`, `Delay = 2 с`, `BackoffType = Exponential`, `UseJitter = true` |
| Таймаут попытки | `10 с` |
| Общий таймаут запроса | `30 с` |
| Circuit breaker | `FailureRatio = 0.1`, `MinimumThroughput = 100`, `SamplingDuration = 30 с`, `BreakDuration = 5 с` |

Ретраи применяются к «транзиентным» ответам: сетевые исключения, таймауты, `5xx`, `408` и `429`. Проверено: `408`, `429` и `501` — по 4 вызова (1 + 3 ретрая); на `400` и `404` повторов нет (1 вызов); на постоянном `500` — 4 вызова и затем итоговый `500`.

## Таймаут

Таймаут `HttpClient` по умолчанию — 100 секунд (проверено: `00:01:40`): слишком много. Конвейер ограничивает каждую попытку и весь запрос. Проверено с `AttemptTimeout = 300 мс` и медленным (3 с) сервисом: попыток было 2 (1 + 1 ретрай), итог — `TimeoutRejectedException` примерно через 630 мс.

## Частые ошибки новичка

- ❌ `new HttpClient()` на каждый запрос → ✅ исчерпание сокетов; используйте `IHttpClientFactory`.
- ❌ Один статический `HttpClient` навсегда → ✅ не обновляет DNS; фабрика ротирует обработчики ([грани](expert.ru.md#грани-и-подвохи)).
- ❌ Ретраи на `POST` без идемпотентности → ✅ дубликаты ([production](advanced.ru.md#идемпотентность-и-ретраи)).
- ❌ Оставить таймаут по умолчанию (100 с) → ✅ конвейер с таймаутами.
- ❌ Ретраи без паузы и джиттера → ✅ экспоненциальная задержка с джиттером (по умолчанию в стандартном обработчике).

## Связанные материалы

- Дальше: [production](advanced.ru.md) — типизированные клиенты, свой конвейер, circuit breaker, тесты.
- [10 · async/await](../../modules/10-async-await/overview.ru.md) — отмена и таймауты.
- [13 · DI, конфигурация, логирование](../../modules/13-di-config-logging/overview.ru.md) — регистрация клиентов.
- [Rate limiting](../rate-limiting/overview.ru.md) — серверная сторона: что делать с `429`.
- [Python](../../../python/overview.ru.md) — httpx с ретраями.

## Источники

- [Make HTTP requests with `IHttpClientFactory`](https://learn.microsoft.com/dotnet/core/extensions/httpclient-factory)
- [Build resilient HTTP apps](https://learn.microsoft.com/dotnet/core/resilience/http-resilience)
- [Polly: resilience strategies](https://www.pollydocs.org/strategies/index.html)
