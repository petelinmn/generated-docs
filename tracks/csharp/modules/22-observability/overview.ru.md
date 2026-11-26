---
title: "Наблюдаемость"
lang: ru
track: csharp
kind: module
doc: overview
slug: 22-observability
part: 3
levels: [1, 2, 3, 4]
tags: [observability, logging, metrics, tracing, opentelemetry, health-checks, sampling]
versions: ".NET 11, C# 15; OpenTelemetry 1.21.0"
status: draft
updated: 2026-11-22
---

# Наблюдаемость

🌐 **Русский** · [English](overview.en.md)

## Коротко

Наблюдаемость — способность понять, что происходит внутри работающей системы, по сигналам, которые она сама излучает: логам (что произошло), метрикам (сколько и как часто) и трассам (куда ушло время запроса через сервисы). В .NET источники сигналов встроены в платформу (`ILogger`, `Meter`, `ActivitySource`), а OpenTelemetry — стандарт, который подписывается на них и отправляет данные в любой бэкенд. Проверки здоровья дают оркестратору простой ответ «жив» и «готов».

## Ключевые идеи

- **Три сигнала:** логи, метрики (`Counter`, `Histogram`, `ObservableGauge`), трассы (`Activity` = спан).
- **Источник — слушатель:** без слушателя `StartActivity` возвращает `null`, поэтому инструментацию добавляют свободно (`?.`).
- **Контекст трассы:** `traceparent` (W3C) передаётся через `HttpClient`/ASP.NET Core; в проверенном примере пять спанов оказались в одной трассе.
- **Кардинальность тегов:** `region` допустим, `user_id` — нет; детали — в трассах и логах.
- **OpenTelemetry:** `AddSource`/`AddMeter`, инструментации, экспортёры (OTLP, in-memory для тестов).
- **Health checks:** liveness без зависимостей, readiness с ними; `Degraded` по умолчанию — HTTP 200.
- **Сэмплирование и SLO:** сохраняем не всё; алертим по симптомам, цели задаём через SLI/SLO.

## Минимальный пример

```csharp
var source = new ActivitySource("Demo.Shop");
var meter = new Meter("Demo.Shop");
var orders = meter.CreateCounter<long>("shop.orders");

using (var span = source.StartActivity("PlaceOrder"))     // null when nobody listens
{
    span?.SetTag("order.id", 42);
    orders.Add(1, new KeyValuePair<string, object?>("region", "eu"));
}
```

## Когда применять, а когда нет

- ✅ Любой сервис в продакшене: логи с `TraceId`, базовые метрики, health checks.
- ✅ Несколько сервисов: распределённая трассировка и согласованное сэмплирование.
- ✅ SLO и алерты по пользовательским симптомам.
- ❌ Теги метрик с идентификаторами пользователей.
- ❌ Секреты и персональные данные в логах, тегах, baggage.
- ❌ Liveness-проверка, зависящая от БД или внешних сервисов.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | три сигнала, структурные логи, `Meter` и инструменты, `ActivitySource`, health checks |
| [2. Продвинутая теория](advanced.ru.md) | OpenTelemetry SDK, распространение контекста W3C, теги и статусы, сэмплирование, практики метрик, корреляция, пробы |
| [3. Экспертная теория](expert.ru.md) | модель источник—слушатель, `Tags` и `TagObjects`, кардинальность, head/tail-выборка, SLI/SLO, диагностика, подвохи, 13 вопросов с собеседований |
| [4. Самопроверка](quiz.ru.md) | 11 вопросов и [5 упражнений](exercises.ru.md) с решениями |
| 5. Запускаемые примеры | ещё не создан — `/learn csharp 22-observability level 5` |
| 6. Тесты | ещё не создан — `/learn csharp 22-observability level 6` |

## Связанные материалы

- Пререквизиты: [13 · DI, конфигурация, логирование](../13-di-config-logging/overview.ru.md), [14 · ASP.NET Core: Minimal API](../14-aspnet-core-minimal-api/overview.ru.md)
- Рядом: [19 · Производительность](../19-performance/overview.ru.md), [21 · Архитектура приложений](../21-architecture/overview.ru.md)
- Развёртывание: [Kubernetes](../../../kubernetes/overview.ru.md) — пробы и метрики
- То же в других треках: [Python](../../../python/overview.ru.md) — логирование и мониторинг
- Рецепты и библиотеки: [BenchmarkDotNet](../../libraries/benchmarkdotnet/overview.ru.md), [Polly](../../libraries/polly/overview.ru.md), [Serilog](../../libraries/serilog/overview.ru.md), [Фоновые задачи](../../recipes/background-jobs/overview.ru.md), [Кэширование](../../recipes/caching/overview.ru.md), [gRPC-сервис](../../recipes/grpc-service/overview.ru.md), [Устойчивый HttpClient](../../recipes/http-client-resilience/overview.ru.md), [JWT-аутентификация](../../recipes/jwt-auth/overview.ru.md), [Rate limiting](../../recipes/rate-limiting/overview.ru.md), [Real-time через SignalR](../../recipes/signalr-realtime/overview.ru.md)
- [SLI, SLO и оценки нагрузки](../../../system-design/modules/01-design-process/overview.ru.md)
