---
title: "Security and compliance"
lang: en
track: claude
kind: module
doc: overview
slug: 16-security
part: 3
tags: [claude, security, prompt-injection, secrets, compliance]
versions: "Claude 5.5 family; Claude Code 2.1.332; anthropic 2.14.0"
levels: [1, 2, 3, 4]
status: draft
updated: 2026-11-25
---

# Security and compliance

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

How to keep a Claude application or agent from doing harm: **keys**, checking model actions, **prompt injection**, permissions and sandbox, data retention.

## Key ideas

- Model output is untrusted input.
- Least privilege matters more than prompt wording.
- Third-party text may carry instructions.
- Secrets do not live in code, logs or the browser.
- Retention depends on contract, model and feature.

## Minimal example

```python
# safe_tools.py
from pathlib import Path

ROOT = Path("workspace").resolve()


def safe_path(relative):
    """Resolve a model-supplied path and refuse anything outside ROOT."""
    path = (ROOT / relative).resolve()
    if not path.is_relative_to(ROOT):
        raise PermissionError(f"path outside the workspace: {relative}")
    return path


ALLOWED = {"read_file": False, "send_email": True}     # tool -> needs human confirmation


def authorise(tool, confirmed=False):
    if tool not in ALLOWED:
        raise PermissionError(f"unknown tool: {tool}")
    if ALLOWED[tool] and not confirmed:
        raise PermissionError(f"{tool} needs confirmation")
```

## When to use it and when not

Whenever the model can act or sees other people's data.

## Material levels

| Level | File | Add with |
|---|---|---|
| 1. Fundamentals | [basics.en.md](basics.en.md) | |
| 2. Advanced theory | [advanced.en.md](advanced.en.md) | |
| 3. Expert theory | [expert.en.md](expert.en.md) | |
| 4. Self-check | [quiz.en.md](quiz.en.md), [exercises.en.md](exercises.en.md) | |
| 5. Runnable examples | — | `/learn claude 16-security level 5` |
| 6. Tests | — | `/learn claude 16-security level 6` |

## Related materials

- [Claude track](../../overview.en.md)
- [07 · Tool use](../07-tool-use/overview.en.md)
- [09 · Claude Code](../09-claude-code/overview.en.md)
- [15 · Observability](../15-observability/overview.en.md)
- [17 · Deployment and production](../17-production/overview.en.md)
