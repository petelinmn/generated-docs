---
title: "Claude Code: упражнения"
lang: ru
track: claude
kind: module
doc: exercises
slug: 09-claude-code
part: 2
tags: [claude, claude-code, permissions, headless, ci]
versions: "Claude Code 2.1.332"
status: draft
updated: 2026-11-25
---

# Claude Code: упражнения

🌐 **Русский** · [English](exercises.en.md)

> Выполняйте в учебном репозитории. Нужны установленный Claude Code и вход.

## 1. Запрет секретов

Запретите чтение `.env` для всей команды.

<details><summary>Подсказка</summary>

`permissions.deny` в проектном файле.

</details>

<details><summary>Решение</summary>

```json
{"permissions": {"deny": ["Read(./.env)"]}}
```

</details>

## 2. Только тесты

Запустите неинтерактивно так, чтобы из команд можно было выполнить только `npm test`.

<details><summary>Подсказка</summary>

`--allowedTools` и режим, отклоняющий остальное.

</details>

<details><summary>Решение</summary>

```bash
claude -p "Run the tests and summarise failures" --allowedTools "Bash(npm test)" --permission-mode dontAsk
```

</details>

## 3. Проверка в скрипте

Завершите скрипт ошибкой, если что-то было запрещено.

<details><summary>Подсказка</summary>

Разберите JSON.

</details>

<details><summary>Решение</summary>

```bash
claude -p "Fix the lint errors" --output-format json > out.json
python -c "import json,sys; d=json.load(open('out.json')); sys.exit(1 if d['is_error'] or d['permission_denials'] else 0)"
```

</details>

## 4. Продолжить сессию

Сделайте два запуска, где второй помнит первый.

<details><summary>Подсказка</summary>

`session_id` и `--resume`.

</details>

<details><summary>Решение</summary>

```bash
SID=$(claude -p "Remember the number 7" --output-format json | python -c "import json,sys; print(json.load(sys.stdin)['session_id'])")
claude -p "Which number?" --resume "$SID"
```

</details>

## 5. Структурированный ответ

Получите объект с полями `files` (число) и `language` (строка).

<details><summary>Подсказка</summary>

`--json-schema`.

</details>

<details><summary>Решение</summary>

```bash
claude -p "Describe this repository" --output-format json --json-schema '{"type":"object","properties":{"files":{"type":"integer"},"language":{"type":"string"}},"required":["files","language"]}'
# the object is in the structured_output field
```

</details>
