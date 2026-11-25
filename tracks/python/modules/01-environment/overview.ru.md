---
title: "Окружение и инструменты"
lang: ru
track: python
kind: module
doc: overview
slug: 01-environment
part: 1
levels: [1, 2, 3, 4]
tags: [python, interpreter, uv, virtual-environment, pyproject, repl, ruff]
versions: "Python 3.12+"
status: draft
updated: 2026-11-25
---

# Окружение и инструменты

🌐 **Русский** · [English](overview.en.md)

## Коротко

Python-код выполняет интерпретатор CPython; для управления проектами удобнее использовать uv. Окружение хранит конкретные зависимости проекта и отделяет его от системного Python, а `pyproject.toml` описывает метаданные и инструменты. REPL нужен для быстрых проверок, а Ruff — для линтинга и форматирования.

## Ключевые идеи

- **Интерпретатор запускает скрипты, модули и интерактивную консоль.**
- **uv устанавливает Python и управляет проектами и виртуальными окружениями.**
- **Виртуальное окружение содержит изолированный набор пакетов.**
- **`pyproject.toml` заменяет разрозненные настройки одного проекта.**
- **`python -m module` запускает модуль через стандартный механизм импорта.**
- **Ruff проверяет и форматирует код без изменения смысла программы.**

## Минимальный пример

```bash
uv init hello
cd hello
uv run python -c "import sys; print(sys.version.split()[0])"
```

```text
3.12.x
```

> Точный выпуск зависит от установленного Python.

## Когда применять

- ✅ Используйте uv для создания проектов и запуска команд.
- ✅ Запускайте проект через `uv run`, чтобы зависимости брались из проекта.
- ✅ Создавайте отдельное окружение для каждого проекта.
- ❌ Не устанавливайте зависимости проекта глобально в системный Python.
- ❌ Не отправляйте `.venv/` и локальные кэши в Git.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | установка, запуск, uv, REPL, `pyproject.toml`, Ruff |
| [2. Продвинутая теория](advanced.ru.md) | пути модулей, выбор интерпретатора, зависимости, переменные окружения |
| [3. Экспертная теория](expert.ru.md) | внутреннее устройство запуска, справочник и типичные подвохи |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| 5. Запускаемые примеры | ещё не создан — `/learn python 01-environment level 5` |
| 6. Тесты | ещё не создан — `/learn python 01-environment level 6` |

## Связанные материалы

- Пререквизиты: нет.
- Дальше: следующий модуль по [дорожной карте трека Python](../../overview.ru.md).
- Аналог в JavaScript: [01 · Окружение и инструменты](../../../javascript/modules/01-environment/overview.ru.md).
- Аналог в C#: [01 · Платформа .NET](../../../csharp/modules/01-dotnet-platform/overview.ru.md).

## Источники

- [Python command line and environment](https://docs.python.org/3/using/cmdline.html)
- [Python virtual environments](https://docs.python.org/3/tutorial/venv.html)
- [uv: Getting started](https://docs.astral.sh/uv/getting-started/)
- [uv: Projects](https://docs.astral.sh/uv/guides/projects/)
- [Ruff](https://docs.astral.sh/ruff/)
