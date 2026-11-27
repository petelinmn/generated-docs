---
title: "Files and permissions"
lang: en
track: linux
kind: module
doc: overview
slug: 04-files-and-permissions
part: 1
levels: [1, 2, 3, 4]
tags: [linux, file-permissions, chmod, umask, acl, setuid]
versions: "Ubuntu 26.04 LTS, Bash 5.3, uutils coreutils 0.10"
status: draft
updated: 2026-11-27
---

# Files and permissions

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

Every file has an owner, a group and permission bits. They decide who may read, change and run it. The module explains how to read and change them, why directories behave differently, what the special bits are and how ACLs extend the model.

## Key ideas

- Three classes: owner, group, others. Only one of them applies to a user.
- On a directory `r` lists names, `w` creates and removes them, `x` lets you pass through.
- Deleting a file needs permission on the directory, not on the file.
- `umask` removes bits from new files.
- setgid and sticky bits make shared directories work.
- ACLs grant rights to extra users and groups.

## Minimal example

```bash
touch script.sh
stat -c '%A %a %n' script.sh
chmod u+x script.sh
stat -c '%A %a %n' script.sh
```

```text
-rw-r--r-- 644 script.sh
-rwxr--r-- 744 script.sh
```

## When to use and when not

- ✅ Owner, group and mode for almost everything.
- ✅ A setgid directory for team work; an ACL for one extra user.
- ❌ `chmod 777` to "make it work".
- ❌ Your own setuid programs without a very good reason.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | reading permissions, `chmod`, `chown`, directories |
| [2. Advanced](advanced.en.md) | `umask`, special bits, ACLs |
| [3. Expert](expert.en.md) | reference, gotchas, interview questions |
| [4. Self-check](quiz.en.md) | a quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn linux 04-files-and-permissions level 5` |
| 6. Tests | not created yet — `/learn linux 04-files-and-permissions level 6` |

## Related materials

- Prerequisites: [02 · Filesystem](../02-filesystem/overview.en.md), [03 · Shell basics](../03-shell-basics/overview.en.md)
- Environment: [01 · Setup](../01-setup/overview.en.md)
- Next: the following module of the [Linux roadmap](../../overview.en.md)
