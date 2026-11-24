---
title: "Tool use: экспертный уровень"
lang: ru
track: claude
kind: module
doc: expert
slug: 07-tool-use
part: 1
tags: [claude, tool-use, tool-choice, stop-reason, server-tools, parallel-tool-calls, tool-result]
versions: "Claude 5.5 family; Messages API, anthropic-version 2026-10-21; Python SDK anthropic 2.13.0"
status: draft
updated: 2026-11-24
---

# Tool use: экспертный уровень

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · [2. Продвинутый](advanced.ru.md) · **3. Эксперт** · [4. Квиз](quiz.ru.md)

Опирается на [базовую](basics.ru.md) и [продвинутую](advanced.ru.md) теорию. Справочник — со страниц документации, прочитанных на дату подготовки (обзор, define tools, handle tool calls, tool reference целиком; parallel tool use и stop reasons — частично); зависящее от пересказа помечено.

## Справочник

### Поля описания инструмента

| Поле | Правило |
|---|---|
| `name` | `^[a-zA-Z0-9_-]{1,128}$` |
| `description` | обычный текст; документация советует не меньше 3–4 предложений |
| `input_schema` | объект JSON Schema |
| `input_examples` | необязательно; каждый пример должен проходить `input_schema` (API обычно отвечает 400, если нет; в некоторых случаях проверка пропускается, например схема с `pattern`); ~100 токенов (плоский) / ~200 (вложенный) на пример; не для серверных инструментов |
| `strict` | необязательно; проверка схемы имён и входов; не для `mcp_toolset` и наборов computer/browser; `strict`-инструмент не может указывать вызывающего из code execution в `allowed_callers` |
| `cache_control` | необязательно; точка кэша на этом описании; для записей наборов — на самой записи |
| `defer_loading` | необязательно; загружает инструмент по требованию через tool search; нельзя сочетать с `cache_control` |
| `allowed_callers` | необязательно; `"direct"` (по умолчанию) или `"code_execution_20260120"` |
| `eager_input_streaming` | необязательный boolean; только пользовательские инструменты |

### Блоки tool_use и tool_result

| Блок | Поля |
|---|---|
| `tool_use` | `type`, `id` (напр. `toolu_…`), `name`, `input`; `caller` для программных вызовов; `toolset_name` (`computer`/`browser`) для членов наборов |
| `server_tool_use` | как `tool_use`, префикс id `srvtoolu_`; результат — отдельный блок |
| `tool_result` | `type`, `tool_use_id`, необязательный `content` (строка или список блоков `text`, `image`, `document`, `search_result`), необязательный `is_error`; результаты computer/browser повторяют `toolset_name` |

Пустой результат (без `content`) допустим.

### tool_choice

| Значение | Эффект |
|---|---|
| `auto` | решает модель; по умолчанию при `tools` |
| `any` | какой-то инструмент обязан быть вызван |
| `tool` (+ `name`) | именно этот инструмент |
| `none` | без инструментов; по умолчанию без `tools` |
| `disable_parallel_tool_use: true` | поле **внутри** `tool_choice`; с `auto` не больше одного вызова, с `any`/`tool` ровно один |

С `any`/`tool` ответ начинается с вызова (без текста). Не поддерживается с ручным thinking и даёт 400 на Fable 5.2, Mythos 5.2, Opus 5.5, Sonnet 5.5, Fable 5.1, Mythos 5.1. Наборы computer/browser отвергают `tool`. Смена `tool_choice` инвалидирует кэшированные блоки сообщений.

### Значения stop_reason

| Значение | Смысл | Что делаете |
|---|---|---|
| `end_turn` | естественный конец | используете ответ |
| `max_tokens` | упёрлись в `max_tokens` | поднимаете; если последний блок — неполный `tool_use`, повторяете с большим |
| `stop_sequence` | сработала ваша стоп-последовательность | читаете `stop_sequence` |
| `tool_use` | вызван ваш инструмент | выполняете, возвращаете результат |
| `pause_turn` | цикл серверного инструмента достиг лимита | отправляете ответ снова как ход ассистента |
| `refusal` | отказ | читаете `stop_details` |
| `model_context_window_exceeded` | контекст заполнен | считаете усечённым |

### Типы серверных инструментов

| Инструмент | Строки типов |
|---|---|
| Веб-поиск | `web_search_20260318`, `web_search_20260209`, `web_search_20250305` |
| Загрузка страниц | `web_fetch_20260318`, `web_fetch_20260309`, `web_fetch_20260209`, `web_fetch_20250910` |
| Выполнение кода | `code_execution_20260521`, `code_execution_20260120`, `code_execution_20250825` (устаревший `code_execution_20250522`, только Python) |
| Поиск инструментов | `tool_search_tool_regex_20251119`, `tool_search_tool_bm25_20251119` (+ алиасы без даты) |
| Advisor | `advisor_20260301` (бета-заголовок `advisor-tool-2026-03-01`) |
| MCP connector | `mcp_toolset` (бета-заголовок `mcp-client-2025-11-20`) |

Клиентские инструменты Anthropic: `memory_20250818`, `bash_20250124`, `text_editor_20250728` (Claude 4+) / `text_editor_20250124`, computer use (`computer_toolset_20260801`, `computer_20251124`, `computer_20250124`), browser use (`browser_toolset_20260801`).

## Внутреннее устройство

- **Без состояния.** API ничего не хранит между запросами: вся история, включая блоки `tool_use` ассистента и ваши `tool_result`, едет с каждым запросом. Соответствие по `id` проверяется при каждом вызове.
- **Промпт инструментов.** API добавляет системный промпт для tool use (сотни токенов, зависят от модели и `tool_choice`) перед вашими инструментами; он считается входными токенами.
- **Параллелизм — выбор модели.** API не предписывает порядок выполнения; Claude 4 и новее вызывают параллельно по умолчанию. Вызовы можно выполнять одновременно или по очереди. Возвращайте по одному `tool_result` на каждый `tool_use`, все — в следующем сообщении пользователя.
- **Пропущенные вызовы.** Если вызов не выполнен, ответьте на него `is_error: true` («Not executed: …»): вызов без ответа — 400.
- **Серверные циклы.** Серверные инструменты итерируют внутри одного запроса; при превышении лимита API возвращает `pause_turn` и ждёт, что ответ отправят обратно.

> ⚠️ Проверить: лимит по умолчанию в 10 итераций серверного цикла и точная процедура повторной отправки при `pause_turn` взяты из сокращённого пересказа страницы stop reasons.

## Подвохи

### 1. Текст перед результатами

Сообщение пользователя с текстом перед блоками `tool_result` отклоняется: «tool_use ids were found without tool_result blocks immediately after.» Мок воспроизвёл это как 400.

### 2. Текст после результатов

Допустим, если в ходе только клиентские инструменты, но завершающий текст может вызвать пустой `end_turn` (2–3 токена); если есть серверный инструмент без результата, сообщение пользователя должно содержать только `tool_result`, иначе запрос падает с 400. Отправляйте результаты одни.

### 3. На каждый вызов нужен ответ

Упавший или пропущенный вызов всё равно получает `tool_result` (`is_error: true`).

### 4. Принудительный вызов на моделях 5.5

`any` и `tool` дают 400 на Opus 5.5 и Sonnet 5.5; их заменяют `auto` со `strict`-инструментами или structured outputs.

### 5. `tool_choice` и кэш

Смена `tool_choice` между запросами инвалидирует кэшированные блоки сообщений, тогда как описания инструментов и системный промпт остаются в кэше.

### 6. Цикл без границ

Если модель продолжает вызывать инструменты, `while True` не закончится: ограничивайте шаги. Проверка из упражнения 5 бросает `no final answer after 3 steps`.

### 7. `is_error` для серверных инструментов

Не отправляйте; ошибка внутри блока результата серверного инструмента.

### 8. Обрезанный `tool_use`

При `max_tokens` последний `tool_use` может быть неполным: не выполняйте его; повторите с большим лимитом.

### 9. Недоверенные результаты

Результат — данные, а не инструкции: держите его в `tool_result`, не в системном промпте.

## Компромиссы и альтернативы

| Подход | Плюсы | Минусы |
|---|---|---|
| Ручной цикл | полный контроль: лимиты, логи, подтверждения | больше кода |
| Tool runner в SDK | коротко | контроль менее заметен |
| Серверные инструменты | не нужно хостить код | меньше контроля над деталями |
| Structured outputs | гарантированная форма, без цикла | нет побочных эффектов и внешних данных |
| Сценарий в коде | предсказуемо, дёшево | шаги выбирает не модель |
| Managed agents | цикл и песочница размещены за вас | другая поверхность ([агенты](../../overview.ru.md)) |

## Вопросы с собеседований

<details><summary>1. Что возвращает модель, когда хочет вызвать инструмент?</summary>

Ответ с блоком `tool_use` (`id`, `name`, `input`) и `stop_reason: "tool_use"`.

→ [Базовая теория](basics.ru.md#цикл-из-четырёх-шагов)

</details>

<details><summary>2. Кто выполняет клиентский инструмент?</summary>

Ваше приложение. Модель только просит.

→ [Базовая теория](basics.ru.md#зачем-это-нужно)

</details>

<details><summary>3. Что идёт в следующий запрос после <code>tool_use</code>?</summary>

`content` ответа без изменений как сообщение `assistant`, затем сообщение `user` с одним `tool_result` на вызов.

→ [Базовая теория](basics.ru.md#вызов-и-результат)

</details>

<details><summary>4. Подвох: может ли текст идти перед блоками <code>tool_result</code>?</summary>

Нет: результаты идут первыми и сразу за вызовом, иначе 400 («tool_use ids were found without tool_result blocks immediately after»).

→ [Подвох 1](#1-текст-перед-результатами)

</details>

<details><summary>5. Как возвращают параллельные результаты?</summary>

Все в одном сообщении `user`, по одному `tool_result` на `tool_use`.

→ [Базовая теория](basics.ru.md#параллельные-вызовы)

</details>

<details><summary>6. Что делать, когда инструмент упал?</summary>

Вернуть `tool_result` с `is_error: true` и понятным сообщением.

→ [Базовая теория](basics.ru.md#ошибки-инструментов)

</details>

<details><summary>7. Подвох: куда ставить <code>disable_parallel_tool_use</code>?</summary>

Внутрь объекта `tool_choice`, а не на верхний уровень запроса.

→ [Справочник](#tool_choice)

</details>

<details><summary>8. Что такое <code>any</code> и <code>tool</code> и в чём их подвох?</summary>

Они принуждают вызвать инструмент (любой / этот); ответ начинается с вызова. С ручным thinking не поддерживаются, а на перечисленных в документации моделях Claude 5.5 дают 400.

→ [Продвинутая теория](advanced.ru.md#принудительный-вызов-и-модели)

</details>

<details><summary>9. Что такое <code>pause_turn</code>?</summary>

Цикл серверного инструмента достиг лимита итераций; отправьте ответ снова как ход ассистента, чтобы продолжить. Вызов вашего инструмента заканчивается `tool_use`, никогда не `pause_turn`.

→ [Продвинутая теория](advanced.ru.md#клиентские-и-серверные-инструменты)

</details>

<details><summary>10. Подвох: нужно ли отправлять <code>is_error</code> при сбое веб-поиска?</summary>

Нет; ошибки серверных инструментов внутри блока результата.

→ [Подвох 7](#7-is_error-для-серверных-инструментов)

</details>

<details><summary>11. Как инструменты влияют на стоимость?</summary>

Описания, автоматический системный промпт инструментов (около 286 токенов на Opus 5.5 / Sonnet 5.5 для `auto`/`none`) и блоки `tool_use`/`tool_result` в истории.

→ [Продвинутая теория](advanced.ru.md#стоимость-в-токенах)

</details>

<details><summary>12. Как защититься от инъекции промпта через результаты инструментов?</summary>

Держать результаты внутри `tool_result`, не в системном промпте; проверять аргументы по правам; требовать подтверждения человека для необратимых действий.

→ [Продвинутая теория](advanced.ru.md#безопасность)

</details>

## Шпаргалка

```text
tool: {"name", "description", "input_schema", "strict"?, "input_examples"?, "cache_control"?}
reply: stop_reason == "tool_use" -> blocks {"type":"tool_use","id","name","input"}
next:  assistant = reply.content (unchanged); user = [tool_result per call (first!), ...]
tool_result: {"type":"tool_result","tool_use_id","content"?,"is_error"?}
tool_choice: auto | any | tool(+name) | none ; disable_parallel_tool_use inside it
any/tool: 400 on Opus 5.5, Sonnet 5.5, Fable 5.2 ...
pause_turn: resend the reply as the assistant turn
```

- Результаты первыми, все одним сообщением.
- На каждый вызов — ответ.
- Ограничивайте шаги.

## Связанные материалы

- Предыдущий уровень: [продвинутая теория](advanced.ru.md)
- Кэширование: [05 · Prompt caching](../05-prompt-caching/expert.ru.md)
- Структурированный вывод: [04 · Structured outputs](../04-structured-output/expert.ru.md)

## Источники

- [Tool use overview](https://platform.claude.com/docs/en/agents-and-tools/tool-use/overview)
- [Define tools](https://platform.claude.com/docs/en/agents-and-tools/tool-use/define-tools)
- [Handle tool calls](https://platform.claude.com/docs/en/agents-and-tools/tool-use/handle-tool-calls)
- [Parallel tool use](https://platform.claude.com/docs/en/agents-and-tools/tool-use/parallel-tool-use)
- [Tool reference](https://platform.claude.com/docs/en/agents-and-tools/tool-use/tool-reference)
- [Handling stop reasons](https://platform.claude.com/docs/en/build-with-claude/handling-stop-reasons)
