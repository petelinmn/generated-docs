---
title: "Окружение и инструменты: базовая теория"
lang: ru
track: python
kind: module
doc: basics
slug: 01-environment
part: 1
tags: [python, interpreter, uv, virtual-environment, pyproject, repl, ruff]
versions: "Python 3.12+"
status: draft
updated: 2026-11-25
---

# Окружение и инструменты: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** нет · **Время:** ~1 ч

## Зачем это нужно

Python-код нужно установить, запустить и изолировать от других проектов. uv помогает выполнить эти задачи одной системой команд, а `pyproject.toml` хранит конфигурацию проекта рядом с исходниками.

## Установка и запуск

Проверить доступный Python можно командой:

```bash
uv python install 3.12
uv python find 3.12
python --version
```

Скрипт запускается через путь к файлу:

```bash
python hello.py
```

Модуль стандартной библиотеки или проекта запускается без расширения `.py`:

```bash
python -m pytest
python -m mypackage.cli
```

В интерактивной оболочке можно проверить выражение построчно:

```python
>>> 2 + 3
5
>>> print("hello")
hello
```

## uv-проект

Создание минимального проекта:

```bash
uv init demo
cd demo
uv add rich
uv run python -c "import rich; print(rich.__version__)"
```

Команда `uv init` создаёт структуру проекта. `uv add` изменяет зависимости в `pyproject.toml` и обновляет блокировку `uv.lock`. `uv run` создаёт или обновляет окружение и запускает команду в контексте проекта.

## Виртуальное окружение

uv обычно создаёт `.venv` автоматически. Эквивалентный минимальный файл конфигурации выглядит так:

```toml
[project]
name = "demo"
version = "0.1.0"
requires-python = ">=3.12"
dependencies = []

[tool.ruff]
line-length = 88
```

Вручную создать окружение стандартной библиотекой можно так:

```bash
python -m venv .venv
```

На Windows активируют окружение через `.venv\Scripts\activate`, но с uv обычно достаточно `uv run`.

## Ruff

Проверка и форматирование:

```bash
uvx ruff check .
uvx ruff format .
```

Ruff выполняет статические проверки и меняет форматирование файлов. Он не запускает код и не заменяет тесты.

## Ошибки новичка

- ❌ Запускать `python` и устанавливать пакеты в разные окружения → ✅ проверять интрепретатор через `uv run`.
- ❌ Устанавливать пакеты системно → ✅ указывать проектные зависимости через `uv add`.
- ❌ Коммитить `.venv/` → ✅ игнорировать каталог в `.gitignore`.
- ❌ Запускать модуль как `python mypackage/cli.py` → ✅ для импортов пакета использовать `python -m mypackage.cli`.
- ❌ Смешивать несколько версий Python без явного указания → ✅ задавать `requires-python` и выбирать версию uv.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md).
- Следующий модуль: по [дорожной карте трека Python](../../overview.ru.md).

## Источники

- [Python tutorial: virtual environments](https://docs.python.org/3/tutorial/venv.html)
- [Python command-line interface](https://docs.python.org/3/using/cmdline.html)
- [uv projects](https://docs.astral.sh/uv/guides/projects/)
