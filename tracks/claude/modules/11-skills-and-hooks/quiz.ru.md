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

**C.** Hook запускает сама программа по событию. Строка в `CLAUDE.md`, описание инструмента и skill лишь влияют на решение модели.
→ [Базовая теория](basics.ru.md)

</details>

## 2. 🟢 Где лежит проектный skill `deploy`?

- A) `.claude/skills/deploy/SKILL.md`
- B) `.claude/deploy.skill`
- C) `skills/deploy.json`
- D) `.claude/hooks/deploy`

<details><summary>Ответ</summary>

**A.** Skill — это каталог с файлом `SKILL.md` внутри `.claude/skills/`. Остальные пути Claude Code как skill не распознаёт.
→ [Базовая теория](basics.ru.md)

</details>

## 3. 🟢 Каким кодом hook блокирует вызов инструмента?

- A) 1
- B) 0
- C) -1
- D) 2

<details><summary>Ответ</summary>

**D.** Код 2 — блокирующий: вызов отменяется, а stderr получает модель. Код 0 — успех, прочие коды считаются неблокирующей ошибкой.
→ [Базовая теория](basics.ru.md)

</details>

## 4. 🟢 Что из skill постоянно занимает контекст?

- A) ничего никогда
- B) описание в списке
- C) всё тело
- D) приложенные файлы

<details><summary>Ответ</summary>

**B.** В контексте постоянно только описание в списке, чтобы модель знала, когда применить skill. Тело читается при вызове, приложенные файлы — по необходимости.
→ [Базовая теория](basics.ru.md)

</details>

## 5. 🟡 Как сделать skill вызываемым только человеком?

- A) `allowed-tools: none`
- B) `disable-model-invocation: true`
- C) `user-invocable: false`
- D) `context: fork`

<details><summary>Ответ</summary>

**B.** Это поле запрещает модели вызывать skill самой. `user-invocable: false` делает обратное: скрывает от человека; `allowed-tools` и `context: fork` касаются прав и места выполнения.
→ [Продвинутая теория](advanced.ru.md)

</details>

## 6. 🟡 Hook вернул `allow`, а правило говорит `deny`:

- A) разрешено
- B) спросит
- C) ошибка настройки
- D) запрещено

<details><summary>Ответ</summary>

**D.** `allow` от hook не отменяет `deny`: запрещающие правила действуют всегда. Это не ошибка настройки и не повод спрашивать.
→ [Продвинутая теория](advanced.ru.md)

</details>

## 7. 🟡 `/greet "Ann Lee" x`. Чему равен `$0`?

- A) `Ann Lee`
- B) `greet`
- C) `"Ann`
- D) `x`

<details><summary>Ответ</summary>

**A.** Аргументы нумеруются с нуля и разбираются с учётом кавычек, поэтому первый — `Ann Lee` целиком. Имя команды в аргументы не входит, `x` — это `$1`.
→ [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Что верно о `UserPromptSubmit`?

- [ ] A) stdout заменяет сообщение
- [ ] B) срабатывает после ответа
- [ ] C) stdout добавляется в контекст
- [ ] D) код 2 отклоняет сообщение

<details><summary>Ответ</summary>

**C, D.** У этого события stdout при коде 0 попадает в контекст, а код 2 отклоняет само сообщение. Сообщение пользователя не заменяется, и срабатывает hook до обработки.
→ [Продвинутая теория](advanced.ru.md)

</details>

## 9. 🔴 Hook превысил `timeout`. Действие:

- A) блокируется
- B) повторяется
- C) завершает сессию
- D) выполняется, как будто hook не блокировал

<details><summary>Ответ</summary>

**D.** Таймаут — неблокирующий сбой, поэтому действие продолжается. Защитный hook должен быть быстрым и не может рассчитывать, что зависание остановит операцию.
→ [Экспертная теория](expert.ru.md)

</details>

## 10. 🔴 Зачем в `Stop`-hook проверять `stop_hook_active`?

- A) это обязательное поле ответа
- B) чтобы не заставлять продолжать бесконечно
- C) чтобы узнать модель
- D) чтобы вернуть JSON

<details><summary>Ответ</summary>

**B.** Флаг сообщает, что Claude уже продолжает из-за `Stop`-hook; без проверки он может блокировать остановку снова и снова. Модель из него не узнать, и в ответе он не обязателен.
→ [Экспертная теория](expert.ru.md)

</details>

## 11. 🔴 Чем `PreToolUse` отличается от `PostToolUse` с кодом 2?

- A) второй откатывает файлы
- B) первый игнорируется
- C) первый предотвращает действие, второй только сообщает
- D) ничем

<details><summary>Ответ</summary>

**C.** `PreToolUse` срабатывает до вызова и может его не допустить, `PostToolUse` — после, когда действие уже выполнено. Файлы он не откатывает.
→ [Экспертная теория](expert.ru.md)

</details>

## 12. 🔴 Открытый вопрос: «после каждой правки кода должен запускаться форматтер, а деплой — только по команде человека». Как настроить?

<details><summary>Ответ</summary>

`PostToolUse` с matcher `Edit|Write`, запускающий форматтер; skill `deploy` с `disable-model-invocation: true`; правило `ask`/`deny` или `PreToolUse` на сами команды деплоя, чтобы их нельзя было вызвать в обход skill.
→ [Продвинутая теория](advanced.ru.md)

</details>
