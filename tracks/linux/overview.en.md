---
title: "Linux"
lang: en
track: linux
kind: track
doc: overview
slug: linux
tags: [linux, bash, shell]
versions: "Ubuntu LTS (WSL2)"
default_level: 4
status: draft
updated: 2026-11-20
---

# Linux

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

Linux is an open-source operating system kernel. Distributions (Ubuntu, Debian, Fedora and others) add system utilities, a package manager and a user environment on top of the kernel. Linux runs most servers on the internet and in the cloud, and underpins containers and Android. For a developer it is the environment where their applications run in production, so confident work with the shell, permissions, processes and networking is a core skill.

## Environment

| What | Tool |
|---|---|
| Where to practice | WSL2 (Ubuntu) — `wsl --install -d Ubuntu`; risky experiments go to a VM or a container |
| Shell | bash |
| Module code | `code/` — `examples/*.sh`, `solutions/`, `tests/*.bats` |
| Run all module tests | `bats tests` inside `code/` (bats-core: `sudo apt install bats`) |
| Script linting | shellcheck — `sudo apt install shellcheck` |

> 💡 The repository lives on the Windows drive and is available in WSL as `/mnt/c/...`. Permissions (`chmod`) and performance behave differently there than on a native Linux file system. Do exercises with permissions and files in your WSL home directory (`~`).

## Roadmap

Learning progress is tracked in [PROGRESS.md](../../PROGRESS.md). Default detail level for modules: **4** (theory and self-check, no code), see [detail levels](../../README.en.md#detail-levels).

### Part 1. Fundamentals

- [01 · Setting up the environment](modules/01-setup/overview.en.md) — WSL2, distributions, terminal, where to experiment safely
- [02 · File system](modules/02-filesystem/overview.en.md) — FHS (/etc, /var, /proc…), paths, inodes, hard and symbolic links
- [03 · Shell basics](modules/03-shell-basics/overview.en.md) — navigation, help (man, --help), history, redirections, pipes
- [04 · Files and permissions](modules/04-files-and-permissions/overview.en.md) — chmod/chown, umask, setuid/setgid/sticky bit, ACL
- [05 · Text processing](modules/05-text-processing/overview.en.md) — grep, sed, awk, cut, sort, uniq, xargs, jq
- [06 · Processes and signals](modules/06-processes/overview.en.md) — ps, top, kill and signals, jobs, nice, /proc
- [07 · Packages](modules/07-packages/overview.en.md) — apt and dnf, repositories, dependencies
- [08 · Users and groups](modules/08-users-and-groups/overview.en.md) — useradd, sudo, /etc/passwd, /etc/shadow

### Part 2. Advanced topics

- [09 · Bash scripting](modules/09-bash-scripting/overview.en.md) — variables, conditions, loops, functions, `set -euo pipefail`, shellcheck
- [10 · Networking](modules/10-networking/overview.en.md) — ip, ss, ping, curl, dig, routing, firewall (nftables, ufw)
- [11 · systemd and the journal](modules/11-systemd/overview.en.md) — unit files, systemctl, journalctl, timers
- [12 · SSH](modules/12-ssh/overview.en.md) — keys, `~/.ssh/config`, tunnels, scp and rsync
- [13 · Disks and file systems](modules/13-storage-and-disks/overview.en.md) — partitions, mount, fstab, LVM, df and du
- [14 · Scheduling tasks](modules/14-scheduling-tasks/overview.en.md) — cron, systemd timers, at

### Part 3. Expert topics

- [15 · Performance and troubleshooting](modules/15-performance-troubleshooting/overview.en.md) — load average, vmstat, iostat, strace, lsof
- [16 · Security](modules/16-security/overview.en.md) — hardening, sudoers, AppArmor and SELinux, auditing, fail2ban
- [17 · Namespaces and cgroups](modules/17-namespaces-and-cgroups/overview.en.md) — process isolation, the foundation of containers (a bridge to Kubernetes)
- [18 · Boot and kernel](modules/18-boot-and-kernel/overview.en.md) — UEFI, GRUB, initramfs, kernel modules, sysctl

## Tools

- [tmux](tools/tmux/overview.en.md) — terminal multiplexer, sessions on a server
- [Vim](tools/vim/overview.en.md) — the minimum needed to edit confidently on a server
- [fzf](tools/fzf/overview.en.md) — fuzzy search over files and history
- [ripgrep](tools/ripgrep/overview.en.md) — fast code search
- [jq](tools/jq/overview.en.md) — JSON processing in the terminal

## Recipes

- [systemd service for an app](recipes/systemd-service-for-app/overview.en.md) — autostart, restart, logs
- [Nginx as a reverse proxy](recipes/nginx-reverse-proxy/overview.en.md) — proxying and TLS (Let's Encrypt)
- [Backup script](recipes/backup-script/overview.en.md) — rotation, logging, scheduled runs
- [SSH hardening](recipes/ssh-hardening/overview.en.md) — secure SSH server configuration
- [Log analysis](recipes/log-analysis/overview.en.md) — one-liners for typical questions about logs
- [Disk is full](recipes/disk-full-troubleshooting/overview.en.md) — find and free up space

## Projects

- [Server setup from scratch](projects/server-setup/overview.en.md) — users, SSH, firewall, a service, backups, monitoring
- [Dotfiles](projects/dotfiles/overview.en.md) — your environment settings and an install script

## Related tracks

- [Kubernetes](../kubernetes/overview.en.md) — the next step: modules 01–06 of this track are required for Kubernetes

## Sources

- `man` and `--help` — the primary source for any command
- [Ubuntu Server documentation](https://documentation.ubuntu.com/server/)
- [Bash Reference Manual](https://www.gnu.org/software/bash/manual/)
