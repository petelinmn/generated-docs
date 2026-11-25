---
title: "Environment setup"
lang: en
track: linux
kind: module
doc: overview
slug: 01-setup
part: 1
tags: [linux, wsl, distribution, terminal, shell, setup]
versions: "Ubuntu 26.04 LTS, Bash 5.3, WSL 2"
levels: [1, 2, 3, 4]
status: draft
updated: 2026-11-25
---

# Environment setup

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

This module covers where to practise: WSL 2 with Ubuntu, what a kernel, distribution, terminal and shell are, and where it is safe to break things.

## Key ideas

- WSL 2 is a real Linux kernel in a lightweight VM managed by Windows.
- The kernel version and the distribution version are different things.
- Study files belong in `~`, not in `/mnt/c`.
- Dangerous things go into a disposable environment only.
- `wsl --unregister` deletes everything for good.
- Ubuntu 26.04 defaults to uutils and sudo-rs.

## Minimal example

```powershell
wsl --install -d Ubuntu-26.04    # PowerShell, as administrator
wsl -l -v
```

## When to use it and when not

- ✅ WSL for learning and for developing for Linux on Windows.
- ✅ A container for throwaway experiments.
- ❌ For boot and disk partitioning, use a virtual machine.

## Material levels

| Level | What's inside |
|---|---|
| [1. Fundamentals](basics.en.md) | installation, vocabulary, first commands, where to experiment |
| [2. Advanced theory](advanced.en.md) | managing distributions, configuration, Windows and Linux files |
| [3. Expert theory](expert.en.md) | how WSL 2 works, reference, gotchas, interview questions |
| 4. Self-check | [quiz](quiz.en.md), [exercises](exercises.en.md) |
| 5. Runnable examples | not generated yet — `/learn linux 01-setup level 5` |
| 6. Tests | not generated yet — `/learn linux 01-setup level 6` |

## Related materials

- Next: [02 · Filesystem](../02-filesystem/overview.en.md), [03 · Shell basics](../03-shell-basics/overview.en.md)
- Containers as a disposable environment: [Kubernetes 01 · Containers](../../../kubernetes/modules/01-containers/overview.en.md)

## Sources

- [Install WSL](https://learn.microsoft.com/en-us/windows/wsl/install)
- [Basic commands for WSL](https://learn.microsoft.com/en-us/windows/wsl/basic-commands)
- [Working across file systems](https://learn.microsoft.com/en-us/windows/wsl/filesystems)
- [Advanced settings configuration in WSL](https://learn.microsoft.com/en-us/windows/wsl/wsl-config)
- [Comparing WSL versions](https://learn.microsoft.com/en-us/windows/wsl/compare-versions)
- [File permissions for WSL](https://learn.microsoft.com/en-us/windows/wsl/file-permissions)
- [Case sensitivity](https://learn.microsoft.com/en-us/windows/wsl/case-sensitivity)
- [Ubuntu 26.04 LTS release notes: summary for LTS users](https://documentation.ubuntu.com/release-notes/26.04/summary-for-lts-users/)
