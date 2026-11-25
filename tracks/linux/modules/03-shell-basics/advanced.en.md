---
title: "Shell basics: advanced"
lang: en
track: linux
kind: module
doc: advanced
slug: 03-shell-basics
part: 1
tags: [linux, bash, shell, redirection, pipeline, quoting, exit-status]
versions: "Ubuntu 26.04 LTS, Bash 5.3, WSL 2"
status: draft
updated: 2026-11-25
---

# Shell basics: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## Order of redirections

Redirections are applied left to right. `2>&1` means "point 2 where 1 points now", not "tie them together".

```bash
ls /etc/hostname nope > all 2>&1
cat all
echo ---
ls /etc/hostname nope 2>&1 > only-out
echo ---
cat only-out
```

```text
ls: cannot access 'nope': No such file or directory
/etc/hostname
---
ls: cannot access 'nope': No such file or directory
---
/etc/hostname
```

In the second case stderr got stdout's old target (the screen) before stdout moved to the file. Bash has the shorthand `&> file`, and `|&` for pipes. `sh` has neither.

## Exit status of a pipeline

By default it is the status of the last command; a failure in the middle is lost.

```bash
false | true; echo "$?  ${PIPESTATUS[*]}"
false | true; echo "${PIPESTATUS[@]}"
set -o pipefail
false | true; echo $?
```

```text
0  1 0
1 0
1
```

## Redirection and sudo

```bash
echo x > /etc/demo
echo x | sudo tee /etc/demo
sudo rm /etc/demo
```

```text
bash: line 4: /etc/demo: Permission denied
x
```

Your shell opens the file, not `sudo`. `tee` runs as root and writes itself; use `tee -a` to append.

## Here-documents, here-strings, tee

Quoting the delimiter turns expansion off.

```bash
cat <<EOF
home is $HOME
EOF
cat <<'EOF'
home is $HOME
EOF
wc -w <<< 'here string'
echo logged | tee a.log b.log
```

```text
home is /home/ubuntu
home is $HOME
2
logged
```

## Variables and the environment

A plain variable is visible to the shell only. `export` adds it to the environment of child processes. A child cannot change the parent's variables.

```bash
A=1
bash -c 'echo "A=[$A]"'
export A
bash -c 'echo "A=[$A]"'
B=2 bash -c 'echo "B=[$B]"'
echo "B=[$B]"
```

```text
A=[]
A=[1]
B=[2]
B=[]
```

## PATH and how programs are found

`PATH` lists the directories the shell searches.

```bash
echo "$PATH"
printf '#!/bin/sh\necho mine\n' > hello
chmod +x hello
hello; echo "exit=$?"
./hello
```

```text
/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin
bash: line 7: hello: command not found
exit=127
mine
```

Codes: `127` means command not found, `126` found but not executable (for instance, no execute bit).

## Process substitution

`<(command)` yields a file name from which the command's output can be read.

```bash
printf 'b\na\n' > one
printf 'a\nc\n' > two
diff <(sort one) <(sort two)
```

```text
2c2
< b
---
> c
```

## Configuration: .bashrc and .profile

- A login shell reads `/etc/profile`, then the first of `~/.bash_profile`, `~/.bash_login`, `~/.profile`.
- An interactive non-login shell reads `~/.bashrc`.
- Ubuntu's default `~/.profile` sources `~/.bashrc`.
- Keep aliases and functions in `~/.bashrc`; apply changes with `source ~/.bashrc`.

Ubuntu's default `.bashrc` sets `HISTSIZE=1000`, `HISTFILESIZE=2000`, `HISTCONTROL=ignoreboth` (skip consecutive duplicates and lines starting with a space) and the alias `ll='ls -alF'`.

## Best practices

- Double-quote variables: `"$var"`.
- Preview a pattern with `echo` before `rm`.
- Don't type secrets into a command: it stays in history and shows up in the process list.

## Common mistakes

- ❌ Using `a && b || c` as if/else → ✅ `c` also runs when `a` succeeds but `b` fails; use `if`.
- ❌ `sudo echo ... > /etc/...` → ✅ `| sudo tee`.
- ❌ Relying on an alias in a script → ✅ write the command in full: non-interactive shells don't expand aliases.

## Related materials

- Prerequisites: [02 · Filesystem](../02-filesystem/overview.en.md)
- Practice environment: [01 · Environment setup](../01-setup/overview.en.md)
- Streams and pipelines from a program: [Node.js: files, streams, processes](../../../javascript/modules/16-nodejs-files-streams-processes/overview.en.md)

## Sources

- [bash(1)](https://man7.org/linux/man-pages/man1/bash.1.html)
- [man(1)](https://man7.org/linux/man-pages/man1/man.1.html)
