---
title: "Квиз: жизненный цикл индексов"
lang: ru
track: opensearch
kind: module
doc: quiz
slug: 08-index-lifecycle
part: 2
tags: [index-lifecycle, data-stream, ism, retention, rollover]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Квиз: жизненный цикл индексов

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте сами, затем раскройте ответ. 🟢 [базовый](basics.ru.md) · 🟡 [продвинутый](advanced.ru.md) · 🔴 [экспертный](expert.ru.md).

## 1. 🟢 Какое поле обязательно в документах data stream?

- A) `date`
- B) `created_at`
- C) `@timestamp`
- D) `_id`

<details><summary>Ответ</summary>

**C.**
→ [Базовая теория](basics.ru.md)

</details>

## 2. 🟢 Куда пишется новый документ потока?

- A) в текущий индекс записи
- B) в индекс, соответствующий его дате
- C) во все backing-индексы
- D) в первый индекс

<details><summary>Ответ</summary>

**A.**
→ [Базовая теория](basics.ru.md)

</details>

## 3. 🟢 Что разрешено по имени потока?

- A) перезапись по `_id`
- B) удаление по `_id`
- C) частичный `update` по `_id`
- D) добавление через `create`

<details><summary>Ответ</summary>

**D.**
→ [Базовая теория](basics.ru.md)

</details>

## 4. 🟢 Можно ли удалить текущий индекс записи потока?

- A) да, но данные останутся
- B) нет, запрос отклоняется
- C) да
- D) да, поток переключится на старый

<details><summary>Ответ</summary>

**B.**
→ [Базовая теория](basics.ru.md)

</details>

## 5. 🟡 `job_interval` по умолчанию?

- A) непрерывно
- B) 5 минут
- C) 1 минута
- D) 1 час

<details><summary>Ответ</summary>

**B.**
→ [Продвинутая теория](advanced.ru.md)

</details>

## 6. 🟡 Что нужно обычному индексу для ISM-rollover?

- [ ] A) настройка `rollover_alias`
- [ ] B) поле `@timestamp` в mapping
- [ ] C) две реплики
- [ ] D) псевдоним записи

<details><summary>Ответ</summary>

**A, D.**
→ [Продвинутая теория](advanced.ru.md)

</details>

## 7. 🟡 Политику изменили. Уже привязанный индекс:

- A) работает по прежней версии до `change_policy`
- B) переходит на новую сразу
- C) удаляется
- D) получает состояние failed

<details><summary>Ответ</summary>

**A.**
→ [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 `min_index_age: 7d` в переходе отсчитывается от:

- A) последней записи
- B) значения `@timestamp`
- C) создания индекса
- D) входа в состояние

<details><summary>Ответ</summary>

**C.**
→ [Продвинутая теория](advanced.ru.md)

</details>

## 9. 🔴 Куда ISM добавляет повторы по умолчанию?

- A) без повторов
- B) бесконечные повторы
- C) одна попытка через час
- D) 3 попытки с экспоненциальной задержкой

<details><summary>Ответ</summary>

**D.**
→ [Экспертная теория](expert.ru.md)

</details>

## 10. 🔴 Повторный `PUT` существующей политики без номеров последовательности:

- A) удалит старую
- B) 409
- C) обновит
- D) создаст вторую

<details><summary>Ответ</summary>

**B.**
→ [Экспертная теория](expert.ru.md)

</details>

## 11. 🔴 Как исправить один документ в потоке?

- A) удалить поток
- B) `_rollover`
- C) записать в backing-индекс с `if_seq_no` и `if_primary_term`
- D) `PUT stream/_doc/id`

<details><summary>Ответ</summary>

**C.**
→ [Экспертная теория](expert.ru.md)

</details>

## 12. 🔴 Открытый вопрос: логи 100 ГиБ в день, хранить 30 дней. Что настроите?

<details><summary>Ответ</summary>

Data stream с шаблоном; rollover по размеру первичного шарда и по возрасту; число шардов, чтобы шард был 30–50 ГиБ; после остывания `read_only` и `force_merge`; удаление по возрасту; снимки, если нужна копия; оценка диска с репликами и запасом.
→ [Продвинутая теория](advanced.ru.md)

</details>
