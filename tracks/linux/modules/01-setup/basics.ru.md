---
title: "Подготовка окружения: основы"
lang: ru
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

# Подготовка окружения: основы

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** Windows 10 (версия 2004+) или Windows 11, права администратора для установки · **Время:** ~1 ч

## Зачем это нужно

Linux изучают руками. Нужна среда, где можно набирать команды, ошибаться и без потерь начинать заново. На Windows проще всего взять WSL 2 (Windows Subsystem for Linux): настоящее ядро Linux в лёгкой виртуальной машине, которой управляет Windows.

## Словарь

| Термин | Что это |
|---|---|
| Ядро (kernel) | управляет процессами, памятью, устройствами, файловыми системами; это и есть «Linux» |
| Дистрибутив | ядро плюс утилиты, менеджер пакетов и настройки: Ubuntu, Debian, Fedora |
| Терминал | программа-окно, которая показывает текст и передаёт ввод |
| Shell | программа внутри терминала, которая разбирает и запускает команды: bash |

## Установка WSL

Команды этого раздела выполняются в PowerShell на Windows. Установка — от имени администратора, после неё нужна перезагрузка.

```powershell
wsl --list --online              # distributions available to install
wsl --install -d Ubuntu-26.04    # install a specific one
wsl --list --verbose             # installed distributions, state and WSL version
wsl -d Ubuntu-26.04              # start a shell in it
```

`wsl --install` без `-d` включает нужные компоненты Windows и ставит дистрибутив Ubuntu; это сработает, только если WSL ещё не установлен. Новые дистрибутивы получают WSL 2. При первом запуске система просит придумать имя пользователя и пароль: это пользователь Linux, не связанный с учётной записью Windows.

Если Ubuntu у вас уже установлена, посмотрите `wsl -l -v` и проверьте версию командой из следующего раздела: материалы трека проверялись на Ubuntu 26.04 LTS.

## Первые команды: где я

Теперь команды выполняются внутри Linux. Вывод ниже снят в контейнере Ubuntu 26.04, запущенном на ядре WSL 2; имя пользователя у вас будет своё.

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

- `/etc/os-release` описывает дистрибутив; `uname` — ядро. Это разные версии разных вещей.
- Приглашение вида `ubuntu@lab:~$` — это «пользователь@машина:каталог». `$` — обычный пользователь, `#` — root.
- `sudo команда` выполняет одну команду с правами root и спрашивает ваш пароль Linux.

## Обновление пакетов

Сразу после установки обновите список пакетов и сами пакеты:

```bash
sudo apt update          # refresh the package index
sudo apt upgrade         # install newer versions
sudo apt install man-db manpages tree shellcheck bats
```

Подробно о пакетах — модуль 07 дорожной карты.

## Где экспериментировать

| Что делаете | Где |
|---|---|
| Обычные упражнения | домашний каталог WSL (`~`) |
| Всё, что может сломать систему (`rm -rf`, права на `/etc`, firewall) | одноразовый контейнер или отдельный дистрибутив, который не жалко |
| Нужен чистый дистрибутив ещё раз | `wsl --export` до экспериментов, потом `wsl --import` под другим именем |

Одноразовый контейнер (нужен Docker):

```bash
docker run --rm -it ubuntu:26.04 bash    # everything inside is deleted on exit
```

Образ контейнера урезан: в нём нет man-страниц и многих утилит, и вы работаете сразу под root.

## Ошибки новичка

- ❌ Держать учебные файлы в `/mnt/c/...` → ✅ в `~`: там права работают как в Linux и операции быстрее.
- ❌ Работать всё время под root → ✅ обычный пользователь и `sudo` для отдельных команд.
- ❌ Выполнить `wsl --unregister` «чтобы перезапустить» → ✅ для перезапуска есть `wsl --terminate`; `--unregister` безвозвратно удаляет дистрибутив со всеми данными.

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
