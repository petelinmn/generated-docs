---
title: "Квиз: structured outputs"
lang: ru
track: claude
kind: module
doc: quiz
slug: 04-structured-output
part: 1
tags: [claude, structured-outputs, json-schema, pydantic, strict-tool-use]
status: draft
updated: 2026-11-25
---

# Квиз: structured outputs

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте сами, потом откройте ответ. Метка показывает уровень вопроса: 🟢 [базовая](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 В каком параметре передаётся JSON-схема ответа?

- A) `output_format` верхнего уровня
- B) `output_config.format` с типом `json_schema`
- C) `system`
- D) `metadata`

<details><summary>Ответ</summary>

**B.** Старый `output_format` объявлен устаревшим.
→ [Базовая теория](basics.ru.md#схема-в-output_configformat)

</details>

## 2. 🟢 Откуда читать готовый объект при `client.messages.parse(…, output_format=Model)`?

- A) `response.content[0].text`
- B) `response.parsed_output`
- C) `response.output`
- D) `response.json()`

<details><summary>Ответ</summary>

**B.** `parse` возвращает уже проверенный экземпляр модели в `parsed_output`. `content[0].text` — это строка JSON, которую пришлось бы разбирать самому; `output` и `json()` для этого не служат.
→ [Базовая теория](basics.ru.md#messagesparse-и-pydantic)

</details>

## 3. 🟢 Что приходит при `messages.create` со схемой?

- A) Готовый словарь в `response.parsed_output`
- B) JSON-текст в блоке `text`, который нужно разобрать
- C) Файл
- D) Ничего: нужен `parse`

<details><summary>Ответ</summary>

**B.** `json.loads(text)` и, лучше, валидация в Pydantic.
→ [Базовая теория](basics.ru.md#messagescreate-и-jsonloads)

</details>

## 4. 🟢 Что нужно проверить до разбора JSON?

- A) `model`
- B) `stop_reason` (`max_tokens`, `refusal`)
- C) `id`
- D) Ничего

<details><summary>Ответ</summary>

**B.** Обрезанный или отказной ответ может не соответствовать схеме.
→ [Базовая теория](basics.ru.md#проверяйте-stop_reason)

</details>

## 5. 🟡 Что произойдёт с `Field(ge=1, le=5)` в схеме Pydantic?

- A) Модель не сможет вернуть 9
- B) Ограничение уйдёт в `description` как подсказка; проверять нужно самим
- C) Запрос вернёт 400
- D) SDK удалит поле

<details><summary>Ответ</summary>

**B.** Числовые границы схема не поддерживает.
→ [Продвинутая теория](advanced.ru.md#что-делает-sdk-со-схемой-transform_schema)

</details>

## 6. 🟡 Как описать «словарь» с неизвестными ключами?

- A) Полем `dict`
- B) Списком пар `{key, value}`
- C) Рекурсивной схемой
- D) `oneOf`

<details><summary>Ответ</summary>

**B.** Поле `dict` даёт пустые `properties`, и модель вернёт `{}`.
→ [Продвинутая теория](advanced.ru.md#что-сделать-с-полями)

</details>

## 7. 🟡 Что гарантирует `strict: true` у инструмента?

- A) Что инструмент всегда вызывается
- B) Что аргументы вызова соответствуют `input_schema`, а имя инструмента допустимо
- C) Что ответ будет JSON
- D) Что кэш не сбросится

<details><summary>Ответ</summary>

**B.** Это отличается от JSON outputs, которые ограничивают формат ответа.
→ [Продвинутая теория](advanced.ru.md#strict-tool-use)

</details>

## 8. 🟡 Что сбрасывает кэш скомпилированной схемы?

- A) Изменение `description`
- B) Изменение структуры схемы
- C) Новый запрос с теми же полями
- D) Смена `max_tokens`

<details><summary>Ответ</summary>

**B.** Правка только `name`/`description` его не сбрасывает.
→ [Продвинутая теория](advanced.ru.md#кэш-схемы-и-задержка)

</details>

## 9. 🟡 Выберите все верные утверждения

- [ ] A) Схема не поддерживает `oneOf` и рекурсию
- [ ] B) `additionalProperties` должен быть `false`
- [ ] C) Валидный JSON гарантирует верный смысл значений
- [ ] D) Цитаты вместе с `output_config.format` дают 400

<details><summary>Ответ</summary>

**A, B, D.** C неверно: гарантируется форма, а не смысл.
→ [Эксперт](expert.ru.md#справочник)

</details>

## 10. 🔴 Найдите ошибку

> «Если ответ пришёл с кодом 200, его всегда можно безопасно разбирать по схеме».

<details><summary>Ответ</summary>

Нет: `refusal` приходит с кодом 200, а `max_tokens` даёт обрезанный JSON. Нужно проверять `stop_reason`.
→ [Подвох 5](expert.ru.md#5-отказ-приходит-с-кодом-200)

</details>

## 11. 🔴 Почему `tags: dict` в модели Pydantic — плохая идея для structured outputs?

- A) Pydantic не поддерживает `dict`
- B) Схема получит пустые `properties`, и модель сможет вернуть только `{}`
- C) API не знает тип `object`
- D) Это замедляет декодирование

<details><summary>Ответ</summary>

**B.** В опыте получилось `{"type": "object", "properties": {}, "additionalProperties": false}`.
→ [Подвох 3](expert.ru.md#3-dict-даёт-пустую-схему)

</details>

## 12. 🔴 Чем первый запрос с новой схемой отличается от повторных?

- A) Ничем
- B) Он медленнее: схема компилируется в грамматику, затем результат кэшируется
- C) Он дешевле
- D) Он не поддерживает стриминг

<details><summary>Ответ</summary>

**B.** Кэш действует по документации до 24 часов с последнего использования.
→ [Внутреннее устройство](expert.ru.md#внутреннее-устройство)

</details>

## 13. 🔴 Открытый вопрос

Спроектируйте извлечение полей из счетов (номер, дата, сумма, позиции): схема, обработка сбоев, валидация.

<details><summary>Ключевые мысли хорошего ответа</summary>

- Модель Pydantic с вложенной моделью позиций, `date` как строка формата, сумма как число; без `dict` и рекурсии.
- `messages.parse`, запас `max_tokens`, проверка `stop_reason`.
- Валидация после разбора (диапазоны, сумма позиций = итого), ограниченные повторы, обработка `refusal`.
- Стабильная схема ради кэша; свободный текст при сомнениях выделять отдельным шагом.

</details>

→ [Продвинутая теория](advanced.ru.md)
