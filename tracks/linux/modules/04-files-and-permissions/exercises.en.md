---
title: "Exercises: files and permissions"
lang: en
track: linux
kind: module
doc: exercises
slug: 04-files-and-permissions
part: 1
tags: [linux, file-permissions, chmod, umask, acl, setuid]
versions: "Ubuntu 26.04 LTS, Bash 5.3, uutils coreutils 0.10"
status: draft
updated: 2026-11-27
---

# Exercises: files and permissions

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. Work in a disposable container with the setup from [basics](basics.en.md#reading-permissions).

## 1. 🟡 A team directory

**Task:** members of `devs` edit each other's files in `team`, even with `umask 077`; others see nothing.

<details><summary>Hint</summary>

setgid and a default ACL.

</details>

<details><summary>Solution</summary>

```bash
mkdir team && chgrp devs team && chmod 2770 team
setfacl -d -m g:devs:rw team       # new files stay group-writable even with umask 077
su ann -c 'umask 077; echo from-ann > /lab/team/a'
su bob -c 'echo from-bob >> /lab/team/a'
```

</details>

## 2. 🟢 Repair a web tree

**Task:** everything in `site` is `777`; make directories `755` and files `644`.

<details><summary>Hint</summary>

`find -type`.

</details>

<details><summary>Solution</summary>

```bash
find site -type d -exec chmod 755 {} +
find site -type f -exec chmod 644 {} +
```

</details>

## 3. 🟡 One extra reader

**Task:** `ann` owns `report` and edits it, `bob` only reads, nobody else has access.

<details><summary>Hint</summary>

An ACL.

</details>

<details><summary>Solution</summary>

```bash
chmod 600 report && chown ann report
setfacl -m u:bob:r report
```

</details>

## 4. 🔴 Drop box

**Task:** users create files in `drop` but cannot list it or remove files of others.

<details><summary>Hint</summary>

`w` and `x` without `r`, plus sticky.

</details>

<details><summary>Solution</summary>

```bash
mkdir drop && chmod 1733 drop      # drwx-wx-wt
su ann -c 'umask 077; echo a > /lab/drop/ann.txt'
```

A file can still be read by someone who knows its name if its mode allows it, hence the `umask 077`.

</details>

## 5. 🟢 Audit

**Task:** list all setuid and setgid files of the root filesystem.

<details><summary>Hint</summary>

`find -perm /6000`.

</details>

<details><summary>Solution</summary>

```bash
find / -xdev -type f -perm /6000 -exec ls -l {} + 2>/dev/null
```

</details>
