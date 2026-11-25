---
title: "Python"
lang: en
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

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

Python is an interpreted, general-purpose language with dynamic, strong typing and a focus on readable code. The reference implementation is CPython. A huge package ecosystem on PyPI has made Python the main language for automation, data analysis and machine learning, and one of the popular languages for backends. Type annotations are optional, but they are checked by external tools (mypy, pyright) and widely used in modern code.

## Environment

| What | Tool |
|---|---|
| Python | 3.12+, installed via uv — `uv python install` |
| Project manager | uv — dependencies, virtual environments, running code |
| Editor | VS Code + Python + Ruff, or PyCharm |
| Module code | `code/` — `pyproject.toml` (dependencies and pytest settings), `examples/`, `solutions/`, `tests/` |
| Run all module tests | `uv run pytest` inside `code/` |
| Run one example | `uv run pytest -k <name>` |
| Linter and formatter | ruff — `uvx ruff check`, `uvx ruff format` |

## Roadmap

Learning progress is tracked in [PROGRESS.md](../../PROGRESS.md). Default detail level for modules: **4** (theory and self-check, no code), see [detail levels](../../README.en.md#detail-levels).

### Part 1. Fundamentals

- [01 · Environment and tools](modules/01-environment/overview.en.md) — interpreter, uv, venv, pyproject.toml, REPL, ruff
- [02 · Types and data structures](modules/02-types-and-data-structures/overview.en.md) — numbers, strings, list/tuple/dict/set, mutability, objects and references
- [03 · Control flow and functions](modules/03-control-flow-and-functions/overview.en.md) — if/match, loops, `*args`/`**kwargs`, scopes
- [04 · Modules and packages](modules/04-modules-and-packages/overview.en.md) — import, `__init__.py`, project layout, `__name__ == "__main__"`
- [05 · OOP](modules/05-oop/overview.en.md) — classes, inheritance and MRO, dunder methods, property, dataclasses
- [06 · Exceptions and context managers](modules/06-exceptions-and-context-managers/overview.en.md) — try/except/else/finally, custom exceptions, ExceptionGroup, with
- [07 · Iterators and generators](modules/07-iterators-and-generators/overview.en.md) — iteration protocol, yield, yield from, itertools
- [08 · Comprehensions and functional style](modules/08-comprehensions-and-functional/overview.en.md) — comprehensions, lambda, map/filter, functools

### Part 2. Advanced topics

- [09 · Type hints](modules/09-type-hints/overview.en.md) — typing, generics, Protocol, TypedDict, mypy and pyright
- [10 · Decorators and closures](modules/10-decorators-and-closures/overview.en.md) — closures, parameterized decorators, `functools.wraps`
- [11 · Files and I/O](modules/11-files-and-io/overview.en.md) — pathlib, encodings, json/csv/tomllib, serialization
- [12 · Testing: pytest](modules/12-testing-pytest/overview.en.md) — tests, fixtures, parametrize, unittest.mock, coverage
- [13 · Testing: advanced](modules/13-testing-advanced/overview.en.md) — property-based testing (Hypothesis), testing async code, integration tests
- [14 · Asyncio](modules/14-asyncio/overview.en.md) — event loop, coroutines, TaskGroup, timeouts, async iterators
- [15 · Threads and processes](modules/15-concurrency/overview.en.md) — threading, multiprocessing, concurrent.futures, the GIL and free-threaded Python
- [16 · Packaging and publishing](modules/16-packaging/overview.en.md) — pyproject, building wheels, dependencies, publishing a package

### Part 3. Expert topics

- [17 · Data model and internals](modules/17-data-model-internals/overview.en.md) — descriptors, `__slots__`, metaclasses, memory management
- [18 · Performance](modules/18-performance/overview.en.md) — profiling (cProfile, py-spy), optimization, native extensions overview
- [19 · Web with FastAPI](modules/19-web-fastapi/overview.en.md) — routing, Pydantic models, dependencies, async, OpenAPI, tests
- [20 · Databases](modules/20-databases/overview.en.md) — sqlite3, SQLAlchemy 2.0, Alembic, async access
- [21 · Data analysis](modules/21-data-analysis/overview.en.md) — NumPy, pandas, Polars: basics and comparison

## Libraries

- [httpx](libraries/httpx/overview.en.md) — HTTP client, sync and async
- [Pydantic](libraries/pydantic/overview.en.md) — data validation and settings (pydantic-settings)
- [Typer](libraries/typer/overview.en.md) — CLIs built on type annotations
- [Rich](libraries/rich/overview.en.md) — formatted terminal output
- [Tenacity](libraries/tenacity/overview.en.md) — retries with backoff
- [structlog](libraries/structlog/overview.en.md) — structured logging
- [Beautiful Soup](libraries/beautifulsoup/overview.en.md) — HTML parsing

## Recipes

- [CLI application](recipes/cli-app/overview.en.md) — subcommands, configuration, progress output
- [Application configuration](recipes/config-management/overview.en.md) — environment variables, `.env`, pydantic-settings
- [Logging setup](recipes/logging-setup/overview.en.md) — logging for an application and a library
- [Async scraper](recipes/async-scraper/overview.en.md) — limiting concurrency, retries
- [Background tasks](recipes/background-tasks/overview.en.md) — task queues (Celery, RQ, arq)
- [Authentication in FastAPI](recipes/fastapi-auth/overview.en.md) — OAuth2 + JWT

## Projects

- [CLI tool as a package](projects/cli-tool/overview.en.md) — a tool installable with `uv tool install`
- [FastAPI service](projects/fastapi-service/overview.en.md) — REST API: FastAPI, SQLAlchemy, tests, Docker
- [ETL pipeline](projects/data-pipeline/overview.en.md) — loading, processing and reporting on data

## Related tracks

- [C# / .NET](../csharp/overview.en.md) — a second language: handy for comparing approaches
- [LLM](../llm/overview.en.md) — the main language of the LLM track, modules 01–08 are required
- [Kubernetes](../kubernetes/overview.en.md) — deploying Python applications
- [JavaScript](../javascript/overview.en.md) — another dynamic language: compare the execution model and asynchrony
- [TypeScript](../typescript/overview.en.md) — static types in another ecosystem: compare with Python type annotations
- [PostgreSQL](../postgresql/overview.en.md) — databases for Python applications

## Sources

- [Python documentation](https://docs.python.org/3/)
- [uv documentation](https://docs.astral.sh/uv/)
- [pytest documentation](https://docs.pytest.org/)
