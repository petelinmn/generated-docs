---
title: "Skills и hooks: квиз"
lang: ru
track: claude
kind: module
doc: quiz
slug: 11-skills-and-hooks
part: 2
tags: [claude, claude-code, skills, hooks, automation]
versions: "Claude Code 2.1.332"
status: draft
updated: 2026-11-25
---

# Skills и hooks: квиз

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте сами, затем раскройте ответ. 🟢 [базовый](basics.ru.md) · 🟡 [продвинутый](advanced.ru.md) · 🔴 [экспертный](expert.ru.md).

## 1. 🟢 Что всегда выполняется независимо от решения модели?

- A) строка в CLAUDE.md
- B) описание инструмента
- C) hook
- D) skill

<details><summary>Ответ</summary>

**C.**
→ [Базовая теория](basics.ru.md)

</details>

## 2. 🟢 Где лежит проектный skill `deploy`?

- A) `.claude/skills/deploy/SKILL.md`
- B) `.claude/deploy.skill`
- C) `skills/deploy.json`
- D) `.claude/hooks/deploy`

<details><summary>Ответ</summary>

**A.**
→ [Базовая теория](basics.ru.md)

</details>

## 3. 🟢 Каким кодом hook блокирует вызов инструмента?

- A) 1
- B) 0
- C) -1
- D) 2

<details><summary>Ответ</summary>

**D.**
→ [Базовая теория](basics.ru.md)

</details>

## 4. 🟢 Что из skill постоянно занимает контекст?

- A) ничего никогда
- B) описание в списке
- C) всё тело
- D) приложенные файлы

<details><summary>Ответ</summary>

**B.**
→ [Базовая теория](basics.ru.md)

</details>

## 5. 🟡 Как сделать skill вызываемым только человеком?

- A) `allowed-tools: none`
- B) `disable-model-invocation: true`
- C) `user-invocable: false`
- D) `context: fork`

<details><summary>Ответ</summary>

**B.**
→ [Продвинутая теория](advanced.ru.md)

</details>

## 6. 🟡 Hook вернул `allow`, а правило говорит `deny`:

- A) разрешено
- B) спросит
- C) ошибка настройки
- D) запрещено

<details><summary>Ответ</summary>

**D.**
→ [Продвинутая теория](advanced.ru.md)

</details>

## 7. 🟡 `/greet "Ann Lee" x`. Чему равен `$0`?

- A) `Ann Lee`
- B) `greet`
- C) `"Ann`
- D) `x`

<details><summary>Ответ</summary>

**A.**
→ [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Что верно о `UserPromptSubmit`?

- [ ] A) stdout заменяет сообщение
- [ ] B) срабатывает после ответа
- [ ] C) stdout добавляется в контекст
- [ ] D) код 2 отклоняет сообщение

<details><summary>Ответ</summary>

**C, D.**
→ [Продвинутая теория](advanced.ru.md)

</details>

## 9. 🔴 Hook превысил `timeout`. Действие:

- A) блокируется
- B) повторяется
- C) завершает сессию
- D) выполняется, как будто hook не блокировал

<details><summary>Ответ</summary>

**D.**
→ [Экспертная теория](expert.ru.md)

</details>

## 10. 🔴 Зачем в `Stop`-hook проверять `stop_hook_active`?

- A) это обязательное поле ответа
- B) чтобы не заставлять продолжать бесконечно
- C) чтобы узнать модель
- D) чтобы вернуть JSON

<details><summary>Ответ</summary>

**B.**
→ [Экспертная теория](expert.ru.md)

</details>

## 11. 🔴 Чем `PreToolUse` отличается от `PostToolUse` с кодом 2?

- A) второй откатывает файлы
- B) первый игнорируется
- C) первый предотвращает действие, второй только сообщает
- D) ничем

<details><summary>Ответ</summary>

**C.**
→ [Экспертная теория](expert.ru.md)

</details>

## 12. 🔴 Открытый вопрос: «после каждой правки кода должен запускаться форматтер, а деплой — только по команде человека». Как настроить?

<details><summary>Ответ</summary>

`PostToolUse` с matcher `Edit|Write`, запускающий форматтер; skill `deploy` с `disable-model-invocation: true`; правило `ask`/`deny` или `PreToolUse` на сами команды деплоя, чтобы их нельзя было вызвать в обход skill.
→ [Продвинутая теория](advanced.ru.md)

</details>
