---
title: "Linux filesystem: exercises"
lang: en
track: linux
kind: module
doc: exercises
slug: 02-filesystem
part: 1
tags: [linux, filesystem, fhs, inode, hard-link, symlink, path]
versions: "Ubuntu 26.04 LTS, Bash 5.3, WSL 2"
status: draft
updated: 2026-11-25
---

# Linux filesystem: exercises

🌐 [Русский](exercises.ru.md) · **English**

> Work in an empty directory under `~`. Your inode numbers will differ.

## 1. 🟢 A project tree

Create `app/{src,tests,docs}` with one command, put an empty file in each and list all files.

<details><summary>Hint</summary>

Brace expansion and `mkdir -p`.

</details>

<details><summary>Solution</summary>

```bash
mkdir -p app/{src,tests,docs}
touch app/{src,tests,docs}/.keep
find app -type f | sort
```

```text
app/docs/.keep
app/src/.keep
app/tests/.keep
```

Bash expands the braces before running the command.

</details>

## 2. 🟡 Releases behind a link

Make `releases/1`, `releases/2` and a link `current`. Switch it from 1 to 2 and back without leaving stray files inside the releases.

<details><summary>Hint</summary>

The `-n` option.

</details>

<details><summary>Solution</summary>

```bash
mkdir -p releases/1 releases/2
echo one > releases/1/version; echo two > releases/2/version
ln -s releases/1 current;   cat current/version
ln -sfn releases/2 current; cat current/version
ln -sfn releases/1 current; cat current/version
find releases | sort
```

```text
one
two
one
releases
releases/1
releases/1/version
releases/2
releases/2/version
```

`-n` stops `ln` from descending into the directory behind the old link.

</details>

## 3. 🟡 Dangling links

Create a working and a dangling link and find only the dangling one.

<details><summary>Hint</summary>

The `-xtype` test.

</details>

<details><summary>Solution</summary>

```bash
echo x > real
ln -s real good
ln -s missing bad
find . -xtype l
```

```text
./bad
```

`-type l` would match both: it looks at the link itself.

</details>

## 4. 🔴 Rescue a deleted file

A background process holds a file open and you "accidentally" delete it. Recover the content.

<details><summary>Hint</summary>

`/proc/<pid>/fd`.

</details>

<details><summary>Solution</summary>

```bash
echo 'precious' > data
sleep 30 < data &
pid=$!
sleep 0.2
rm data
cp "/proc/$pid/fd/0" restored
cat restored
kill "$pid"
```

```text
precious
```

It works only while the process is alive and holds the descriptor. `$!` is the PID of the last background command.

</details>
