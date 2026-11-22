---
title: "Устойчивый HttpClient: production"
lang: ru
track: csharp
kind: recipe
doc: advanced
slug: http-client-resilience
part: 2
tags: [httpclient, typed-client, circuit-breaker, polly, idempotency, resilience]
versions: ".NET 11, C# 15; Microsoft.Extensions.Http.Resilience 11.0.1"
status: draft
updated: 2026-11-22
---

# Устойчивый HttpClient: production

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Минимум](basics.ru.md) · **2. Production** · [3. Масштаб и грани](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [минимальное решение](basics.ru.md). **Время:** ~45 мин.

## Типизированный клиент

Вместо строкового имени — класс, которому фабрика сама подставляет настроенный `HttpClient`:

```csharp
services.AddHttpClient<GitClient>(c => c.BaseAddress = new Uri(baseUrl))
        .AddStandardResilienceHandler();

class GitClient(HttpClient http)
{
    public async Task<int> GetAsync() { var r = await http.GetAsync("/dep"); return (int)r.StatusCode; }
}
```

Клиент регистрируется как transient; не храните его в singleton дольше срока жизни обработчика (иначе потеряется ротация DNS) — внедряйте `GitClient` туда, где он нужен, или используйте `IHttpClientFactory`.

## Circuit breaker

Если сервис долго недоступен, ретраи только добавляют ему нагрузку. Circuit breaker «размыкает цепь»: после серии сбоев вызовы сразу отклоняются, не доходя до сервера. Проверено (`MinimumThroughput = 2`, `FailureRatio = 0.5`, `BreakDuration = 1 с`; сервер отвечает `500`; один ретрай):

| Вызов | Результат |
|---|---|
| 1 | `500`, сервер получил запрос (две попытки провалились — цепь разомкнулась) |
| 2–5 | `BrokenCircuitException`, **сервер запрос не получил** |
| через 1,2 с (сервер вылечен) | `200` — полуоткрытое состояние пропустило пробный вызов, цепь замкнулась |
| следующий | `200` |

Ловите `BrokenCircuitException` (`Polly.CircuitBreaker`) и отвечайте деградацией: кэш, значение по умолчанию, понятная ошибка.

## Свой конвейер

Стандартный обработчик подходит большинству. Свой набор стратегий — `AddResilienceHandler`:

```csharp
services.AddHttpClient("custom", c => c.BaseAddress = new Uri(baseUrl))
  .AddResilienceHandler("my", p =>
  {
      p.AddTimeout(TimeSpan.FromSeconds(1));
      p.AddRetry(new HttpRetryStrategyOptions
      {
          MaxRetryAttempts = 2, Delay = TimeSpan.FromMilliseconds(20),
          BackoffType = DelayBackoffType.Constant, UseJitter = false
      });
  });
```

Проверено: на постоянном `500` — `500` после 3 вызовов (1 + 2 ретрая). Порядок стратегий важен: внешняя оборачивает внутренние.

Настройки стандартного обработчика — через лямбду: `AddStandardResilienceHandler(o => { o.Retry.MaxRetryAttempts = 5; o.AttemptTimeout.Timeout = …; })`. Параметры валидируются при первом создании клиента: проверено, что `Retry.MaxRetryAttempts = 0` — ошибка `OptionsValidationException` («must be between 1 and …»). Чтобы отключить ретраи, берите свой конвейер без `AddRetry`.

## Идемпотентность и ретраи

Повтор безопасен только для идемпотентных операций: `GET`, `PUT`, `DELETE`. Стандартный обработчик по умолчанию ретраит и `POST` (проверено: `POST` с `503` — 4 вызова), что может создать дубликаты (сервер выполнил запрос, но ответ потерялся). Для небезопасных операций:

- передавайте ключ идемпотентности (`Idempotency-Key`), который сервер дедуплицирует;
- или отключите ретраи для таких запросов (`DisableForUnsafeHttpMethods()` у `HttpRetryStrategyOptions`, либо отдельный клиент без ретраев).

> ⚠️ Проверить: имя и поведение `DisableForUnsafeHttpMethods()` — сверьте с документацией вашей версии пакета.

## Тестирование

Не ходите в реальную сеть: подставьте `HttpMessageHandler`-заглушку или поднимите тестовый сервер ([17 · Интеграционные тесты](../../modules/17-testing-integration/overview.ru.md)). Сервер, отвечающий `503` N раз и затем `200`, — самый простой способ проверить ретраи (так сделаны примеры выше).

## Best practices

| Правило | Почему |
|---|---|
| `IHttpClientFactory`, не `new HttpClient()` | пулы соединений и ротация DNS |
| Таймаут попытки и общий таймаут | не висеть 100 с |
| Ретраи с экспонентой и джиттером | не усиливать шторм |
| Circuit breaker | не добивать лежащий сервис |
| Ретраи только идемпотентного | нет дубликатов |
| Деградация вместо падения | устойчивость пользовательского сценария |
| Метрики и логи ретраев | видно, что сервис «лечится» ретраями ([22](../../modules/22-observability/overview.ru.md)) |

## Связанные материалы

- Назад: [минимум](basics.ru.md). Дальше: [масштаб и грани](expert.ru.md).
- [17 · Интеграционные тесты](../../modules/17-testing-integration/overview.ru.md)
- [22 · Наблюдаемость](../../modules/22-observability/overview.ru.md)
- [Фоновые задачи](../background-jobs/overview.ru.md) — вызовы внешних сервисов из воркеров.

## Источники

- [Build resilient HTTP apps](https://learn.microsoft.com/dotnet/core/resilience/http-resilience)
- [Polly: circuit breaker](https://www.pollydocs.org/strategies/circuit-breaker.html)
- [Polly: retry](https://www.pollydocs.org/strategies/retry.html)
