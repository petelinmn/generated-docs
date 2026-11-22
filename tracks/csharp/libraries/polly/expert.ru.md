---
title: "Polly: экспертный уровень"
lang: ru
track: csharp
kind: library
doc: expert
slug: polly
part: 2
tags: [polly, resilience, chaos-engineering, circuit-breaker, idempotency, interview-questions]
versions: ".NET 11, C# 15; Polly 8.9.1"
status: draft
updated: 2026-11-22
---

# Polly: экспертный уровень

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Основы](basics.ru.md) · [2. Продвинутый](advanced.ru.md) · **3. Эксперт** · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [основы](basics.ru.md) и [продвинутый](advanced.ru.md). **Время:** ~30 мин.

## Внутреннее устройство

`ResiliencePipeline` — цепочка «стратегий» (middleware): каждая получает вызов и «следующий» делегат, выполняет логику до/после и решает, вызывать ли следующий. Это то же, что конвейер `HttpClient`-обработчиков. Особенности Polly 8:

- конвейер **неизменяем** после `Build()`, потокобезопасен и предназначен для повторного использования;
- результат операции (значение или исключение) представлен как `Outcome<T>`; `ShouldHandle` (`PredicateBuilder`) решает, считать ли исход сбоем;
- у вызова есть `ResilienceContext` (токен отмены, свойства) из пула `ResilienceContextPool`;
- стратегии оптимизированы на малые выделения памяти (`ValueTask`, `struct`-аргументы).

> ⚠️ Проверить: детали реализации (внутренние типы, пулы, бенчмарки аллокаций) в этом материале не исследовались; сверьтесь с исходниками и документацией Polly.

## Грани и подвохи

**1. Timeout кооперативный.** Проверено: операция с `Thread.Sleep(300)` под таймаутом 100 мс завершилась штатно за ~300 мс — токен игнорировался.

**2. Порядок стратегий.** Проверено: `retry` снаружи, `timeout` внутри — 3 запуска тела; обратный порядок даёт один общий таймаут.

**3. Состояние circuit breaker живёт в конвейере.** Новый конвейер на каждый вызов — breaker никогда не сработает. Конвейер — singleton/DI.

**4. Breaker и пороги.** Нужно `MinimumThroughput` вызовов за `SamplingDuration` (проверено: при 2 вызовах цепь разомкнулась; со значениями по умолчанию потребуется больше трафика).

**5. `BrokenCircuitException` — не операция.** Тело при разомкнутой цепи не выполняется (проверено: счётчик вызовов не растёт).

**6. Retry и побочные эффекты.** Повтор неидемпотентной операции создаёт дубликаты.

**7. Hedging умножает нагрузку.** Проверено: запущено 2 вызова ради одного ответа; для идемпотентных и дорогих по задержке запросов.

**8. Generic и non-generic конвейеры.** `ResiliencePipelineBuilder<T>` нужен для `Fallback` с результатом; при вызове иногда необходим явный `ExecuteAsync<T>` (иначе CS0411, проверено).

**9. `RateLimiter` — отдельный пакет.** `Polly.RateLimiting` (и `System.Threading.RateLimiting`); без него — ошибка компиляции (проверено).

**10. Исключения в `OnRetry`.** Обработчики событий не должны бросать — они влияют на выполнение конвейера.

> ⚠️ Проверить: поведение исключений внутри обработчиков событий стратегий в этом материале не проверялось.

## Компромиссы и альтернативы

| Подход | Плюсы | Минусы |
|---|---|---|
| Polly 8 | богатый набор стратегий, конвейеры, DI, телеметрия | нужно понимать порядок и состояние |
| `Microsoft.Extensions.Http.Resilience` | готовые настройки для `HttpClient` | только HTTP |
| Ручной цикл retry | нет зависимости | легко ошибиться, нет breaker |
| Service mesh | вне приложения | не знает бизнес-семантику |
| Очередь + повтор по расписанию | надёжно, асинхронно | другая архитектура |

## Вопросы с собеседований

<details><summary>1. Что такое Polly и какие стратегии есть в версии 8?</summary>

Библиотека отказоустойчивости: retry, timeout, circuit breaker, fallback, hedging, rate limiter; собираются в `ResiliencePipeline`.

</details>

<details><summary>2. Как работает retry с экспоненциальной задержкой и джиттером?</summary>

Задержка растёт степенью (20 → 40 → 80 мс, проверено без джиттера); джиттер добавляет случайный разброс, чтобы клиенты не повторяли синхронно.

</details>

<details><summary>3. Что произойдёт, когда попытки retry исчерпаны?</summary>

Последнее исключение пробрасывается (проверено: после 4 попыток).

</details>

<details><summary>4. Как работает circuit breaker?</summary>

Closed → при достаточной доле сбоев Open (вызовы отклоняются без выполнения), после `BreakDuration` Half-Open с пробным вызовом (проверено на примере).

</details>

<details><summary>5. Почему timeout не всегда прерывает операцию?</summary>

Он кооперативный: отменяет токен; код, игнорирующий токен, не прервётся (проверено).

</details>

<details><summary>6. Как порядок стратегий влияет на поведение?</summary>

Первая — внешняя: `retry`→`timeout` даёт таймаут на каждую попытку, `timeout`→`retry` — общий.

</details>

<details><summary>7. Как использовать Polly с DI?</summary>

`AddResiliencePipeline("name", …)` и `ResiliencePipelineProvider<string>.GetPipeline("name")` (проверено).

</details>

<details><summary>8. Что такое hedging и когда он опасен?</summary>

Параллельный запуск дублирующих вызовов при медленном первом; нагружает сервис, только для идемпотентных операций (проверено: 2 вызова).

</details>

<details><summary>9. Подвох: создаём `ResiliencePipeline` внутри метода на каждый вызов — breaker не размыкается. Почему?</summary>

Состояние breaker принадлежит экземпляру конвейера; новый конвейер — новое состояние. Нужен singleton.

</details>

<details><summary>10. Подвох: `await fb.ExecuteAsync(async ct => { throw … })` не компилируется (CS0411). Почему?</summary>

Для `ResiliencePipeline<T>` вывод типа результата невозможен из лямбды, которая только бросает; укажите `ExecuteAsync<string>(…)`.

</details>

<details><summary>11. Как получить результат без исключений?</summary>

`ExecuteOutcomeAsync` возвращает `Outcome<T>` с `Result` или `Exception` (проверено).

</details>

## Связанные материалы

- Назад: [продвинутый](advanced.ru.md). Проверьте себя: [квиз](quiz.ru.md) и [упражнения](exercises.ru.md).
- [21 · Архитектура приложения](../../modules/21-architecture/overview.ru.md) — каскадные сбои и bulkhead.
- [22 · Наблюдаемость](../../modules/22-observability/overview.ru.md) — метрики устойчивости.
- [Kubernetes](../../../kubernetes/overview.ru.md) — устойчивость на уровне инфраструктуры.

## Источники

- [Polly docs](https://www.pollydocs.org/)
- [Polly: Resilience context](https://www.pollydocs.org/advanced/resilience-context.html)
- [Polly: Telemetry](https://www.pollydocs.org/advanced/telemetry.html)
