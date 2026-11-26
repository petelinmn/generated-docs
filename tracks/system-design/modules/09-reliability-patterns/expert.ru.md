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

### Адаптивный лимит конкурентности

Фиксированный лимит устаревает, когда меняется мощность зависимости. Адаптивный увеличивает его понемногу, пока всё хорошо, и резко уменьшает при ошибках или росте задержки (AIMD).

```csharp
_limit = failed || latency > latencyTarget
    ? Math.Max(min, _limit * 0.5)
    : Math.Min(max, _limit + 1 / _limit);
```

С лимитом 20 после 21 хорошего ответа стало 21, после одного медленного — 10. Файл: [Ex04_AdaptiveLimit.cs](code/Examples/Ex04_AdaptiveLimit.cs). Это только правило управления, не готовый ограничитель.

### Hedging

Если первая попытка не ответила за заданное время, запускают вторую и берут того, кто успел.

```csharp
.AddHedging(new HedgingStrategyOptions<string> { MaxHedgedAttempts = 1, Delay = delay })
```

В Polly по умолчанию одна дополнительная попытка и задержка 2 секунды; `Delay = TimeSpan.Zero` запускает все сразу. Обе попытки доходят до зависимости, поэтому приём годится только для безопасных операций. Задержку обычно берут около высокого перцентиля, чтобы дублировать малую долю запросов. Файл: [Ex05_Hedging.cs](code/Examples/Ex05_Hedging.cs).

### Дедлайны в gRPC

По умолчанию у вызова gRPC нет дедлайна. Клиент задаёт `CallOptions.Deadline` (UTC); по истечении вызов получает `DeadlineExceeded`, а на сервере отменяется `ServerCallContext.CancellationToken` — метод продолжает работать, пока не вернётся, поэтому токен нужно передавать дальше. Дедлайн охватывает и повторы. `EnableCallContextPropagation()` передаёт его дочерним вызовам автоматически.

### Chaos engineering

Порядок: описать устойчивое состояние измеримой метрикой, выдвинуть гипотезу, что оно сохранится, внести реалистичный сбой и попытаться гипотезу опровергнуть, ограничив радиус поражения. В Polly есть стратегии `Fault`, `Outcome`, `Latency` и `Behavior`.

```csharp
.AddChaosFault(new ChaosFaultStrategyOptions
{
    InjectionRate = rate, // always set it: the default is not zero
    Enabled = enabled,
```

### Деградация и feature flags

Постепенное включение держит каждого пользователя на одной стороне и расширяет долю без перетасовки.

```csharp
uint hash = BinaryPrimitives.ReadUInt32BigEndian(SHA256.HashData(Encoding.UTF8.GetBytes($"{flag}\n{userId}")));
return hash % 100 < percent;
```

Все, кто был в 10%, остаются при 50%. В приложении берите `Microsoft.FeatureManagement`: у него есть фильтры по проценту, времени и аудитории. Флаг — это ещё и аварийный выключатель второстепенной функции. Файл: [Ex06_FeatureFlags.cs](code/Examples/Ex06_FeatureFlags.cs).

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

| Способ ограничить нагрузку | Что знает заранее | Подстраивается |
|---|---|---|
| Фиксированный лимит | число | нет |
| Rate limiter | частоту | нет |
| AIMD | цель по задержке | да |
| Load shedding по приоритету | важность запросов | по состоянию |

| Polly chaos | Что внедряет |
|---|---|
| `Fault` | исключение |
| `Outcome` | подменённый результат |
| `Latency` | задержку |
| `Behavior` | любое действие |

| Уровень деградации | Пример |
|---|---|
| Устаревшие данные | кэш вместо источника |
| Упрощённый ответ | без рекомендаций |
| Только чтение | запись временно отключена |
| Отказ части запросов | сначала фоновые и неважные |

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

### Chaos включён по умолчанию

```csharp
var c = new ChaosFaultStrategyOptions();
Console.WriteLine($"{c.InjectionRate} {c.Enabled}");
```

Результат: `0.001` и `True` (разделитель дроби зависит от культуры). Добавив стратегию без настроек, вы начинаете внедрять сбои. Включайте явно и только там, где это задумано.

### Hedging вызывает зависимость дважды

В тесте первая попытка зависла, вторая ответила, а счётчик вызовов равен 2. Для платежа без идемпотентности это двойное списание.

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

**11. Как работает AIMD?**

<details><summary>Ответ</summary>

Лимит растёт аддитивно при успехах и уменьшается кратно при признаках перегрузки.

</details>

**12. Чем hedging отличается от retry?**

<details><summary>Ответ</summary>

Не ждёт отказа: дублирует медленный запрос, пока первый ещё выполняется.

</details>

**13. Есть ли у gRPC-вызова дедлайн по умолчанию?**

<details><summary>Ответ</summary>

Нет, его задают явно.

</details>

**14. Что должно быть до chaos-эксперимента?**

<details><summary>Ответ</summary>

Измеримое устойчивое состояние, гипотеза, ограниченный радиус и способ остановить эксперимент.

</details>

**15. Подвох: увеличили долю флага с 10% до 50% при случайном выборе на каждый запрос. Что не так?**

<details><summary>Ответ</summary>

Пользователь будет переключаться между вариантами. Нужна стабильная корзина по пользователю.

</details>

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [Квиз](quiz.ru.md) и [упражнения](exercises.ru.md)

## Источники

- [Polly: retry](https://www.pollydocs.org/strategies/retry.html)
- [Polly: circuit breaker](https://www.pollydocs.org/strategies/circuit-breaker.html)
- [.NET: HTTP resilience](https://learn.microsoft.com/dotnet/core/resilience/http-resilience)
- [Polly: hedging](https://www.pollydocs.org/strategies/hedging.html)
- [Polly: chaos engineering](https://www.pollydocs.org/chaos/)
- [gRPC: дедлайны и отмена](https://learn.microsoft.com/aspnet/core/grpc/deadlines-cancellation)
- [Principles of Chaos Engineering](https://principlesofchaos.org/)
- [Microsoft.FeatureManagement](https://learn.microsoft.com/azure/azure-app-configuration/feature-management-dotnet-reference)
