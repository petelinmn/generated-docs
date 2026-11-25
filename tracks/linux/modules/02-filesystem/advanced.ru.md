---
title: "Файловая система Linux: продвинутый уровень"
lang: ru
track: linux
kind: module
doc: advanced
slug: 02-filesystem
part: 1
tags: [linux, filesystem, fhs, inode, hard-link, symlink, path]
versions: "Ubuntu 26.04 LTS, Bash 5.3, WSL 2"
status: draft
updated: 2026-11-25
---

# Файловая система Linux: продвинутый уровень

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Относительные символические ссылки

Относительный путь в ссылке считается от каталога, где лежит ссылка, а не от текущего. `ln -sr` вычисляет его сам.

```bash
mkdir d1 d2
echo data > d1/f
ln -s d1/f d2/wrong
cat d2/wrong
ln -sr d1/f d2/right
readlink d2/right
cat d2/right
realpath d2/right
```

```text
cat: d2/wrong: No such file or directory
../d1/f
data
/home/ubuntu/lab/d1/f
```

## Переключение ссылки на каталог

Приём «`current` указывает на нужную версию» ломается без `-n`:

```bash
mkdir v1 v2
ln -s v1 current
ln -sf v2 current
readlink current
ls v1
rm v1/v2
ln -sfn v2 current
readlink current
```

```text
v1
v2
v2
```

`ln -sf v2 current` пошёл по ссылке `current` в каталог `v1` и создал ссылку `v1/v2`. С `-n` ссылка на каталог обрабатывается как обычный файл.

## Ограничения жёстких ссылок

Нельзя ссылаться на каталог и на другую файловую систему: номера inode уникальны только внутри одной.

```bash
mkdir d; echo x > f
ln d d-link
ln f /dev/shm/f-link
```

```text
ln: d: hard link not allowed for directory
ln: failed to create hard link 'f' => '/dev/shm/f-link': Invalid cross-device link
```

| | Жёсткая | Символическая |
|---|---|---|
| Что хранит | тот же inode | путь |
| Каталоги | нет | да |
| Другая файловая система | нет | да |
| Удалили оригинал | данные доступны | ссылка «висит» |
| `ls -l` | неотличима от оригинала | `имя -> цель` |

## Размер: ls, du, df

- `ls -l` показывает логический размер файла.
- `du` — сколько места занято на диске.
- `df` — заполненность файловой системы; `df -i` — inode: место может «кончиться», когда кончились inode.

```bash
echo hi > small
truncate -s 1G sparse
ls -lh sparse | awk '{print $5}'
du -h sparse small
```

```text
1.0G
0	sparse
4.0K	small
```

Разреженный файл (sparse) объявляет гигабайт, но блоков не занимает.

## Поиск по дереву

```bash
mkdir -p a/b; echo x > a/b/one.log; ln a/b/one.log a/twin
find . -name '*.log'
find . -type d
find . -samefile a/twin | sort
```

```text
./a/b/one.log
.
./a
./a/b
./a/b/one.log
./a/twin
```

Полезные условия `find`: `-name`, `-iname`, `-type f|d|l`, `-size +10M`, `-mtime -1`, `-xtype l` (висящие ссылки), `-xdev` (не переходить на другие файловые системы). Шаблон берите в кавычки, иначе его раскроет shell.

## Лучшие практики

- В скриптах используйте абсолютные пути или явно переходите в нужный каталог с проверкой: `cd /srv/app || exit 1`.
- Для переключения версий — `ln -sfn`.
- Перед массовым удалением сначала выполните тот же `find` без `-delete`.

## Частые ошибки

- ❌ `find . -name *.log` без кавычек → ✅ `-name '*.log'`.
- ❌ `find . -delete -name x` → ✅ условия до `-delete`: выражение вычисляется слева направо.
- ❌ Освобождать место, глядя только на `du` → ✅ сверьте с `df`: удалённые, но открытые файлы `du` не видит.

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
