---
title: "Shell basics: fundamentals"
lang: en
track: linux
kind: module
doc: basics
slug: 03-shell-basics
part: 1
tags: [linux, bash, shell, redirection, pipeline, quoting, exit-status]
versions: "Ubuntu 26.04 LTS, Bash 5.3, WSL 2"
status: draft
updated: 2026-11-25
---

# Shell basics: fundamentals

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [filesystem](../02-filesystem/basics.en.md) · **Time:** ~2 h

## Why it matters

Servers are managed from the shell. It starts programs, connects them to each other and repeats actions without a mouse.

## Anatomy of a command

`command -options arguments`: first the shell processes the line (quotes, variables, patterns), then it starts the command with the finished argument list. Short options often combine: `-l -a` = `-la`. `--` means "only arguments from here".

A command is either built into the shell or a program on disk:

```bash
type cd ls
type -a echo
help cd | head -n 3
```

```text
cd is a shell builtin
ls is /usr/bin/ls
echo is a shell builtin
echo is /usr/bin/echo
echo is /bin/echo
cd: cd [-L|[-P [-e]]] [-@] [dir]
    Change the shell working directory.
    
```

## Getting help

| You need | Command |
|---|---|
| A short option list | `command --help` |
| The full description | `man command` (search `/text`, quit `q`) |
| A bash builtin | `help command` |
| You don't know the name | `apropos word` (or `man -k`) |

man sections: 1 commands, 5 file formats, 8 administration. `man 5 passwd` and `man 1 passwd` are different pages.

```bash
man -f passwd
apropos -s 5 'password file'
ls --help | head -n 1
```

```text
passwd (1)           - change user password
passwd (5)           - the password file
passwd (5)           - the password file
shadow (5)           - shadowed password file
List directory contents.
```

## Keys and history

| Keys | Effect |
|---|---|
| `Tab` | complete a name |
| `↑`, `↓` | previous commands |
| `Ctrl+R` | search the history |
| `Ctrl+C` | interrupt the command |
| `Ctrl+D` | end of input; on an empty line, exit |
| `Ctrl+A`, `Ctrl+E` | start and end of line |
| `Ctrl+L` | clear the screen |
| `!!`, `sudo !!` | repeat the last command, repeat it with sudo |

`history` prints the list. It is saved in `~/.bash_history`.

## Three streams and redirections

A program has standard input (0, stdin), output (1, stdout) and error (2, stderr).

| Syntax | Effect |
|---|---|
| `> f` | stdout to a file, overwriting it |
| `>> f` | append |
| `2> f` | stderr to a file |
| `< f` | stdin from a file |
| `> f 2>&1` | both to the file |

```bash
ls /etc/hostname nope > out.txt 2> err.txt
cat out.txt
cat err.txt
echo again >> out.txt
wc -l < out.txt
```

```text
/etc/hostname
ls: cannot access 'nope': No such file or directory
2
```

## Pipelines

`a | b` feeds the stdout of `a` into the stdin of `b`. stderr does not enter the pipe. The commands run concurrently.

```bash
printf 'pear\napple\npear\nfig\n' > fruit
sort fruit | uniq -c | sort -rn | head -n 2
```

```text
      2 pear
      1 fig
```

## Exit status

Every command returns a number from 0 to 255: `0` means success. It is stored in `$?`. `&&` runs the next command after success, `||` after failure.

```bash
ls /etc/hostname > /dev/null; echo $?
ls nope 2> /dev/null; echo $?
no-such-command 2> /dev/null; echo $?
mkdir demo && cd demo && echo inside
cd /nope 2> /dev/null || echo 'cannot enter'
```

```text
0
2
127
inside
cannot enter
```

## Quoting

- Unquoted: variables and patterns expand, and the result is split into words.
- `"..."`: variables and `$(...)` expand; no word splitting, no patterns.
- `'...'`: everything is literal.

```bash
name=world
echo "hi $name"
echo 'hi $name'
touch 1.log 2.log
echo *.log
echo "*.log"
echo "today: $(basename /usr/bin)"
```

```text
hi world
hi $name
1.log 2.log
*.log
today: bin
```

## Beginner mistakes

- ❌ `sort f > f` → ✅ `sort -o f f`: the shell truncates the file before `sort` starts.
- ❌ `rm $file` → ✅ `rm -- "$file"`.
- ❌ Running your script as `script.sh` → ✅ `./script.sh`: the current directory is not in `PATH`.

## Related materials

- Prerequisites: [02 · Filesystem](../02-filesystem/overview.en.md)
- Practice environment: [01 · Environment setup](../01-setup/overview.en.md)
- Streams and pipelines from a program: [Node.js: files, streams, processes](../../../javascript/modules/16-nodejs-files-streams-processes/overview.en.md)

## Sources

- [bash(1)](https://man7.org/linux/man-pages/man1/bash.1.html)
- [man(1)](https://man7.org/linux/man-pages/man1/man.1.html)
