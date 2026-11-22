---
title: "Наблюдаемость: базовая теория"
lang: ru
track: csharp
kind: module
doc: basics
slug: 22-observability
part: 3
tags: [observability, logging, metrics, tracing, opentelemetry, health-checks]
versions: ".NET 11, C# 15; OpenTelemetry 1.21.0"
status: draft
updated: 2026-11-22
---

# Наблюдаемость: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [13 · DI, конфигурация, логирование](../13-di-config-logging/overview.ru.md), [14 · ASP.NET Core: Minimal API](../14-aspnet-core-minimal-api/overview.ru.md). **Время:** ~1 ч.

## Зачем это нужно

Приложение в продакшене нельзя «просто запустить в отладчике». Когда что-то идёт не так, нужно понять, **что происходит внутри**, по сигналам, которые приложение испускает само. Способность отвечать на вопросы о поведении системы по её наблюдаемым выходам называют **наблюдаемостью** (observability). Её основа — три вида сигналов:

| Сигнал | Отвечает на вопрос | Пример |
|---|---|---|
| **Логи** (logs) | что именно произошло | «заказ 42 не оформлен: нет товара» |
| **Метрики** (metrics) | сколько и как часто, какая тенденция | заказов в минуту, 95-й перцентиль времени ответа |
| **Трассировка** (traces) | куда ушло время одного запроса через сервисы | запрос → API → БД → внешний сервис |

Дополняют их **проверки здоровья** (health checks) — простой ответ «жив/готов» для оркестратора. В .NET стандарт де-факто — **OpenTelemetry**: единый набор API и протокол, с которым работают разные системы мониторинга.

## Логи

Структурные логи через `ILogger` ([13 · DI, конфигурация, логирование](../13-di-config-logging/overview.ru.md)): шаблон с именованными местами, уровни, области. Для наблюдаемости важно, чтобы в записи были **идентификаторы для поиска** (заказ, пользователь) и **контекст трассы** — тогда запись логов можно связать с конкретным запросом. Проверено: внутри активности (`Activity`) текущий `Activity.Current` доступен, а `TraceId` — 32 символа:

```csharp
using (var act = source.StartActivity("Work"))
{
    log.LogInformation("inside activity");
    // Activity.Current == act; act.TraceId is 32 hex characters
}
```

## Метрики

Метрика — числовой ряд во времени. В .NET их создают через `System.Diagnostics.Metrics`:

```csharp
var meter = new Meter("Demo.Shop", "1.0.0");
var orders = meter.CreateCounter<long>("shop.orders", unit: "{order}", description: "Placed orders");
var latency = meter.CreateHistogram<double>("shop.order.duration", unit: "ms");
meter.CreateObservableGauge("shop.queue.length", () => queue);

orders.Add(1, new KeyValuePair<string, object?>("region", "eu"));
latency.Record(12.5);
```

Виды инструментов:

| Инструмент | Смысл | Пример |
|---|---|---|
| `Counter<T>` | монотонно растущий счётчик событий | число заказов |
| `UpDownCounter<T>` | величина, которая растёт и убывает | активные соединения |
| `Histogram<T>` | распределение значений | время обработки |
| `ObservableGauge<T>` | текущее значение, читается по запросу | длина очереди |

К измерению добавляют **теги** (`region=eu`) — по ним метрику разрезают; тегов с большим числом значений (идентификатор пользователя) избегают, иначе число рядов взрывается. Измерения собирает **слушатель** (`MeterListener` или экспортёр OpenTelemetry). Проверено через `MeterListener`: получены `shop.orders=1 [region:eu]`, `shop.orders=2 [region:us]`, гистограмма `12,5` (в текущей культуре) и `shop.queue.length=3`.

## Трассировка

Трассировка описывает путь одного запроса через систему. Единица — **спан** (span): операция с началом и концом, именем, тегами и родителем. В .NET спан представляет `Activity`, источник — `ActivitySource`:

```csharp
var source = new ActivitySource("Demo.Shop");

using (var parent = source.StartActivity("PlaceOrder", ActivityKind.Server))
{
    parent?.SetTag("order.id", 42);
    using (var child = source.StartActivity("SaveToDb"))
    {
        child?.SetTag("db.system", "sqlite");
        child?.AddEvent(new ActivityEvent("saved"));
    }
    using (var failing = source.StartActivity("Notify"))
    {
        failing?.SetStatus(ActivityStatusCode.Error, "smtp down");
    }
}
```

Проверено: `SaveToDb` и `Notify` имеют родителя `PlaceOrder`, все спаны — в одной трассе, у `Notify` статус `Error`, у `PlaceOrder` вид `Server`. Если на источник никто не подписан, `StartActivity` возвращает `null` — поэтому везде `?.` (проверено). Вложенность строится автоматически через `Activity.Current`. Трасса — это дерево спанов с общим `TraceId` (32 hex-символа), у каждого спана свой `SpanId` (16).

## Проверки здоровья

Оркестратор (Kubernetes, балансировщик) спрашивает приложение, живо ли оно и готово ли принимать трафик:

```csharp
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy("ok"), tags: ["live"])
    .AddCheck("db", () => dbUp ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy("db down"), tags: ["ready"]);

app.MapHealthChecks("/health/live", new() { Predicate = r => r.Tags.Contains("live") });
app.MapHealthChecks("/health/ready", new() { Predicate = r => r.Tags.Contains("ready") });
```

Проверено: пока БД «жива», все три адреса отвечают `200 Healthy`; когда `dbUp = false`, `/health/ready` и `/health` дают `503 Unhealthy`, а `/health/live` остаётся `200`. **Liveness** («процесс жив») не должен зависеть от внешних систем, иначе оркестратор перезапустит приложение из-за чужого сбоя; **readiness** («готов принимать запросы») проверяет зависимости.

## Ошибки новичка

- ❌ Только логи «чтобы было» → ✅ три сигнала: логи, метрики, трассы.
- ❌ Логи без идентификаторов и контекста трассы → ✅ структурные поля и `TraceId`.
- ❌ Метрики с тегами огромной кардинальности (id пользователя) → ✅ ограниченный набор значений.
- ❌ Liveness-проверка ходит в БД → ✅ liveness простая, readiness — с зависимостями.
- ❌ Не вызывать `?.` у результата `StartActivity` → ✅ он может быть `null`.
- ❌ Никаких алертов → ✅ метрики и логи нужны, чтобы вас оповестили до жалоб пользователей.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — OpenTelemetry, распространение контекста, корреляция.
- [13 · DI, конфигурация, логирование](../13-di-config-logging/overview.ru.md) — `ILogger`.
- [14 · ASP.NET Core: Minimal API](../14-aspnet-core-minimal-api/overview.ru.md) — конечные точки проверок здоровья.
- [19 · Производительность](../19-performance/overview.ru.md) — метрики и профилирование.
- [Kubernetes](../../../kubernetes/overview.ru.md) — liveness/readiness-пробы.

## Источники

- [Наблюдаемость в .NET](https://learn.microsoft.com/dotnet/core/diagnostics/observability-with-otel)
- [Метрики: `System.Diagnostics.Metrics`](https://learn.microsoft.com/dotnet/core/diagnostics/metrics)
- [Трассировка: `ActivitySource` и `Activity`](https://learn.microsoft.com/dotnet/core/diagnostics/distributed-tracing)
- [Проверки здоровья в ASP.NET Core](https://learn.microsoft.com/aspnet/core/host-and-deploy/health-checks)
- [OpenTelemetry](https://opentelemetry.io/docs/languages/dotnet/)
