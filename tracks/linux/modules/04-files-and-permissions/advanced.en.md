---
title: "Files and permissions: advanced"
lang: en
track: linux
kind: module
doc: advanced
slug: 04-files-and-permissions
part: 1
tags: [linux, file-permissions, chmod, umask, acl, setuid]
versions: "Ubuntu 26.04 LTS, Bash 5.3, uutils coreutils 0.10"
status: draft
updated: 2026-11-27
---

# Files and permissions: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

The examples use the setup from [basics](basics.en.md#reading-permissions).

## Which class applies

```bash
echo x > own && chown ann:devs own && chmod 060 own
su ann -c 'cat /lab/own'          # ann owns the file and is in devs
su bob -c 'cat /lab/own'          # bob is only in devs
```

```text
cat: /lab/own: Permission denied
x
```

The kernel picks one class: owner if the user owns the file, otherwise group if they are in it, otherwise others. Rights of the other classes are not added.

## Deleting is a directory operation

```bash
mkdir open && chmod 777 open
touch open/rootfile && chmod 400 open/rootfile
su ann -c 'rm -f /lab/open/rootfile && echo removed'
```

```text
removed
```

## umask

A program asks for a mode (usually `666` for files and `777` for directories), and the bits set in `umask` are removed.

```bash
( umask 027; touch f27; mkdir d27; stat -c '%A %a %n' f27 d27 )
( umask 002; touch f02; stat -c '%A %a %n' f02 )
```

```text
-rw-r----- 640 f27
drwxr-x--- 750 d27
-rw-rw-r-- 664 f02
```

In the container root had `0022`, and a user session opened with `su` had `0002`.

## X: execute only where it makes sense

```bash
# before: tree and tree/sub 700, tree/file 600, tree/sub/script 700
chmod -R go+rX tree
stat -c '%A %n' tree tree/sub tree/file tree/sub/script
```

```text
drwxr-xr-x tree
drwxr-xr-x tree/sub
-rw-r--r-- tree/file
-rwxr-xr-x tree/sub/script
```

`X` adds `x` to directories and to files that already have it for somebody.

## setgid on a directory

```bash
mkdir sg && chgrp devs sg && chmod 2775 sg
su ann -c 'touch /lab/sg/f; mkdir /lab/sg/sd'
stat -c '%A %U:%G %n' sg sg/f sg/sd
```

```text
drwxrwsr-x root:devs sg
-rw-rw-r-- ann:devs sg/f
drwxrwsr-x ann:devs sg/sd
```

New entries get the directory's group, and new subdirectories inherit the bit.

## Sticky bit

```bash
mkdir st && chmod 1777 st
su ann -c 'echo a > /lab/st/annfile'
su bob -c 'rm -f /lab/st/annfile'
stat -c '%A %a %n' st /tmp
```

```text
rm: cannot remove '/lab/st/annfile': Operation not permitted
drwxrwxrwt 1777 st
drwxrwxrwt 1777 /tmp
```

In a sticky directory a user can remove only their own entries (the directory owner and root can remove any).

## setuid

```bash
cp /usr/bin/id myid && chmod u+s myid
stat -c '%A %a' myid
su ann -c '/lab/myid'
```

```text
-rwsr-xr-x 4755
uid=1001(ann) gid=1001(ann) euid=0(root) groups=1001(ann),1003(devs)
```

The program runs with the file owner's effective UID. Linux ignores setuid on interpreted scripts.

## ACL

```bash
echo secret > af && chmod 600 af
setfacl -m u:bob:r af
su bob -c 'cat /lab/af'
stat -c '%A %a' af
getfacl -c af
```

```text
secret
-rw-r----- 640
user::rw-
user:bob:r--
group::---
mask::r--
other::---
```

With an ACL the middle group of the mode shows the **mask**, the upper limit for named users and groups. `ls -l` marks such a file with `+`.

```bash
chmod g-r af               # with an ACL this changes the mask, not the group entry
getfacl -c af
su bob -c 'cat /lab/af'
```

```text
user::rw-
user:bob:r--	#effective:---
group::---
mask::---
other::---

cat: /lab/af: Permission denied
```

Default ACLs on a directory (`setfacl -d`) are inherited by new files.

## Best practices

- Grant through groups, not to "others".
- Shared directory: group, setgid, and a default ACL if users have a strict umask.
- Audit setuid files.

## Common mistakes

- ❌ Expecting a new group to work in the current session → ✅ log in again.
- ❌ `chmod` on a file with ACL without checking the mask → ✅ look at `getfacl`.

## Related materials

- [Basics](basics.en.md)
- [Expert theory](expert.en.md)

## Sources

- [chmod(1)](https://man7.org/linux/man-pages/man1/chmod.1.html)
- [umask(2)](https://man7.org/linux/man-pages/man2/umask.2.html)
- [inode(7)](https://man7.org/linux/man-pages/man7/inode.7.html)
- [execve(2)](https://man7.org/linux/man-pages/man2/execve.2.html)
- [setfacl(1)](https://man7.org/linux/man-pages/man1/setfacl.1.html)
- [acl(5)](https://man7.org/linux/man-pages/man5/acl.5.html)
