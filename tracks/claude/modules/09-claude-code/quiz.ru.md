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

**C.** `-p` (print) выполняет запрос и завершает процесс. `--bg` запускает фоновую сессию, `claude mcp` управляет серверами, `-c` продолжает прошлый разговор.
→ [Базовая теория](basics.ru.md)

</details>

## 2. 🟢 Какой режим не даёт менять файлы и нужен для плана?

- A) `plan`
- B) `acceptEdits`
- C) `bypassPermissions`
- D) `auto`

<details><summary>Ответ</summary>

**A.** В режиме `plan` Claude исследует и предлагает план, не редактируя файлы. `acceptEdits` и `auto` правки выполняют, а `bypassPermissions` снимает проверки вовсе.
→ [Базовая теория](basics.ru.md)

</details>

## 3. 🟢 Какая команда создаёт `CLAUDE.md`?

- A) `/clear`
- B) `/context`
- C) `/model`
- D) `/init`

<details><summary>Ответ</summary>

**D.** `/init` изучает проект и создаёт `CLAUDE.md`. `/clear` очищает разговор, `/context` показывает заполнение окна, `/model` меняет модель.
→ [Базовая теория](basics.ru.md)

</details>

## 4. 🟢 Что переключает `Shift+Tab`?

- A) темы
- B) режимы прав
- C) модели
- D) сессии

<details><summary>Ответ</summary>

**B.** `Shift+Tab` по кругу переключает режимы прав. Темы, модели и сессии меняются другими средствами.
→ [Базовая теория](basics.ru.md)

</details>

## 5. 🟡 Команда есть и в `allow`, и в `deny`:

- A) зависит от порядка в файле
- B) запрещена
- C) разрешена
- D) спросит

<details><summary>Ответ</summary>

**B.** `deny` проверяется первым и побеждает независимо от порядка в файле. Поэтому ни `allow`, ни вопрос пользователю здесь не сработают.
→ [Продвинутая теория](advanced.ru.md)

</details>

## 6. 🟡 `claude -p` получил отказ на `Write`. Код выхода?

- A) 1
- B) 2
- C) запуск зависнет
- D) 0, отказ виден в `permission_denials`

<details><summary>Ответ</summary>

**D.** Отказ в правах не считается сбоем запуска: инструмент получает отказ, а список виден в `permission_denials`. Спросить в `-p` некого, но процесс не зависает.
→ [Продвинутая теория](advanced.ru.md)

</details>

## 7. 🟡 Что перекрывает `.claude/settings.json`?

- [ ] A) `.claude/settings.local.json`
- [ ] B) флаг командной строки
- [ ] C) `~/.claude/settings.json`
- [ ] D) `README.md`

<details><summary>Ответ</summary>

**A, B.** Приоритет выше у локальных настроек проекта и у флагов командной строки. Пользовательский файл стоит ниже проектного, а `README.md` настройками не является.
→ [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Что `/rewind` не восстановит?

- A) разговор
- B) правку через Write
- C) файл, изменённый только командами Bash
- D) правку через Edit

<details><summary>Ответ</summary>

**C.** Checkpoint фиксирует правки инструментами редактирования. Изменения, сделанные командами Bash, не отслеживаются; разговор и правки Write и Edit восстанавливаются.
→ [Продвинутая теория](advanced.ru.md)

</details>

## 9. 🔴 Разрешено `Bash(touch *)`. Команда `touch a && rm a`:

- A) выполнится целиком
- B) выполнит только `rm`
- C) вызовет ошибку синтаксиса
- D) потребует разрешения на `rm`

<details><summary>Ответ</summary>

**D.** Составная команда проверяется по частям: `touch` разрешён, `rm` — нет. Правило на первую часть не распространяется на всю строку, и синтаксис здесь корректен.
→ [Экспертная теория](expert.ru.md)

</details>

## 10. 🔴 `--output-format stream-json` с `-p` требует:

- A) `--model`
- B) `--verbose`
- C) `--bare`
- D) `--continue`

<details><summary>Ответ</summary>

**B.** Без `--verbose` такое сочетание завершается ошибкой. `--model`, `--bare` и `--continue` для потокового вывода не обязательны.
→ [Экспертная теория](expert.ru.md)

</details>

## 11. 🔴 Что вернёт исчерпанный `--max-turns 1`?

- A) пустой вывод
- B) повторный запрос
- C) `error_max_turns` и код 1
- D) `success` и код 0

<details><summary>Ответ</summary>

**C.** Исчерпание лимита ходов — это ошибка: подтип результата `error_max_turns`, код выхода 1. Успехом это не считается, и вывод не пуст.
→ [Экспертная теория](expert.ru.md)

</details>

## 12. 🔴 Открытый вопрос: настройте ночной запуск, который чинит lint-ошибки и открывает PR.

<details><summary>Ответ</summary>

Изолированный раннер; `-p` с точной задачей; разрешить только редактирование и команды линтера, `dontAsk`; `--max-turns` и бюджет; ветка и PR вместо записи в main; проверка тестами; человек смотрит PR; разбирать `permission_denials` и код выхода.
→ [Продвинутая теория](advanced.ru.md)

</details>
