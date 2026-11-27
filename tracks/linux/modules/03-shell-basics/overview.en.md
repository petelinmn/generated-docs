---
title: "Shell basics"
lang: en
track: linux
kind: module
doc: overview
slug: 03-shell-basics
part: 1
tags: [linux, bash, shell, redirection, pipeline, quoting, exit-status]
versions: "Ubuntu 26.04 LTS, Bash 5.3, WSL 2"
levels: [1, 2, 3, 4]
status: draft
updated: 2026-11-26
---

# Shell basics

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

The shell parses the line you type and starts programs. This module teaches how to find help, use history, control streams, connect commands with pipes and get quoting right.

## Key ideas

- The shell expands the line first, then runs the command.
- `--help`, `man`, `help`, `type`, `apropos`: help is always at hand.
- Three streams: stdin, stdout, stderr.
- Redirections apply left to right.
- Status 0 is success; a pipeline reports its last command.
- Double-quote your variables.

## Minimal example

```bash
printf 'pear\napple\npear\nfig\n' > fruit
sort fruit | uniq -c | sort -rn | head -n 2
```

```text
      2 pear
      1 fig
```

## When to use it and when not

- ✅ Shell for connecting utilities and automating short tasks.
- ❌ For complex logic and data structures use a general-purpose language.

## Material levels

| Level | What's inside |
|---|---|
| [1. Fundamentals](basics.en.md) | commands, help, history, streams, pipes, statuses, quoting |
| [2. Advanced theory](advanced.en.md) | redirection order, `pipefail`, environment, `PATH`, startup files |
| [3. Expert theory](expert.en.md) | expansion order, reference, gotchas, interview questions |
| 4. Self-check | [quiz](quiz.en.md), [exercises](exercises.en.md) |
| 5. Runnable examples | not generated yet — `/learn linux 03-shell-basics level 5` |
| 6. Tests | not generated yet — `/learn linux 03-shell-basics level 6` |

## Related materials

- Prerequisites: [02 · Filesystem](../02-filesystem/overview.en.md)
- Practice environment: [01 · Environment setup](../01-setup/overview.en.md)
- Streams and pipelines from a program: [Node.js: files, streams, processes](../../../javascript/modules/16-nodejs-files-streams-processes/overview.en.md)
- [04 · Files and permissions](../04-files-and-permissions/overview.en.md)

## Sources

- [bash(1)](https://man7.org/linux/man-pages/man1/bash.1.html)
- [man(1)](https://man7.org/linux/man-pages/man1/man.1.html)
