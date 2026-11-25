---
title: "CLAUDE.md и память: квиз"
lang: ru
track: claude
kind: module
doc: quiz
slug: 10-memory
part: 2
tags: [claude, claude-code, claude-md, memory, context]
versions: "Claude Code 2.1.332"
status: draft
updated: 2026-11-25
---

# CLAUDE.md и память: квиз

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте сами, затем раскройте ответ. 🟢 [базовый](basics.ru.md) · 🟡 [продвинутый](advanced.ru.md) · 🔴 [экспертный](expert.ru.md).

## 1. 🟢 Кто пишет автопамять?

- A) администратор
- B) git
- C) Claude
- D) вы вручную при установке

<details><summary>Ответ</summary>

**C.**
→ [Базовая теория](basics.ru.md)

</details>

## 2. 🟢 Какой файл не должен попадать в git?

- A) `CLAUDE.local.md`
- B) `CLAUDE.md`
- C) `.claude/rules/general.md`
- D) `AGENTS.md`

<details><summary>Ответ</summary>

**A.**
→ [Базовая теория](basics.ru.md)

</details>

## 3. 🟢 Как надёжно запретить чтение `.env`?

- A) написать об этом в `CLAUDE.md`
- B) добавить в автопамять
- C) сказать в первом сообщении
- D) правило прав `deny`

<details><summary>Ответ</summary>

**D.**
→ [Базовая теория](basics.ru.md)

</details>

## 4. 🟢 Какой ориентир размера `CLAUDE.md`?

- A) без ограничений
- B) меньше 200 строк
- C) меньше 20 строк
- D) до 25 МБ

<details><summary>Ответ</summary>

**B.**
→ [Базовая теория](basics.ru.md)

</details>

## 5. 🟡 Когда загрузится правило с `paths: ["**/*.py"]`?

- A) после сжатия обязательно
- B) когда Claude откроет подходящий файл
- C) всегда при старте
- D) только по `/memory`

<details><summary>Ответ</summary>

**B.**
→ [Продвинутая теория](advanced.ru.md)

</details>

## 6. 🟡 Что верно об импортах?

- [ ] A) в обратных кавычках не срабатывают
- [ ] B) не занимают контекст
- [ ] C) пути считаются от каталога запуска
- [ ] D) не больше четырёх переходов

<details><summary>Ответ</summary>

**A, D.**
→ [Продвинутая теория](advanced.ru.md)

</details>

## 7. 🟡 В репозитории есть `CLAUDE.md` и `AGENTS.md` (настройки по умолчанию):

- A) читается только `CLAUDE.md`
- B) читаются оба
- C) только `AGENTS.md`
- D) ни один

<details><summary>Ответ</summary>

**A.**
→ [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Сколько из `MEMORY.md` грузится в начале?

- A) первые 20 строк
- B) ничего
- C) 200 строк или 25 КБ
- D) весь файл

<details><summary>Ответ</summary>

**C.**
→ [Продвинутая теория](advanced.ru.md)

</details>

## 9. 🔴 Как `CLAUDE.md` попадает в запрос?

- A) заменяет системный промпт
- B) как инструмент
- C) как заголовок HTTP
- D) сообщением пользователя

<details><summary>Ответ</summary>

**D.**
→ [Экспертная теория](expert.ru.md)

</details>

## 10. 🔴 HTML-комментарий в `CLAUDE.md`:

- A) исполняется
- B) вырезается до отправки
- C) отправляется как есть
- D) вызывает ошибку

<details><summary>Ответ</summary>

**B.**
→ [Экспертная теория](expert.ru.md)

</details>

## 11. 🔴 Что загрузит `claude --bare`?

- A) только проектный
- B) все
- C) никакие из них
- D) только пользовательский

<details><summary>Ответ</summary>

**C.**
→ [Экспертная теория](expert.ru.md)

</details>

## 12. 🔴 Открытый вопрос: `CLAUDE.md` вырос до 900 строк, Claude пропускает правила. Что делать?

<details><summary>Ответ</summary>

Удалить очевидное и устаревшее, убрать противоречия; правила для частей кода вынести в `.claude/rules` с `paths`; редкие процедуры — в skills; обязательные запреты — в права и hooks; проверить `/context`.
→ [Продвинутая теория](advanced.ru.md)

</details>
