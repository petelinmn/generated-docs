---
title: "Environment and tools: advanced"
lang: en
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

# Environment and tools: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

This builds on [basics](basics.en.md).

## Interpreter selection

The file must run with the same Python installed in the project:

```python
import sys
import sysconfig

print(sys.executable)
print(sys.version)
print(sysconfig.get_paths()["purelib"])
```

Check from uv:

```bash
uv run python -c "import sys; print(sys.executable)"
```

If the editor uses another interpreter, VS Code may report an installed package as missing.

## Module search paths

When a script starts, Python builds `sys.path`, which defines where modules are searched:

```python
import sys

for path in sys.path:
    print(path)
```

`PYTHONPATH` adds search directories but should not replace dependency management in a project.

## Dependency management

Distinguish three files:

| File | Purpose |
|---|---|
| `pyproject.toml` | declared dependencies and tool settings |
| `uv.lock` | reproducible set of exact versions |
| `.venv/` | locally installed environment |

Typical cycle:

```bash
uv add httpx
uv run python -c "import httpx; print(httpx.__version__)"
uv sync --locked
```

`uv sync --locked` verifies that the lock is not changed and installs its environment state.

## Environment variables

Export variables explicitly for local processes:

```bash
set APP_ENV=development
uv run python -c "import os; print(os.getenv('APP_ENV'))"
```

On macOS/Linux Bash, the command is: `APP_ENV=development uv run python -c "..."`

Do not write secrets into `pyproject.toml`, source code, or Git. Use environment variables or a secret manager for application storage.

## Best practices

- Fix a minimum version through `requires-python`.
- Run the project through `uv run`.
- Commit `pyproject.toml` and `uv.lock`, but not `.venv/`.
- Configure the editor to use the project interpreter.
- Use Ruff through uv or uvx.
- Separate production dependencies from development tools.

## Common mistakes

- ❌ Run a script with system Python → ✅ run `uv run python`.
- ❌ Permanently add package directories with `PYTHONPATH` → ✅ package and declare dependencies correctly.
- ❌ Delete `uv.lock` for “soft” versions → ✅ preserve reproducibility.
- ❌ Store tokens in source code → ✅ pass them through environment variables.

## Related materials

- Basics: [01 · Environment and tools](basics.en.md).
- Next module: the [Python track roadmap](../../overview.en.md).
- Environment comparison: [JavaScript: environment](../../../javascript/modules/01-environment/overview.en.md).

## Sources

- [Python: virtual environment tutorial](https://docs.python.org/3/tutorial/venv.html)
- [Python: sys.path](https://docs.python.org/3/library/sys.html#sys.path)
- [uv: project dependencies](https://docs.astral.sh/uv/concepts/projects/dependencies/)
- [uv: locking and syncing](https://docs.astral.sh/uv/concepts/projects/sync/)
