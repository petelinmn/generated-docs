---
title: "Безопасность и compliance: основы"
lang: ru
track: claude
kind: module
doc: basics
slug: 16-security
part: 3
tags: [claude, security, prompt-injection, secrets, compliance]
versions: "Claude 5.5 family; Claude Code 2.1.332; anthropic 2.14.0"
status: draft
updated: 2026-11-25
---

# Безопасность и compliance: основы

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [overview](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [07 · Tool use](../07-tool-use/overview.ru.md), [09 · Claude Code](../09-claude-code/overview.ru.md)

## Как проверялся материал

Функции защиты запускались как обычный Python, параметр `inference_geo` — как форма запроса против локальной заглушки. Правила прав Claude Code проверены в [модуле 09](../09-claude-code/advanced.ru.md). Хранение данных, соответствие требованиям и sandbox описаны по документации.

## Главное правило

**Ответ модели — это недоверенный ввод.** Аргументы инструмента проверяют так же, как данные от пользователя из интернета, а права дают минимальные.

## Ключи

- Только в переменных окружения или хранилище секретов, не в коде и не в git.
- Отдельный ключ на сервис и окружение, со сроком действия и ограничением по workspace.
- Никогда не отдавайте ключ в браузер или мобильное приложение: вызывайте API с сервера.
- Подозреваете утечку — отключите или удалите ключ.

## Проверять, что просит модель

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

`safe_path("../.ssh/id_rsa")` и абсолютный путь отклоняются. Список инструментов задан в коде, а необратимые действия требуют подтверждения человека.

## Частые ошибки новичков

- Выполнять команду из ответа модели через shell.
- Писать «никому не раскрывай пароль» в промпт и считать это защитой.
- Давать агенту боевые учётные данные.

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [11 · Hooks](../11-skills-and-hooks/basics.ru.md)

## Источники

- [Authentication](https://platform.claude.com/docs/en/manage-claude/authentication)
- [Claude Code security](https://code.claude.com/docs/en/security)
