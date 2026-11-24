---
title: "Structured outputs: экспертный уровень"
lang: ru
track: claude
kind: module
doc: expert
slug: 04-structured-output
part: 1
tags: [claude, structured-outputs, json-schema, constrained-decoding, strict-tool-use, pydantic, schema-compilation]
versions: "Claude 5.5 family; Messages API, anthropic-version 2026-10-21; Python SDK anthropic 2.13.0"
status: draft
updated: 2026-11-24
---

# Structured outputs: экспертный уровень

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · [2. Продвинутый](advanced.ru.md) · **3. Эксперт** · [4. Квиз](quiz.ru.md)

Опирается на [базовую](basics.ru.md) и [продвинутую](advanced.ru.md) теорию. Справочные данные — со страниц документации «Structured outputs» и «Strict tool use»; страница структурированных ответов прочитана лишь частично (остальное — пересказ), поэтому строки таблиц, помеченные ⚠️, нужно сверить с живой страницей. Что запускалось, помечено «в опыте» (мок-сервер, SDK 2.13.0).

## Справочник

### Способы получить структуру

| Способ | Параметр | Результат | Где читать |
|---|---|---|---|
| JSON outputs (вручную) | `output_config={"format": {"type": "json_schema", "schema": …}}` в `messages.create` | JSON-текст в блоке `text` | `json.loads(text)` |
| JSON outputs (помощник) | `client.messages.parse(…, output_format=Model)` | объект модели | `response.parsed_output` |
| Strict tool use | `strict: true` у инструмента | `tool_use` с `input` по схеме | `block.input` |
| Устаревшее | параметр `output_format` | требовал бета-заголовок `structured-outputs-2025-11-13`; словарь в `output_format` в SDK v1+ даёт `TypeError` | не использовать |

### Ключевые слова схемы

| Статус | Ключевые слова |
|---|---|
| Поддерживаются | типы (`string`, `number`, `integer`, `boolean`, `object`, `array`, `null`), `enum`, `const`, `anyOf`, `allOf` (с оговорками), `$ref`/`$defs`/`definitions`, `default`, `required`, `additionalProperties: false`, форматы строк (`date-time`, `time`, `date`, `duration`, `email`, `hostname`, `uri`, `ipv4`, `ipv6`, `uuid`), `minItems` 0 или 1, ограниченное подмножество `pattern` |
| Не поддерживаются (400) | рекурсивные схемы, `oneOf`, `not`, `anyOf` вместе с `type`/`properties`, внешние `$ref`, `minimum`/`maximum`/`multipleOf`, `minLength`/`maxLength`, массивы с ограничениями сверх `minItems` 0/1, `additionalProperties` не `false`, в `pattern` — обратные ссылки, lookaround, `\b`/`\B` |

Обязательные свойства идут первыми в ответе; значения `enum`/`const` могут отличаться регистром.

### Поведение при проблемах

| Ситуация | Результат |
|---|---|
| `stop_reason: "max_tokens"` | вывод неполный и не соответствует схеме (в опыте — обрезанный JSON → `JSONDecodeError`) |
| `stop_reason: "refusal"` | HTTP 200, токены оплачиваются, вывод может не соответствовать схеме; поле схемы с «рассуждениями шаг за шагом» может вызвать отказ `reasoning_extraction` |
| Неподдерживаемое ключевое слово | 400 |
| Слишком сложная схема | 400 `Schema is too complex for compilation`; компиляция ограничена примерно 180 секундами |
| Цитаты (`citations`) вместе с `output_config.format` | 400 |
| Prefill ответа | несовместим |
| `computer_toolset_20260801`, `browser_toolset_20260801` с `strict: true` | отклоняются |

### Лимиты strict tool use

В одном запросе — до 20 строгих инструментов, до 24 необязательных параметров и до 16 параметров с объединением типов (из пересказа страницы; сверьте с живой страницей).

### Поддержка

Модели Claude 5.5 (`claude-opus-5-5`, `claude-sonnet-5-5`, `claude-haiku-5-5`) и ряд предыдущих, на Claude API и облачных платформах (Bedrock, Google Cloud, Foundry); конечная точка — `/v1/messages`. Managed Agents не поддерживаются. Полный перечень — в метаданных страницы документации.

## Внутреннее устройство

- **Ограничение декодирования.** Схема компилируется в грамматику, и генерация токенов ограничивается так, чтобы результат ей соответствовал. Поэтому гарантия относится к **синтаксису и структуре** (типы, обязательные поля, `enum`), а не к смыслу значений.
- **Компиляция и кэш.** Первая отправка новой схемы медленнее из-за компиляции; результат кэшируется до 24 часов с последнего использования. Структурное изменение схемы сбрасывает кэш, переименование и правка описаний — нет; смена набора инструментов тоже сбрасывает его.
- **Что делает SDK.** `messages.parse` строит схему из Pydantic через `transform_schema`, отправляет её в `output_config.format`, затем валидирует ответ и кладёт объект в `parsed_output` (в опыте поля `output_format` в теле запроса не было). Ограничения, которых нет в схеме, SDK переносит в `description`; их проверка выполняется на вашей стороне при разборе.
- **Порядок и виды ответа.** Результат — обычный блок `text` в `content`: рассуждения (`thinking`) и структура разделены; при strict tool use структура приходит блоком `tool_use`.
- **Отказы вне схемы.** Классификаторы безопасности могут вернуть `refusal` независимо от схемы, поэтому код всегда проверяет `stop_reason`.

> ⚠️ Проверить: детали грамматики и поведение ограничения декодирования выведены из документации, в опытах на реальной модели не проверялись.

## Подвохи

### 1. Валидный JSON ≠ правильный ответ

Схема не гарантирует смысл: имя может быть выдуманным.

### 2. Ограничения Pydantic не доходят до модели

`Field(ge=1, le=5)` превратилось в подсказку в `description`; проверка — только при разборе (в опыте `less_than_equal`).

### 3. `dict` даёт пустую схему

Модель вернёт `{}` (в опыте `properties: {}`).

### 4. Обрезка по `max_tokens`

Неполный JSON не соответствует схеме (в опыте `JSONDecodeError`).

### 5. Отказ приходит с кодом 200

Не пытайтесь разбирать ответ без проверки `stop_reason`.

### 6. Цитаты несовместимы

Включённые `citations` вместе с `output_config.format` → 400.

### 7. Смена схемы сбрасывает кэш

Меняйте структуру схемы редко; не генерируйте схему «на лету» на каждый запрос.

### 8. Регистр `enum`

Значение может прийти в другом регистре: нормализуйте при сравнении.

### 9. Устаревший `output_format`

Словарь в `output_format` в новых SDK — ошибка; используйте `output_config.format`.

## Компромиссы и альтернативы

| Подход | Плюсы | Минусы |
|---|---|---|
| JSON outputs | гарантия формата | ограничения схемы, первая задержка |
| `strict: true` у инструментов | корректные аргументы | лимиты на число инструментов |
| Просьба в промпте + разбор | работает везде | нет гарантии, повторы |
| XML-теги + `re` | просто | хрупко к вариациям |
| Библиотеки (Instructor и др.) | удобные обёртки | зависимость; не запускались |
| Двухэтапная схема (свободный текст → извлечение) | лучше для сложных рассуждений | два вызова |

## Вопросы с собеседований

<details><summary>1. Чем structured outputs лучше просьбы «верни JSON»?</summary>

Схема ограничивает декодирование: формат гарантирован, а не «вероятен»; не нужны повторы из-за поясняющего текста.

→ [Базовая теория](basics.ru.md#схема-в-output_configformat)

</details>

<details><summary>2. Как получить объект Pydantic из ответа?</summary>

`client.messages.parse(…, output_format=Model)` и `response.parsed_output`.

→ [Базовая теория](basics.ru.md#messagesparse-и-pydantic)

</details>

<details><summary>3. Подвох: почему <code>Field(ge=1, le=5)</code> не мешает модели вернуть 9?</summary>

Числовые границы не поддерживаются схемой: SDK кладёт их в `description`; проверка — на вашей стороне.

→ [Подвох 2](#2-ограничения-pydantic-не-доходят-до-модели)

</details>

<details><summary>4. Что делает <code>strict: true</code> у инструмента?</summary>

Аргументы вызова строго соответствуют `input_schema`, имя инструмента допустимо.

→ [Продвинутая теория](advanced.ru.md#strict-tool-use)

</details>

<details><summary>5. Подвох: ответ пришёл с кодом 200, но JSON не разбирается. Причины?</summary>

`max_tokens` (обрезка) или `refusal`; проверьте `stop_reason`.

→ [Базовая теория](basics.ru.md#проверяйте-stop_reason)

</details>

<details><summary>6. Какие ключевые слова схемы не поддерживаются?</summary>

Рекурсия, `oneOf`, `not`, числовые и строковые ограничения (`minimum`, `maxLength`…) и др.

→ [Справочник](#ключевые-слова-схемы)

</details>

<details><summary>7. Почему первый запрос с новой схемой медленнее?</summary>

Схема компилируется в грамматику; результат кэшируется.

→ [Продвинутая теория](advanced.ru.md#кэш-схемы-и-задержка)

</details>

<details><summary>8. Как описать «словарь» с произвольными ключами?</summary>

Списком пар `{key, value}`: поле `dict` даст пустые `properties`.

→ [Подвох 3](#3-dict-даёт-пустую-схему)

</details>

<details><summary>9. Чем JSON outputs отличаются от strict tool use?</summary>

Первое ограничивает формат ответа (блок `text`), второе — аргументы вызова (блок `tool_use`); можно сочетать.

→ [Продвинутая теория](advanced.ru.md#strict-tool-use)

</details>

<details><summary>10. С чем несовместимы structured outputs?</summary>

С цитатами (400) и prefill; на Managed Agents не поддерживаются.

→ [Справочник](#поведение-при-проблемах)

</details>

<details><summary>11. Подвох: SDK добавил в схему <code>additionalProperties: false</code>. Зачем?</summary>

API требует `additionalProperties: false` для объектов; `transform_schema` приводит схему к допустимому виду.

→ [Продвинутая теория](advanced.ru.md#что-делает-sdk-со-схемой-transform_schema)

</details>

<details><summary>12. Как быть с отказом (<code>refusal</code>)?</summary>

Не разбирать ответ как структуру; изменить запрос или вернуть пользователю сообщение; повтор бесполезен.

→ [Подвох 5](#5-отказ-приходит-с-кодом-200)

</details>

## Шпаргалка

```text
parse:  client.messages.parse(model=…, max_tokens=…, messages=[…], output_format=Model) → r.parsed_output
create: output_config={"format": {"type": "json_schema", "schema": {…, "additionalProperties": False}}} → json.loads(text)
tool:   {"name", "description", "strict": True, "input_schema": {…}}
schema: нет рекурсии/oneOf/minimum/maxLength; dict → список пар; enum — сравнивать без регистра
проверяй stop_reason (max_tokens, refusal) и значения после разбора
```

- Схема гарантирует форму, не смысл.
- Ограничения Pydantic — подсказка, проверять самим.
- Цитаты несовместимы.

## Связанные материалы

- Предыдущий уровень: [продвинутая теория](advanced.ru.md)
- Параметры запроса и `stop_reason`: [02 · Messages API](../02-messages-api/expert.ru.md)
- Формат через промпт: [03 · Prompt engineering](../03-prompt-engineering/expert.ru.md)

## Источники

- [Structured outputs](https://platform.claude.com/docs/en/build-with-claude/structured-outputs)
- [Strict tool use](https://platform.claude.com/docs/en/agents-and-tools/tool-use/strict-tool-use)
- [Ошибки API](https://platform.claude.com/docs/en/api/errors)
