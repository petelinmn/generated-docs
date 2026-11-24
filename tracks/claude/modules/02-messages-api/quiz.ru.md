---
title: "Квиз: Messages API"
lang: ru
track: claude
kind: module
doc: quiz
slug: 02-messages-api
part: 1
tags: [claude, messages-api, stop-reason, streaming, api-errors]
status: draft
updated: 2026-11-24
---

# Квиз: Messages API

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте сами, потом откройте ответ. Метка показывает уровень вопроса: 🟢 [базовая](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Какие поля обязательны в запросе `POST /v1/messages`?

- A) `model` и `messages`
- B) `model`, `max_tokens`, `messages`
- C) `model`, `system`, `messages`
- D) только `messages`

<details><summary>Ответ</summary>

**B.** Обязательны `model`, `max_tokens` и `messages`; `system` необязателен.
→ [Базовая теория](basics.ru.md#запрос-и-ответ)

</details>

## 2. 🟢 Как продолжить разговор с моделью?

- A) Отправить только новое сообщение: сервер помнит историю
- B) Отправить всю историю сообщений и новое сообщение
- C) Передать `conversation_id`
- D) Вызвать `client.messages.continue()`

<details><summary>Ответ</summary>

**B.** API не хранит состояние: клиент присылает всю историю при каждом запросе.
→ [Базовая теория](basics.ru.md#диалог-api-не-помнит-историю)

</details>

## 3. 🟢 Что означает `stop_reason == "max_tokens"`?

- A) Модель закончила ответ
- B) Ответ обрезан лимитом `max_tokens` и неполный
- C) Закончились токены на счёте
- D) Сработала стоп-последовательность

<details><summary>Ответ</summary>

**B.** Нужно увеличить лимит или использовать стриминг.
→ [Базовая теория](basics.ru.md#max_tokens-и-stop_reason)

</details>

## 4. 🟢 Как безопасно прочитать текст ответа?

- A) `message.content[0].text`
- B) Перебрать `message.content` и брать `.text` у блоков с `type == "text"`
- C) `str(message)`
- D) `message.text`

<details><summary>Ответ</summary>

**B.** `content` — список блоков разных типов; первым может оказаться `thinking` или `tool_use`.
→ [Базовая теория](basics.ru.md#запрос-и-ответ)

</details>

## 5. 🟡 Что вернёт `stream.get_final_message()`?

- A) Только текст
- B) Итоговый `Message`, как у обычного запроса
- C) Последнее событие потока
- D) Список событий

<details><summary>Ответ</summary>

**B.** SDK собирает сообщение из событий; это удобно для больших ответов без обработки событий.
→ [Продвинутая теория](advanced.ru.md#потоковая-выдача)

</details>

## 6. 🟡 Что произойдёт при `stop_sequences=["4"]` и ответе «1, 2, 3, 4, 5»?

- A) Ответ «1, 2, 3, 4», `stop_reason == "end_turn"`
- B) Ответ «1, 2, 3, » без стоп-строки, `stop_reason == "stop_sequence"`, `stop_sequence == "4"`
- C) Ошибка 400
- D) Ответ «1, 2, 3, 4, 5»

<details><summary>Ответ</summary>

**B.** Стоп-строка отсекается и возвращается отдельным полем.
→ [Продвинутая теория](advanced.ru.md#stop_sequences)

</details>

## 7. 🟡 Сколько раз SDK по умолчанию повторяет запрос при 429 или 5xx?

- A) Ни разу
- B) Один раз
- C) Два раза
- D) Бесконечно

<details><summary>Ответ</summary>

**C.** По умолчанию `max_retries = 2`, с экспоненциальной паузой и учётом `retry-after`.
→ [Продвинутая теория](advanced.ru.md#повторы-таймауты-и-параметры-клиента)

</details>

## 8. 🟡 Что произойдёт, если отправить `temperature` при версии API `2026-10-21`?

- A) Параметр применится
- B) Параметр будет проигнорирован
- C) Запрос вернёт 400
- D) Модель переключится на старую

<details><summary>Ответ</summary>

**C.** `temperature`, `top_p`, `top_k` удалены: запрос с ними даёт 400 на любой модели.
→ [Продвинутая теория](advanced.ru.md#параметры-удалённые-в-версии-api-2026-10-21)

</details>

## 9. 🟡 Выберите все верные утверждения

- [ ] A) `count_tokens` создаёт ответ модели
- [ ] B) Ошибка `RateLimitError` наследует `APIStatusError`
- [ ] C) `with_raw_response` даёт доступ к заголовкам ответа
- [ ] D) `message._request_id` содержит идентификатор запроса

<details><summary>Ответ</summary>

**B, C, D.** A неверно: `count_tokens` только оценивает размер запроса.
→ [Продвинутая теория](advanced.ru.md#request-id-и-заголовки-ответа)

</details>

## 10. 🔴 Найдите ошибку

> «Если поток начался с HTTP 200, ошибок больше быть не может».

<details><summary>Ответ</summary>

Ошибка может прийти событием `error` внутри потока уже после ответа 200 (например, `overloaded_error`).
→ [Подвох 10](expert.ru.md#10-ошибка-после-200-в-стриме)

</details>

## 11. 🔴 В каком порядке приходят события обычного потока?

- A) `message_start`, `content_block_start`, `content_block_delta`…, `content_block_stop`, `message_delta`, `message_stop`
- B) `content_block_start`, `message_start`, `message_stop`
- C) `message_delta`, `message_start`, `message_stop`
- D) `message_stop`, затем остальные

<details><summary>Ответ</summary>

**A.** `ping` и `error` возможны в любой момент.
→ [Протокол SSE](expert.ru.md#протокол-sse-потоковых-событий)

</details>

## 12. 🔴 Почему клиент не должен падать на неизвестном значении `stop_reason` или типе события?

- A) Это невозможно
- B) В рамках одной версии API Anthropic вправе добавлять новые значения перечислений и типы событий
- C) Это ошибка SDK
- D) Только при бета-заголовке

<details><summary>Ответ</summary>

**B.** Правила версионирования допускают расширение выходных значений.
→ [Внутреннее устройство](expert.ru.md#внутреннее-устройство)

</details>

## 13. 🔴 Открытый вопрос

Спроектируйте обёртку над Messages API для чат-бота: что хранить, что проверять в ответе и как обрабатывать сбои.

<details><summary>Ключевые мысли хорошего ответа</summary>

- Хранить историю `messages` (добавлять весь `content` ответа, включая блоки `thinking`) и `system` отдельно; следить за ростом контекста (`count_tokens`, компактизация).
- Читать ответ по блокам; проверять `stop_reason` (`max_tokens`, `refusal`, `pause_turn`).
- Для больших ответов — стриминг и `get_final_message()`.
- Ловить типизированные исключения, логировать `request_id`, полагаться на повторы SDK, задать таймаут; не использовать `temperature`.

</details>

→ [Экспертная теория](expert.ru.md)
