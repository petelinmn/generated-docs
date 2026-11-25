---
title: "Подготовка окружения"
lang: ru
track: linux
kind: module
doc: overview
slug: 01-setup
part: 1
tags: [linux, wsl, distribution, terminal, shell, setup]
versions: "Ubuntu 26.04 LTS, Bash 5.3, WSL 2"
levels: [1, 2, 3, 4]
status: draft
updated: 2026-11-25
---

# Подготовка окружения

🌐 **Русский** · [English](overview.en.md)

## Коротко

Модуль о том, где и как практиковаться: WSL 2 с Ubuntu, что такое ядро, дистрибутив, терминал и shell, и где безопасно ломать.

## Ключевые идеи

- WSL 2 — настоящее ядро Linux в лёгкой VM под управлением Windows.
- Версия ядра и версия дистрибутива — разные вещи.
- Учебные файлы держат в `~`, а не в `/mnt/c`.
- Опасное — только в одноразовой среде.
- `wsl --unregister` удаляет всё безвозвратно.
- Ubuntu 26.04 по умолчанию использует uutils и sudo-rs.

## Минимальный пример

```powershell
wsl --install -d Ubuntu-26.04    # PowerShell, as administrator
wsl -l -v
```

## Когда применять и когда нет

- ✅ WSL — для учёбы и разработки под Linux на Windows.
- ✅ Контейнер — для экспериментов, которые не жалко выбросить.
- ❌ Для изучения загрузки и разметки дисков берите виртуальную машину.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | установка, словарь, первые команды, где экспериментировать |
| [2. Продвинутая теория](advanced.ru.md) | управление дистрибутивами, настройки, файлы Windows и Linux |
| [3. Экспертная теория](expert.ru.md) | устройство WSL 2, справочник, подвохи, вопросы с собеседований |
| 4. Самопроверка | [квиз](quiz.ru.md), [упражнения](exercises.ru.md) |
| 5. Запускаемые примеры | ещё не создан — `/learn linux 01-setup level 5` |
| 6. Тесты | ещё не создан — `/learn linux 01-setup level 6` |

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
