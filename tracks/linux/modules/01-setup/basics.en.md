---
title: "Environment setup: fundamentals"
lang: en
track: linux
kind: module
doc: basics
slug: 01-setup
part: 1
tags: [linux, wsl, distribution, terminal, shell, setup]
versions: "Ubuntu 26.04 LTS, Bash 5.3, WSL 2"
status: draft
updated: 2026-11-25
---

# Environment setup: fundamentals

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** Windows 10 (version 2004+) or Windows 11, administrator rights for the installation · **Time:** ~1 h

## Why it matters

You learn Linux by using it. You need a place to type commands, make mistakes and start over without losing anything. On Windows the shortest path is WSL 2 (Windows Subsystem for Linux): a real Linux kernel in a lightweight virtual machine managed by Windows.

## Vocabulary

| Term | What it is |
|---|---|
| Kernel | manages processes, memory, devices and filesystems; strictly speaking this is "Linux" |
| Distribution | the kernel plus utilities, a package manager and defaults: Ubuntu, Debian, Fedora |
| Terminal | the window program that shows text and passes input along |
| Shell | the program inside the terminal that parses and runs commands: bash |

## Installing WSL

The commands in this section run in PowerShell on Windows. Install from an administrator prompt, then reboot.

```powershell
wsl --list --online              # distributions available to install
wsl --install -d Ubuntu-26.04    # install a specific one
wsl --list --verbose             # installed distributions, state and WSL version
wsl -d Ubuntu-26.04              # start a shell in it
```

`wsl --install` without `-d` enables the required Windows features and installs the Ubuntu distribution; it only works if WSL is not installed yet. New distributions get WSL 2. On first start you are asked for a user name and a password: this is a Linux user, unrelated to your Windows account.

If you already have Ubuntu, run `wsl -l -v` and check its version with the commands in the next section: this track was verified on Ubuntu 26.04 LTS.

## First commands: where am I

From here on, commands run inside Linux. The output below comes from an Ubuntu 26.04 container on a WSL 2 kernel; your user name will differ.

```bash
grep -E '^(PRETTY_NAME|ID|ID_LIKE|VERSION_CODENAME)=' /etc/os-release
uname -s -r -m
whoami
echo "$SHELL"
bash --version | head -n 1
```

```text
PRETTY_NAME="Ubuntu 26.04.1 LTS"
VERSION_CODENAME=resolute
ID=ubuntu
ID_LIKE=debian
Linux 6.6.87.2-microsoft-standard-WSL2 x86_64
ubuntu
/bin/bash
GNU bash, version 5.3.9(1)-release (x86_64-pc-linux-gnu)
```

- `/etc/os-release` describes the distribution; `uname` describes the kernel. They are different versions of different things.
- A prompt like `ubuntu@lab:~$` reads as "user@host:directory". `$` means a regular user, `#` means root.
- `sudo command` runs a single command as root and asks for your Linux password.

## Updating packages

Right after installing, refresh the package index and upgrade:

```bash
sudo apt update          # refresh the package index
sudo apt upgrade         # install newer versions
sudo apt install man-db manpages tree shellcheck bats
```

Packages are covered in module 07 of the roadmap.

## Where to experiment

| What you are doing | Where |
|---|---|
| Ordinary exercises | your WSL home directory (`~`) |
| Anything that can break the system (`rm -rf`, permissions on `/etc`, firewall) | a disposable container or a spare distribution |
| You want the clean distribution again | `wsl --export` before experimenting, then `wsl --import` under another name |

A disposable container (needs Docker):

```bash
docker run --rm -it ubuntu:26.04 bash    # everything inside is deleted on exit
```

The container image is minimised: no man pages, many tools are missing, and you are root.

## Beginner mistakes

- ❌ Keeping study files in `/mnt/c/...` → ✅ use `~`: permissions behave like Linux and file operations are faster.
- ❌ Working as root all the time → ✅ a regular user plus `sudo` for single commands.
- ❌ Running `wsl --unregister` to "restart" → ✅ `wsl --terminate` restarts; `--unregister` permanently deletes the distribution and all its data.

## Related materials

- Next: [02 · Filesystem](../02-filesystem/overview.en.md), [03 · Shell basics](../03-shell-basics/overview.en.md)
- Containers as a disposable environment: [Kubernetes 01 · Containers](../../../kubernetes/modules/01-containers/overview.en.md)

## Sources

- [Install WSL](https://learn.microsoft.com/en-us/windows/wsl/install)
- [Basic commands for WSL](https://learn.microsoft.com/en-us/windows/wsl/basic-commands)
- [Working across file systems](https://learn.microsoft.com/en-us/windows/wsl/filesystems)
- [Advanced settings configuration in WSL](https://learn.microsoft.com/en-us/windows/wsl/wsl-config)
- [Comparing WSL versions](https://learn.microsoft.com/en-us/windows/wsl/compare-versions)
- [File permissions for WSL](https://learn.microsoft.com/en-us/windows/wsl/file-permissions)
- [Case sensitivity](https://learn.microsoft.com/en-us/windows/wsl/case-sensitivity)
- [Ubuntu 26.04 LTS release notes: summary for LTS users](https://documentation.ubuntu.com/release-notes/26.04/summary-for-lts-users/)
