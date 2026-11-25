---
title: "Файловая система Linux: упражнения"
lang: ru
track: linux
kind: module
doc: exercises
slug: 02-filesystem
part: 1
tags: [linux, filesystem, fhs, inode, hard-link, symlink, path]
versions: "Ubuntu 26.04 LTS, Bash 5.3, WSL 2"
status: draft
updated: 2026-11-25
---

# Файловая система Linux: упражнения

🌐 **Русский** · [English](exercises.en.md)

> Решайте в пустом каталоге внутри `~`. Числа inode у вас будут другими.

## 1. 🟢 Дерево проекта

Одной командой создайте `app/{src,tests,docs}`, положите по пустому файлу в каждый и выведите все файлы.

<details><summary>Подсказка</summary>

Фигурные скобки и `mkdir -p`.

</details>

<details><summary>Решение</summary>

```bash
mkdir -p app/{src,tests,docs}
touch app/{src,tests,docs}/.keep
find app -type f | sort
```

```text
app/docs/.keep
app/src/.keep
app/tests/.keep
```

Скобки раскрывает bash до запуска команды.

</details>

## 2. 🟡 Релизы со ссылкой

Сделайте `releases/1`, `releases/2` и ссылку `current`. Переключите её с 1 на 2 и обратно так, чтобы внутри релизов не появилось лишних файлов.

<details><summary>Подсказка</summary>

Опция `-n`.

</details>

<details><summary>Решение</summary>

```bash
mkdir -p releases/1 releases/2
echo one > releases/1/version; echo two > releases/2/version
ln -s releases/1 current;   cat current/version
ln -sfn releases/2 current; cat current/version
ln -sfn releases/1 current; cat current/version
find releases | sort
```

```text
one
two
one
releases
releases/1
releases/1/version
releases/2
releases/2/version
```

`-n` не даёт `ln` зайти в каталог по старой ссылке.

</details>

## 3. 🟡 Висящие ссылки

Создайте рабочую и висящую ссылки и найдите только висящую.

<details><summary>Подсказка</summary>

Тест `-xtype`.

</details>

<details><summary>Решение</summary>

```bash
echo x > real
ln -s real good
ln -s missing bad
find . -xtype l
```

```text
./bad
```

`-type l` нашёл бы обе: он смотрит на саму ссылку.

</details>

## 4. 🔴 Спасти удалённый файл

Фоновый процесс держит файл открытым, а вы его «случайно» удалили. Восстановите содержимое.

<details><summary>Подсказка</summary>

`/proc/<pid>/fd`.

</details>

<details><summary>Решение</summary>

```bash
echo 'precious' > data
sleep 30 < data &
pid=$!
sleep 0.2
rm data
cp "/proc/$pid/fd/0" restored
cat restored
kill "$pid"
```

```text
precious
```

Работает только пока процесс жив и держит дескриптор. `$!` — PID последней фоновой команды.

</details>
