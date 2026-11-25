---
title: "Environment and tools"
lang: en
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

# Environment and tools

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

The CPython interpreter executes Python code; uv is convenient for project management. A virtual environment keeps a project's dependencies separate from the system Python, while `pyproject.toml` describes project metadata and tools. The REPL is useful for quick checks, and Ruff for linting and formatting.

## Key ideas

- **The interpreter runs scripts, modules, and an interactive console.**
- **uv installs Python and manages projects and virtual environments.**
- **A virtual environment contains an isolated set of packages.**
- **`pyproject.toml` replaces scattered settings for one project.**
- **`python -m module` runs a module through the standard import mechanism.**
- **Ruff checks and formats code without changing program meaning.**

## Minimal example

```bash
uv init hello
cd hello
uv run python -c "import sys; print(sys.version.split()[0])"
```

```text
3.12.x
```

> The exact release depends on the installed Python.

## When to use and when not

- ✅ Use uv to create projects and run commands.
- ✅ Run the project through `uv run` so dependencies come from the project.
- ✅ Create a separate environment for every project.
- ❌ Do not install project dependencies globally into system Python.
- ❌ Do not commit `.venv/` or local caches to Git.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | installation, execution, uv, REPL, `pyproject.toml`, Ruff |
| [2. Advanced](advanced.en.md) | module paths, interpreter selection, dependencies, environment variables |
| [3. Expert](expert.en.md) | execution internals, reference, and common traps |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not generated yet — `/learn python 01-environment level 5` |
| 6. Tests | not generated yet — `/learn python 01-environment level 6` |

## Related materials

- Prerequisites: none.
- Next: the next module in the [Python track roadmap](../../overview.en.md).
- JavaScript analogue: [01 · Environment and tools](../../../javascript/modules/01-environment/overview.en.md).
- C# analogue: [01 · The .NET platform](../../../csharp/modules/01-dotnet-platform/overview.en.md).

## Sources

- [Python command line and environment](https://docs.python.org/3/using/cmdline.html)
- [Python virtual environments](https://docs.python.org/3/tutorial/venv.html)
- [uv: Getting started](https://docs.astral.sh/uv/getting-started/)
