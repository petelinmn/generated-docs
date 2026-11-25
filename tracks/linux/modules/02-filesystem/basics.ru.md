---
title: "Файловая система Linux: основы"
lang: ru
track: linux
kind: module
doc: basics
slug: 02-filesystem
part: 1
tags: [linux, filesystem, fhs, inode, hard-link, symlink, path]
versions: "Ubuntu 26.04 LTS, Bash 5.3, WSL 2"
status: draft
updated: 2026-11-25
---

# Файловая система Linux: основы

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [подготовка окружения](../01-setup/basics.ru.md) · **Время:** ~2 ч

## Зачем это нужно

Настройки, журналы, программы, устройства и сведения о процессах в Linux доступны как файлы в одном дереве. Кто знает, где что лежит и как устроены пути, тот находит причину проблемы, а не перебирает каталоги.

## Одно дерево

Дисков с буквами нет: всё начинается с корня `/`, а другие диски и файловые системы подключаются (монтируются) в каталоги дерева.

| Каталог | Что там |
|---|---|
| `/etc` | настройки этой машины |
| `/home`, `/root` | домашние каталоги пользователей и root |
| `/usr` | программы и библиотеки из пакетов; `/usr/local` — установленное вами вручную |
| `/opt` | сторонние пакеты отдельными каталогами |
| `/var` | изменяющиеся данные: `/var/log` — журналы, `/var/lib` — состояние сервисов |
| `/tmp`, `/var/tmp` | временные файлы; могут быть удалены без предупреждения |
| `/run` | сведения о работающей системе с момента загрузки |
| `/dev` | файлы устройств |
| `/proc`, `/sys` | виртуальные файловые системы: процессы и ядро |
| `/mnt`, `/media` | точки временного монтирования и съёмные носители |
| `/srv` | данные, которые отдаёт этот сервер |
| `/boot` | файлы загрузчика и ядра |

В Ubuntu `/bin`, `/sbin` и `/lib` — символические ссылки на каталоги внутри `/usr`. Справка прямо в системе: `man hier`.

## Пути

- **Абсолютный** начинается с `/`; **относительный** считается от текущего каталога.
- `.` — текущий каталог, `..` — родительский, `~` — домашний (раскрывает shell), `cd -` — предыдущий.

```bash
pwd
mkdir -p project/src project/docs
cd project/src; pwd
cd ../docs; pwd
cd -
cd ~; pwd
```

```text
/home/ubuntu/lab
/home/ubuntu/lab/project/src
/home/ubuntu/lab/project/docs
/home/ubuntu/lab/project/src
/home/ubuntu
```

## Основные операции

```bash
mkdir notes
echo 'first line' > notes/a.txt
cp notes/a.txt notes/b.txt
mv notes/b.txt notes/c.txt
ls notes
cat notes/c.txt
rm notes/c.txt
cp -r notes backup
ls -R .
```

```text
a.txt
c.txt
first line
.:
backup
notes

./backup:
a.txt

./notes:
a.txt
```

`rm` удаляет без корзины. `cp -r` и `rm -r` работают с каталогом вместе с содержимым; `mkdir -p` создаёт недостающие родительские каталоги.

## Имена файлов

Регистр важен. Расширение — часть имени, на тип файла оно не влияет. Имя, начинающееся с точки, скрыто от `ls`. Имя с пробелами берут в кавычки.

```bash
touch visible .hidden 'two words.txt'
ls
ls -A
ls 'two words.txt'
```

```text
two words.txt
visible
.hidden
two words.txt
visible
two words.txt
```

## Типы файлов

Первый символ в `ls -l`: `-` обычный файл, `d` каталог, `l` символическая ссылка, `c` и `b` символьное и блочное устройство, `p` именованный канал, `s` сокет. `file` определяет тип по содержимому.

```bash
file /etc /bin /etc/hostname /dev/null
ls -l /dev/null
mkfifo pipe; ls -l pipe | cut -c1-10
```

```text
/etc:          directory
/bin:          symbolic link to usr/bin
/etc/hostname: ASCII text
/dev/null:     character special (1/3)
crw-rw-rw- 1 root root 1, 3 Nov 25 18:10 /dev/null
prw-r--r--
```

## inode и ссылки

У файла есть **inode** — запись с типом, правами, владельцем, размером, временем и расположением данных. Имени в inode нет: каталог сопоставляет имена номерам inode.

- **Жёсткая ссылка** — ещё одно имя того же inode.
- **Символическая ссылка** — отдельный файл, в котором записан путь.

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

Первый столбец — номер inode (у вас будет другой). У `a.txt` и `hard.txt` он общий, а число `2` после прав — это количество имён. Что будет, если удалить исходное имя:

```bash
echo hello > a.txt
ln a.txt hard.txt
ln -s a.txt soft.txt
rm a.txt
cat hard.txt
cat soft.txt
```

```text
hello
cat: soft.txt: No such file or directory
```

Данные живы, пока есть хотя бы одно имя. Символическая ссылка осталась указывать в пустоту.

## Ошибки новичка

- ❌ `rm -r $dir/*` с пустой переменной → ✅ сначала `echo`, чтобы увидеть, что раскроется; в скриптах `"${dir:?}"`.
- ❌ Искать «расширение» как тип файла → ✅ `file`.
- ❌ Складывать важное в `/tmp` → ✅ домашний каталог.

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
