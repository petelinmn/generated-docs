---
title: "Подготовка окружения: продвинутый уровень"
lang: ru
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

# Подготовка окружения: продвинутый уровень

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Управление дистрибутивами

Команды PowerShell:

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

У импортированного дистрибутива пользователь по умолчанию задаётся в `/etc/wsl.conf` (секция `[user]`, ключ `default`).

## WSL 1 и WSL 2

| | WSL 1 | WSL 2 |
|---|---|---|
| Настоящее ядро Linux в управляемой VM | нет | да |
| Полная совместимость системных вызовов | нет | да |
| systemd | нет | да |
| Скорость при работе с файлами другой ОС | выше | ниже |

Для учёбы нужен WSL 2. Преобразовать дистрибутив: `wsl --set-version <имя> 2` — перед этим сделайте резервную копию.

## Два файла настроек

| | `/etc/wsl.conf` | `%UserProfile%\.wslconfig` |
|---|---|---|
| Действует на | один дистрибутив | все дистрибутивы WSL 2 |
| Настраивает | `[boot]`, `[automount]`, `[network]`, `[interop]`, `[user]` | память, процессоры, swap, сеть виртуальной машины (`[wsl2]`) |

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

Это два разных файла, показанных в одном блоке. Изменения применяются после остановки: `wsl --terminate <имя>` или `wsl --shutdown`. Если просто закрыть окна, дистрибутив остановится не сразу (документация называет около 8 секунд). Синтаксически неверный `.wslconfig` молча игнорируется: WSL запускается без этих настроек.

## Файлы Windows и Linux

- Диски Windows смонтированы в `/mnt/c`, `/mnt/d`. Из Windows файлы Linux видны по пути `\\wsl$`; `explorer.exe .` открывает текущий каталог в Проводнике.
- Проект, с которым работают инструменты Linux, держите в файловой системе Linux (`~/src/...`): так быстрее. Редактировать из Windows можно через VS Code с расширением WSL.
- Без опции `metadata` права Linux на `/mnt/c` не сохраняются: `chmod` умеет только поставить или снять атрибут Windows «только чтение» (когда убраны все биты записи).
- С `metadata` владелец и режим хранятся в расширенных атрибутах NTFS, но превысить права пользователя Windows всё равно нельзя.

## Что особенного в Ubuntu 26.04

Многие инструкции в интернете написаны для GNU coreutils и классического sudo. В Ubuntu 26.04 по умолчанию стоят их реализации на Rust: uutils (rust-coreutils) и sudo-rs. При этом `cp`, `mv` и `rm` пока остаются из GNU.

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

Версии в выводе зависят от установленных обновлений. Если поведение расходится с инструкцией или текст ошибки другой, проверьте, какая реализация перед вами (`--version`).

## Лучшие практики

- Перед рискованными экспериментами — `wsl --export`.
- Настройки окружения (`~/.bashrc`, список пакетов) держите в репозитории (проект Dotfiles в дорожной карте): тогда новый дистрибутив настраивается за минуты.
- В скриптах, которые должны работать и на других дистрибутивах, не полагайтесь на особенности одной реализации утилит.

## Частые ошибки

- ❌ Изменить `wsl.conf` и ждать эффекта в уже открытом окне → ✅ остановить дистрибутив.
- ❌ Клонировать репозиторий в `/mnt/c` и жаловаться на медленный `git status` → ✅ клонировать в `~`.
- ❌ Редактировать скрипт в Windows с переводами строк CRLF → ✅ сохранять LF; иначе bash видит `\r` как часть команды.

## Связанные материалы

- Дальше: [02 · Файловая система](../02-filesystem/overview.ru.md), [03 · Основы shell](../03-shell-basics/overview.ru.md)
- Контейнеры как одноразовая среда: [Kubernetes 01 · Контейнеры](../../../kubernetes/modules/01-containers/overview.ru.md)

## Источники

- [Install WSL](https://learn.microsoft.com/en-us/windows/wsl/install)
- [Basic commands for WSL](https://learn.microsoft.com/en-us/windows/wsl/basic-commands)
- [Working across file systems](https://learn.microsoft.com/en-us/windows/wsl/filesystems)
- [Advanced settings configuration in WSL](https://learn.microsoft.com/en-us/windows/wsl/wsl-config)
- [Comparing WSL versions](https://learn.microsoft.com/en-us/windows/wsl/compare-versions)
- [File permissions for WSL](https://learn.microsoft.com/en-us/windows/wsl/file-permissions)
- [Case sensitivity](https://learn.microsoft.com/en-us/windows/wsl/case-sensitivity)
- [Ubuntu 26.04 LTS release notes: summary for LTS users](https://documentation.ubuntu.com/release-notes/26.04/summary-for-lts-users/)
