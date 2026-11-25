---
title: "Окружение и инструменты: продвинутая теория"
lang: ru
track: python
kind: module
doc: advanced
slug: 01-environment
part: 1
tags: [python, sys-path, interpreter-selection, dependencies, environment-variables]
versions: "Python 3.12+"
status: draft
updated: 2026-11-25
---

# Окружение и инструменты: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

Опирается на [базовую теорию](basics.ru.md).

## Выбор интерпретатора

Файл должен запускаться тем же Python, который установлен в проекте:

```python
import sys
import sysconfig

print(sys.executable)
print(sys.version)
print(sysconfig.get_paths()["purelib"])
```

Проверка из uv:

```bash
uv run python -c "import sys; print(sys.executable)"
```

Если редактор использует другой интерпретатор, VS Code может подсвечивать правильно установленный пакет как отсутствующий.

## Пути поиска модулей

При запуске скрипта Python формирует `sys.path`, который показывает, где искать модули:

```python
import sys

for path in sys.path:
    print(path)
```

`PYTHONPATH` добавляет каталоги поиска, но не должен заменять управление зависимостями в проекте.

## Управление зависимостями

Различайте три файла:

| Файл | Назначение |
|---|---|
| `pyproject.toml` | декларируемые зависимости и настройки инструментов |
| `uv.lock` | воспроизводимый набор точных версий |
| `.venv/` | локально установленное окружение |

Типичный цикл:

```bash
uv add httpx
uv run python -c "import httpx; print(httpx.__version__)"
uv sync --locked
```

`uv sync --locked` проверяет, что блокировка не меняется, и устанавливает состояние окружения по ней.

## Переменные окружения

Для локальных процессов экспортируйте переменные явно:

```bash
set APP_ENV=development
uv run python -c "import os; print(os.getenv('APP_ENV'))"
```

В Bash на macOS/Linux команда другая: `APP_ENV=development uv run python -c "..."`.

Секреты не следует записывать в `pyproject.toml`, исходники и Git. Для хранилища приложений используйте переменные среды или секретный менеджер.

## Лучшие практики

- фиксируйте минимальную версию через `requires-python`;
- запускайте проект через `uv run`;
- коммитьте `pyproject.toml` и `uv.lock`, но не `.venv/`;
- настройте редактор на интерпретатор проекта;
- используйте Ruff из uv или uvx;
- отделяйте production-зависимости от инструментов разработки.

## Частые ошибки

- ❌ Запускать скрипт из системного Python → ✅ запускать `uv run python`.
- ❌ Добавлять каталог с пакетом через `PYTHONPATH` навсегда → ✅ оформлять пакет и зависимости нормально.
- ❌ Удалять `uv.lock` для «мягких версий» → ✅ блокировать воспроизводимость.
- ❌ Хранить токены в коде → ✅ передавать их переменными среды.

## Связанные материалы

- Базовый уровень: [01 · Окружение и инструменты](basics.ru.md).
- Следующий модуль: по [дорожной карте трека Python](../../overview.ru.md).
- Сравнение окружений: [JavaScript: окружение](../../../javascript/modules/01-environment/overview.ru.md).

## Источники

- [Python: virtual environment tutorial](https://docs.python.org/3/tutorial/venv.html)
- [Python: sys.path](https://docs.python.org/3/library/sys.html#sys.path)
- [uv: project dependencies](https://docs.astral.sh/uv/concepts/projects/dependencies/)
- [uv: locking and syncing](https://docs.astral.sh/uv/concepts/projects/sync/)
