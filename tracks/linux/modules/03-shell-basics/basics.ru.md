---
title: "Основы shell: основы"
lang: ru
track: linux
kind: module
doc: basics
slug: 03-shell-basics
part: 1
tags: [linux, bash, shell, redirection, pipeline, quoting, exit-status]
versions: "Ubuntu 26.04 LTS, Bash 5.3, WSL 2"
status: draft
updated: 2026-11-25
---

# Основы shell: основы

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [файловая система](../02-filesystem/basics.ru.md) · **Время:** ~2 ч

## Зачем это нужно

Сервером управляют из shell. Он позволяет запускать программы, соединять их друг с другом и повторять действия без мыши.

## Из чего состоит команда

`команда -опции аргументы`: сначала shell разбирает строку (кавычки, переменные, шаблоны), затем запускает команду с готовым списком аргументов. Короткие опции часто объединяются: `-l -a` = `-la`. `--` означает «дальше только аргументы».

Команда может быть встроенной в shell или программой на диске:

```bash
type cd ls
type -a echo
help cd | head -n 3
```

```text
cd is a shell builtin
ls is /usr/bin/ls
echo is a shell builtin
echo is /usr/bin/echo
echo is /bin/echo
cd: cd [-L|[-P [-e]]] [-@] [dir]
    Change the shell working directory.
    
```

## Справка

| Что нужно | Команда |
|---|---|
| Краткие опции | `команда --help` |
| Полное описание | `man команда` (поиск `/текст`, выход `q`) |
| Встроенная команда bash | `help команда` |
| Не знаю названия | `apropos слово` (или `man -k`) |

Разделы man: 1 — команды, 5 — форматы файлов, 8 — администрирование. `man 5 passwd` и `man 1 passwd` — разные страницы.

```bash
man -f passwd
apropos -s 5 'password file'
ls --help | head -n 1
```

```text
passwd (1)           - change user password
passwd (5)           - the password file
passwd (5)           - the password file
shadow (5)           - shadowed password file
List directory contents.
```

## Клавиши и история

| Клавиши | Действие |
|---|---|
| `Tab` | дополнить имя |
| `↑`, `↓` | предыдущие команды |
| `Ctrl+R` | поиск по истории |
| `Ctrl+C` | прервать команду |
| `Ctrl+D` | конец ввода; в пустой строке — выход |
| `Ctrl+A`, `Ctrl+E` | в начало, в конец строки |
| `Ctrl+L` | очистить экран |
| `!!`, `sudo !!` | повторить прошлую команду, повторить с sudo |

`history` выводит список. История сохраняется в `~/.bash_history`.

## Три потока и перенаправления

У программы есть стандартный ввод (0, stdin), вывод (1, stdout) и поток ошибок (2, stderr).

| Запись | Действие |
|---|---|
| `> f` | stdout в файл, файл перезаписывается |
| `>> f` | дописать |
| `2> f` | stderr в файл |
| `< f` | stdin из файла |
| `> f 2>&1` | оба в файл |

```bash
ls /etc/hostname nope > out.txt 2> err.txt
cat out.txt
cat err.txt
echo again >> out.txt
wc -l < out.txt
```

```text
/etc/hostname
ls: cannot access 'nope': No such file or directory
2
```

## Конвейеры

`a | b` передаёт stdout команды `a` на stdin команды `b`. stderr в конвейер не попадает. Команды работают одновременно.

```bash
printf 'pear\napple\npear\nfig\n' > fruit
sort fruit | uniq -c | sort -rn | head -n 2
```

```text
      2 pear
      1 fig
```

## Код возврата

Каждая команда возвращает число от 0 до 255: `0` — успех. Он лежит в `$?`. `&&` выполняет следующую команду после успеха, `||` — после неудачи.

```bash
ls /etc/hostname > /dev/null; echo $?
ls nope 2> /dev/null; echo $?
no-such-command 2> /dev/null; echo $?
mkdir demo && cd demo && echo inside
cd /nope 2> /dev/null || echo 'cannot enter'
```

```text
0
2
127
inside
cannot enter
```

## Кавычки

- Без кавычек: раскрываются переменные и шаблоны, строка делится на слова.
- `"..."`: переменные и `$(...)` раскрываются, деления на слова и шаблонов нет.
- `'...'`: всё буквально.

```bash
name=world
echo "hi $name"
echo 'hi $name'
touch 1.log 2.log
echo *.log
echo "*.log"
echo "today: $(basename /usr/bin)"
```

```text
hi world
hi $name
1.log 2.log
*.log
today: bin
```

## Ошибки новичка

- ❌ `sort f > f` → ✅ `sort -o f f`: shell очищает файл до запуска `sort`.
- ❌ `rm $file` → ✅ `rm -- "$file"`.
- ❌ Запустить свой скрипт как `script.sh` → ✅ `./script.sh`: текущего каталога нет в `PATH`.

## Связанные материалы

- Пререквизиты: [02 · Файловая система](../02-filesystem/overview.ru.md)
- Среда для практики: [01 · Подготовка окружения](../01-setup/overview.ru.md)
- Потоки и конвейеры из программы: [Node.js: файлы, потоки, процессы](../../../javascript/modules/16-nodejs-files-streams-processes/overview.ru.md)

## Источники

- [bash(1)](https://man7.org/linux/man-pages/man1/bash.1.html)
- [man(1)](https://man7.org/linux/man-pages/man1/man.1.html)
