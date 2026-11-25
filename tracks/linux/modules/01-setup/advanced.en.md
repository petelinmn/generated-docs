---
title: "Environment setup: advanced"
lang: en
track: linux
kind: module
doc: advanced
slug: 01-setup
part: 1
tags: [linux, wsl, distribution, terminal, shell, setup]
versions: "Ubuntu 26.04 LTS, Bash 5.3, WSL 2"
status: draft
updated: 2026-11-25
---

# Environment setup: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## Managing distributions

PowerShell commands:

```powershell
wsl --status                                  # default distribution and version
wsl --version                                 # versions of WSL, the kernel and Windows
wsl --set-default Ubuntu-26.04                # which distribution plain `wsl` starts
wsl --terminate Ubuntu-26.04                  # stop one distribution
wsl --shutdown                                # stop all of them and the WSL 2 VM
wsl --export Ubuntu-26.04 D:\backup\ubuntu.tar
wsl --import Lab D:\wsl\Lab D:\backup\ubuntu.tar   # a second copy under a new name
wsl --unregister Lab                          # DESTRUCTIVE: deletes Lab and all its data
```

For an imported distribution the default user is set in `/etc/wsl.conf` (section `[user]`, key `default`).

## WSL 1 and WSL 2

| | WSL 1 | WSL 2 |
|---|---|---|
| Real Linux kernel in a managed VM | no | yes |
| Full system call compatibility | no | yes |
| systemd | no | yes |
| Performance across OS file systems | better | worse |

For learning you want WSL 2. To convert a distribution: `wsl --set-version <name> 2`; back it up first.

## Two configuration files

| | `/etc/wsl.conf` | `%UserProfile%\.wslconfig` |
|---|---|---|
| Applies to | one distribution | all WSL 2 distributions |
| Configures | `[boot]`, `[automount]`, `[network]`, `[interop]`, `[user]` | VM memory, processors, swap, networking (`[wsl2]`) |

```ini
# /etc/wsl.conf
[boot]
systemd=true

[automount]
options = "metadata,umask=022"

# %UserProfile%\.wslconfig
[wsl2]
memory=8GB
processors=4
```

These are two separate files shown in one block. Changes apply after a stop: `wsl --terminate <name>` or `wsl --shutdown`. If you only close the windows, the distribution keeps running for a while (the docs say about 8 seconds). A malformed `.wslconfig` is silently ignored: WSL starts without those settings.

## Windows files and Linux files

- Windows drives are mounted at `/mnt/c`, `/mnt/d`. From Windows, Linux files are visible under `\\wsl$`; `explorer.exe .` opens the current directory in Explorer.
- Keep a project that Linux tools work on in the Linux filesystem (`~/src/...`): it is faster. You can still edit it from Windows with VS Code and its WSL extension.
- Without the `metadata` mount option, Linux permissions on `/mnt/c` are not stored: `chmod` can only set or clear the Windows read-only attribute (when all write bits are removed).
- With `metadata`, owner and mode are stored in NTFS extended attributes, but you still cannot exceed your Windows permissions.

## What is special about Ubuntu 26.04

Many guides online assume GNU coreutils and the classic sudo. Ubuntu 26.04 defaults to Rust implementations: uutils (rust-coreutils) and sudo-rs. `cp`, `mv` and `rm` still come from GNU.

```bash
ls --version | head -n 1
cp --version | head -n 1
sudo --version | head -n 1
ls -l /usr/bin/ls /usr/bin/cp
```

```text
ls (uutils coreutils) 0.10.0
cp (GNU coreutils) 9.7
sudo-rs 0.2.13-0ubuntu1.2
lrwxrwxrwx 1 root root  5 Mar 30  2026 /usr/bin/cp -> gnucp
lrwxrwxrwx 1 root root 29 Mar 30  2026 /usr/bin/ls -> ../lib/cargo/bin/coreutils/ls
```

Version numbers depend on installed updates. If behaviour differs from a guide or an error message looks different, check which implementation you are running (`--version`).

## Best practices

- Run `wsl --export` before risky experiments.
- Keep your environment settings (`~/.bashrc`, package list) in a repository (the Dotfiles project on the roadmap): a new distribution is then set up in minutes.
- In scripts that must run on other distributions, don't depend on quirks of one implementation.

## Common mistakes

- ❌ Editing `wsl.conf` and expecting it to work in an open window → ✅ stop the distribution.
- ❌ Cloning into `/mnt/c` and then blaming `git status` for being slow → ✅ clone into `~`.
- ❌ Saving a script with CRLF line endings on Windows → ✅ save with LF; otherwise bash treats `\r` as part of the command.

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
