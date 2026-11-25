---
title: "Упражнения: окружение и инструменты"
lang: ru
track: python
kind: module
doc: exercises
slug: 01-environment
part: 1
tags: [python, uv, virtual-environment, pyproject, ruff]
status: draft
updated: 2026-11-25
---

# Упражнения: окружение и инструменты

🌐 **Русский** · [English](exercises.en.md)

> Решайте самостоятельно до открытия подсказки. Примеры проверяются запуском в установленном Python 3.12+.

## 1. 🟢 Создайте проект

**Задача:** создайте каталог `demo-env`, инициализируйте uv-проект и запустите Python в его окружении.

**Ожидаемый результат:** команда печатает executable и версию Python.

<details><summary>Подсказка</summary>

Используйте `uv init`, `cd` и `uv run python -c`.

</details>

<details><summary>Решение</summary>

```bash
uv init demo-env
cd demo-env
uv run python -c "import sys; print(sys.executable); print(sys.version)"
```

</details>

## 2. 🟢 Добавьте зависимость

**Задача:** добавьте пакет `rich` и проверьте его импорт через проектный интерпретатор.

**Ожидаемый результат:** команда завершается без ошибки.

<details><summary>Подсказка</summary>

Не используйте глобальный `pip`; сначала выполните `uv add`.

</details>

<details><summary>Решение</summary>

```bash
uv add rich
uv run python -c "import rich; print(rich.__version__)"
```

</details>

## 3. 🟡 Определите окружение

**Задача:** запустите скрипт, который печатает executable, префикс и признак virtual environment.

**Ожидаемый результат:** вне venv префиксы совпадают; внутри виртуального окружения отличаются.

<details><summary>Подсказка</summary>

Сравните `sys.prefix` и `sys.base_prefix`.

</details>

<details><summary>Решение</summary>

```python
import sys

print(sys.executable)
print("virtual:", sys.prefix != sys.base_prefix)
print("prefix:", sys.prefix)
print("base:", sys.base_prefix)
```

Запускайте через `uv run python env_info.py`.

</details>

## 4. 🟡 Диагностируйте поиск модулей

**Задача:** напишите скрипт, который печатает все первые три элемента `sys.path`.

**Ожидаемый результат:** видны каталог запуска и каталог проекта или стандартные библиотеки.

<details><summary>Подсказка</summary>

Используйте срез `sys.path[:3]`.

</details>

<details><summary>Решение</summary>

```python
import sys

for index, path in enumerate(sys.path[:3]):
    print(index, path)
```

</details>

## 5. 🔴 Предложите воспроизводимую сборку

**Задача:** объясните, какие файлы должны попасть в Git и как установить проект на другой машине.

**Ожидаемый результат:** описаны `pyproject.toml`, `uv.lock`, отсутствие `.venv/` и команда `uv sync --locked`.

<details><summary>Подсказка</summary>

Разделите декларацию зависимостей, точную блокировку и локальное состояние.

</details>

<details><summary>Решение</summary>

В Git коммитятся `pyproject.toml` и `uv.lock`. Каталог `.venv/` игнорируется. На новой машине выполняется `uv sync --locked`, после чего проект запускается через `uv run`.

```bash
uv sync --locked
uv run python --version
```

</details>
