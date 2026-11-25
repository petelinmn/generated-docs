---
title: "Квиз: окружение и инструменты"
lang: ru
track: python
kind: module
doc: quiz
slug: 01-environment
part: 1
tags: [python, uv, virtual-environment, pyproject, repl, ruff]
versions: "Python 3.12+"
status: draft
updated: 2026-11-25
---

# Квиз: окружение и инструменты

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте сами, потом откройте ответ. Метка показывает уровень: 🟢 [базовая](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Какая команда запускает модуль проекта как исполняемую точку входа?

- A) `python -c package.module`
- B) `python package.module`
- C) `python -m package.module`
- D) `python --run package.module`

<details><summary>Ответ</summary>

**C.** Флаг `-m` находит модуль через механизм импорта и запускает его.
→ [Базовая теория](basics.ru.md#установка-и-запуск)

</details>

## 2. 🟢 Для чего нужен `uv run`?

- A) Только устанавливает Python
- B) Запускает команду в окружении проекта
- C) Форматирует все файлы
- D) Открывает интерактивный shell без изменений

<details><summary>Ответ</summary>

**B.** `uv run` синхронизирует состояние окружения проекта и запускает указанную команду.
→ [Базовая теория](basics.ru.md#uv-проект)

</details>

## 3. 🟢 Какой файл описывает зависимости проекта?

- A) `.venv`
- B) `uv.lock`
- C) `pyproject.toml`
- D) `__pycache__/`

<details><summary>Ответ</summary>

**C.** Декларируемые метаданные и зависимости проекта находятся в `pyproject.toml`.
→ [Базовая теория](basics.ru.md#uv-проект)

</details>

## 4. 🟢 Что делает Ruff?

- A) Устанавливает пакеты
- B) Проверяет и форматирует исходный код
- C) Запускает приложение
- D) Создаёт виртуальное окружение

<details><summary>Ответ</summary>

**B.** Ruff выполняет статический анализ и форматирование; тесты остаются отдельной проверкой.
→ [Базовая теория](basics.ru.md#ruff)

</details>

## 5. 🟢 Что обычно не коммитят в Git?

- A) `pyproject.toml`
- B) `uv.lock`
- C) `.venv/`
- D) Исходный `.py`

<details><summary>Ответ</summary>

**C.** `.venv/` является локальным зависимым от платформы артефактом.
→ [Продвинутая теория](advanced.ru.md#управление-зависимостями)

</details>

## 6. 🟡 Как проверить активный интерпретатор из кода?

- A) `import sys; print(sys.executable)`
- B) `import os; print(os.path)`
- C) `print(__name__)`
- D) `print(locals())`

<details><summary>Ответ</summary>

**A.** `sys.executable` указывает на исполняемый файл интерпретатора.
→ [Продвинутая теория](advanced.ru.md#выбор-интерпретатора)

</details>

## 7. 🟡 Для чего служит `uv.lock`?

- A) Блокирует редактирование исходников
- B) Фиксирует точные выбранные версии
- C) Содержит системные переменные
- D) Форматирует Python-файлы

<details><summary>Ответ</summary>

**B.** Lock-файл нужен для воспроизводимой установки после разрешения зависимостей.
→ [Продвинутая теория](advanced.ru.md#управление-зависимостями)

</details>

## 8. 🟡 Что выведет этот код вне и внутри virtual environment при равных префиксах?

```python
import sys
print(sys.prefix == sys.base_prefix)
```

- A) Всегда `True`
- B) Вне окружения `True`, внутри `False`
- C) Вне окружения `False`, внутри `True`
- D) Код всегда вызывает ошибку

<details><summary>Ответ</summary>

**B.** В базовом интерпретаторе префиксы совпадают; в virtual environment обычно отличаются.
→ [Экспертная теория](expert.ru.md#виртуальные-окружения)

</details>

## 9. 🔴 Найдите проблему

Команда `pip install rich` выполняется успешно, но `python app.py` сообщает `ModuleNotFoundError: rich`.

<details><summary>Ответ</summary>

Вероятно, `pip` принадлежит другому интерпретатору или другому virtual environment. Проверьте `sys.executable` для Python и используйте `uv add rich` или `python -m pip install rich`.
→ [Экспертная теория](expert.ru.md#два-python-и-один-pip)

</details>

## 10. 🔴 Открытый вопрос

Назовите три артефакта проекта и объясните, какие из них хранить в Git.

<details><summary>Ключевые пункты ответа</summary>

- `pyproject.toml` хранить в Git как манифест.
- `uv.lock` хранить для воспроизводимости зависимостей.
- `.venv/` не хранить; оно локальное и платформенно-зависимое.

</details>

→ [Продвинутая теория](advanced.ru.md#управление-зависимостями)
