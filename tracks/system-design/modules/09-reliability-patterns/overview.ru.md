---
title: "Паттерны надёжности"
lang: ru
track: system-design
kind: module
doc: overview
slug: 09-reliability-patterns
part: 2
levels: [1, 2, 3, 4, 5, 6]
tags: [system-design, resilience, retry, circuit-breaker, bulkhead, rate-limiting, load-shedding]
versions: ".NET 11, C# 15; Polly.Core 8.9.1; System.Threading.RateLimiting 11.0.0"
status: draft
updated: 2026-11-26
---

# Паттерны надёжности

🌐 **Русский** · [English](overview.en.md)

## Коротко

Зависимости медленны и ломаются. Паттерны надёжности не дают одному сбою распространиться: ограничивают ожидание, повторы, одновременную работу и входящий поток.

## Ключевые идеи

- У каждого сетевого вызова должен быть таймаут.
- Повторяют только временные ошибки и только безопасные операции.
- Задержку между повторами увеличивают и размывают.
- Повторы на нескольких уровнях умножаются.
- Circuit breaker перестаёт звать то, что не отвечает.
- Bulkhead не даёт одной зависимости занять все ресурсы.
- Перегруженная система должна отказывать быстро.

## Минимальный пример

```csharp
using RateLimitLease lease = limiter.AttemptAcquire();
if (!lease.IsAcquired) return (false, default);
```

## Когда применять, а когда нет

- ✅ Любой вызов по сети.
- ✅ Общий ресурс, который можно перегрузить.
- ❌ Повторы неидемпотентной операции без ключа.
- ❌ Circuit breaker вокруг локального вычисления.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | таймауты, повторы, backoff |
| [2. Продвинутая теория](advanced.ru.md) | circuit breaker, bulkhead, rate limiting, load shedding |
| [3. Экспертная теория](expert.ru.md) | усиление повторов, значения по умолчанию, подвохи, вопросы |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| [5. Запускаемые примеры](code/Examples/) | Polly, rate limiters |
| [6. Тесты](code/Tests/) | `dotnet test` в папке `code/` |

## Связанные материалы

- Пререквизиты: [проектирование API](../05-api-design/overview.ru.md), [сообщения и события](../08-messaging-and-events/overview.ru.md)
- В других треках: [Polly](../../../csharp/libraries/polly/overview.ru.md), [устойчивый HttpClient](../../../csharp/recipes/http-client-resilience/overview.ru.md), [rate limiting](../../../csharp/recipes/rate-limiting/overview.ru.md)
- Доступность и бюджет ошибок: [процесс проектирования](../01-design-process/overview.ru.md)
- [Наблюдаемость и SLO](../14-observability-and-slo/overview.ru.md)
- [Безопасность архитектуры](../15-security-architecture/overview.ru.md)
- [Разбор задач](../17-design-case-studies/overview.ru.md)
- [Запросы из эффектов React](../../../react/modules/06-effects/overview.ru.md)
- [Рецепт: распределённый rate limiter](../../recipes/distributed-rate-limiter/overview.ru.md)
