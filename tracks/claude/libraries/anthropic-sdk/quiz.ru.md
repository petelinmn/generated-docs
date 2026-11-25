---
title: "Anthropic SDK: квиз"
lang: ru
track: claude
kind: library
doc: quiz
slug: anthropic-sdk
part: 2
tags: [claude, sdk, anthropic-sdk, python, typescript]
versions: "anthropic 2.14.0; @anthropic-ai/sdk 1.14.0"
status: draft
updated: 2026-11-25
---

# Anthropic SDK: квиз

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте сами, затем раскройте ответ. 🟢 [базовый](basics.ru.md) · 🟡 [продвинутый](advanced.ru.md) · 🔴 [экспертный](expert.ru.md).

## 1. 🟢 Как называется Python-пакет клиента?

- A) `claude-sdk`
- B) `anthropic-api`
- C) `anthropic`
- D) `claude`

<details><summary>Ответ</summary>

**C.** Клиент API устанавливается как `anthropic`. `claude-agent-sdk` — другой пакет, для агентов; остальные имена к клиенту не относятся.
→ [Базовая теория](basics.ru.md)

</details>

## 2. 🟢 Откуда клиент по умолчанию берёт ключ?

- A) из `ANTHROPIC_API_KEY`
- B) из `.env` в любом случае
- C) из `~/.ssh`
- D) спрашивает в терминале

<details><summary>Ответ</summary>

**A.** SDK читает переменную окружения. Файл `.env` сам по себе не загружается, и интерактивного ввода нет.
→ [Базовая теория](basics.ru.md)

</details>

## 3. 🟢 Как получить идентификатор запроса из ответа?

- A) `message.id`
- B) `message.usage`
- C) `client.request_id`
- D) `message._request_id`

<details><summary>Ответ</summary>

**D.** `_request_id` берётся из заголовка `request-id`. `message.id` — идентификатор сообщения, `usage` — токены, у клиента такого поля нет.
→ [Базовая теория](basics.ru.md)

</details>

## 4. 🟢 Что даёт `messages.stream(...)` по сравнению с `stream=True`?

- A) отключает повторы
- B) накапливает итоговое сообщение
- C) стоит дешевле
- D) не требует ключа

<details><summary>Ответ</summary>

**B.** Помощник собирает события в сообщение и даёт `text_stream`. На цену, авторизацию и повторы выбор не влияет.
→ [Базовая теория](basics.ru.md)

</details>

## 5. 🟡 Как поменять таймаут только для одного вызова?

- A) параметр `max_tokens`
- B) `client.with_options(timeout=...)`
- C) изменить `client.timeout` и вернуть
- D) `ANTHROPIC_LOG`

<details><summary>Ответ</summary>

**B.** `with_options` возвращает копию. Изменение общего клиента затронет параллельные запросы, а лог и `max_tokens` таймаут не задают.
→ [Продвинутая теория](advanced.ru.md)

</details>

## 6. 🟡 Что делает `for m in client.models.list(limit=2)`?

- A) возвращает только две модели
- B) бросает исключение на третьей
- C) делает один запрос всегда
- D) обходит все страницы

<details><summary>Ответ</summary>

**D.** `limit` — размер страницы, а итерация догружает следующие. Только `page.data` ограничен первой.
→ [Продвинутая теория](advanced.ru.md)

</details>

## 7. 🟡 Что происходит с исключением внутри `@beta_tool` в tool runner?

- [ ] A) уходит модели как `tool_result` с `is_error`
- [ ] B) его текст видит модель
- [ ] C) сразу падает ваш код
- [ ] D) молча игнорируется

<details><summary>Ответ</summary>

**A, B.** Runner превращает исключение в результат с ошибкой, содержащий `repr`. Цикл продолжается, поэтому это не падение и не игнорирование.
→ [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Что пишет `ANTHROPIC_LOG=debug`?

- A) ничего без handler в коде
- B) только ключ
- C) в том числе тексты сообщений
- D) только код ответа

<details><summary>Ответ</summary>

**C.** Debug выводит параметры запроса целиком. Уровень info даёт строку на запрос; ключ в проверке не выводился.
→ [Продвинутая теория](advanced.ru.md)

</details>

## 9. 🔴 Что делает tool runner на Claude API без `thread=False`?

- A) шлёт всю историю каждый раз
- B) сохраняет её на диск клиента
- C) отключает инструменты
- D) хранит разговор на сервере в thread

<details><summary>Ответ</summary>

**D.** Следующие запросы несут только новые сообщения и ссылку на предыдущее. Полную историю слали клиенты Bedrock и Vertex.
→ [Экспертная теория](expert.ru.md)

</details>

## 10. 🔴 Что будет, если `extra_body` содержит `max_tokens`?

- A) значения сложатся
- B) значение из `extra_body` перекроет аргумент
- C) победит аргумент
- D) ошибка SDK

<details><summary>Ответ</summary>

**B.** `extra_*` применяются поверх обычных параметров, поэтому недоверенные данные туда передавать нельзя.
→ [Экспертная теория](expert.ru.md)

</details>

## 11. 🔴 Почему `respx` не перехватывает запросы SDK 2.14.0?

- A) запросы идут через `curl`
- B) `respx` работает только с GET
- C) SDK использует `httpx2`
- D) SDK использует `requests`

<details><summary>Ответ</summary>

**C.** `respx` подменяет `httpx`, а это другой пакет. Нужен `httpx2.alias_httpx()` или транспорт `MockTransport`.
→ [Экспертная теория](expert.ru.md)

</details>

## 12. 🔴 Открытый вопрос: сервис под ZDR использует tool runner. Что проверить?

<details><summary>Ответ</summary>

Что везде задан `thread=False` (в TypeScript `thread: false`); что нет других бета-функций вне ZDR; что debug-лог выключен; что ваши собственные логи не хранят содержимое; тест, проверяющий отсутствие поля `thread` в запросе.
→ [Продвинутая теория](advanced.ru.md)

</details>
