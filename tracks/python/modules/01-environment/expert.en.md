---
title: "Environment and tools: expert"
lang: en
track: python
kind: module
doc: expert
slug: 01-environment
part: 1
tags: [python, interpreter, sys-path, virtual-environment, pyproject, uv]
versions: "Python 3.12+"
status: draft
updated: 2026-11-25
---

# Environment and tools: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

This builds on [advanced theory](advanced.en.md).

## Execution reference

| Command | Purpose |
|---|---|
| `python script.py` | execute a file |
| `python -c "code"` | execute supplied code |
| `python -m package.module` | find and execute a module |
| `python -i script.py` | execute and retain the interactive shell |
| `python -I script.py` | isolated CPython mode |
| `python -m venv .venv` | create an environment |

Arguments after `--` are passed to the program and are not interpreted as Python flags.

## How execution works

1. An executable interpreter is selected.
2. CPython parses the options and entry point.
3. The script or module is compiled to bytecode.
4. Modules are imported and objects are created as execution proceeds.
5. `sys.path` and environment variables influence module search.

For `python script.py`, the first `sys.argv` element contains the script path. For `python -m package.module`, the module starts with the `__main__` flag, and the source module name remains available through `__name__`.

## Virtual environments

A virtual environment isolates package directories. It does not copy every Python component and does not guarantee dependency reproducibility by itself: a lock file or exact constraints is required.

An environment contains links or executable wrappers configured for a specific interpreter. Changing system Python should not replace an existing environment.

## Traps

### 1. `uv run` and an activated `venv`

Activating an environment changes `PATH`, but uv inside a project still chooses that project's environment. Commands can differ if the terminal previously activated another project.

### 2. Two Python installations and one pip

`pip` may belong to another interpreter. Check with:

```python
import sys
print(sys.executable)
print(sys.prefix)
print(sys.base_prefix)
```

If `sys.prefix == sys.base_prefix`, the process is outside a virtual environment.

### 3. The current directory is not always the package name

Running `python package/cli.py` changes the first `sys.path` element to `package`, so relative imports can differ from `python -m package.cli`.

### 4. Bytecode does not replace source

Cache metadata and timestamp checks do not guarantee source immutability in every environment. Production tests verify dependencies and source rather than relying only on `.pyc`.

## Interview questions

<details><summary>1. How does a script differ from a module?</summary>

A script is passed to the interpreter as a file; a module is located by the import mechanism. `python -m` adds module semantics and handles packages correctly.

</details>

<details><summary>2. Why is `uv.lock` useful?</summary>

It fixes exact selected versions and reproduces the environment even when ranges in `pyproject.toml` could change.

</details>

<details><summary>3. Why should `.venv` not be committed?</summary>

It contains platform-dependent interpreter and cached package state. A repository should store dependency descriptions rather than a copy of the environment.

</details>

<details><summary>4. How do `sys.prefix` and `sys.base_prefix` differ?</summary>

In a normal virtual environment, `sys.prefix` points to the environment and `sys.base_prefix` points to the base interpreter.

</details>

<details><summary>5. What does `PYTHONPATH` do?</summary>

It adds directories to module lookup according to interpreter and launch parameters. It is a development convenience or debugging setup, not a dependency management system.

</details>

<details><summary>6. How can you verify that a package belongs to the active environment?</summary>

Compare `sys.executable`, `sys.prefix`, and the package location from `importlib.util.find_spec()`.

</details>

<details><summary>7. How does isolated mode `-I` differ from normal execution?</summary>

It ignores `PYTHON*`, and it does not add the current directory or user packages to search paths, making the script less sensitive to local state.

</details>

<details><summary>8. Why is a virtual environment not portable?</summary>

Its configuration is commonly tied to absolute paths and the operating system. Port manifests and lock files, then recreate the environment.

</details>

<details><summary>9. What happens if a script starts before dependencies are installed?</summary>

Importing the required package raises `ModuleNotFoundError`, even if that package is installed in another venv.

</details>

<details><summary>10. How does `.pyc` bytecode differ from `.py` source?</summary>

`.pyc` contains CPython bytecode compiled from source and is a derived artifact. Source remains the primary material and can be read by other tools.

</details>

## Tradeoffs and alternatives

- uv is convenient for a project; the system package manager is useful for system libraries.
- `venv` is part of the standard library; uv is faster and combines several operations.
- Conda helps with complex native dependencies but does not replace package metadata in pyproject.

## Related materials

- Basics: [01 · Environment and tools](basics.en.md).
- Advanced: [01 · Environment and tools](advanced.en.md).
- Next module: the [Python track roadmap](../../overview.en.md).

## Sources

- [Python command line](https://docs.python.org/3/using/cmdline.html)
- [Python venv](https://docs.python.org/3/library/venv.html)
- [Python sys module](https://docs.python.org/3/library/sys.html)
- [uv concepts](https://docs.astral.sh/uv/concepts/projects/)
