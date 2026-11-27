---
title: "Файлы и права: базовая теория"
lang: ru
track: linux
kind: module
doc: basics
slug: 04-files-and-permissions
part: 1
tags: [linux, file-permissions, chmod, umask, acl, setuid]
versions: "Ubuntu 26.04 LTS, Bash 5.3, uutils coreutils 0.10"
status: draft
updated: 2026-11-27
---

# Файлы и права: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [файловая система](../02-filesystem/basics.ru.md), [shell](../03-shell-basics/basics.ru.md) · **Время:** ~2 ч

Вывод получен в контейнере `ubuntu:26.04`. В этом выпуске `ls`, `chmod`, `stat` и другие — из `uutils coreutils`, поэтому часть текстов ошибок отличается от GNU coreutils.

> ⚠️ Примеры создают пользователей и меняют владельцев. Запускайте их только в одноразовой среде от root: `docker run --rm -it ubuntu:26.04 bash`, затем `apt-get update && apt-get install -y acl`.

Подготовка для примеров:

```bash
useradd -m ann && useradd -m bob && groupadd devs
usermod -aG devs ann && usermod -aG devs bob
mkdir /lab && chmod 755 /lab && cd /lab
```

## Чтение прав

```bash
umask 022
touch f && mkdir d
stat -c '%A %a %U:%G %n' f d
```

```text
-rw-r--r-- 644 root:root f
drwxr-xr-x 755 root:root d
```

Первый символ — тип (`-` файл, `d` каталог, `l` ссылка). Дальше три группы `rwx`: владелец, группа, остальные. В числах r=4, w=2, x=1, поэтому `rw-r--r--` — это `644`.

## Изменение режима

```bash
touch m
chmod 640 m;            stat -c '%A %a' m
chmod u+x,g-r,o=r m;    stat -c '%A %a' m
chmod a=rw m;           stat -c '%A %a' m
```

```text
-rw-r----- 640
-rwx---r-- 704
-rw-rw-rw- 666
```

Числовая форма задаёт все биты сразу. Символьная — это «кому» (`u g o a`), операция (`+ - =`) и права.

## Смена владельца и группы

```bash
touch ch
chown ann ch;   stat -c %U:%G ch
chown :devs ch; stat -c %U:%G ch
```

```text
ann:root
ann:devs
```

Передать файл другому пользователю может только root. Обычный владелец может сменить группу лишь на ту, в которой состоит.

## Каталоги

| Бит | Файл | Каталог |
|---|---|---|
| `r` | читать содержимое | перечислять имена |
| `w` | менять содержимое | создавать, переименовывать, удалять записи (вместе с `x`) |
| `x` | запускать | входить и обращаться к записям по имени |

```bash
mkdir dd && echo data > dd/a && chmod 644 dd/a

chmod 744 dd                      # others: r, no x
su ann -c 'ls /lab/dd; cat /lab/dd/a'

chmod 711 dd                      # others: x, no r
su ann -c 'ls /lab/dd; cat /lab/dd/a'
```

```text
a
cat: /lab/dd/a: Permission denied
ls: cannot open directory '/lab/dd': Permission denied
data
```

## Ошибки новичка

- ❌ `chmod 777` → ✅ выяснить, какому пользователю какое право нужно.
- ❌ `chmod -R 644 dir` отнимает `x` у каталогов → ✅ разделять файлы и каталоги или использовать `X`.
- ❌ Защищать файл от удаления его режимом → ✅ решает каталог.

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)

## Источники

- [chmod(1)](https://man7.org/linux/man-pages/man1/chmod.1.html)
- [chown(1)](https://man7.org/linux/man-pages/man1/chown.1.html)
- [path_resolution(7)](https://man7.org/linux/man-pages/man7/path_resolution.7.html)
- [inode(7)](https://man7.org/linux/man-pages/man7/inode.7.html)
