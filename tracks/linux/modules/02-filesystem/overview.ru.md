---
title: "Файловая система Linux"
lang: ru
track: linux
kind: module
doc: overview
slug: 02-filesystem
part: 1
tags: [linux, filesystem, fhs, inode, hard-link, symlink, path]
versions: "Ubuntu 26.04 LTS, Bash 5.3, WSL 2"
levels: [1, 2, 3, 4]
status: draft
updated: 2026-11-25
---

# Файловая система Linux

🌐 **Русский** · [English](overview.en.md)

## Коротко

В Linux одно дерево каталогов, в котором доступны настройки, программы, журналы, устройства и сведения о ядре. Модуль объясняет, где что лежит, как работают пути, что такое inode и чем различаются ссылки.

## Ключевые идеи

- Дерево начинается с `/`; диски монтируются в каталоги.
- Имя файла живёт в каталоге, а метаданные — в inode.
- Жёсткая ссылка — ещё одно имя; символическая — файл с путём.
- Данные удаляются, когда нет имён и открытых дескрипторов.
- `df` и `du` отвечают на разные вопросы.
- `/proc` и `/sys` — виртуальные: содержимое создаётся при чтении.

## Минимальный пример

```bash
echo hello > a.txt
ln a.txt hard.txt
ln -s a.txt soft.txt
ls -li
```

```text
total 8
336077 -rw-r--r-- 2 ubuntu ubuntu 6 Nov 25 18:24 a.txt
336077 -rw-r--r-- 2 ubuntu ubuntu 6 Nov 25 18:24 hard.txt
336084 lrwxrwxrwx 1 ubuntu ubuntu 5 Nov 25 18:24 soft.txt -> a.txt
```

## Когда применять и когда нет

- ✅ Символические ссылки — переключать версии и давать удобные пути.
- ✅ Переименование на той же файловой системе — атомарная замена файла.
- ❌ Не храните важное в `/tmp`.
- ❌ Не рассчитывайте на жёсткие ссылки между файловыми системами.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | FHS, пути, операции, типы файлов, inode и ссылки |
| [2. Продвинутая теория](advanced.ru.md) | относительные ссылки, переключение версий, `du`/`df`, `find` |
| [3. Экспертная теория](expert.ru.md) | разбор пути, времена, удалённые открытые файлы, подвохи, вопросы |
| 4. Самопроверка | [квиз](quiz.ru.md), [упражнения](exercises.ru.md) |
| 5. Запускаемые примеры | ещё не создан — `/learn linux 02-filesystem level 5` |
| 6. Тесты | ещё не создан — `/learn linux 02-filesystem level 6` |

## Связанные материалы

- Пререквизиты: [01 · Подготовка окружения](../01-setup/overview.ru.md)
- Дальше: [03 · Основы shell](../03-shell-basics/overview.ru.md)
- Файлы и потоки из программы: [Node.js: файлы, потоки, процессы](../../../javascript/modules/16-nodejs-files-streams-processes/overview.ru.md)
- Хранение в кластере: [Kubernetes 08 · Хранилище](../../../kubernetes/modules/08-storage/overview.ru.md)

## Источники

- [hier(7)](https://man7.org/linux/man-pages/man7/hier.7.html)
- [inode(7)](https://man7.org/linux/man-pages/man7/inode.7.html)
- [symlink(7)](https://man7.org/linux/man-pages/man7/symlink.7.html)
- [path_resolution(7)](https://man7.org/linux/man-pages/man7/path_resolution.7.html)
- [proc_sys_fs(5)](https://man7.org/linux/man-pages/man5/proc_sys_fs.5.html)
- [ln(1)](https://man7.org/linux/man-pages/man1/ln.1.html)
