---
title: "Exercises: environment and tools"
lang: en
track: python
kind: module
doc: exercises
slug: 01-environment
part: 1
tags: [python, uv, virtual-environment, pyproject, ruff]
status: draft
updated: 2026-11-25
---

# Exercises: environment and tools

🌐 [Русский](exercises.ru.md) · **English**

> Solve them yourself before opening hints. Examples are validated with Python 3.12+.

## 1. 🟢 Create a project

**Task:** create a `demo-env` directory, initialize a uv project, and run Python in its environment.

**Expected result:** the command prints the executable and Python version.

<details><summary>Hint</summary>

Use `uv init`, `cd`, and `uv run python -c`.

</details>

<details><summary>Solution</summary>

```bash
uv init demo-env
cd demo-env
uv run python -c "import sys; print(sys.executable); print(sys.version)"
```

</details>

## 2. 🟢 Add a dependency

**Task:** add the `rich` package and verify its import through the project interpreter.

**Expected result:** the command completes without an error.

<details><summary>Hint</summary>

Do not use global `pip`; first run `uv add`.

</details>

<details><summary>Solution</summary>

```bash
uv add rich
uv run python -c "import rich; print(rich.__version__)"
```

</details>

## 3. 🟡 Detect the environment

**Task:** run a script that prints the executable, prefix, and virtual-environment indicator.

**Expected result:** outside venv the prefixes match; inside a virtual environment they differ.

<details><summary>Hint</summary>

Compare `sys.prefix` and `sys.base_prefix`.

</details>

<details><summary>Solution</summary>

```python
import sys

print(sys.executable)
print("virtual:", sys.prefix != sys.base_prefix)
print("prefix:", sys.prefix)
print("base:", sys.base_prefix)
```

Run it through `uv run python env_info.py`.

</details>

## 4. 🟡 Diagnose module lookup

**Task:** write a script that prints the first three elements of `sys.path`.

**Expected result:** the launch directory and project or standard library directories are visible.

<details><summary>Hint</summary>

Use `sys.path[:3]`.

</details>

<details><summary>Solution</summary>

```python
import sys

for index, path in enumerate(sys.path[:3]):
    print(index, path)
```

</details>

## 5. 🔴 Design a reproducible setup

**Task:** explain which files should go into Git and how to install the project on another machine.

**Expected result:** describe `pyproject.toml`, `uv.lock`, exclusion of `.venv/`, and `uv sync --locked`.

<details><summary>Hint</summary>

Separate dependency declarations, exact locking, and local state.

</details>

<details><summary>Solution</summary>

Commit `pyproject.toml` and `uv.lock` to Git. Ignore `.venv/`. On a new machine run `uv sync --locked`, then use `uv run`.

```bash
uv sync --locked
uv run python --version
```

</details>
