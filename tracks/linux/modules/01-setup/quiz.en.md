---
title: "Environment setup: quiz"
lang: en
track: linux
kind: module
doc: quiz
slug: 01-setup
part: 1
tags: [linux, wsl, distribution, terminal, shell, setup]
versions: "Ubuntu 26.04 LTS, Bash 5.3, WSL 2"
status: draft
updated: 2026-11-25
---

# Environment setup: quiz

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What is the main difference between WSL 2 and WSL 1?

- A) WSL 2 needs no virtualisation
- B) WSL 2 is a Docker container
- C) WSL 2 runs a real Linux kernel in a lightweight virtual machine
- D) WSL 2 translates Linux system calls into Windows calls

<details><summary>Answer</summary>

**C.** A real kernel gives full system call compatibility and systemd. Translating calls is the WSL 1 approach. WSL 2 does need virtualisation, and Docker is a separate product.
→ [Fundamentals](basics.en.md)

</details>

## 2. 🟢 What does `wsl --unregister Ubuntu` do?

- A) Permanently deletes the distribution and all its data
- B) Stops the distribution
- C) Removes it as the default, files stay
- D) Backs it up and then deletes it

<details><summary>Answer</summary>

**A.** Data, settings and software are lost. Stopping is `--terminate`; a backup is your job, with `--export`.
→ [Fundamentals](basics.en.md)

</details>

## 3. 🟢 Where are Linux tools faster?

- A) In `/mnt/c/Users/me/project`
- B) There is no difference
- C) Only on `/mnt/d`
- D) In `~/project` inside the Linux filesystem

<details><summary>Answer</summary>

**D.** Access to a mounted Windows drive crosses the OS boundary and is noticeably slower. The drive letter does not matter.
→ [Fundamentals](basics.en.md)

</details>

## 4. 🟢 How do `uname -r` and `/etc/os-release` differ?

- A) The second is the WSL version
- B) The first is the kernel version, the second describes the distribution
- C) They are two ways to read the same version
- D) The first is the bash version

<details><summary>Answer</summary>

**B.** A container makes this obvious: the distribution comes from the image, the kernel is shared with the host.
→ [Fundamentals](basics.en.md)

</details>

## 5. 🟢 What does `#` at the end of the prompt mean?

- A) The shell is read-only
- B) The shell runs as root
- C) The command is commented out
- D) A background job is running

<details><summary>Answer</summary>

**B.** By convention `$` is a regular user and `#` is root. `#` starts a comment only inside the typed line.
→ [Fundamentals](basics.en.md)

</details>

## 6. 🟡 You changed `/etc/wsl.conf`. What now?

- A) Run `source /etc/wsl.conf`
- B) Nothing, it is read before every command
- C) Only restart bash with `exec bash`
- D) Stop the distribution (`wsl --terminate`) and start it again

<details><summary>Answer</summary>

**D.** Settings are read when the distribution starts. It is not a shell script, and restarting bash does not restart the distribution.
→ [Advanced theory](advanced.en.md)

</details>

## 7. 🟡 Where do you limit the memory of the WSL 2 virtual machine?

- A) In `%UserProfile%\.wslconfig`
- B) In `/etc/wsl.conf`
- C) In `~/.bashrc`
- D) In `/etc/os-release`

<details><summary>Answer</summary>

**A.** Memory belongs to the VM shared by all distributions, so the setting is global. `wsl.conf` applies to one distribution.
→ [Advanced theory](advanced.en.md)

</details>

## 8. 🟡 Select the true statements about `chmod` on `/mnt/c` without the `metadata` option.

- [ ] A) `chmod 600` hides the file from other Windows users
- [ ] B) The filesystem becomes read-only
- [ ] C) The Linux mode is not stored
- [ ] D) Removing all write bits sets the Windows read-only attribute

<details><summary>Answer</summary>

**C, D.** Permissions there are derived from Windows access. `chmod` does not affect what other Windows users can do.
→ [Advanced theory](advanced.en.md)

</details>

## 9. 🟡 A script with CRLF fails with `required file not found` (or `bad interpreter`). Why?

- A) The file is not executable
- B) bash is not installed
- C) The script must be run with `sudo`
- D) The first line names `/bin/bash\r` as the interpreter, and no such file exists

<details><summary>Answer</summary>

**D.** `\r` is an ordinary character on Linux. A missing execute bit would give `Permission denied`.
→ [Advanced theory](advanced.en.md)

</details>

## 10. 🟡 What is true of Ubuntu 26.04 by default?

- A) bash was replaced by a Rust shell
- B) Many core utilities come from uutils, and `sudo` is sudo-rs
- C) All utilities are still GNU
- D) `cp`, `mv` and `rm` are uutils too

<details><summary>Answer</summary>

**B.** `cp`, `mv` and `rm` stay GNU in 26.04. The shell is still bash.
→ [Advanced theory](advanced.en.md)

</details>

## 11. 🔴 Open question: you want to try a command that could wreck the system. How do you set up the experiment?

<details><summary>Answer</summary>

Key points: a disposable environment (a `--rm` container or an imported copy of the distribution); don't mount working directories into it, and remember that WSL exposes Windows drives under `/mnt`; know how to roll back beforehand; check what the command will touch first (`echo` it to see the expansions).
→ [Fundamentals](basics.en.md)

</details>
