---
title: "Security and compliance: fundamentals"
lang: en
track: claude
kind: module
doc: basics
slug: 16-security
part: 3
tags: [claude, security, prompt-injection, secrets, compliance]
versions: "Claude 5.5 family; Claude Code 2.1.332; anthropic 2.14.0"
status: draft
updated: 2026-11-25
---

# Security and compliance: fundamentals

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [07 · Tool use](../07-tool-use/overview.en.md), [09 · Claude Code](../09-claude-code/overview.en.md)

## How this was checked

The protective functions ran as ordinary Python, `inference_geo` as a request shape against a local stand-in. Claude Code permission rules were checked in [module 09](../09-claude-code/advanced.en.md). Data retention, compliance and the sandbox are described from the documentation.

## The main rule

**Model output is untrusted input.** Check tool arguments as you would data from a stranger on the internet, and grant the least privilege.

## Keys

- Environment variables or a secrets manager only, never in code or git.
- A separate key per service and environment, with an expiry and a workspace scope.
- Never ship a key to a browser or mobile app: call the API from your server.
- If you suspect a leak, disable or delete the key.

## Check what the model asks for

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

`safe_path("../.ssh/id_rsa")` and absolute paths are refused. The tool list is fixed in code and irreversible actions need a human confirmation.

## Common beginner mistakes

- Running a command from the model's reply through a shell.
- Writing "never reveal the password" in the prompt and calling it protection.
- Giving an agent production credentials.

## Related materials

- [Advanced theory](advanced.en.md)
- [11 · Hooks](../11-skills-and-hooks/basics.en.md)

## Sources

- [Authentication](https://platform.claude.com/docs/en/manage-claude/authentication)
- [Claude Code security](https://code.claude.com/docs/en/security)
