---
title: "Quiz: environment and tools"
lang: en
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

# Quiz: environment and tools

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows the level: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 Which command runs a project module as an executable entry point?

- A) `python -c package.module`
- B) `python package.module`
- C) `python -m package.module`
- D) `python --run package.module`

<details><summary>Answer</summary>

**C.** The `-m` flag finds a module through the import mechanism and runs it.
→ [Basics](basics.en.md#installation-and-execution)

</details>

## 2. 🟢 Why use `uv run`?

- A) It only installs Python
- B) It runs a command in the project environment
- C) It formats all files
- D) It opens an interactive shell without changes

<details><summary>Answer</summary>

**B.** `uv run` synchronizes project environment state and runs the requested command.
→ [Basics](basics.en.md#an-uv-project)

</details>

## 3. 🟢 Which file describes project dependencies?

- A) `.venv`
- B) `uv.lock`
- C) `pyproject.toml`
- D) `__pycache__/`

<details><summary>Answer</summary>

**C.** Declared project metadata and dependencies live in `pyproject.toml`.
→ [Basics](basics.en.md#an-uv-project)

</details>

## 4. 🟢 What does Ruff do?

- A) Install packages
- B) Check and format source code
- C) Run the application
- D) Create a virtual environment

<details><summary>Answer</summary>

**B.** Ruff performs static checks and formatting; tests remain a separate validation.
→ [Basics](basics.en.md#ruff)

</details>

## 5. 🟢 Which item should normally not be committed to Git?

- A) `pyproject.toml`
- B) `uv.lock`
- C) `.venv/`
- D) Source `.py`

<details><summary>Answer</summary>

**C.** `.venv/` is a local platform-dependent artifact.
→ [Advanced](advanced.en.md#dependency-management)

</details>

## 6. 🟡 How can code check the active interpreter?

- A) `import sys; print(sys.executable)`
- B) `import os; print(os.path)`
- C) `print(__name__)`
- D) `print(locals())`

<details><summary>Answer</summary>

**A.** `sys.executable` points to the interpreter executable.
→ [Advanced](advanced.en.md#interpreter-selection)

</details>

## 7. 🟡 What is `uv.lock` for?

- A) Blocking source edits
- B) Fixing exact selected versions
- C) Storing system variables
- D) Formatting Python files

<details><summary>Answer</summary>

**B.** A lock file enables reproducible installation after dependency resolution.
→ [Advanced](advanced.en.md#dependency-management)

</details>

## 8. 🟡 What does this print outside and inside a virtual environment when prefixes match?

```python
import sys
print(sys.prefix == sys.base_prefix)
```

- A) Always `True`
- B) `True` outside, `False` inside
- C) `False` outside, `True` inside
- D) It always raises an error

<details><summary>Answer</summary>

**B.** In the base interpreter the prefixes match; inside a virtual environment they usually differ.
→ [Expert](expert.en.md#virtual-environments)

</details>

## 9. 🔴 Find the problem

`pip install rich` succeeds, but `python app.py` reports `ModuleNotFoundError: rich`.

<details><summary>Answer</summary>

`pip` probably belongs to another interpreter or virtual environment. Check `sys.executable` and use `uv add rich` or `python -m pip install rich`.
→ [Expert](expert.en.md#two-python-installations-and-one-pip)

</details>

## 10. 🔴 Open question

Name three project artifacts and explain which ones belong in Git.

<details><summary>Key points of a good answer</summary>

- Store `pyproject.toml` in Git as the manifest.
- Store `uv.lock` for reproducible dependencies.
- Do not store `.venv/`; it is local and platform-dependent.

</details>

→ [Advanced](advanced.en.md#dependency-management)
