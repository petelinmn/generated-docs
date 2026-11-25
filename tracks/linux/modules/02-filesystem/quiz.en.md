---
title: "Linux filesystem: quiz"
lang: en
track: linux
kind: module
doc: quiz
slug: 02-filesystem
part: 1
tags: [linux, filesystem, fhs, inode, hard-link, symlink, path]
versions: "Ubuntu 26.04 LTS, Bash 5.3, WSL 2"
status: draft
updated: 2026-11-25
---

# Linux filesystem: quiz

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 Where does system configuration live?

- A) `/usr`
- B) `/opt`
- C) `/etc`
- D) `/var`

<details><summary>Answer</summary>

**C.** `/etc` holds machine configuration. `/var` is changing data, `/usr` programs, `/opt` add-on packages.
→ [Fundamentals](basics.en.md)

</details>

## 2. 🟢 What does a directory store?

- A) A mapping of names to inode numbers
- B) The file contents
- C) The permissions of all files inside
- D) The absolute paths of files

<details><summary>Answer</summary>

**A.** Data and permissions belong to each file's inode and blocks; a path is assembled by walking the tree.
→ [Fundamentals](basics.en.md)

</details>

## 3. 🟢 You removed the original that had a hard link and a symlink. What is true?

- A) Both read the data
- B) Both break
- C) The symlink works, the hard link does not
- D) The hard link reads the data, the symlink dangles

<details><summary>Answer</summary>

**D.** A hard link is a name of the same inode. A symlink stores a path that no longer exists.
→ [Fundamentals](basics.en.md)

</details>

## 4. 🟡 From where is a relative symlink target resolved?

- A) From the root
- B) From the directory containing the link
- C) From the current directory
- D) From the home directory

<details><summary>Answer</summary>

**B.** Otherwise the link would change meaning with every `cd`.
→ [Advanced theory](advanced.en.md)

</details>

## 5. 🟡 Which limits do hard links have? Select all that apply.

- [ ] A) Cannot be renamed
- [ ] B) Cannot link a directory
- [ ] C) Cannot cross a filesystem boundary
- [ ] D) At most two names per inode

<details><summary>Answer</summary>

**B, C.** Inode numbers are unique only within a filesystem; there can be many names, and each can be renamed.
→ [Advanced theory](advanced.en.md)

</details>

## 6. 🟡 `current` points at directory `v1`. What does `ln -sf v2 current` do?

- A) Repoints `current` to `v2`
- B) Deletes `v1`
- C) Always fails
- D) Creates `v1/v2` and leaves `current` unchanged

<details><summary>Answer</summary>

**D.** `ln` followed the link into the directory. Use `ln -sfn`.
→ [Advanced theory](advanced.en.md)

</details>

## 7. 🟡 What does `du` show that `ls -l` does not?

- A) Disk space actually used
- B) Logical size
- C) Number of inodes in the filesystem
- D) Free space

<details><summary>Answer</summary>

**A.** For a sparse file the two differ sharply. Free space and inodes are shown by `df`.
→ [Advanced theory](advanced.en.md)

</details>

## 8. 🔴 What does `chmod` do to file times?

- A) Changes both
- B) Changes nothing
- C) Changes ctime but not mtime
- D) Changes mtime but not ctime

<details><summary>Answer</summary>

**C.** The inode changes, not the content.
→ [Expert theory](expert.en.md)

</details>

## 9. 🔴 When is the space of a deleted file freed?

- A) Immediately after `rm`
- B) Only ever after a reboot
- C) After a minute
- D) When no names remain and nobody holds it open

<details><summary>Answer</summary>

**D.** An open descriptor keeps the inode alive.
→ [Expert theory](expert.en.md)

</details>

## 10. 🟡 40% is free but files cannot be created. What is the likeliest cause?

- A) `relatime` is off
- B) Inodes are exhausted
- C) `PATH` is too long
- D) Too many hard links to `/etc`

<details><summary>Answer</summary>

**B.** Check with `df -i`.
→ [Advanced theory](advanced.en.md)

</details>

## 11. 🔴 Open question: how do you safely replace a config that a service may read at any moment?

<details><summary>Answer</summary>

Key points: a temporary file in the same directory; same permissions and owner; validate the content; `mv` over it (atomic rename); remember hard links and symlinks to the old file, and that a service holding a descriptor still reads the old version.
→ [Expert theory](expert.en.md)

</details>
