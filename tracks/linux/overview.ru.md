---
title: "Linux"
lang: ru
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

🌐 **Русский** · [English](overview.en.md)

## Коротко

Linux — ядро операционной системы с открытым исходным кодом. Дистрибутивы (Ubuntu, Debian, Fedora и другие) добавляют к ядру системные утилиты, менеджер пакетов и пользовательское окружение. Linux работает на большинстве серверов в интернете и в облаке, лежит в основе контейнеров и Android. Для разработчика это среда, в которой его приложения работают в продакшене, поэтому уверенная работа в shell, с правами, процессами и сетью — базовый навык.

## Окружение

| Что | Инструмент |
|---|---|
| Где практиковаться | WSL2 (Ubuntu) — `wsl --install -d Ubuntu`; опасные эксперименты — в VM или контейнере |
| Shell | bash |
| Код модуля | `code/` — `examples/*.sh`, `solutions/`, `tests/*.bats` |
| Запустить все тесты модуля | `bats tests` в папке `code/` (bats-core: `sudo apt install bats`) |
| Проверка скриптов | shellcheck — `sudo apt install shellcheck` |

> 💡 Репозиторий лежит на диске Windows, в WSL он доступен как `/mnt/c/...`. Права (`chmod`) и производительность там ведут себя иначе, чем в родной файловой системе Linux. Упражнения с правами и файлами выполняй в домашнем каталоге WSL (`~`).

## Дорожная карта

Прогресс изучения — в [PROGRESS.md](../../PROGRESS.md). Уровень детализации модулей по умолчанию — **4** (теория и самопроверка, без кода), см. [уровни детализации](../../README.ru.md#уровни-детализации).

### Часть 1. Основы

- [01 · Подготовка окружения](modules/01-setup/overview.ru.md) — WSL2, дистрибутивы, терминал, где безопасно экспериментировать
- [02 · Файловая система](modules/02-filesystem/overview.ru.md) — FHS (/etc, /var, /proc…), пути, inode, жёсткие и символические ссылки
- [03 · Основы shell](modules/03-shell-basics/overview.ru.md) — навигация, справка (man, --help), история, перенаправления, пайпы
- [04 · Файлы и права](modules/04-files-and-permissions/overview.ru.md) — chmod/chown, umask, setuid/setgid/sticky bit, ACL
- [05 · Обработка текста](modules/05-text-processing/overview.ru.md) — grep, sed, awk, cut, sort, uniq, xargs, jq
- [06 · Процессы и сигналы](modules/06-processes/overview.ru.md) — ps, top, kill и сигналы, jobs, nice, /proc
- [07 · Пакеты](modules/07-packages/overview.ru.md) — apt и dnf, репозитории, зависимости
- [08 · Пользователи и группы](modules/08-users-and-groups/overview.ru.md) — useradd, sudo, /etc/passwd, /etc/shadow

### Часть 2. Продвинутые темы

- [09 · Bash-скрипты](modules/09-bash-scripting/overview.ru.md) — переменные, условия, циклы, функции, `set -euo pipefail`, shellcheck
- [10 · Сеть](modules/10-networking/overview.ru.md) — ip, ss, ping, curl, dig, маршрутизация, firewall (nftables, ufw)
- [11 · systemd и журнал](modules/11-systemd/overview.ru.md) — unit-файлы, systemctl, journalctl, таймеры
- [12 · SSH](modules/12-ssh/overview.ru.md) — ключи, `~/.ssh/config`, туннели, scp и rsync
- [13 · Диски и файловые системы](modules/13-storage-and-disks/overview.ru.md) — разделы, mount, fstab, LVM, df и du
- [14 · Планирование задач](modules/14-scheduling-tasks/overview.ru.md) — cron, systemd timers, at

### Часть 3. Темы для опытных

- [15 · Производительность и диагностика](modules/15-performance-troubleshooting/overview.ru.md) — load average, vmstat, iostat, strace, lsof
- [16 · Безопасность](modules/16-security/overview.ru.md) — hardening, sudoers, AppArmor и SELinux, аудит, fail2ban
- [17 · Namespaces и cgroups](modules/17-namespaces-and-cgroups/overview.ru.md) — изоляция процессов, основа контейнеров (мост к Kubernetes)
- [18 · Загрузка и ядро](modules/18-boot-and-kernel/overview.ru.md) — UEFI, GRUB, initramfs, модули ядра, sysctl

## Инструменты

- [tmux](tools/tmux/overview.ru.md) — мультиплексор терминала, сессии на сервере
- [Vim](tools/vim/overview.ru.md) — минимум, чтобы уверенно редактировать на сервере
- [fzf](tools/fzf/overview.ru.md) — нечёткий поиск по файлам и истории
- [ripgrep](tools/ripgrep/overview.ru.md) — быстрый поиск по коду
- [jq](tools/jq/overview.ru.md) — обработка JSON в терминале

## Рецепты

- [systemd-сервис для приложения](recipes/systemd-service-for-app/overview.ru.md) — автозапуск, рестарт, логи
- [Nginx как reverse proxy](recipes/nginx-reverse-proxy/overview.ru.md) — проксирование и TLS (Let's Encrypt)
- [Скрипт бэкапа](recipes/backup-script/overview.ru.md) — ротация, логирование, запуск по расписанию
- [Hardening SSH](recipes/ssh-hardening/overview.ru.md) — безопасная настройка SSH-сервера
- [Анализ логов](recipes/log-analysis/overview.ru.md) — однострочники для типичных вопросов к логам
- [Закончилось место на диске](recipes/disk-full-troubleshooting/overview.ru.md) — найти и освободить

## Проекты

- [Настройка сервера с нуля](projects/server-setup/overview.ru.md) — пользователи, SSH, firewall, сервис, бэкапы, мониторинг
- [Dotfiles](projects/dotfiles/overview.ru.md) — свои настройки окружения и скрипт установки

## Связанные треки

- [Kubernetes](../kubernetes/overview.ru.md) — следующий шаг: модули 01–06 этого трека нужны для Kubernetes

## Источники

- `man` и `--help` — первоисточник по любой команде
- [Документация Ubuntu Server](https://documentation.ubuntu.com/server/)
- [Bash Reference Manual](https://www.gnu.org/software/bash/manual/)
