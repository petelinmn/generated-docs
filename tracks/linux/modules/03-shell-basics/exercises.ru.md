---
title: "Основы shell: упражнения"
lang: ru
track: linux
kind: module
doc: exercises
slug: 03-shell-basics
part: 1
tags: [linux, bash, shell, redirection, pipeline, quoting, exit-status]
versions: "Ubuntu 26.04 LTS, Bash 5.3, WSL 2"
status: draft
updated: 2026-11-25
---

# Основы shell: упражнения

🌐 **Русский** · [English](exercises.en.md)

> Сначала решите сами, в пустом каталоге внутри `~`.

## 1. 🟢 Развести потоки

Выполните `ls /etc/hostname nope`: вывод — в `ok.txt`, ошибки — в `bad.txt`, затем напечатайте код возврата и оба файла.

<details><summary>Подсказка</summary>

Прочитайте `$?` сразу после команды.

</details>

<details><summary>Решение</summary>

```bash
ls /etc/hostname nope > ok.txt 2> bad.txt
echo "status=$?"
cat ok.txt bad.txt
```

```text
status=2
/etc/hostname
ls: cannot access 'nope': No such file or directory
```

Код 2 у `ls` означает серьёзную проблему, например недоступный аргумент командной строки.

</details>

## 2. 🟡 Топ слов

Из текста получите два самых частых слова с количеством, одним конвейером.

<details><summary>Подсказка</summary>

`tr`, `sort`, `uniq -c`.

</details>

<details><summary>Решение</summary>

```bash
echo 'to be or not to be to' | tr ' ' '\n' | sort | uniq -c | sort -rn | head -n 2
```

```text
      3 to
      2 be
```

`uniq` считает только соседние повторы, поэтому сначала `sort`.

</details>

## 3. 🟡 Не потерять ошибку

Покажите, что конвейер с упавшей первой командой «успешен», и исправьте это.

<details><summary>Подсказка</summary>

`pipefail`.

</details>

<details><summary>Решение</summary>

```bash
cat missing 2>/dev/null | wc -l; echo "status=$?"
set -o pipefail
cat missing 2>/dev/null | wc -l; echo "status=$?"
```

```text
0
status=0
0
status=1
```

`wc` в обоих случаях печатает 0; различается только код конвейера.

</details>

## 4. 🔴 Файлы с пробелами

Посчитайте строки во всех `.txt`, имена которых содержат пробелы, не сломав их на части.

<details><summary>Подсказка</summary>

Цикл по шаблону и кавычки.

</details>

<details><summary>Решение</summary>

```bash
printf 'a\nb\n' > 'first file.txt'
printf 'c\n' > 'second file.txt'
for f in *.txt; do
    printf '%s: %s\n' "$f" "$(wc -l < "$f")"
done
```

```text
first file.txt: 2
second file.txt: 1
```

Результаты раскрытия шаблона на слова не делятся, а `"$f"` в кавычках сохраняет имя целым.

</details>
