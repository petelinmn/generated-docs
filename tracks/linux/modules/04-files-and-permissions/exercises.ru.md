---
title: "Упражнения: файлы и права"
lang: ru
track: linux
kind: module
doc: exercises
slug: 04-files-and-permissions
part: 1
tags: [linux, file-permissions, chmod, umask, acl, setuid]
versions: "Ubuntu 26.04 LTS, Bash 5.3, uutils coreutils 0.10"
status: draft
updated: 2026-11-27
---

# Упражнения: файлы и права

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки. Работай в одноразовом контейнере с подготовкой из [базовой теории](basics.ru.md#чтение-прав).

## 1. 🟡 Каталог команды

**Задача:** участники `devs` редактируют файлы друг друга в `team` даже при `umask 077`; остальные ничего не видят.

<details><summary>Подсказка</summary>

setgid и ACL по умолчанию.

</details>

<details><summary>Решение</summary>

```bash
mkdir team && chgrp devs team && chmod 2770 team
setfacl -d -m g:devs:rw team       # new files stay group-writable even with umask 077
su ann -c 'umask 077; echo from-ann > /lab/team/a'
su bob -c 'echo from-bob >> /lab/team/a'
```

</details>

## 2. 🟢 Починить дерево сайта

**Задача:** всё в `site` имеет `777`; сделать каталогам `755`, файлам `644`.

<details><summary>Подсказка</summary>

`find -type`.

</details>

<details><summary>Решение</summary>

```bash
find site -type d -exec chmod 755 {} +
find site -type f -exec chmod 644 {} +
```

</details>

## 3. 🟡 Один дополнительный читатель

**Задача:** `ann` владеет `report` и редактирует его, `bob` только читает, остальным доступа нет.

<details><summary>Подсказка</summary>

ACL.

</details>

<details><summary>Решение</summary>

```bash
chmod 600 report && chown ann report
setfacl -m u:bob:r report
```

</details>

## 4. 🔴 Ящик для сдачи работ

**Задача:** пользователи создают файлы в `drop`, но не видят список и не удаляют чужие.

<details><summary>Подсказка</summary>

`w` и `x` без `r` и sticky.

</details>

<details><summary>Решение</summary>

```bash
mkdir drop && chmod 1733 drop      # drwx-wx-wt
su ann -c 'umask 077; echo a > /lab/drop/ann.txt'
```

Знающий имя всё равно прочитает файл, если это позволяет его режим, поэтому нужен `umask 077`.

</details>

## 5. 🟢 Аудит

**Задача:** перечислить все файлы с setuid и setgid в корневой файловой системе.

<details><summary>Подсказка</summary>

`find -perm /6000`.

</details>

<details><summary>Решение</summary>

```bash
find / -xdev -type f -perm /6000 -exec ls -l {} + 2>/dev/null
```

</details>
