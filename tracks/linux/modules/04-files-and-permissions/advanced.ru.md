---
title: "Файлы и права: продвинутая теория"
lang: ru
track: linux
kind: module
doc: advanced
slug: 04-files-and-permissions
part: 1
tags: [linux, file-permissions, chmod, umask, acl, setuid]
versions: "Ubuntu 26.04 LTS, Bash 5.3, uutils coreutils 0.10"
status: draft
updated: 2026-11-27
---

# Файлы и права: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

Примеры используют подготовку из [базовой теории](basics.ru.md#чтение-прав).

## Какой класс применяется

```bash
echo x > own && chown ann:devs own && chmod 060 own
su ann -c 'cat /lab/own'          # ann owns the file and is in devs
su bob -c 'cat /lab/own'          # bob is only in devs
```

```text
cat: /lab/own: Permission denied
x
```

Ядро выбирает один класс: владелец, если пользователь владеет файлом; иначе группа, если он в ней состоит; иначе остальные. Права других классов не складываются.

## Удаление — операция с каталогом

```bash
mkdir open && chmod 777 open
touch open/rootfile && chmod 400 open/rootfile
su ann -c 'rm -f /lab/open/rootfile && echo removed'
```

```text
removed
```

## umask

Программа запрашивает режим (обычно `666` для файлов и `777` для каталогов), и биты, заданные в `umask`, убираются.

```bash
( umask 027; touch f27; mkdir d27; stat -c '%A %a %n' f27 d27 )
( umask 002; touch f02; stat -c '%A %a %n' f02 )
```

```text
-rw-r----- 640 f27
drwxr-x--- 750 d27
-rw-rw-r-- 664 f02
```

В контейнере у root было `0022`, а в сеансе пользователя через `su` — `0002`.

## X: выполнение только там, где оно имеет смысл

```bash
# before: tree and tree/sub 700, tree/file 600, tree/sub/script 700
chmod -R go+rX tree
stat -c '%A %n' tree tree/sub tree/file tree/sub/script
```

```text
drwxr-xr-x tree
drwxr-xr-x tree/sub
-rw-r--r-- tree/file
-rwxr-xr-x tree/sub/script
```

`X` добавляет `x` каталогам и тем файлам, у которых он уже есть хотя бы для кого-то.

## setgid на каталоге

```bash
mkdir sg && chgrp devs sg && chmod 2775 sg
su ann -c 'touch /lab/sg/f; mkdir /lab/sg/sd'
stat -c '%A %U:%G %n' sg sg/f sg/sd
```

```text
drwxrwsr-x root:devs sg
-rw-rw-r-- ann:devs sg/f
drwxrwsr-x ann:devs sg/sd
```

Новые записи получают группу каталога, а новые подкаталоги наследуют бит.

## Sticky bit

```bash
mkdir st && chmod 1777 st
su ann -c 'echo a > /lab/st/annfile'
su bob -c 'rm -f /lab/st/annfile'
stat -c '%A %a %n' st /tmp
```

```text
rm: cannot remove '/lab/st/annfile': Operation not permitted
drwxrwxrwt 1777 st
drwxrwxrwt 1777 /tmp
```

В каталоге со sticky пользователь удаляет только свои записи (владелец каталога и root — любые).

## setuid

```bash
cp /usr/bin/id myid && chmod u+s myid
stat -c '%A %a' myid
su ann -c '/lab/myid'
```

```text
-rwsr-xr-x 4755
uid=1001(ann) gid=1001(ann) euid=0(root) groups=1001(ann),1003(devs)
```

Программа работает с эффективным UID владельца файла. На интерпретируемые скрипты Linux setuid не действует.

## ACL

```bash
echo secret > af && chmod 600 af
setfacl -m u:bob:r af
su bob -c 'cat /lab/af'
stat -c '%A %a' af
getfacl -c af
```

```text
secret
-rw-r----- 640
user::rw-
user:bob:r--
group::---
mask::r--
other::---
```

При наличии ACL средняя тройка режима показывает **маску** — верхнюю границу для именованных пользователей и групп. `ls -l` помечает такой файл знаком `+`.

```bash
chmod g-r af               # with an ACL this changes the mask, not the group entry
getfacl -c af
su bob -c 'cat /lab/af'
```

```text
user::rw-
user:bob:r--	#effective:---
group::---
mask::---
other::---

cat: /lab/af: Permission denied
```

ACL по умолчанию на каталоге (`setfacl -d`) наследуются новыми файлами.

## Лучшие практики

- Давайте права через группы, а не «остальным».
- Общий каталог: группа, setgid и ACL по умолчанию, если у пользователей строгий umask.
- Проверяйте setuid-файлы.

## Частые ошибки

- ❌ Ждать, что новая группа заработает в текущем сеансе → ✅ войти заново.
- ❌ Делать `chmod` файлу с ACL, не глядя на маску → ✅ смотреть `getfacl`.

## Связанные материалы

- [Базовая теория](basics.ru.md)
- [Экспертная теория](expert.ru.md)

## Источники

- [chmod(1)](https://man7.org/linux/man-pages/man1/chmod.1.html)
- [umask(2)](https://man7.org/linux/man-pages/man2/umask.2.html)
- [inode(7)](https://man7.org/linux/man-pages/man7/inode.7.html)
- [execve(2)](https://man7.org/linux/man-pages/man2/execve.2.html)
- [setfacl(1)](https://man7.org/linux/man-pages/man1/setfacl.1.html)
- [acl(5)](https://man7.org/linux/man-pages/man5/acl.5.html)
