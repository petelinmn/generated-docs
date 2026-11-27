---
title: "Основы shell"
lang: ru
track: linux
kind: module
doc: overview
slug: 03-shell-basics
part: 1
tags: [linux, bash, shell, redirection, pipeline, quoting, exit-status]
versions: "Ubuntu 26.04 LTS, Bash 5.3, WSL 2"
levels: [1, 2, 3, 4]
status: draft
updated: 2026-11-26
---

# Основы shell

🌐 **Русский** · [English](overview.en.md)

## Коротко

Shell разбирает набранную строку и запускает программы. Модуль учит находить справку, пользоваться историей, управлять потоками, соединять команды конвейерами и не ошибаться с кавычками.

## Ключевые идеи

- Сначала shell раскрывает строку, потом запускает команду.
- `--help`, `man`, `help`, `type`, `apropos` — справка всегда рядом.
- Три потока: stdin, stdout, stderr.
- Перенаправления выполняются слева направо.
- Код 0 — успех; у конвейера — код последней команды.
- Переменные берут в двойные кавычки.

## Минимальный пример

```bash
printf 'pear\napple\npear\nfig\n' > fruit
sort fruit | uniq -c | sort -rn | head -n 2
```

```text
      2 pear
      1 fig
```

## Когда применять и когда нет

- ✅ Shell — соединять утилиты и автоматизировать короткие действия.
- ❌ Для сложной логики и структур данных возьмите язык общего назначения.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | команды, справка, история, потоки, конвейеры, коды, кавычки |
| [2. Продвинутая теория](advanced.ru.md) | порядок перенаправлений, `pipefail`, окружение, `PATH`, файлы настроек |
| [3. Экспертная теория](expert.ru.md) | порядок раскрытий, справочник, подвохи, вопросы с собеседований |
| 4. Самопроверка | [квиз](quiz.ru.md), [упражнения](exercises.ru.md) |
| 5. Запускаемые примеры | ещё не создан — `/learn linux 03-shell-basics level 5` |
| 6. Тесты | ещё не создан — `/learn linux 03-shell-basics level 6` |

## Связанные материалы

- Пререквизиты: [02 · Файловая система](../02-filesystem/overview.ru.md)
- Среда для практики: [01 · Подготовка окружения](../01-setup/overview.ru.md)
- Потоки и конвейеры из программы: [Node.js: файлы, потоки, процессы](../../../javascript/modules/16-nodejs-files-streams-processes/overview.ru.md)
- [04 · Файлы и права](../04-files-and-permissions/overview.ru.md)

## Источники

- [bash(1)](https://man7.org/linux/man-pages/man1/bash.1.html)
- [man(1)](https://man7.org/linux/man-pages/man1/man.1.html)
