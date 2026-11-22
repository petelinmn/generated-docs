---
title: "Устойчивый HttpClient: масштаб и грани"
lang: ru
track: csharp
kind: recipe
doc: expert
slug: http-client-resilience
part: 2
tags: [httpclient, retry-storm, hedging, bulkhead, dns, interview-questions]
versions: ".NET 11, C# 15; Microsoft.Extensions.Http.Resilience 11.0.1"
status: draft
updated: 2026-11-22
---

# Устойчивый HttpClient: масштаб и грани

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Минимум](basics.ru.md) · [2. Production](advanced.ru.md) · **3. Масштаб и грани** · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [минимум](basics.ru.md) и [production](advanced.ru.md). **Время:** ~30 мин.

## Масштаб

- **Шторм ретраев.** 100 экземпляров × 3 ретрая = до 4× нагрузки на уже больной сервис. Защита: джиттер, ограничение ретраев, circuit breaker, уважение `Retry-After` и (на стороне сервиса) [rate limiting](../rate-limiting/overview.ru.md).
- **Каскадные сбои.** Таймауты складываются: если A ждёт B 30 с, а B ждёт C 30 с, поток держится минутами. Бюджет времени всей цепочки должен быть меньше, чем у вызывающего.
- **Bulkhead.** Изолируйте пулы: отдельный клиент и лимит параллелизма на каждую зависимость, чтобы одна медленная не съела все потоки/соединения.
- **Hedging.** Для задержечувствительных идемпотентных вызовов — второй запрос на другой экземпляр, если первый долго не отвечает (`AddStandardHedgingHandler`).

> ⚠️ Проверить: `AddStandardHedgingHandler` и его настройки в этом материале не запускались — сверьтесь с документацией перед использованием.

## Грани и подвохи

**1. `MaxRetryAttempts = 0` запрещён.** Проверено: `OptionsValidationException` («must be between 1 and …») при первом создании клиента, а не при регистрации.

**2. Circuit breaker «не срабатывает» в тестах.** Значения по умолчанию (`MinimumThroughput = 100`, окно 30 с) требуют сотню запросов; в тестах уменьшайте пороги (проверено: при `MinimumThroughput = 2` цепь разомкнулась после первого вызова с двумя попытками).

**3. Исключение вместо ответа.** При разомкнутой цепи — `BrokenCircuitException`, при таймауте — `TimeoutRejectedException` (проверено), а не `HttpResponseMessage`. Код должен ловить их, а не только смотреть на статус.

**4. Итоговый ответ после исчерпания ретраев.** Проверено: на постоянном `500` вернулся последний `500` (4 вызова), исключения нет — `EnsureSuccessStatusCode` решает вы.

**5. Большинство `4xx` не ретраится.** Проверено: `400` и `404` — один вызов, но `408` и `429` ретраятся (по 4 вызова). Для `429` учитывайте `Retry-After` и не усугубляйте перегрузку.

**6. Время жизни обработчика.** Фабрика пересоздаёт `HttpMessageHandler` (по умолчанию раз в 2 минуты), чтобы подхватывать DNS; клиент, захваченный singleton-ом, этого лишается.

**7. `HttpClient.Timeout` и конвейер.** Таймаут по умолчанию — 100 с (проверено): если он меньше суммы попыток с задержками, клиент оборвёт ретраи раньше конвейера.

**8. Повторное чтение тела.** Тело запроса с `StreamContent` нельзя отправить дважды; для ретраев используйте буферизуемое содержимое.

**9. Ретрай `POST` без идемпотентности.** Дубликаты заказов/платежей ([production](advanced.ru.md#идемпотентность-и-ретраи)).

## Компромиссы и альтернативы

| Подход | Плюсы | Минусы |
|---|---|---|
| `AddStandardResilienceHandler` | готовые разумные значения | универсальные, подгонять под сервис |
| `AddResilienceHandler` | полный контроль, Polly v8 | больше кода |
| Ручной Polly (`ResiliencePipeline`) | не только HTTP | нет интеграции с фабрикой «из коробки» |
| Service mesh (Istio, Linkerd) | ретраи/CB на инфраструктуре | не видит бизнес-семантику |
| Очередь вместо синхронного вызова | сбой не виден пользователю | асинхронность, [фоновые задачи](../background-jobs/overview.ru.md) |

## Вопросы с собеседований

<details><summary>1. Почему нельзя создавать `new HttpClient()` на каждый запрос?</summary>

Каждый экземпляр держит собственный пул соединений: при частом создании сокеты остаются в `TIME_WAIT`, возможно исчерпание портов. Используйте `IHttpClientFactory`.

</details>

<details><summary>2. А почему плох один статический `HttpClient`?</summary>

Соединения живут вечно и не подхватывают смену DNS. Фабрика ротирует обработчики; альтернатива — `SocketsHttpHandler.PooledConnectionLifetime`.

</details>

<details><summary>3. Что входит в стандартный resilience-обработчик?</summary>

Общий таймаут, ретраи, circuit breaker, таймаут попытки (и ограничитель параллелизма). Значения по умолчанию проверены: 3 ретрая, экспонента с джиттером от 2 с, таймаут попытки 10 с, общий 30 с.

</details>

<details><summary>4. Какие ответы ретраятся по умолчанию?</summary>

Сетевые исключения, таймауты, `5xx`, `408`, `429`. `400` и `404` не ретраятся (проверено).

</details>

<details><summary>5. Как работает circuit breaker?</summary>

Closed → при доле сбоев выше порога на достаточном трафике переходит в Open: вызовы отклоняются сразу; через `BreakDuration` — Half-Open: пробный вызов; успех замыкает цепь. Проверено на практике.

</details>

<details><summary>6. Что получит код при разомкнутой цепи?</summary>

`BrokenCircuitException`, запрос на сервер не уходит (проверено).

</details>

<details><summary>7. Зачем джиттер?</summary>

Чтобы клиенты не повторяли запросы синхронно (thundering herd) — случайный разброс задержек.

</details>

<details><summary>8. Безопасно ли ретраить `POST`?</summary>

Только с ключом идемпотентности: иначе дубликаты, когда сервер выполнил запрос, но ответ потерялся.

</details>

<details><summary>9. Подвох: `MaxRetryAttempts = 0`, чтобы отключить ретраи в стандартном обработчике. Что произойдёт?</summary>

`OptionsValidationException` при создании клиента (проверено). Нужен свой конвейер без `AddRetry`.

</details>

<details><summary>10. Подвох: в тесте разомкнуть circuit breaker не удаётся, хотя сервис всегда отвечает `500`. Почему?</summary>

Значения по умолчанию (минимум 100 запросов за 30 с, доля сбоев 10%) не достигнуты; в тесте задайте `MinimumThroughput` и окно поменьше.

</details>

## Связанные материалы

- Назад: [production](advanced.ru.md). Проверьте себя: [квиз](quiz.ru.md) и [упражнения](exercises.ru.md).
- [21 · Архитектура приложения](../../modules/21-architecture/overview.ru.md) — границы сервисов и каскадные сбои.
- [Rate limiting](../rate-limiting/overview.ru.md) — серверная сторона `429` и `Retry-After`.
- [Kubernetes](../../../kubernetes/overview.ru.md) — service mesh и ретраи на уровне инфраструктуры.

## Источники

- [Build resilient HTTP apps](https://learn.microsoft.com/dotnet/core/resilience/http-resilience)
- [Polly: hedging](https://www.pollydocs.org/strategies/hedging.html)
- [Guidelines for using HttpClient](https://learn.microsoft.com/dotnet/fundamentals/networking/http/httpclient-guidelines)
