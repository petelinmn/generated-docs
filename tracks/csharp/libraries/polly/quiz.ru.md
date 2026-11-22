---
title: "Квиз: Polly"
lang: ru
track: csharp
kind: library
doc: quiz
slug: polly
part: 2
tags: [polly, resilience, retry, circuit-breaker]
status: draft
updated: 2026-11-22
---

# Квиз: Polly

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте, потом открывайте ответ. Метка: 🟢 [основы](basics.ru.md) · 🟡 [продвинутый](advanced.ru.md) · 🔴 [эксперт](expert.ru.md).

## 1. 🟢 Сколько попыток выполнится при `MaxRetryAttempts = 3` и постоянном сбое?

- A) 3
- B) 4
- C) 1
- D) бесконечно

<details><summary>Ответ</summary>

**B.** Проверено: 1 исходная + 3 повтора = 4, затем исключение пробрасывается.
→ [Основы](basics.ru.md#retry)

</details>

## 2. 🟢 Ретраится ли исключение типа, не указанного в `ShouldHandle`?

- A) да
- B) нет — одна попытка и исключение
- C) да, один раз
- D) зависит от задержки

<details><summary>Ответ</summary>

**B.** Проверено: `ArgumentException` — 1 попытка.
→ [Основы](basics.ru.md#retry)

</details>

## 3. 🟢 Прервёт ли timeout операцию с `Thread.Sleep(300)`, не смотрящую на токен?

- A) да
- B) нет — таймаут кооперативный
- C) да, всегда
- D) только в `Debug`

<details><summary>Ответ</summary>

**B.** Проверено: завершилась штатно за ~300 мс.
→ [Основы](basics.ru.md#timeout)

</details>

## 4. 🟢 Что получит код при разомкнутой цепи?

- A) результат операции
- B) `BrokenCircuitException`, тело не выполняется
- C) `TimeoutRejectedException`
- D) `null`

<details><summary>Ответ</summary>

**B.** Проверено: счётчик вызовов не вырос.
→ [Основы](basics.ru.md#circuit-breaker)

</details>

## 5. 🟡 `AddRetry(...).AddTimeout(50 мс)`: сколько раз запустится тело при постоянном долгом выполнении и 2 повторах?

- A) 1
- B) 2
- C) 3
- D) 50

<details><summary>Ответ</summary>

**C.** Проверено: 3 запуска — retry снаружи, каждый получил свой таймаут.
→ [Продвинутый](advanced.ru.md#композиция-стратегий)

</details>

## 6. 🟡 Как получить конвейер из DI?

- A) `new ResiliencePipeline()`
- B) `AddResiliencePipeline("name", …)` и `ResiliencePipelineProvider<string>.GetPipeline("name")`
- C) через `static`
- D) нельзя

<details><summary>Ответ</summary>

**B.** Проверено.
→ [Продвинутый](advanced.ru.md#dependency-injection)

</details>

## 7. 🟡 Выберите все верные утверждения

- [ ] A) hedging запускает дополнительные вызовы, если первый медленный
- [ ] B) `ExecuteOutcomeAsync` бросает исключение при сбое
- [ ] C) rate limiter в Polly — отдельный пакет `Polly.RateLimiting`
- [ ] D) конвейер можно переиспользовать из нескольких потоков

<details><summary>Ответ</summary>

**A, C, D.** B неверно: возвращает `Outcome<T>` с `Exception` (проверено).
→ [Продвинутый](advanced.ru.md#результат-без-исключений)

</details>

## 8. 🟡 Открытый вопрос: как защитить вызов нестабильного сервиса?

<details><summary>Ключевые пункты хорошего ответа</summary>

- общий таймаут → retry с экспонентой и джиттером для транзиентных сбоев → circuit breaker → таймаут попытки;
- fallback/деградация на `BrokenCircuitException` и таймаут;
- идемпотентность; логирование срабатываний;
- конвейер — singleton.

</details>

## 9. 🔴 Почему breaker не размыкается, когда конвейер создаётся на каждый вызов?

- A) баг Polly
- B) состояние breaker принадлежит экземпляру конвейера
- C) нужен `Retry`
- D) нужен `Fallback`

<details><summary>Ответ</summary>

**B.** Конвейер должен быть singleton/из DI.
→ [Эксперт](expert.ru.md#грани-и-подвохи)

</details>

## 10. 🔴 Чем опасен hedging?

- A) ничем
- B) умножает нагрузку на сервис и годится только для идемпотентных операций
- C) отключает retry
- D) ломает токены

<details><summary>Ответ</summary>

**B.** Проверено: запущено 2 вызова ради одного ответа.
→ [Эксперт](expert.ru.md#грани-и-подвохи)

</details>
