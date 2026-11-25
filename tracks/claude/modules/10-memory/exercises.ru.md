---
title: "CLAUDE.md и память: упражнения"
lang: ru
track: claude
kind: module
doc: exercises
slug: 10-memory
part: 2
tags: [claude, claude-code, claude-md, memory, context]
versions: "Claude Code 2.1.332"
status: draft
updated: 2026-11-25
---

# CLAUDE.md и память: упражнения

🌐 **Русский** · [English](exercises.en.md)

> Нужен учебный репозиторий и установленный Claude Code.

## 1. Короткий CLAUDE.md

Напишите файл с командой тестов и двумя соглашениями.

<details><summary>Подсказка</summary>

Конкретно и проверяемо.

</details>

<details><summary>Решение</summary>

```markdown
# Project

- Tests: `pytest -q`.
- Public functions have type hints.
- Do not edit files under `vendor/`.
```

</details>

## 2. Правило по пути

Правило только для тестов.

<details><summary>Подсказка</summary>

`paths` во front matter.

</details>

<details><summary>Решение</summary>

```markdown
---
paths:
  - "tests/**"
---

- One behaviour per test.
```

</details>

## 3. Общий файл с другими агентами

Есть `AGENTS.md`; добавьте одно указание только для Claude.

<details><summary>Подсказка</summary>

Импорт.

</details>

<details><summary>Решение</summary>

```markdown
@AGENTS.md

## Claude Code

- Use plan mode for changes under `src/billing/`.
```

</details>

## 4. Заметка для людей

Оставьте в файле пометку, не тратящую контекст.

<details><summary>Подсказка</summary>

HTML-комментарий.

</details>

<details><summary>Решение</summary>

```markdown
<!-- Owner: platform team. Review quarterly. -->
```

</details>

## 5. Диагностика

Claude не видит ваш файл. Как выяснить почему?

<details><summary>Подсказка</summary>

Две команды и один флаг.

</details>

<details><summary>Решение</summary>

`/memory` и `/context` покажут загруженные файлы; `claude --debug-file debug.log` и поиск `CLAUDE.md` в журнале — причину пропуска. Проверьте каталог запуска, `claudeMdExcludes` и `--setting-sources`.

</details>
