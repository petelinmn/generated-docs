---
title: "Linux filesystem: fundamentals"
lang: en
track: linux
kind: module
doc: basics
slug: 02-filesystem
part: 1
tags: [linux, filesystem, fhs, inode, hard-link, symlink, path]
versions: "Ubuntu 26.04 LTS, Bash 5.3, WSL 2"
status: draft
updated: 2026-11-25
---

# Linux filesystem: fundamentals

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [environment setup](../01-setup/basics.en.md) · **Time:** ~2 h

## Why it matters

Settings, logs, programs, devices and process information are all exposed as files in a single tree. If you know where things live and how paths work, you find the cause of a problem instead of wandering through directories.

## One tree

There are no drive letters: everything starts at the root `/`, and other disks and filesystems are mounted onto directories of that tree.

| Directory | Contents |
|---|---|
| `/etc` | configuration of this machine |
| `/home`, `/root` | home directories of users and of root |
| `/usr` | programs and libraries from packages; `/usr/local` is what you install by hand |
| `/opt` | third-party add-on packages, one directory each |
| `/var` | changing data: `/var/log` for logs, `/var/lib` for service state |
| `/tmp`, `/var/tmp` | temporary files; may be deleted without notice |
| `/run` | information about the running system since boot |
| `/dev` | device files |
| `/proc`, `/sys` | virtual filesystems: processes and the kernel |
| `/mnt`, `/media` | temporary mounts and removable media |
| `/srv` | data served by this server |
| `/boot` | boot loader and kernel files |

On Ubuntu `/bin`, `/sbin` and `/lib` are symbolic links into `/usr`. The reference is built in: `man hier`.

## Paths

- An **absolute** path starts with `/`; a **relative** one is resolved from the current directory.
- `.` is the current directory, `..` the parent, `~` your home (expanded by the shell), `cd -` the previous directory.

```bash
pwd
mkdir -p project/src project/docs
cd project/src; pwd
cd ../docs; pwd
cd -
cd ~; pwd
```

```text
/home/ubuntu/lab
/home/ubuntu/lab/project/src
/home/ubuntu/lab/project/docs
/home/ubuntu/lab/project/src
/home/ubuntu
```

## Basic operations

```bash
mkdir notes
echo 'first line' > notes/a.txt
cp notes/a.txt notes/b.txt
mv notes/b.txt notes/c.txt
ls notes
cat notes/c.txt
rm notes/c.txt
cp -r notes backup
ls -R .
```

```text
a.txt
c.txt
first line
.:
backup
notes

./backup:
a.txt

./notes:
a.txt
```

`rm` deletes with no recycle bin. `cp -r` and `rm -r` handle a directory with its contents; `mkdir -p` creates missing parents.

## File names

Case matters. An extension is just part of the name and does not define the type. A name starting with a dot is hidden from `ls`. Quote names that contain spaces.

```bash
touch visible .hidden 'two words.txt'
ls
ls -A
ls 'two words.txt'
```

```text
two words.txt
visible
.hidden
two words.txt
visible
two words.txt
```

## File types

The first character in `ls -l`: `-` regular file, `d` directory, `l` symbolic link, `c` and `b` character and block device, `p` named pipe, `s` socket. `file` determines the type from the content.

```bash
file /etc /bin /etc/hostname /dev/null
ls -l /dev/null
mkfifo pipe; ls -l pipe | cut -c1-10
```

```text
/etc:          directory
/bin:          symbolic link to usr/bin
/etc/hostname: ASCII text
/dev/null:     character special (1/3)
crw-rw-rw- 1 root root 1, 3 Nov 25 18:10 /dev/null
prw-r--r--
```

## inodes and links

A file has an **inode**: a record with type, permissions, owner, size, timestamps and data location. The name is not in the inode: a directory maps names to inode numbers.

- A **hard link** is another name for the same inode.
- A **symbolic link** is a separate file that stores a path.

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

The first column is the inode number (yours will differ). `a.txt` and `hard.txt` share it, and the `2` after the permissions is the number of names. Now remove the original name:

```bash
echo hello > a.txt
ln a.txt hard.txt
ln -s a.txt soft.txt
rm a.txt
cat hard.txt
cat soft.txt
```

```text
hello
cat: soft.txt: No such file or directory
```

The data lives while at least one name exists. The symbolic link now points to nothing.

## Beginner mistakes

- ❌ `rm -r $dir/*` with an empty variable → ✅ `echo` it first to see the expansion; use `"${dir:?}"` in scripts.
- ❌ Treating the extension as the file type → ✅ `file`.
- ❌ Keeping anything important in `/tmp` → ✅ use your home directory.

## Related materials

- Prerequisites: [01 · Environment setup](../01-setup/overview.en.md)
- Next: [03 · Shell basics](../03-shell-basics/overview.en.md)
- Files and streams from a program: [Node.js: files, streams, processes](../../../javascript/modules/16-nodejs-files-streams-processes/overview.en.md)
- Storage in a cluster: [Kubernetes 08 · Storage](../../../kubernetes/modules/08-storage/overview.en.md)

## Sources

- [hier(7)](https://man7.org/linux/man-pages/man7/hier.7.html)
- [inode(7)](https://man7.org/linux/man-pages/man7/inode.7.html)
- [symlink(7)](https://man7.org/linux/man-pages/man7/symlink.7.html)
- [path_resolution(7)](https://man7.org/linux/man-pages/man7/path_resolution.7.html)
- [proc_sys_fs(5)](https://man7.org/linux/man-pages/man5/proc_sys_fs.5.html)
- [ln(1)](https://man7.org/linux/man-pages/man1/ln.1.html)
