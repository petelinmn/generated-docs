---
title: "Files and permissions: basics"
lang: en
track: linux
kind: module
doc: basics
slug: 04-files-and-permissions
part: 1
tags: [linux, file-permissions, chmod, umask, acl, setuid]
versions: "Ubuntu 26.04 LTS, Bash 5.3, uutils coreutils 0.10"
status: draft
updated: 2026-11-27
---

# Files and permissions: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [filesystem](../02-filesystem/basics.en.md), [shell](../03-shell-basics/basics.en.md) · **Time:** ~2 h

Output was produced in an `ubuntu:26.04` container. This release ships `uutils coreutils` (`ls`, `chmod`, `stat` and others), so some error texts differ from GNU coreutils.

> ⚠️ The examples create users and change owners. Run them only in a disposable environment, as root: `docker run --rm -it ubuntu:26.04 bash`, then `apt-get update && apt-get install -y acl`.

Setup used by the examples:

```bash
useradd -m ann && useradd -m bob && groupadd devs
usermod -aG devs ann && usermod -aG devs bob
mkdir /lab && chmod 755 /lab && cd /lab
```

## Reading permissions

```bash
umask 022
touch f && mkdir d
stat -c '%A %a %U:%G %n' f d
```

```text
-rw-r--r-- 644 root:root f
drwxr-xr-x 755 root:root d
```

The first character is the type (`-` file, `d` directory, `l` link). Then three groups of `rwx`: owner, group, others. In numbers r=4, w=2, x=1, so `rw-r--r--` is `644`.

## Changing the mode

```bash
touch m
chmod 640 m;            stat -c '%A %a' m
chmod u+x,g-r,o=r m;    stat -c '%A %a' m
chmod a=rw m;           stat -c '%A %a' m
```

```text
-rw-r----- 640
-rwx---r-- 704
-rw-rw-rw- 666
```

Numeric form sets all bits at once. Symbolic form is `who` (`u g o a`), an operation (`+ - =`) and rights.

## Changing the owner and group

```bash
touch ch
chown ann ch;   stat -c %U:%G ch
chown :devs ch; stat -c %U:%G ch
```

```text
ann:root
ann:devs
```

Giving a file to another user needs root. An ordinary owner can change the group only to one they belong to.

## Directories

| Bit | File | Directory |
|---|---|---|
| `r` | read the content | list the names |
| `w` | change the content | create, rename, delete entries (together with `x`) |
| `x` | run | enter and access entries by name |

```bash
mkdir dd && echo data > dd/a && chmod 644 dd/a

chmod 744 dd                      # others: r, no x
su ann -c 'ls /lab/dd; cat /lab/dd/a'

chmod 711 dd                      # others: x, no r
su ann -c 'ls /lab/dd; cat /lab/dd/a'
```

```text
a
cat: /lab/dd/a: Permission denied
ls: cannot open directory '/lab/dd': Permission denied
data
```

## Beginner mistakes

- ❌ `chmod 777` → ✅ find out which user needs which right.
- ❌ `chmod -R 644 dir` removes `x` from directories → ✅ separate files and directories, or use `X`.
- ❌ Protecting a file from deletion with its own mode → ✅ the directory decides.

## Related materials

- [Advanced theory](advanced.en.md)

## Sources

- [chmod(1)](https://man7.org/linux/man-pages/man1/chmod.1.html)
- [chown(1)](https://man7.org/linux/man-pages/man1/chown.1.html)
- [path_resolution(7)](https://man7.org/linux/man-pages/man7/path_resolution.7.html)
- [inode(7)](https://man7.org/linux/man-pages/man7/inode.7.html)
