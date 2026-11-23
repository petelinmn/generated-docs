---
title: "Python"
lang: ru
track: python
kind: track
doc: overview
slug: python
tags: [python]
versions: "Python 3.12+"
default_level: 4
status: draft
updated: 2026-11-20
---

# Python

🌐 **Русский** · [English](overview.en.md)

## Коротко

Python — интерпретируемый язык общего назначения с динамической строгой типизацией и упором на читаемость кода. Эталонная реализация — CPython. Огромная экосистема пакетов в PyPI сделала Python основным языком для автоматизации, анализа данных, машинного обучения и одним из популярных языков для бэкенда. Аннотации типов необязательны, но проверяются внешними инструментами (mypy, pyright) и широко используются в современном коде.

## Окружение

| Что | Инструмент |
|---|---|
| Python | 3.12+, устанавливается через uv — `uv python install` |
| Менеджер проектов | uv — зависимости, виртуальные окружения, запуск |
| Редактор | VS Code + Python + Ruff, или PyCharm |
| Код модуля | `code/` — `pyproject.toml` (зависимости и настройки pytest), `examples/`, `solutions/`, `tests/` |
| Запустить все тесты модуля | `uv run pytest` в папке `code/` |
| Запустить один пример | `uv run pytest -k <имя>` |
| Линтер и форматтер | ruff — `uvx ruff check`, `uvx ruff format` |

## Дорожная карта

Прогресс изучения — в [PROGRESS.md](../../PROGRESS.md). Уровень детализации модулей по умолчанию — **4** (теория и самопроверка, без кода), см. [уровни детализации](../../README.ru.md#уровни-детализации).

### Часть 1. Основы

- [01 · Окружение и инструменты](modules/01-environment/overview.ru.md) — интерпретатор, uv, venv, pyproject.toml, REPL, ruff
- [02 · Типы и структуры данных](modules/02-types-and-data-structures/overview.ru.md) — числа, строки, list/tuple/dict/set, изменяемость, объекты и ссылки
- [03 · Управление потоком и функции](modules/03-control-flow-and-functions/overview.ru.md) — if/match, циклы, `*args`/`**kwargs`, области видимости
- [04 · Модули и пакеты](modules/04-modules-and-packages/overview.ru.md) — import, `__init__.py`, структура проекта, `__name__ == "__main__"`
- [05 · ООП](modules/05-oop/overview.ru.md) — классы, наследование и MRO, dunder-методы, property, dataclasses
- [06 · Исключения и контекстные менеджеры](modules/06-exceptions-and-context-managers/overview.ru.md) — try/except/else/finally, свои исключения, ExceptionGroup, with
- [07 · Итераторы и генераторы](modules/07-iterators-and-generators/overview.ru.md) — протокол итерации, yield, yield from, itertools
- [08 · Comprehensions и функциональный стиль](modules/08-comprehensions-and-functional/overview.ru.md) — comprehensions, lambda, map/filter, functools

### Часть 2. Продвинутые темы

- [09 · Аннотации типов](modules/09-type-hints/overview.ru.md) — typing, generics, Protocol, TypedDict, mypy и pyright
- [10 · Декораторы и замыкания](modules/10-decorators-and-closures/overview.ru.md) — замыкания, декораторы с параметрами, `functools.wraps`
- [11 · Файлы и ввод-вывод](modules/11-files-and-io/overview.ru.md) — pathlib, кодировки, json/csv/tomllib, сериализация
- [12 · Тестирование: pytest](modules/12-testing-pytest/overview.ru.md) — тесты, фикстуры, parametrize, unittest.mock, coverage
- [13 · Тестирование: продвинутое](modules/13-testing-advanced/overview.ru.md) — property-based (Hypothesis), тесты async-кода, интеграционные тесты
- [14 · Asyncio](modules/14-asyncio/overview.ru.md) — event loop, корутины, TaskGroup, таймауты, асинхронные итераторы
- [15 · Потоки и процессы](modules/15-concurrency/overview.ru.md) — threading, multiprocessing, concurrent.futures, GIL и free-threaded Python
- [16 · Упаковка и публикация](modules/16-packaging/overview.ru.md) — pyproject, сборка wheel, зависимости, публикация пакета

### Часть 3. Темы для опытных

- [17 · Модель данных и внутренности](modules/17-data-model-internals/overview.ru.md) — дескрипторы, `__slots__`, метаклассы, управление памятью
- [18 · Производительность](modules/18-performance/overview.ru.md) — профилирование (cProfile, py-spy), оптимизация, обзор нативных расширений
- [19 · Веб на FastAPI](modules/19-web-fastapi/overview.ru.md) — роутинг, Pydantic-модели, зависимости, async, OpenAPI, тесты
- [20 · Работа с БД](modules/20-databases/overview.ru.md) — sqlite3, SQLAlchemy 2.0, Alembic, асинхронный доступ
- [21 · Анализ данных](modules/21-data-analysis/overview.ru.md) — NumPy, pandas, Polars: основы и сравнение

## Библиотеки

- [httpx](libraries/httpx/overview.ru.md) — HTTP-клиент, sync и async
- [Pydantic](libraries/pydantic/overview.ru.md) — валидация данных и настройки (pydantic-settings)
- [Typer](libraries/typer/overview.ru.md) — CLI на аннотациях типов
- [Rich](libraries/rich/overview.ru.md) — форматированный вывод в терминал
- [Tenacity](libraries/tenacity/overview.ru.md) — ретраи с backoff
- [structlog](libraries/structlog/overview.ru.md) — структурное логирование
- [Beautiful Soup](libraries/beautifulsoup/overview.ru.md) — парсинг HTML

## Рецепты

- [CLI-приложение](recipes/cli-app/overview.ru.md) — подкоманды, конфиг, вывод прогресса
- [Конфигурация приложения](recipes/config-management/overview.ru.md) — переменные окружения, `.env`, pydantic-settings
- [Настройка логирования](recipes/logging-setup/overview.ru.md) — logging для приложения и библиотеки
- [Асинхронный парсер](recipes/async-scraper/overview.ru.md) — ограничение параллелизма, ретраи
- [Фоновые задачи](recipes/background-tasks/overview.ru.md) — очереди задач (Celery, RQ, arq)
- [Аутентификация в FastAPI](recipes/fastapi-auth/overview.ru.md) — OAuth2 + JWT

## Проекты

- [CLI-утилита как пакет](projects/cli-tool/overview.ru.md) — утилита, которая ставится через `uv tool install`
- [FastAPI-сервис](projects/fastapi-service/overview.ru.md) — REST API: FastAPI, SQLAlchemy, тесты, Docker
- [ETL-пайплайн](projects/data-pipeline/overview.ru.md) — загрузка, обработка и отчёт по данным

## Связанные треки

- [C# / .NET](../csharp/overview.ru.md) — второй язык: удобно сравнивать подходы
- [LLM](../llm/overview.ru.md) — основной язык LLM-трека, нужны модули 01–08
- [Kubernetes](../kubernetes/overview.ru.md) — деплой Python-приложений
- [JavaScript](../javascript/overview.ru.md) — другой динамический язык: сравните модель выполнения и асинхронность
- [TypeScript](../typescript/overview.ru.md) — статические типы в другой экосистеме: сравните с аннотациями типов Python

## Источники

- [Документация Python](https://docs.python.org/3/)
- [Документация uv](https://docs.astral.sh/uv/)
- [Документация pytest](https://docs.pytest.org/)
