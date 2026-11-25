---
title: "Environment and tools: basics"
lang: en
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

# Environment and tools: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** none · **Time:** ~1 h

## Why it matters

Python code must be installed, executed, and isolated from other projects. uv helps with these tasks through one command system, while `pyproject.toml` stores project configuration next to the source.

## Installation and execution

Check an available Python with:

```bash
uv python install 3.12
uv python find 3.12
python --version
```

Run a script through its file path:

```bash
python hello.py
```

Run a standard-library or project module without the `.py` extension:

```bash
python -m pytest
python -m mypackage.cli
```

In the interactive shell, evaluate expressions line by line:

```python
>>> 2 + 3
5
>>> print("hello")
hello
```

## An uv project

Create a minimal project:

```bash
uv init demo
cd demo
uv add rich
uv run python -c "import rich; print(rich.__version__)"
```

`uv init` creates the project structure. `uv add` changes dependencies in `pyproject.toml` and updates `uv.lock`. `uv run` creates or updates the environment and runs the command in the project context.

## Virtual environment

uv usually creates `.venv` automatically. The equivalent minimal configuration looks like this:

```toml
[project]
name = "demo"
version = "0.1.0"
requires-python = ">=3.12"
dependencies = []

[tool.ruff]
line-length = 88
```

Create an environment manually with the standard library:

```bash
python -m venv .venv
```

On Windows, activate it through `.venv\Scripts\activate`, but `uv run` is usually enough.

## Ruff

Check and format:

```bash
uvx ruff check .
uvx ruff format .
```

Ruff performs static checks and changes file formatting. It does not execute code and does not replace tests.

## Beginner mistakes

- ❌ Run `python` and install packages into different environments → ✅ check the interpreter through `uv run`.
- ❌ Install project packages globally → ✅ declare dependencies through `uv add`.
- ❌ Commit `.venv/` → ✅ ignore that directory in `.gitignore`.
- ❌ Run a module as `python mypackage/cli.py` → ✅ use `python -m mypackage.cli` for package imports.
- ❌ Mix several Python versions without explicit selection → ✅ set `requires-python` and select a version with uv.

## Related materials

- Next: [advanced theory](advanced.en.md).
- Next module: the [Python track roadmap](../../overview.en.md).

## Sources

- [Python tutorial: virtual environments](https://docs.python.org/3/tutorial/venv.html)
- [Python command-line interface](https://docs.python.org/3/using/cmdline.html)
- [uv projects](https://docs.astral.sh/uv/guides/projects/)
