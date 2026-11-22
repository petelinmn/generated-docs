---
title: "Квиз: Rate limiting"
lang: ru
track: csharp
kind: recipe
doc: quiz
slug: rate-limiting
part: 2
tags: [rate-limiting, throttling, http-429, token-bucket]
status: draft
updated: 2026-11-22
---

# Квиз: Rate limiting

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает, из какого уровня вопрос: 🟢 [минимум](basics.ru.md) · 🟡 [production](advanced.ru.md) · 🔴 [масштаб и грани](expert.ru.md).

## 1. 🟢 Какой код ответа означает превышение лимита?

- A) 401
- B) 403
- C) 429
- D) 503

<details><summary>Ответ</summary>

**C.** `429 Too Many Requests`; код задаётся `RejectionStatusCode` (в примере — `StatusCodes.Status429TooManyRequests`).
→ [Минимум](basics.ru.md#задача)

</details>

## 2. 🟢 Сколько из 5 одновременных запросов пройдёт при `PermitLimit = 3` в окне 2 с?

- A) все 5
- B) 3, остальные получат 429
- C) 1
- D) ни одного

<details><summary>Ответ</summary>

**B.** Проверено: три `200`, два `429`; после окончания окна запрос снова проходит.
→ [Минимум](basics.ru.md#минимальное-решение)

</details>

## 3. 🟢 Что нужно сделать, чтобы политика реально действовала на точку?

- A) ничего: достаточно `AddRateLimiter`
- B) вызвать `app.UseRateLimiter()` и добавить `RequireRateLimiting("policy")`
- C) перезапустить сервер
- D) установить NuGet-пакет

<details><summary>Ответ</summary>

**B.** Без `UseRateLimiter()` middleware не работает, без `RequireRateLimiting` (или глобального лимитера) точка не ограничена: `/free` пропустила 10 из 10.
→ [Минимум](basics.ru.md#минимальное-решение)

</details>

## 4. 🟢 Какой алгоритм допускает всплеск, но ограничивает среднюю скорость?

- A) fixed window
- B) token bucket
- C) concurrency
- D) нет такого

<details><summary>Ответ</summary>

**B.** Проверено: ёмкость 2, +1 токен/с — два запроса сразу, третий отклонён, через 1,1 с снова `200`.
→ [Минимум](basics.ru.md#алгоритмы)

</details>

## 5. 🟡 Как сделать лимит отдельным для каждого пользователя?

- A) несколько политик вручную
- B) политика с `RateLimitPartition` по ключу (пользователь/ключ API/IP)
- C) нельзя
- D) через кэш

<details><summary>Ответ</summary>

**B.** Проверено: `ann` — `200 200 429`, `bob` — `200`.
→ [Production](advanced.ru.md#ограничение-по-пользователю)

</details>

## 6. 🟡 Какой заголовок сообщает клиенту, когда повторить?

- A) `Retry-After`
- B) `Expires`
- C) `Cache-Control`
- D) `X-Wait`

<details><summary>Ответ</summary>

**A.** Его выставляют в `OnRejected` из метаданных аренды (`MetadataName.RetryAfter`); проверено: `Retry-After=1` и `4`.
→ [Production](advanced.ru.md#ответ-клиенту-retry-after)

</details>

## 7. 🟡 Выберите все верные утверждения

- [ ] A) concurrency limiter ограничивает число одновременных запросов
- [ ] B) большой `QueueLimit` безвреден
- [ ] C) health checks стоит исключать из лимитов
- [ ] D) за прокси без `UseForwardedHeaders` все клиенты могут делить один IP

<details><summary>Ответ</summary>

**A, C, D.** B неверно: очередь удерживает соединения и память.
→ [Production](advanced.ru.md#очереди-и-выбор-алгоритма)

</details>

## 8. 🟡 Открытый вопрос: какие лимиты вы поставите на `/login` и на чтение каталога?

<details><summary>Ключевые пункты хорошего ответа</summary>

- `/login`: строгий лимит по IP и логину (несколько попыток в минуту), `Retry-After`, логирование отказов;
- чтение: мягкий лимит по пользователю/ключу, допускающий всплески (token bucket);
- разные политики для разных рисков, исключения для health checks;
- метрики и алерты по числу 429.

</details>

## 9. 🔴 Почему встроенного лимитера недостаточно при 4 экземплярах приложения?

- A) он не работает при нескольких экземплярах
- B) счётчики в памяти каждого экземпляра: клиент может получить до 4 × лимит; нужны шлюз или общее хранилище
- C) он работает только в Development
- D) из-за HTTPS

<details><summary>Ответ</summary>

**B.** Лимитер локален процессу. Для точной квоты — лимит на шлюзе или счётчики в Redis.
→ [Масштаб и грани](expert.ru.md#масштаб-несколько-экземпляров)

</details>

## 10. 🔴 Клиент делает 6 запросов подряд при лимите 3 за окно и ни одного отказа. Что возможно?

- A) баг клиента
- B) стык окон fixed window (3 в конце и 3 в начале следующего) или несколько экземпляров с независимыми счётчиками
- C) лимит выключен навсегда
- D) 429 не существует

<details><summary>Ответ</summary>

**B.** Fixed window допускает до 2N на стыке; sliding window/token bucket сглаживают это.
→ [Масштаб и грани](expert.ru.md#грани-и-подвохи)

</details>
