---
title: "Linux filesystem: advanced"
lang: en
track: linux
kind: module
doc: advanced
slug: 02-filesystem
part: 1
tags: [linux, filesystem, fhs, inode, hard-link, symlink, path]
versions: "Ubuntu 26.04 LTS, Bash 5.3, WSL 2"
status: draft
updated: 2026-11-25
---

# Linux filesystem: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## Relative symbolic links

A relative path inside a link is resolved from the directory that holds the link, not from your current one. `ln -sr` computes it for you.

```bash
mkdir d1 d2
echo data > d1/f
ln -s d1/f d2/wrong
cat d2/wrong
ln -sr d1/f d2/right
readlink d2/right
cat d2/right
realpath d2/right
```

```text
cat: d2/wrong: No such file or directory
../d1/f
data
/home/ubuntu/lab/d1/f
```

## Repointing a link to a directory

The "`current` points at the live version" trick breaks without `-n`:

```bash
mkdir v1 v2
ln -s v1 current
ln -sf v2 current
readlink current
ls v1
rm v1/v2
ln -sfn v2 current
readlink current
```

```text
v1
v2
v2
```

`ln -sf v2 current` followed `current` into `v1` and created `v1/v2`. With `-n` a link to a directory is treated as a plain file.

## Limits of hard links

You cannot hard-link a directory or cross filesystems: inode numbers are unique only within one.

```bash
mkdir d; echo x > f
ln d d-link
ln f /dev/shm/f-link
```

```text
ln: d: hard link not allowed for directory
ln: failed to create hard link 'f' => '/dev/shm/f-link': Invalid cross-device link
```

| | Hard | Symbolic |
|---|---|---|
| Stores | the same inode | a path |
| Directories | no | yes |
| Another filesystem | no | yes |
| Original removed | data still reachable | link dangles |
| `ls -l` | indistinguishable | `name -> target` |

## Size: ls, du, df

- `ls -l` shows the logical size of a file.
- `du` shows the disk space actually used.
- `df` shows how full a filesystem is; `df -i` shows inodes: you can run out of space by running out of inodes.

```bash
echo hi > small
truncate -s 1G sparse
ls -lh sparse | awk '{print $5}'
du -h sparse small
```

```text
1.0G
0	sparse
4.0K	small
```

A sparse file declares a gigabyte but occupies no blocks.

## Searching the tree

```bash
mkdir -p a/b; echo x > a/b/one.log; ln a/b/one.log a/twin
find . -name '*.log'
find . -type d
find . -samefile a/twin | sort
```

```text
./a/b/one.log
.
./a
./a/b
./a/b/one.log
./a/twin
```

Useful `find` tests: `-name`, `-iname`, `-type f|d|l`, `-size +10M`, `-mtime -1`, `-xtype l` (dangling links), `-xdev` (stay on one filesystem). Quote the pattern, or the shell expands it first.

## Best practices

- In scripts use absolute paths, or change directory with a check: `cd /srv/app || exit 1`.
- Use `ln -sfn` to switch versions.
- Before a mass delete, run the same `find` without `-delete`.

## Common mistakes

- ❌ `find . -name *.log` unquoted → ✅ `-name '*.log'`.
- ❌ `find . -delete -name x` → ✅ put tests before `-delete`: the expression is evaluated left to right.
- ❌ Freeing space by looking at `du` only → ✅ compare with `df`: `du` cannot see deleted files that are still open.

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
