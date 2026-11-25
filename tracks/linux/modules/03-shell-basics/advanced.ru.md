---
title: "Основы shell: продвинутый уровень"
lang: ru
track: linux
kind: module
doc: advanced
slug: 03-shell-basics
part: 1
tags: [linux, bash, shell, redirection, pipeline, quoting, exit-status]
versions: "Ubuntu 26.04 LTS, Bash 5.3, WSL 2"
status: draft
updated: 2026-11-25
---

# Основы shell: продвинутый уровень

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Порядок перенаправлений

Перенаправления выполняются слева направо. `2>&1` означает «направь 2 туда, куда сейчас смотрит 1», а не «связать навсегда».

```bash
ls /etc/hostname nope > all 2>&1
cat all
echo ---
ls /etc/hostname nope 2>&1 > only-out
echo ---
cat only-out
```

```text
ls: cannot access 'nope': No such file or directory
/etc/hostname
---
ls: cannot access 'nope': No such file or directory
---
/etc/hostname
```

Во втором случае stderr получил старое назначение stdout (экран), и только потом stdout ушёл в файл. В bash есть краткая запись `&> файл`, а для конвейера — `|&`. В `sh` их нет.

## Код возврата конвейера

По умолчанию это код последней команды; ошибка в середине теряется.

```bash
false | true; echo "$?  ${PIPESTATUS[*]}"
false | true; echo "${PIPESTATUS[@]}"
set -o pipefail
false | true; echo $?
```

```text
0  1 0
1 0
1
```

## Перенаправление и sudo

```bash
echo x > /etc/demo
echo x | sudo tee /etc/demo
sudo rm /etc/demo
```

```text
bash: line 4: /etc/demo: Permission denied
x
```

Файл открывает ваш shell, а не `sudo`. `tee` запускается от root и пишет сам; для дописывания — `tee -a`.

## Here-документы, here-строки, tee

Кавычки вокруг метки отключают раскрытия.

```bash
cat <<EOF
home is $HOME
EOF
cat <<'EOF'
home is $HOME
EOF
wc -w <<< 'here string'
echo logged | tee a.log b.log
```

```text
home is /home/ubuntu
home is $HOME
2
logged
```

## Переменные и окружение

Обычная переменная видна только shell. `export` добавляет её в окружение дочерних процессов. Дочерний процесс не может изменить переменные родителя.

```bash
A=1
bash -c 'echo "A=[$A]"'
export A
bash -c 'echo "A=[$A]"'
B=2 bash -c 'echo "B=[$B]"'
echo "B=[$B]"
```

```text
A=[]
A=[1]
B=[2]
B=[]
```

## PATH и запуск программ

`PATH` — список каталогов, где shell ищет команду.

```bash
echo "$PATH"
printf '#!/bin/sh\necho mine\n' > hello
chmod +x hello
hello; echo "exit=$?"
./hello
```

```text
/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin
bash: line 7: hello: command not found
exit=127
mine
```

Коды: `127` — команда не найдена, `126` — найдена, но не запускается (например, нет права на выполнение).

## Подстановка процесса

`<(команда)` даёт имя файла, из которого читается вывод команды.

```bash
printf 'b\na\n' > one
printf 'a\nc\n' > two
diff <(sort one) <(sort two)
```

```text
2c2
< b
---
> c
```

## Настройки: .bashrc и .profile

- Login-shell читает `/etc/profile`, затем первый найденный из `~/.bash_profile`, `~/.bash_login`, `~/.profile`.
- Интерактивный не-login shell читает `~/.bashrc`.
- Стандартный `~/.profile` в Ubuntu подключает `~/.bashrc`.
- Алиасы и функции держат в `~/.bashrc`; применить изменения — `source ~/.bashrc`.

В стандартном `.bashrc` Ubuntu: `HISTSIZE=1000`, `HISTFILESIZE=2000`, `HISTCONTROL=ignoreboth` (не запоминать повторы подряд и строки, начинающиеся с пробела), алиас `ll='ls -alF'`.

## Лучшие практики

- Берите переменные в двойные кавычки: `"$var"`.
- Проверяйте шаблон через `echo` перед `rm`.
- Секреты не пишите прямо в команду: она остаётся в истории и видна в списке процессов.

## Частые ошибки

- ❌ `a && b || c` как if/else → ✅ `c` выполнится и тогда, когда `a` успешна, а `b` — нет; используйте `if`.
- ❌ `sudo echo ... > /etc/...` → ✅ `| sudo tee`.
- ❌ Рассчитывать на алиас в скрипте → ✅ пишите команду полностью: в неинтерактивном shell алиасы не раскрываются.

## Связанные материалы

- Пререквизиты: [02 · Файловая система](../02-filesystem/overview.ru.md)
- Среда для практики: [01 · Подготовка окружения](../01-setup/overview.ru.md)
- Потоки и конвейеры из программы: [Node.js: файлы, потоки, процессы](../../../javascript/modules/16-nodejs-files-streams-processes/overview.ru.md)

## Источники

- [bash(1)](https://man7.org/linux/man-pages/man1/bash.1.html)
- [man(1)](https://man7.org/linux/man-pages/man1/man.1.html)
