---
title: "Наблюдаемость: продвинутая теория"
lang: ru
track: csharp
kind: module
doc: advanced
slug: 22-observability
part: 3
tags: [opentelemetry, distributed-tracing, trace-context, instrumentation, exporters, health-checks, correlation]
versions: ".NET 11, C# 15; OpenTelemetry 1.21.0"
status: draft
updated: 2026-11-22
---

# Наблюдаемость: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [базовая теория](basics.ru.md). **Время:** ~1,5 ч.

## OpenTelemetry в приложении

**OpenTelemetry** (OTel) — независимый от поставщика стандарт: API, SDK и протокол OTLP для логов, метрик и трасс. В .NET сами инструменты (`ActivitySource`, `Meter`, `ILogger`) — часть платформы; OTel SDK **подписывается** на них и отправляет данные в экспортёры. Пакеты: `OpenTelemetry.Extensions.Hosting`, инструментации (`OpenTelemetry.Instrumentation.AspNetCore`, `…Http`) и экспортёры (OTLP, консоль, in-memory для тестов).

```csharp
builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("shop-api"))                      // who emits the data
    .WithTracing(t => t.AddAspNetCoreInstrumentation()                      // incoming HTTP requests
                       .AddHttpClientInstrumentation()                      // outgoing HTTP requests
                       .AddSource("Demo.Shop")                              // your own spans
                       .AddInMemoryExporter(spans))                         // replace with OTLP in production
    .WithMetrics(m => m.AddAspNetCoreInstrumentation().AddMeter("Demo.Shop"));
```

Ресурс (`service.name` и другие атрибуты) говорит, **кто** излучает сигналы. Для отправки на сборщик (collector) и в бэкенды (Jaeger, Tempo, Prometheus и т. д.) добавляют экспортёр OTLP (`.AddOtlpExporter()` или `UseOtlpExporter()`); адрес обычно задают переменной окружения `OTEL_EXPORTER_OTLP_ENDPOINT`.

> ⚠️ Проверить: отправка по OTLP на настоящий сборщик в этом материале не запускалась (нет коллектора); проверены только in-memory-экспортёры и локальные слушатели. Схема и имена переменных окружения — по документации OpenTelemetry; сверьте их перед использованием.

Проверено с in-memory-экспортёрами: после `using (source.StartActivity("Checkout")) …` в списке спанов один спан `Checkout` с тегом `user=ann`; метрики `shop.orders` и `shop.queue.length` экспортированы после `ForceFlush`.

## Распространение контекста между сервисами

Чтобы спаны разных сервисов сложились в одну трассу, идентификаторы передают в заголовке **`traceparent`** стандарта **W3C Trace Context**: `00-<trace-id>-<span-id>-<flags>` (проверено: `00-cd3fac0abe95f2592309ede0555db9ec-20a2ab3b656840de-02`; `Activity.IdFormat` — `W3C`). Инструментация `HttpClient` добавляет заголовок к исходящему запросу, инструментация ASP.NET Core читает его при входе и создаёт дочерний спан.

Проверено на приложении, где `/outer` вызывает `/inner` через `HttpClient`: обработчик `/inner` получил заголовок `traceparent`, а экспортёр собрал пять спанов **в одной трассе** (`single trace: True`):

```text
Client   GET                parent=-    source=System.Net.Http     (the test client's request)
Server   GET /outer         parent=yes  source=Microsoft.AspNetCore
Internal business-work      parent=yes  source=Demo.Shop           (our own span)
Client   GET                parent=yes  source=System.Net.Http     (the call to /inner)
Server   GET /inner         parent=yes  source=Microsoft.AspNetCore
```

Собственные спаны становятся детьми текущего через `Activity.Current`, который хранится в `AsyncLocal` и потому течёт через `await` ([10 · Async/await](../10-async-await/expert.ru.md#контекст-выполнения-executioncontext-и-asynclocalt)). Для очередей сообщений контекст передают в атрибутах сообщения вручную (или через инструментацию клиента брокера).

## Теги, события и статусы спана

```csharp
a.SetTag("order.id", 42); a.SetTag("name", "ann");
a.Tags          // name=ann              — only string values
a.TagObjects    // order.id=42 (Int32), name=ann (String) — all values
a.GetTagItem("order.id")   // 42
```

Проверено: свойство `Tags` содержит **только строковые** теги, а число `42` видно в `TagObjects`/`GetTagItem`. Поэтому при отладке смотрите `TagObjects`. События спана (`AddEvent`) — отметки времени внутри спана; статус — `Unset`/`Ok`/`Error` (`SetStatus(ActivityStatusCode.Error, "…")`). Именуйте теги по **семантическим конвенциям** OpenTelemetry (`db.system`, `http.request.method`, `url.path`), чтобы бэкенды понимали данные.

## Сэмплирование

Хранить все трассы дорого. **Сэмплер** решает, записывать ли спан: `AlwaysOn`, `AlwaysOff`, по доле (`TraceIdRatioBased`), родительский (`ParentBased` — следовать решению родителя, что важно для целостных трасс). В `ActivityListener` это `Sample`; проверено: `None` — `StartActivity` возвращает `null`; `PropagationData` — активность создаётся только для передачи контекста (`IsAllDataRequested = False`); `AllData` — данные собираются (`IsAllDataRequested = True`, `Recorded = False`); `AllDataAndRecorded` — ещё и помечена записанной (`Recorded = True`). Без слушателя `StartActivity` тоже возвращает `null`. Метрики не сэмплируют — они агрегируются.

## Метрики: практики

- Единицы в имени и поле `unit` (`ms`, `s`, `By`, `{order}`); имена в стиле `домен.объект.измерение`.
- **Кардинальность** тегов: `region` (3 значения) допустим, `user_id` (миллионы) — нет.
- Для задержек — гистограммы (`Histogram`), по ним строят перцентили p50/p95/p99; среднее скрывает хвосты.
- Метрики платформы (`Microsoft.AspNetCore.*`, `System.Runtime`) собираются инструментациями без кода.
- `ObservableGauge` вызывает ваш делегат при сборе, поэтому делегат быстрый и безопасный.

## Логи, трассы и корреляция

Записи логов связывают с трассой полями `TraceId` и `SpanId` — провайдеры логирования OpenTelemetry/консоли добавляют их автоматически из `Activity.Current`. Проверено: внутри `StartActivity` `Activity.Current` равен созданной активности и у неё есть `TraceId`. Так по одному `TraceId` находят и трассу, и все её логи, и ошибку. Исключения записывают и в лог, и в спан (`SetStatus(Error)`).

## Health checks

| Проверка | Назначение |
|---|---|
| liveness (`/health/live`) | процесс жив; без внешних зависимостей; при провале — перезапуск |
| readiness (`/health/ready`) | приложение готово принимать трафик; проверяет БД, кэш, очередь; при провале — убрать из балансировки |
| startup | приложение закончило медленный старт |

Теги и `Predicate` в `MapHealthChecks` разделяют проверки по адресам (проверено на примере выше). `UnhealthyStatus` бывает `Unhealthy` (HTTP 503) и `Degraded` (по умолчанию HTTP 200). Не делайте проверки тяжёлыми: оркестратор вызывает их каждые секунды.

## Лучшие практики

| Правило | Почему |
|---|---|
| Единые идентификаторы (`TraceId`) в логах, метриках, трассах | сквозной поиск |
| Конвенции именования OpenTelemetry | совместимость с бэкендами |
| Ограничивайте кардинальность тегов | иначе метрики дорогие и тормозят |
| Сэмплируйте трассы, не метрики | стоимость хранения |
| Не логируйте секреты и персональные данные | безопасность |
| Алерты по симптомам (ошибки, задержка), а не по причинам | меньше шума |
| SLO и бюджет ошибок | наблюдаемость привязана к цели |

## Связанные материалы

- Назад: [базовая теория](basics.ru.md). Дальше: [экспертная теория](expert.ru.md) — устройство и подвохи.
- [10 · Async/await](../10-async-await/overview.ru.md) — `AsyncLocal` и поток контекста.
- [14 · ASP.NET Core: Minimal API](../14-aspnet-core-minimal-api/overview.ru.md) — инструментация и health checks.
- [21 · Архитектура приложений](../21-architecture/overview.ru.md) — сквозная трассировка между сервисами.
- [Kubernetes](../../../kubernetes/overview.ru.md) — пробы и метрики кластера.

## Источники

- [OpenTelemetry .NET: начало работы](https://opentelemetry.io/docs/languages/dotnet/getting-started/)
- [W3C Trace Context](https://www.w3.org/TR/trace-context/)
- [Семантические конвенции OpenTelemetry](https://opentelemetry.io/docs/specs/semconv/)
- [Распределённая трассировка в .NET](https://learn.microsoft.com/dotnet/core/diagnostics/distributed-tracing-concepts)
- [Метрики ASP.NET Core](https://learn.microsoft.com/aspnet/core/log-mon/metrics/metrics)
