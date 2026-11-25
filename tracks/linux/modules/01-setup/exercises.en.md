---
title: "Environment setup: exercises"
lang: en
track: linux
kind: module
doc: exercises
slug: 01-setup
part: 1
tags: [linux, wsl, distribution, terminal, shell, setup]
versions: "Ubuntu 26.04 LTS, Bash 5.3, WSL 2"
status: draft
updated: 2026-11-25
---

# Environment setup: exercises

🌐 [Русский](exercises.ru.md) · **English**

> Try on your own first. Do risky steps only in a disposable environment.

## 1. 🟢 System passport

In one block print the distribution name, kernel version, architecture and your numeric user id.

<details><summary>Hint</summary>

`/etc/os-release` can be sourced as a shell file.

</details>

<details><summary>Solution</summary>

```bash
. /etc/os-release
echo "$PRETTY_NAME"
uname -r
uname -m
id -u
```

```text
Ubuntu 26.04.1 LTS
6.6.87.2-microsoft-standard-WSL2
x86_64
1000
```

`/etc/os-release` consists of `KEY=value` assignments, so `.` turns them into variables.

</details>

## 2. 🟡 Find and fix CRLF

Create two scripts, one with CRLF. Find the bad files with one command and fix only those.

<details><summary>Hint</summary>

`grep -l` prints file names; bash writes the CR character as `$'\r'`.

</details>

<details><summary>Solution</summary>

```bash
printf 'echo a\n' > good.sh
printf 'echo b\r\n' > bad.sh
grep -l $'\r' *.sh
for f in $(grep -l $'\r' *.sh); do sed -i 's/\r$//' "$f"; done
grep -l $'\r' *.sh; echo "left: $?"
```

```text
bad.sh
left: 1
```

The second `grep` finds nothing and exits with 1. Looping over command output is only safe for names without spaces.

</details>

## 3. 🟡 A consequence-free experiment

Start a disposable container, delete `/etc/os-release` inside, exit and check that a new container is intact.

<details><summary>Hint</summary>

The `--rm` flag.

</details>

<details><summary>Solution</summary>

```bash
docker run --rm ubuntu:26.04 sh -c 'rm /etc/os-release; ls /etc/os-release'
docker run --rm ubuntu:26.04 head -n 1 /etc/os-release
```

The first command reports the file is gone; the second starts a container from the same image where it is present. The changes lived only in the first container's layer.

</details>

## 4. 🔴 Clone a distribution

Clone your distribution as `Lab`, create a file in it, check the original does not have it, then remove the clone. Check the name twice before the last command.

<details><summary>Hint</summary>

`--export`, `--import`, `--unregister`.

</details>

<details><summary>Solution</summary>

```powershell
wsl --export Ubuntu-26.04 $env:TEMP\ubuntu.tar
wsl --import Lab $env:LOCALAPPDATA\wsl\Lab $env:TEMP\ubuntu.tar
wsl -d Lab -- touch /tmp/only-in-lab
wsl -d Ubuntu-26.04 -- ls /tmp/only-in-lab
wsl --unregister Lab      # DESTRUCTIVE: only the clone
```

The clone is an independent distribution. `--unregister` cannot be undone, so verify the name with `wsl -l -v`.

</details>
