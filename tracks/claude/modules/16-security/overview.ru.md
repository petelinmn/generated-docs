---
title: "Безопасность и compliance"
lang: ru
track: claude
kind: module
doc: overview
slug: 16-security
part: 3
tags: [claude, security, prompt-injection, secrets, compliance]
versions: "Claude 5.5 family; Claude Code 2.1.332; anthropic 2.14.0"
levels: [1, 2, 3, 4]
status: draft
updated: 2026-11-25
---

# Безопасность и compliance

🌐 **Русский** · [English](overview.en.md)

## Коротко

Как не навредить приложением и агентом на Claude: **ключи**, проверка действий модели, **prompt injection**, права и sandbox, хранение данных.

## Ключевые идеи

- Ответ модели — недоверенный ввод.
- Минимальные права важнее формулировок промпта.
- Чужой текст может содержать инструкции.
- Секреты не живут в коде, логах и браузере.
- Хранение данных зависит от договора, модели и функции.

## Минимальный пример

```python
# safe_tools.py
from pathlib import Path

ROOT = Path("workspace").resolve()


def safe_path(relative):
    """Resolve a model-supplied path and refuse anything outside ROOT."""
    path = (ROOT / relative).resolve()
    if not path.is_relative_to(ROOT):
        raise PermissionError(f"path outside the workspace: {relative}")
    return path


ALLOWED = {"read_file": False, "send_email": True}     # tool -> needs human confirmation


def authorise(tool, confirmed=False):
    if tool not in ALLOWED:
        raise PermissionError(f"unknown tool: {tool}")
    if ALLOWED[tool] and not confirmed:
        raise PermissionError(f"{tool} needs confirmation")
```

## Когда применять и когда нет

Всегда, когда модель может что-то сделать или видит чужие данные.

## Уровни материала

| Уровень | Файл | Добавить |
|---|---|---|
| 1. Базовая теория | [basics.ru.md](basics.ru.md) | |
| 2. Продвинутая теория | [advanced.ru.md](advanced.ru.md) | |
| 3. Экспертная теория | [expert.ru.md](expert.ru.md) | |
| 4. Самопроверка | [quiz.ru.md](quiz.ru.md), [exercises.ru.md](exercises.ru.md) | |
| 5. Запускаемые примеры | — | `/learn claude 16-security level 5` |
| 6. Тесты | — | `/learn claude 16-security level 6` |

## Связанные материалы

- [Трек Claude](../../overview.ru.md)
- [07 · Tool use](../07-tool-use/overview.ru.md)
- [09 · Claude Code](../09-claude-code/overview.ru.md)
- [15 · Observability](../15-observability/overview.ru.md)
- [17 · Развёртывание и production](../17-production/overview.ru.md)
