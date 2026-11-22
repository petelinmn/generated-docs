---
title: "Устойчивый HttpClient"
lang: ru
track: csharp
kind: recipe
doc: overview
slug: http-client-resilience
part: 2
levels: [1, 2, 3, 4]
tags: [httpclient, ihttpclientfactory, retry, circuit-breaker, polly, resilience, timeout]
versions: ".NET 11, C# 15; Microsoft.Extensions.Http.Resilience 11.0.1"
status: draft
updated: 2026-11-22
---

# Устойчивый HttpClient

🌐 **Русский** · [English](overview.en.md)

## Коротко

Рецепт: вызывать внешние HTTP-сервисы так, чтобы разовые сбои не ломали запрос, а долгие не клали приложение. `IHttpClientFactory` управляет соединениями и DNS, `AddStandardResilienceHandler()` (Microsoft.Extensions.Http.Resilience, Polly v8) добавляет таймауты, ретраи с экспонентой и джиттером и circuit breaker. Проверено запуском на сервере с искусственными сбоями.

## Ключевые идеи

- `IHttpClientFactory`, не `new HttpClient()` и не вечный static.
- `AddStandardResilienceHandler()`: 3 ретрая, таймаут попытки 10 с, общий 30 с, circuit breaker.
- Ретраятся транзиентные сбои (`5xx`, `408`, `429`, сеть), остальные `4xx` — нет.
- Разомкнутая цепь → `BrokenCircuitException`, запрос не уходит; таймаут → `TimeoutRejectedException`.
- Ретраи только для идемпотентного (или с `Idempotency-Key`).
- Свой конвейер — `AddResilienceHandler`.
- Шторм ретраев и каскадные сбои — задача уровня системы.

## Минимальный пример

```csharp
builder.Services.AddHttpClient("api", c => c.BaseAddress = new Uri("https://api.example.com"))
    .AddStandardResilienceHandler();
```

## Когда применять, а когда нет

- ✅ Любые вызовы внешних HTTP-сервисов.
- ✅ Микросервисные взаимодействия с возможными сбоями.
- ❌ Ретраи неидемпотентных операций без защиты от дубликатов.
- ❌ Слепые ретраи на `4xx` и ошибки валидации.
- ❌ Ретраи там, где сбой — постоянная ошибка конфигурации.
- ❌ Замена ожиданий SLA: ретраи увеличивают задержку.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Минимум](basics.ru.md) | `AddStandardResilienceHandler`, значения по умолчанию, таймауты |
| [2. Production](advanced.ru.md) | типизированные клиенты, circuit breaker, свой конвейер, идемпотентность |
| [3. Масштаб и грани](expert.ru.md) | шторм ретраев, bulkhead, hedging, подвохи, 10 вопросов с собеседований |
| [4. Самопроверка](quiz.ru.md) | 10 вопросов и [4 упражнения](exercises.ru.md) с решениями |
| 5. Запускаемые примеры | не сгенерирован — `/learn csharp http-client-resilience level 5` |
| 6. Тесты | не сгенерирован — `/learn csharp http-client-resilience level 6` |

## Связанные материалы

- Пререквизиты: [10 · async/await](../../modules/10-async-await/overview.ru.md), [13 · DI, конфигурация, логирование](../../modules/13-di-config-logging/overview.ru.md)
- Рядом: [Rate limiting](../rate-limiting/overview.ru.md), [Фоновые задачи](../background-jobs/overview.ru.md), [22 · Наблюдаемость](../../modules/22-observability/overview.ru.md)
- В других треках: [Python](../../../python/overview.ru.md) — httpx с ретраями
- Рецепты и библиотеки: [Polly](../../libraries/polly/overview.ru.md), [Refit](../../libraries/refit/overview.ru.md), [Кэширование](../caching/overview.ru.md), [gRPC-сервис](../grpc-service/overview.ru.md)
