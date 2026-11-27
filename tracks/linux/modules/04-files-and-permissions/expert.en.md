---
title: "Files and permissions: expert"
lang: en
track: linux
kind: module
doc: expert
slug: 04-files-and-permissions
part: 1
tags: [linux, file-permissions, chmod, umask, acl, setuid]
versions: "Ubuntu 26.04 LTS, Bash 5.3, uutils coreutils 0.10"
status: draft
updated: 2026-11-27
---

# Files and permissions: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

## Under the hood

The mode, UID and GID are stored in the inode (see [filesystem](../02-filesystem/expert.en.md)). On every access the kernel checks search (`x`) permission on each directory of the path and then the requested right on the target. Root bypasses most checks through capabilities.

## Reference

| Octal | Symbol | File | Directory |
|---|---|---|---|
| `4000` | setuid | run as the owner | no effect on Linux |
| `2000` | setgid | run with the group | new entries inherit the group |
| `1000` | sticky | no effect | restricted deletion |
| `400/040/004` | r | read | list |
| `200/020/002` | w | write | change entries |
| `100/010/001` | x | execute | search |

In `ls`, `s` and `t` mean the special bit with `x`; capital `S` and `T` mean it without `x`.

| Command | Purpose |
|---|---|
| `chmod [-R] mode path` | change the mode |
| `chown [-R] [-h] user[:group] path` | change owner and group; `-h` changes a link itself |
| `chgrp group path` | change the group |
| `umask [-S] [mask]` | show or set the mask of the shell |
| `stat -c '%A %a %U:%G' path` | inspect |
| `namei -l path` | permissions of the whole path |
| `getfacl`, `setfacl -m/-x/-b/-d` | ACLs |
| `lsattr`, `chattr +i/+a` | filesystem attributes: immutable, append-only |
| `getcap`, `setcap` | file capabilities |
| `find -perm 644`, `-perm -002`, `-perm /6000` | exactly; all of these bits; any of these bits |

## Gotchas

### chown clears setuid and setgid

```bash
touch su1 && chmod 6755 su1
chown ann su1
stat -c '%A %a' su1
```

```text
-rwxr-xr-x 755
```

### A numeric chmod keeps setgid on a directory

```bash
mkdir keep && chmod 2775 keep
chmod 755 keep;   stat -c %a keep
chmod 00755 keep; stat -c %a keep
```

```text
2755
755
```

### chmod without who respects umask

```bash
touch w && chmod 000 w
( umask 022; chmod +w w )
stat -c %A w
```

```text
--w-------
```

### chmod follows a symbolic link

```bash
echo t > target && chmod 600 target && ln -s target link
chmod 644 link
stat -c '%a %n' target
stat -c '%A %n' link
```

```text
644 target
lrwxrwxrwx link
```

The link itself always shows `rwxrwxrwx`.

### Root and execution

```bash
echo 'echo hi' > noexec && chmod 000 noexec
cat noexec          # root reads it anyway
./noexec            # but cannot execute a file with no x bit at all
bash noexec         # bash only reads it
```

```text
echo hi
bash: ./noexec: Permission denied
hi
```

### A closed directory higher up

```bash
mkdir -p path/one && echo z > path/one/z && chmod 700 path
su ann -c 'namei -l /lab/path/one/z'
```

```text
f: /lab/path/one/z
drwxr-xr-x root root /
drwxr-xr-x root root lab
drwx------ root root path
                      one - Permission denied
```

### Immutable files

```bash
touch im && chattr +i im      # needs root (CAP_LINUX_IMMUTABLE)
rm -f im
lsattr im
```

```text
rm: cannot remove 'im': Operation not permitted
----i---------e------- im
```

A default Docker container does not grant this capability, so `chattr +i` fails there.

### Copies

```bash
touch p && chmod 751 p
( umask 022; cp p p2; cp -p p p3 )
install -m 600 -o ann p p4
stat -c '%a %U %n' p2 p3 p4
```

```text
751 root p2
751 root p3
600 ann p4
```

`cp` keeps the mode bits limited by the umask; `cp -p` also keeps ownership and times when allowed; `install` sets mode and owner explicitly.

## Trade-offs and alternatives

| Mechanism | Pro | Con |
|---|---|---|
| Mode bits | simple, everywhere | one owner and one group |
| ACL | fine-grained | easy to overlook, mask |
| `sudo` rules | controlled and logged | configuration |
| Capabilities | less than full root | harder to audit |
| MAC (AppArmor, SELinux) | limits even root processes | policies |

## Interview questions

**1. What does `755` mean?**

<details><summary>Answer</summary>

Owner `rwx`, group and others `r-x`.

</details>

**2. What do `r`, `w`, `x` mean on a directory?**

<details><summary>Answer</summary>

List names; create and remove entries; pass through and access by name.

</details>

**3. Why can a user delete a read-only file of another user?**

<details><summary>Answer</summary>

Deletion changes the directory; write and execute on it are enough unless it is sticky.

</details>

**4. How is the mode of a new file computed?**

<details><summary>Answer</summary>

The requested mode with the `umask` bits cleared (a default ACL replaces the umask).

</details>

**5. What does setuid do?**

<details><summary>Answer</summary>

Runs a program with the owner's effective UID.

</details>

**6. What is the sticky bit for?**

<details><summary>Answer</summary>

So that users of a shared directory cannot remove each other's files, as in `/tmp`.

</details>

**7. What is the ACL mask?**

<details><summary>Answer</summary>

The maximum effective rights of named users, named groups and the owning group.

</details>

**8. Trick question: the owner has no rights but the group does. Can the owner read it?**

<details><summary>Answer</summary>

No: only the owner class applies. The owner can change the mode.

</details>

**9. Trick question: can root execute a file with no `x` bits?**

<details><summary>Answer</summary>

No.

</details>

**10. How do you find why access is denied?**

<details><summary>Answer</summary>

Check every directory of the path (`namei -l`), the owner, group, ACL, the user's current groups (`id`), file attributes and mount options.

</details>

**11. What are capabilities?**

<details><summary>Answer</summary>

Separate parts of root's privileges that can be granted individually.

</details>

## Related materials

- [Advanced theory](advanced.en.md)
- [Quiz](quiz.en.md) and [exercises](exercises.en.md)

## Sources

- [chmod(1)](https://man7.org/linux/man-pages/man1/chmod.1.html)
- [chown(1)](https://man7.org/linux/man-pages/man1/chown.1.html)
- [umask(2)](https://man7.org/linux/man-pages/man2/umask.2.html)
- [path_resolution(7)](https://man7.org/linux/man-pages/man7/path_resolution.7.html)
- [inode(7)](https://man7.org/linux/man-pages/man7/inode.7.html)
- [execve(2)](https://man7.org/linux/man-pages/man2/execve.2.html)
- [chown(2)](https://man7.org/linux/man-pages/man2/chown.2.html)
- [unlink(2)](https://man7.org/linux/man-pages/man2/unlink.2.html)
- [setfacl(1)](https://man7.org/linux/man-pages/man1/setfacl.1.html)
- [acl(5)](https://man7.org/linux/man-pages/man5/acl.5.html)
- [chattr(1)](https://man7.org/linux/man-pages/man1/chattr.1.html)
- [capabilities(7)](https://man7.org/linux/man-pages/man7/capabilities.7.html)
