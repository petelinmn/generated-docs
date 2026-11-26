---
title: "Паттерны надёжности: экспертная теория"
lang: ru
track: system-design
kind: module
doc: expert
slug: 09-reliability-patterns
part: 2
tags: [system-design, resilience, retry, circuit-breaker, bulkhead, rate-limiting, load-shedding]
versions: ".NET 11, C# 15; Polly.Core 8.9.1; System.Threading.RateLimiting 11.0.0"
status: draft
updated: 2026-11-26
---

# Паттерны надёжности: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · [2. Продвинутая](advanced.ru.md) · **3. Экспертная** · [4. Квиз](quiz.ru.md)

## Под капотом

Повторы усиливают нагрузку именно тогда, когда зависимость ослаблена. Если каждый из `n` уровней делает `a` попыток, до нижнего доходит `a^n` вызовов.

```csharp
public static int CallsAtTheBottom(int attemptsPerLayer, int layers) =>
    Enumerable.Repeat(attemptsPerLayer, layers).Aggregate(1, (total, n) => total * n);
```

Четыре попытки на трёх уровнях — 64 вызова. Файл: [Ex03_Amplification.cs](code/Examples/Ex03_Amplification.cs). Retry budget ограничивает повторы долей обычных запросов.

## Справочник

| Polly 8.9.1: настройка | По умолчанию |
|---|---|
| `MaxRetryAttempts` | 3 |
| `Delay` | 2 с |
| `BackoffType` | `Constant` |
| `UseJitter` | `false` |
| `FailureRatio` | 0,1 |
| `MinimumThroughput` | 100 |
| `SamplingDuration` | 30 с |
| `BreakDuration` | 5 с |
| `Timeout` | 30 с |

| Стандартный HTTP-обработчик | Значение |
|---|---|
| rate limiter | 1000 одновременных, без очереди |
| общий таймаут | 30 с |
| retry | 3, экспоненциально с jitter, от 2 с |
| circuit breaker | 10%, минимум 100, окно 30 с, пауза 5 с |
| таймаут попытки | 10 с |

Он обрабатывает HTTP 500 и выше, 408, 429, `HttpRequestException` и `TimeoutRejectedException`. Hedging-обработчик (`AddStandardHedgingHandler`) отправляет дополнительную попытку, не дожидаясь ответа, и годится только для безопасных операций.

## Подвохи

### Retry по умолчанию повторяет и ошибки программиста

```csharp
int calls = 0;
var p = new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions { Delay = TimeSpan.Zero }).Build();
try { p.Execute(() => { calls++; throw new ArgumentNullException(); }); } catch (ArgumentNullException) { }
Console.WriteLine(calls);
```

Результат: `4`. По умолчанию обрабатываются все исключения, кроме `OperationCanceledException`.

### Таймаут не прерывает код, игнорирующий токен

```csharp
var to = new ResiliencePipelineBuilder().AddTimeout(TimeSpan.FromMilliseconds(100)).Build();
await to.ExecuteAsync(async ct => await Task.Delay(600));
```

Результат: вызов длится около 600 мс и завершается успешно, без `TimeoutRejectedException`. Отмена кооперативна: нужно `Task.Delay(600, ct)`.

### Breaker с настройками по умолчанию может не сработать

99 неудачных вызовов подряд через `AddCircuitBreaker(new CircuitBreakerStrategyOptions())` все дошли до зависимости: `MinimumThroughput` равен 100. Для редко вызываемого сервиса это значит, что защиты фактически нет.

## Компромиссы и альтернативы

| Приём | Помогает от | Цена |
|---|---|---|
| Retry | случайных сбоев | усиление нагрузки |
| Hedging | медленного хвоста | лишние запросы |
| Circuit breaker | долгих отказов | отказ, пока открыт |
| Bulkhead | захвата ресурсов | простаивающие слоты |
| Очередь | пиков | задержка |
| Кэш как запасной ответ | недоступности | устаревшие данные |

## Вопросы с собеседований

**1. Зачем таймаут?**

<details><summary>Ответ</summary>

Чтобы ожидание не удерживало ресурсы бесконечно и не распространяло сбой.

</details>

**2. Что такое retry storm?**

<details><summary>Ответ</summary>

Лавина повторов, не дающая зависимости восстановиться.

</details>

**3. Зачем jitter?**

<details><summary>Ответ</summary>

Чтобы клиенты не повторяли синхронно.

</details>

**4. Состояния circuit breaker?**

<details><summary>Ответ</summary>

Closed, open, half-open (в Polly есть ещё isolated для ручного удержания).

</details>

**5. Чем rate limiter отличается от bulkhead?**

<details><summary>Ответ</summary>

Первый обычно ограничивает частоту, второй — одновременные вызовы.

</details>

**6. 429 или 503?**

<details><summary>Ответ</summary>

429 — клиент превысил свой лимит, 503 — сервис не может обслужить.

</details>

**7. Где делать повторы в цепочке из трёх сервисов?**

<details><summary>Ответ</summary>

На одном уровне, обычно ближайшем к сбою или к клиенту, с бюджетом.

</details>

**8. Что такое распространение дедлайна?**

<details><summary>Ответ</summary>

Передача оставшегося времени дальше, чтобы не делать работу, результат которой уже не нужен.

</details>

**9. Можно ли повторить платёж после таймаута?**

<details><summary>Ответ</summary>

Только с idempotency key: таймаут не означает, что платёж не прошёл.

</details>

**10. Подвох: добавили retry, и доступность выросла в тестах. Всё хорошо?**

<details><summary>Ответ</summary>

Под реальной перегрузкой те же повторы могут добить зависимость. Проверяйте поведение при отказе и смотрите на долю повторов.

</details>

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [Квиз](quiz.ru.md) и [упражнения](exercises.ru.md)

## Источники

- [Polly: retry](https://www.pollydocs.org/strategies/retry.html)
- [Polly: circuit breaker](https://www.pollydocs.org/strategies/circuit-breaker.html)
- [.NET: HTTP resilience](https://learn.microsoft.com/dotnet/core/resilience/http-resilience)
