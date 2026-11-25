---
title: "Claude Code: квиз"
lang: ru
track: claude
kind: module
doc: quiz
slug: 09-claude-code
part: 2
tags: [claude, claude-code, permissions, headless, ci]
versions: "Claude Code 2.1.332"
status: draft
updated: 2026-11-25
---

# Claude Code: квиз

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте сами, затем раскройте ответ. 🟢 [базовый](basics.ru.md) · 🟡 [продвинутый](advanced.ru.md) · 🔴 [экспертный](expert.ru.md).

## 1. 🟢 Как получить один ответ без интерактивной сессии?

- A) `claude --bg`
- B) `claude mcp`
- C) `claude -p`
- D) `claude -c`

<details><summary>Ответ</summary>

**C.**
→ [Базовая теория](basics.ru.md)

</details>

## 2. 🟢 Какой режим не даёт менять файлы и нужен для плана?

- A) `plan`
- B) `acceptEdits`
- C) `bypassPermissions`
- D) `auto`

<details><summary>Ответ</summary>

**A.**
→ [Базовая теория](basics.ru.md)

</details>

## 3. 🟢 Какая команда создаёт `CLAUDE.md`?

- A) `/clear`
- B) `/context`
- C) `/model`
- D) `/init`

<details><summary>Ответ</summary>

**D.**
→ [Базовая теория](basics.ru.md)

</details>

## 4. 🟢 Что переключает `Shift+Tab`?

- A) темы
- B) режимы прав
- C) модели
- D) сессии

<details><summary>Ответ</summary>

**B.**
→ [Базовая теория](basics.ru.md)

</details>

## 5. 🟡 Команда есть и в `allow`, и в `deny`:

- A) зависит от порядка в файле
- B) запрещена
- C) разрешена
- D) спросит

<details><summary>Ответ</summary>

**B.**
→ [Продвинутая теория](advanced.ru.md)

</details>

## 6. 🟡 `claude -p` получил отказ на `Write`. Код выхода?

- A) 1
- B) 2
- C) запуск зависнет
- D) 0, отказ виден в `permission_denials`

<details><summary>Ответ</summary>

**D.**
→ [Продвинутая теория](advanced.ru.md)

</details>

## 7. 🟡 Что перекрывает `.claude/settings.json`?

- [ ] A) `.claude/settings.local.json`
- [ ] B) флаг командной строки
- [ ] C) `~/.claude/settings.json`
- [ ] D) `README.md`

<details><summary>Ответ</summary>

**A, B.**
→ [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Что `/rewind` не восстановит?

- A) разговор
- B) правку через Write
- C) файл, изменённый только командами Bash
- D) правку через Edit

<details><summary>Ответ</summary>

**C.**
→ [Продвинутая теория](advanced.ru.md)

</details>

## 9. 🔴 Разрешено `Bash(touch *)`. Команда `touch a && rm a`:

- A) выполнится целиком
- B) выполнит только `rm`
- C) вызовет ошибку синтаксиса
- D) потребует разрешения на `rm`

<details><summary>Ответ</summary>

**D.**
→ [Экспертная теория](expert.ru.md)

</details>

## 10. 🔴 `--output-format stream-json` с `-p` требует:

- A) `--model`
- B) `--verbose`
- C) `--bare`
- D) `--continue`

<details><summary>Ответ</summary>

**B.**
→ [Экспертная теория](expert.ru.md)

</details>

## 11. 🔴 Что вернёт исчерпанный `--max-turns 1`?

- A) пустой вывод
- B) повторный запрос
- C) `error_max_turns` и код 1
- D) `success` и код 0

<details><summary>Ответ</summary>

**C.**
→ [Экспертная теория](expert.ru.md)

</details>

## 12. 🔴 Открытый вопрос: настройте ночной запуск, который чинит lint-ошибки и открывает PR.

<details><summary>Ответ</summary>

Изолированный раннер; `-p` с точной задачей; разрешить только редактирование и команды линтера, `dontAsk`; `--max-turns` и бюджет; ветка и PR вместо записи в main; проверка тестами; человек смотрит PR; разбирать `permission_denials` и код выхода.
→ [Продвинутая теория](advanced.ru.md)

</details>
