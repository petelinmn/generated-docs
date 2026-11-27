---
title: "Linux filesystem"
lang: en
track: linux
kind: module
doc: overview
slug: 02-filesystem
part: 1
tags: [linux, filesystem, fhs, inode, hard-link, symlink, path]
versions: "Ubuntu 26.04 LTS, Bash 5.3, WSL 2"
levels: [1, 2, 3, 4]
status: draft
updated: 2026-11-26
---

# Linux filesystem

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

Linux has a single directory tree exposing configuration, programs, logs, devices and kernel information. This module explains where things live, how paths work, what an inode is and how the two kinds of links differ.

## Key ideas

- The tree starts at `/`; disks are mounted onto directories.
- The name lives in the directory, the metadata in the inode.
- A hard link is another name; a symlink is a file holding a path.
- Data is gone when there are no names and no open descriptors.
- `df` and `du` answer different questions.
- `/proc` and `/sys` are virtual: content is generated on read.

## Minimal example

```bash
echo hello > a.txt
ln a.txt hard.txt
ln -s a.txt soft.txt
ls -li
```

```text
total 8
336077 -rw-r--r-- 2 ubuntu ubuntu 6 Nov 25 18:24 a.txt
336077 -rw-r--r-- 2 ubuntu ubuntu 6 Nov 25 18:24 hard.txt
336084 lrwxrwxrwx 1 ubuntu ubuntu 5 Nov 25 18:24 soft.txt -> a.txt
```

## When to use it and when not

- ✅ Symlinks to switch versions and give convenient paths.
- ✅ A rename on one filesystem for atomic file replacement.
- ❌ Don't keep anything important in `/tmp`.
- ❌ Don't expect hard links across filesystems.

## Material levels

| Level | What's inside |
|---|---|
| [1. Fundamentals](basics.en.md) | FHS, paths, operations, file types, inodes and links |
| [2. Advanced theory](advanced.en.md) | relative links, version switching, `du`/`df`, `find` |
| [3. Expert theory](expert.en.md) | path resolution, timestamps, deleted open files, gotchas, questions |
| 4. Self-check | [quiz](quiz.en.md), [exercises](exercises.en.md) |
| 5. Runnable examples | not generated yet — `/learn linux 02-filesystem level 5` |
| 6. Tests | not generated yet — `/learn linux 02-filesystem level 6` |

## Related materials

- Prerequisites: [01 · Environment setup](../01-setup/overview.en.md)
- Next: [03 · Shell basics](../03-shell-basics/overview.en.md)
- Files and streams from a program: [Node.js: files, streams, processes](../../../javascript/modules/16-nodejs-files-streams-processes/overview.en.md)
- Storage in a cluster: [Kubernetes 08 · Storage](../../../kubernetes/modules/08-storage/overview.en.md)
- [04 · Files and permissions](../04-files-and-permissions/overview.en.md)

## Sources

- [hier(7)](https://man7.org/linux/man-pages/man7/hier.7.html)
- [inode(7)](https://man7.org/linux/man-pages/man7/inode.7.html)
- [symlink(7)](https://man7.org/linux/man-pages/man7/symlink.7.html)
- [path_resolution(7)](https://man7.org/linux/man-pages/man7/path_resolution.7.html)
- [proc_sys_fs(5)](https://man7.org/linux/man-pages/man5/proc_sys_fs.5.html)
- [ln(1)](https://man7.org/linux/man-pages/man1/ln.1.html)
