---
title: "Квиз: паттерны надёжности"
lang: ru
track: system-design
kind: module
doc: quiz
slug: 09-reliability-patterns
part: 2
tags: [system-design, resilience, retry, circuit-breaker, bulkhead, rate-limiting, load-shedding]
versions: ".NET 11, C# 15; Polly.Core 8.9.1; System.Threading.RateLimiting 11.0.0"
status: draft
updated: 2026-11-26
---

# Квиз: паттерны надёжности

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает уровень: 🟢 [базовая теория](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 `MaxRetryAttempts = 3` — это сколько вызовов максимум?

- A) 2
- B) 3
- C) 4
- D) 6

<details><summary>Ответ</summary>

**C.** Первый вызов и три повтора. → [Базовая теория](basics.ru.md)

</details>

## 2. 🟢 Что обычно не повторяют?

- A) Таймаут
- B) 503
- C) Ошибку проверки данных
- D) Обрыв соединения

<details><summary>Ответ</summary>

**C.** Повтор даст то же. → [Базовая теория](basics.ru.md)

</details>

## 3. 🟢 Зачем jitter?

- A) Ускорить повторы
- B) Развести повторы разных клиентов по времени
- C) Шифрование
- D) Сжатие

<details><summary>Ответ</summary>

**B.** → [Базовая теория](basics.ru.md)

</details>

## 4. 🟢 Что бросает Polly при истечении таймаута?

- A) `TimeoutException`
- B) `TimeoutRejectedException`
- C) `BrokenCircuitException`
- D) ничего

<details><summary>Ответ</summary>

**B.** → [Базовая теория](basics.ru.md)

</details>

## 5. 🟡 Что делает открытый circuit breaker?

- A) Повторяет чаще
- B) Отклоняет вызов, не обращаясь к зависимости
- C) Увеличивает таймаут
- D) Пишет в очередь

<details><summary>Ответ</summary>

**B.** → [Продвинутая теория](advanced.ru.md)

</details>

## 6. 🟡 Что выведет код?

```csharp
using var limiter = Limits.TokenBucket(burst: 3, perPeriod: 1);
Console.WriteLine(Enumerable.Range(0, 10).Count(_ => limiter.AttemptAcquire().IsAcquired));
```

<details><summary>Ответ</summary>

```text
3
```

Пополнение выключено, доступен только начальный запас. → [Продвинутая теория](advanced.ru.md)

</details>

## 7. 🟡 Выбери все верные утверждения

- [ ] A) Concurrency limiter ограничивает одновременные операции
- [ ] B) Лимит в памяти одного экземпляра общий для кластера
- [ ] C) Token bucket допускает всплеск
- [ ] D) Bulkhead изолирует ресурсы зависимостей

<details><summary>Ответ</summary>

**A, C, D.** → [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Что внутри стандартного обработчика ближе всего к самому вызову?

- A) rate limiter
- B) retry
- C) таймаут попытки
- D) общий таймаут

<details><summary>Ответ</summary>

**C.** → [Продвинутая теория](advanced.ru.md)

</details>

## 9. 🔴 Что произойдёт?

```csharp
var to = new ResiliencePipelineBuilder().AddTimeout(TimeSpan.FromMilliseconds(100)).Build();
await to.ExecuteAsync(async ct => await Task.Delay(600));
```

<details><summary>Ответ</summary>

Вызов успешно завершится примерно через 600 мс: токен проигнорирован. → [Экспертная теория](expert.ru.md)

</details>

## 10. 🔴 Открытый вопрос

Зависимость отвечает за 10 с вместо 50 мс. Что вы сделаете?

<details><summary>Ключевые пункты ответа</summary>

- Таймаут по реальным перцентилям.
- Bulkhead и circuit breaker.
- Деградация и load shedding.
- Не увеличивать нагрузку повторами; наблюдать.

</details>
