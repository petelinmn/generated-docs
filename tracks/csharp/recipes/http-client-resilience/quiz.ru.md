---
title: "Квиз: Устойчивый HttpClient"
lang: ru
track: csharp
kind: recipe
doc: quiz
slug: http-client-resilience
part: 2
tags: [httpclient, retry, circuit-breaker, timeout, ihttpclientfactory]
status: draft
updated: 2026-11-22
---

# Квиз: Устойчивый HttpClient

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте, потом открывайте ответ. Метка показывает уровень вопроса: 🟢 [минимум](basics.ru.md) · 🟡 [production](advanced.ru.md) · 🔴 [масштаб и грани](expert.ru.md).

## 1. 🟢 Как правильно получать `HttpClient` в приложении?

- A) `new HttpClient()` на каждый запрос
- B) через `IHttpClientFactory` (`AddHttpClient`)
- C) один статический экземпляр навсегда
- D) через `Activator`

<details><summary>Ответ</summary>

**B.** Фабрика управляет пулами соединений и ротацией обработчиков (DNS). `new` на запрос исчерпывает сокеты, статический клиент не подхватывает смену DNS.
→ [Минимум](basics.ru.md#частые-ошибки-новичка)

</details>

## 2. 🟢 Чем закончится вызов сервера, отвечающего `503` два раза и затем `200`, со стандартным обработчиком?

- A) `503`
- B) `200` после 3 вызовов
- C) исключение
- D) `200` после 1 вызова

<details><summary>Ответ</summary>

**B.** Проверено: два ретрая, итог `200` за 3 вызова; обычный клиент вернул `503` за 1 вызов.
→ [Минимум](basics.ru.md#минимальное-решение)

</details>

## 3. 🟢 Сколько ретраев по умолчанию делает стандартный обработчик?

- A) 0
- B) 1
- C) 3
- D) 10

<details><summary>Ответ</summary>

**C.** `MaxRetryAttempts = 3`, экспоненциальная задержка от 2 с с джиттером. На постоянном `500` проверено: 4 вызова (1 + 3).
→ [Минимум](basics.ru.md#значения-по-умолчанию)

</details>

## 4. 🟢 Ретраится ли ответ `400`?

- A) да, 3 раза
- B) нет, возвращается сразу
- C) да, один раз
- D) зависит от размера тела

<details><summary>Ответ</summary>

**B.** Ретраятся транзиентные сбои (сетевые ошибки, таймауты, `5xx`, `408`, `429`); проверено: на `400` был один вызов.
→ [Минимум](basics.ru.md#значения-по-умолчанию)

</details>

## 5. 🟡 Что получит код, когда circuit breaker разомкнут?

- A) `HttpResponseMessage` с `503`
- B) `BrokenCircuitException`, запрос на сервер не уходит
- C) `TaskCanceledException`
- D) ничего, зависнет

<details><summary>Ответ</summary>

**B.** Проверено: вызовы 2–5 — `BrokenCircuitException` без обращения к серверу; через `BreakDuration` пробный вызов замкнул цепь.
→ [Production](advanced.ru.md#circuit-breaker)

</details>

## 6. 🟡 Что случится при `o.Retry.MaxRetryAttempts = 0` в стандартном обработчике?

- A) ретраи отключатся
- B) `OptionsValidationException` при создании клиента
- C) 1 ретрай
- D) обработчик проигнорирует значение

<details><summary>Ответ</summary>

**B.** Допустимо от 1 и выше (проверено). Для отсутствия ретраев — свой конвейер через `AddResilienceHandler` без `AddRetry`.
→ [Production](advanced.ru.md#свой-конвейер)

</details>

## 7. 🟡 Выберите все верные утверждения

- [ ] A) ретраить `POST` безопасно без каких-либо мер
- [ ] B) `HttpClient.Timeout` по умолчанию — 100 с
- [ ] C) типизированный клиент регистрируется через `AddHttpClient<T>()`
- [ ] D) circuit breaker защищает сервис от лишней нагрузки при его падении

<details><summary>Ответ</summary>

**B, C, D.** A неверно: без ключа идемпотентности возможны дубликаты. Таймаут 100 с проверен (`00:01:40`).
→ [Production](advanced.ru.md#идемпотентность-и-ретраи)

</details>

## 8. 🟡 Открытый вопрос: как вы организуете вызов платёжного API, который иногда отвечает `503`?

<details><summary>Ключевые пункты хорошего ответа</summary>

- `Idempotency-Key` на каждую операцию, ретраи с джиттером;
- таймаут попытки и общий таймаут, circuit breaker;
- обработка `BrokenCircuitException`/`TimeoutRejectedException` и деградация (статус «в обработке»);
- метрики и логи ретраев, алерты на разомкнутую цепь.

</details>

## 9. 🔴 Почему circuit breaker не размыкается в тесте, хотя сервис всегда отвечает `500`?

- A) нужен `Retry-After`
- B) значения по умолчанию требуют минимум 100 запросов за 30 с — порог не достигнут
- C) circuit breaker работает только в production
- D) `500` не считается сбоем

<details><summary>Ответ</summary>

**B.** В тесте уменьшите `MinimumThroughput` и окно (проверено: при `MinimumThroughput = 2` цепь разомкнулась).
→ [Масштаб и грани](expert.ru.md#грани-и-подвохи)

</details>

## 10. 🔴 Что опасного в 100 экземплярах с 3 ретраями при сбое зависимости?

- A) ничего
- B) шторм ретраев: до 4× нагрузки на уже больной сервис
- C) ретраи отключатся сами
- D) только лишний лог

<details><summary>Ответ</summary>

**B.** Защита: джиттер, лимит ретраев, circuit breaker, `Retry-After`, rate limiting на стороне сервиса.
→ [Масштаб и грани](expert.ru.md#масштаб)

</details>
